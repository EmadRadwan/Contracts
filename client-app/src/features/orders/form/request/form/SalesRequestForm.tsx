import React, {useCallback, useMemo, useRef, useState} from "react";
import Button from "@mui/material/Button";
import Grid from "@mui/material/Grid";
import {Field, Form, FormElement, FormRenderProps} from "@progress/kendo-react-form";

import {Box, Paper, Typography} from "@mui/material";
import {toast} from "react-toastify";
import {SalesRequest} from "../../../../../app/models/order/SalesRequest";
import {requiredValidator} from "../../../../../app/common/form/Validators";
import {useAddSalesRequestMutation, useUpdateSalesRequestMutation} from "../../../../../app/store/apis/salesRequestApi";
import FormDatePicker from "../../../../../app/common/form/FormDatePicker";
import LoadingComponent from "../../../../../app/layout/LoadingComponent";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import FormTextArea from "../../../../../app/common/form/FormTextArea";
import {FormComboBoxVirtualParty} from "../../../../../app/common/form/FormComboBoxVirtualParty";
import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper";
import ModalContainer from "../../../../../app/common/modals/ModalContainer";
import CreateCustomerModalForm from "../../../../parties/form/CreateCustomerModalForm";
import {useAppDispatch} from "../../../../../app/store/configureStore";
import {FormSimpleComboBoxVirtualApartment} from "../../../../../app/common/form/FormSimpleComboBoxVirtualApartment";
import SalesRequestMenu from "../menu/SalesRequestMenu";
import {toNumber} from "lodash";
import {KeyValue} from "@progress/kendo-react-form";
import PaymentPlanModal from "../dashboard/PaymentPlanModal";

let renderCount = 0;

/* ------------------------------------------------------------------ */
/* Props – removed partyInputRef (now internal)                       */

/* ------------------------------------------------------------------ */
interface Props {
    salesRequest?: SalesRequest;
    editMode: number; // 1 = create, 2 = edit
    cancelEdit: () => void;
    onSalesRequestCreated?: (createdRequest: SalesRequest) => void;
}

function SalesRequestForm({
                              salesRequest,
                              editMode,
                              cancelEdit,
                              onSalesRequestCreated,
                          }: Props) {
    console.log(`SalesRequestForm render #${++renderCount}`);

    const [createSR, {isLoading: isCreating}] = useAddSalesRequestMutation();
    const [updateSR, {isLoading: isUpdating}] = useUpdateSalesRequestMutation();
    const [showNewCustomer, setShowNewCustomer] = useState(false);
    const {getTranslatedLabel} = useTranslationHelper();
    const [showPaymentPlan, setShowPaymentPlan] = useState(false);

    const dispatch = useAppDispatch();
    const formRef = useRef<FormRenderProps | null>(null);
    const [buttonFlag, setButtonFlag] = useState(false);
    const [selectedApartment, setSelectedApartment] = useState<SalesRequest | null>(null);
    const [userEditedAdvance, setUserEditedAdvance] = useState(false);
    const [userEditedMaintenance, setUserEditedMaintenance] = useState(false);
    const [userEditedDiscount, setUserEditedDiscount] = useState(false);

    // -----------------------------------------------------------------
    // Internal ref for party input (no longer passed from parent)
    // -----------------------------------------------------------------
    const partyInputRef = useRef<HTMLInputElement>(null);

    const isoToDate = (iso: string | null | undefined): Date | null => {
        if (!iso) return null;
        const d = new Date(iso);
        return isNaN(d.getTime()) ? null : d;   // safety – malformed strings → null
    };

    // -----------------------------------------------------------------
    // Initial values
    // -----------------------------------------------------------------
    const formInitialValues = useMemo(() => {
        if (editMode === 1) return {}; // create → empty form

        const sr = salesRequest!; // guaranteed in edit mode

        // -----------------------------------------------------------------
        // 1. Party object (fromPartyId combo)
        // -----------------------------------------------------------------
        const partyObj = sr.fromPartyId
            ? {
                fromPartyId: sr.fromPartyId,
                fromPartyName: sr.fromPartyName ?? "",
                fromPartyPhone: sr.fromPartyPhone ?? "",
            }
            : null;

        // -----------------------------------------------------------------
        // 2. Apartment object (productId combo)
        // -----------------------------------------------------------------
        const apartmentObj = sr.apartmentId
            ? {
                apartmentId: sr.apartmentId,
                apartmentName: sr.apartmentName ?? "",
                apartmentType: sr.apartmentType ?? "",
                projectName: sr.projectName ?? "",
                floorNumber: sr.floorNumber ?? "",
                apartmentSpaceM2: sr.apartmentSpaceM2 ?? null,
                gardenSpaceM2: sr.gardenSpaceM2 ?? null,
                gardenPricePerM2: sr.gardenPricePerM2 ?? null,
                apartmentPricePerM2: sr.apartmentPricePerM2 ?? null,
                apartmentStatusId: sr.apartmentStatusId ?? "",
                apartmentStatusDescription: sr.apartmentStatusDescription ?? "",
            }
            : null;

        // -----------------------------------------------------------------
        // 3. Final initial-values object
        // -----------------------------------------------------------------
        return {
            // ----- identifiers -------------------------------------------------
            salesRequestId: sr.salesRequestId ?? null,

            // ----- party combo (object) ---------------------------------------
            fromPartyId: partyObj,
            // keep the display name for the virtual combo (optional but handy)
            fromPartyIdName: sr.fromPartyName ?? "",

            // ----- apartment combo (object) -----------------------------------
            productId: apartmentObj,
            // optional display name for the simple combo
            productIdName: sr.apartmentName ?? "",

            // ----- read-only project info (still useful for display) ----------
            projectName: sr.projectName ?? null,
            floorNumber: sr.floorNumber ?? null,
            apartmentSpaceM2: sr.apartmentSpaceM2 ?? null,
            gardenSpaceM2: sr.gardenSpaceM2 ?? null,
            apartmentStatusDescription: sr.apartmentStatusDescription ?? null,

            // ----- pricing -----------------------------------------------------
            apartmentPricePerM2: sr.apartmentPricePerM2 ?? null,
            gardenPricePerM2: sr.gardenPricePerM2 ?? null,
            discount: sr.discount ?? null,
            totalPrice: sr.totalPrice ?? null,
            maintenanceDeposit: sr.maintenanceDeposit ?? null,

            // ----- payment plan ------------------------------------------------
            advancePayment: sr.advancePayment ?? null,
            numberOfInstallments: sr.numberOfInstallments ?? null,
            monthsBetweenInstallments: sr.monthsBetweenInstallments ?? null,

            // ----- dates – convert to real Date objects ------------------------
            saleDate: isoToDate(sr.saleDate),
            dateOfFirstInstallment: isoToDate(sr.dateOfFirstInstallment),

            // ----- free text ---------------------------------------------------
            comments: sr.comments ?? null,
        };
    }, [editMode, salesRequest]);


    // -----------------------------------------------------------------
// Helper – turn combo-box objects into simple strings
// -----------------------------------------------------------------
    const flattenComboValues = (data: any) => {
        const copy = {...data};

        // ---- productId ------------------------------------------------
        // Kendo ComboBox returns the whole selected apartment object
        if (copy.productId && typeof copy.productId === "object") {
            // Why: Backend only wants the apartment ID (string)
            copy.productId = copy.productId.apartmentId ?? copy.productId.ProductId;
        }

        // ---- fromPartyId ---------------------------------------------
        // Party combo returns { fromPartyId: "19", fromPartyName: "…" }
        if (copy.fromPartyId && typeof copy.fromPartyId === "object") {
            copy.fromPartyId = copy.fromPartyId.fromPartyId ?? copy.fromPartyId.partyId;
        }

        return copy;
    };

    // -----------------------------------------------------------------
// Submit
// -----------------------------------------------------------------
    async function handleSubmitData(data: any) {
        console.log('data', data)
        setButtonFlag(true);
        try {
            // 1. Normalise numeric fields ("" → null)
            const normalize = (obj: any): any => {
                const copy = {...obj};
                const numericFields = [
                    "apartmentPricePerM2",
                    "gardenPricePerM2",
                    "discount",
                    "totalPrice",
                    "advancePayment",
                    "numberOfInstallments",
                    "monthsBetweenInstallments",
                    "maintenanceDeposit",
                ] as const;
                numericFields.forEach(field => {
                    if (copy[field] === "" || copy[field] == null) copy[field] = null;
                });
                return copy;
            };

            // 2. Flatten combo-box objects → plain strings
            const flattened = flattenComboValues(normalize(data));

            // 3. Wrap for the server (same pattern as CreateProduct)
            const payload = {salesRequestDto: {...flattened}};

            if (editMode === 2) {
                await updateSR(payload).unwrap();
                toast.success(
                    getTranslatedLabel("salesRequest.form.updateSuccess", "Sales request updated")
                );
            } else {
                const createdRequest = await createSR(payload).unwrap() as SalesRequest;

                toast.success(
                    getTranslatedLabel("salesRequest.form.createSuccess", "Sales request created")
                );

                // Pass full object to parent
                onSalesRequestCreated?.(createdRequest);
            }
        } catch (error: any) {
            console.error("Sales request submission failed:", error);
            toast.error(
                error?.data?.errors
                    ? Object.values(error.data.errors).flat().join(" ")
                    : getTranslatedLabel("salesRequest.form.error", "Failed to process sales request")
            );
        } finally {
            setButtonFlag(false);
        }
    }

    const updateCustomerDropDown = useCallback(
        (newCustomer: { partyId: string; description: string }) => {
            const fieldName = "fromPartyId";               // <-- field name in SalesRequest
            formRef.current?.onChange(fieldName, {
                value: newCustomer.partyId,
                valid: true,
            });
            // The virtual party combo also expects a display name field
            formRef.current?.onChange(`${fieldName}Name`, {
                value: newCustomer.description,
                valid: true,
            });
        },
        [dispatch]
    );

    const calculateBaseTotal = useCallback((
        aptM2: number | null,
        aptPrice: number | null,
        gardenM2: number | null,
        gardenPrice: number | null
    ): number | null => {
        if (aptM2 == null || aptPrice == null || gardenM2 == null || gardenPrice == null) return null;
        return aptM2 * aptPrice + gardenM2 * gardenPrice;
    }, []);

    const calculateFinalTotal = useCallback((
        baseTotal: number | null,
        discount: number | null
    ): number | null => {
        if (baseTotal == null) return null;
        return discount != null ? Math.max(0, baseTotal - discount) : baseTotal;
    }, []);

    const autoSetDerivedFields = useCallback((
        formRenderProps: FormRenderProps,
        finalTotal: number | null
    ) => {
        if (finalTotal !== null) {
            if (!userEditedAdvance) {
                formRenderProps.onChange("advancePayment", {value: finalTotal * 0.20});
            }
            if (!userEditedMaintenance) {
                formRenderProps.onChange("maintenanceDeposit", {value: finalTotal * 0.07});
            }
        } else {
            if (!userEditedAdvance) formRenderProps.onChange("advancePayment", {value: null});
            if (!userEditedMaintenance) formRenderProps.onChange("maintenanceDeposit", {value: null});
        }
    }, [userEditedAdvance, userEditedMaintenance]);

    const handleProductChange = useCallback(
        (formRenderProps: FormRenderProps, e: any) => {
            const apartment = e.value as SalesRequest | null;
            setSelectedApartment(apartment);

            // Reset edit flags on new selection
            setUserEditedAdvance(false);
            setUserEditedDiscount(false);
            setUserEditedMaintenance(false);

            formRenderProps.onChange("apartmentPricePerM2", {value: apartment?.apartmentPricePerM2 ?? null});
            formRenderProps.onChange("gardenPricePerM2", {value: apartment?.gardenPricePerM2 ?? null});
            formRenderProps.onChange("discount", {value: null});

            const baseTotal = calculateBaseTotal(
                toNumber(apartment?.apartmentSpaceM2),
                toNumber(apartment?.apartmentPricePerM2),
                toNumber(apartment?.gardenSpaceM2),
                toNumber(apartment?.gardenPricePerM2)
            );

            const finalTotal = calculateFinalTotal(baseTotal, null);
            formRenderProps.onChange("totalPrice", {value: finalTotal});
            autoSetDerivedFields(formRenderProps, finalTotal);
        },
        [calculateBaseTotal, calculateFinalTotal, autoSetDerivedFields]
    );

    const handlePricePerM2Change = useCallback((
        formRenderProps: FormRenderProps,
        fieldName: "apartmentPricePerM2" | "gardenPricePerM2",
        value: number | null
    ) => {
        formRenderProps.onChange(fieldName, {value});

        const aptM2 = toNumber(selectedApartment?.apartmentSpaceM2);
        const aptPrice = fieldName === "apartmentPricePerM2" ? value : toNumber(formRenderProps.valueGetter("apartmentPricePerM2"));
        const gardenM2 = toNumber(selectedApartment?.gardenSpaceM2);
        const gardenPrice = fieldName === "gardenPricePerM2" ? value : toNumber(formRenderProps.valueGetter("gardenPricePerM2"));
        const discount = userEditedDiscount ? toNumber(formRenderProps.valueGetter("discount")) : null;

        const baseTotal = calculateBaseTotal(aptM2, aptPrice, gardenM2, gardenPrice);
        const finalTotal = calculateFinalTotal(baseTotal, discount);

        formRenderProps.onChange("totalPrice", {value: finalTotal});
        autoSetDerivedFields(formRenderProps, finalTotal);
    }, [selectedApartment, calculateBaseTotal, calculateFinalTotal, userEditedDiscount, autoSetDerivedFields]);

    const handleDiscountChange = useCallback((
        formRenderProps: FormRenderProps,
        value: number | null
    ) => {
        setUserEditedDiscount(true);
        formRenderProps.onChange("discount", {value});

        const baseTotal = calculateBaseTotal(
            toNumber(selectedApartment?.apartmentSpaceM2),
            toNumber(formRenderProps.valueGetter("apartmentPricePerM2")),
            toNumber(selectedApartment?.gardenSpaceM2),
            toNumber(formRenderProps.valueGetter("gardenPricePerM2"))
        );

        const finalTotal = calculateFinalTotal(baseTotal, value);
        formRenderProps.onChange("totalPrice", {value: finalTotal});
        autoSetDerivedFields(formRenderProps, finalTotal);
    }, [selectedApartment, calculateBaseTotal, calculateFinalTotal, autoSetDerivedFields]);

    const handleAdvanceChange = useCallback((
        formRenderProps: FormRenderProps,
        value: number | null
    ) => {
        setUserEditedAdvance(true);
        formRenderProps.onChange("advancePayment", {value});

        const currentTotal = toNumber(formRenderProps.valueGetter("totalPrice"));
        const baseTotal = calculateBaseTotal(
            toNumber(selectedApartment?.apartmentSpaceM2),
            toNumber(formRenderProps.valueGetter("apartmentPricePerM2")),
            toNumber(selectedApartment?.gardenSpaceM2),
            toNumber(formRenderProps.valueGetter("gardenPricePerM2"))
        );
        const discount = userEditedDiscount ? toNumber(formRenderProps.valueGetter("discount")) : 0;

        if (value != null && baseTotal != null && value > currentTotal) {
            // User wants higher advance → scale total up (preserving discount)
            const newFinalTotal = value;
            formRenderProps.onChange("totalPrice", {value: newFinalTotal});
        }
    }, [selectedApartment, calculateBaseTotal, userEditedDiscount]);

    const salesRequestValidator = (values: any): KeyValue<string> | undefined => {
        const t = getTranslatedLabel;                     // shortcut (defined later in render)

        const adv = Number(values.advancePayment ?? 0);
        const tot = Number(values.totalPrice ?? 0);
        const installments = values.numberOfInstallments;
        const firstInstDate = values.dateOfFirstInstallment;
        const duration = values.monthsBetweenInstallments;

        // ---- CASE A: full payment (advance === total) -----------------
        if (adv === tot && tot > 0) {
            if (installments != null ||
                firstInstDate != null ||
                duration != null) {
                return {
                    VALIDATION_SUMMARY: t(
                        "salesRequest.form.validation.fullPayment",
                        "Full payment – installment fields must be empty."
                    )
                };
            }
            return; // OK
        }

        // ---- CASE B: partial payment (advance < total) ---------------
        if (adv < tot && tot > 0) {
            const missing: string[] = [];

            if (!installments) missing.push(t("salesRequest.form.installments", "Installments"));
            if (!firstInstDate) missing.push(t("salesRequest.form.firstInstallmentDate", "First Installment Date"));
            if (!duration) missing.push(t("salesRequest.form.duration", "Days Between Installments"));

            if (missing.length) {
                return {
                    VALIDATION_SUMMARY: `${t(
                        "salesRequest.form.validation.missingInstallments",
                        "The following fields are required when advance payment is less than total price:"
                    )} ${missing.join(", ")}.`
                };
            }
            return; // OK
        }

        // ---- No price information yet – let field validators handle required *
        return;
    };

    let apartmentForModal: { productName: string } | undefined = undefined;

    // This will be set inside the Form render callback
    if (formRef.current) {
        const productIdObj = formRef.current.valueGetter("productId");
        if (productIdObj && typeof productIdObj === "object") {
            apartmentForModal = {
                productName:
                    productIdObj.apartmentName ??
                    productIdObj.productName ??
                    "Unknown Unit",
            };
        }
    }

    return (
        <>
            <SalesRequestMenu
                onMenuSelect={(key) => {
                    if (key === "salesRequest.menu.salesRequests") {
                        cancelEdit(); // ← Forces back to list view
                    }
                }}
            />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container spacing={2}>
                    <Grid item xs={6}>
                        <Box display="flex" justifyContent="space-between">
                            <Typography
                                color={salesRequest?.salesRequestId ? "black" : "green"}
                                sx={{p: 2}}
                                variant="h4"
                            >
                                {salesRequest?.salesRequestId
                                    ? salesRequest.salesRequestId
                                    : getTranslatedLabel("salesRequest.form.new", "New Sales Request")}
                            </Typography>
                        </Box>
                    </Grid>
                </Grid>

                {/* --------------------------------------------------------------- */}
                {/*  Form – attach ref so updateCustomerDropDown can reach it        */}
                {/* --------------------------------------------------------------- */}
                <Form
                    key={editMode}
                    initialValues={formInitialValues}
                    onSubmit={handleSubmitData}
                    validator={salesRequestValidator}
                    render={(formRenderProps: FormRenderProps) => {
                        formRef.current = formRenderProps;
                        const {visited, errors, valueGetter} = formRenderProps;

                        const apt = valueGetter("productId");
                        const party = valueGetter("fromPartyId");

                        const currentFormValues: SalesRequest = {
                            salesRequestId: valueGetter("salesRequestId"),
                            fromPartyId: party?.fromPartyId ?? party?.partyId ?? null,
                            fromPartyName: party?.fromPartyName ?? "",
                            apartmentId: apt?.apartmentId ?? apt?.ProductId ?? null,
                            apartmentName: apt?.apartmentName ?? apt?.productName ?? "",
                            projectName: apt?.projectName ?? null,
                            apartmentSpaceM2: apt?.apartmentSpaceM2 ?? null,
                            gardenSpaceM2: apt?.gardenSpaceM2 ?? null,
                            apartmentPricePerM2: apt?.apartmentPricePerM2 ?? null,
                            gardenPricePerM2: apt?.gardenPricePerM2 ?? null,
                            apartmentStatusDescription: apt?.apartmentStatusDescription ?? "",
                            // pricing
                            apartmentPricePerM2: valueGetter("apartmentPricePerM2"),
                            gardenPricePerM2: valueGetter("gardenPricePerM2"),
                            discount: valueGetter("discount"),
                            totalPrice: valueGetter("totalPrice"),
                            maintenanceDeposit: valueGetter("maintenanceDeposit"),
                            // payment plan
                            advancePayment: valueGetter("advancePayment"),
                            numberOfInstallments: valueGetter("numberOfInstallments"),
                            monthsBetweenInstallments: valueGetter("monthsBetweenInstallments"),
                            dateOfFirstInstallment: valueGetter("dateOfFirstInstallment"),
                            // dates
                            saleDate: valueGetter("saleDate"),
                            // free text
                            comments: valueGetter("comments"),
                        };

                        // -----------------------------------------------------------------
                        // 2. Apartment for the legacy prop
                        // -----------------------------------------------------------------
                        const apartmentForModal = currentFormValues.apartmentId
                            ? {productName: currentFormValues.apartmentName ?? "Unknown Unit"}
                            : undefined;
                        
                        return (
                            <FormElement>
                                <fieldset className="k-form-fieldset">
                                    <Grid container spacing={1} alignItems="flex-end">
                                        <Grid item xs={4}>
                                            <Field
                                                id="productId"
                                                name="productId"
                                                label={getTranslatedLabel("projects.certificate.items.list.product", "Product *")}
                                                component={FormSimpleComboBoxVirtualApartment}
                                                autoComplete="off"
                                                validator={requiredValidator}
                                                onChange={(e) => handleProductChange(formRenderProps, e)}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="saleDate"
                                                name="saleDate"
                                                label={getTranslatedLabel("salesRequest.form.saleDate", "Sale Date *")}
                                                component={FormDatePicker}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={2.5}>
                                            <Field
                                                id="fromPartyId"
                                                name="fromPartyId"
                                                label={getTranslatedLabel("salesRequest.form.from", "From *")}
                                                component={FormComboBoxVirtualParty}
                                                autoComplete="off"
                                                validator={requiredValidator}
                                                inputRef={partyInputRef}
                                            />
                                        </Grid>
                                        <Grid item xs={0.5}>
                                            <Button
                                                size="small"
                                                color="secondary"
                                                onClick={() => setShowNewCustomer(true)}
                                                variant="outlined"
                                                sx={{height: "100%", minWidth: 32, p: 0}}
                                            >
                                                +
                                            </Button>
                                        </Grid>
                                    </Grid>

                                    <Grid container spacing={1} mt={0.5}>
                                        {/* Helper to safely read a nested field */}
                                        {(() => {
                                            const apt = formRenderProps.valueGetter("productId"); // full apartment object or null
                                            return (
                                                <>
                                                    <Grid item xs={3}>
                                                        <Typography variant="caption" color="textSecondary">
                                                            {getTranslatedLabel("salesRequest.form.project", "Project")}
                                                        </Typography>
                                                        <Typography>{apt?.projectName ?? "-"}</Typography>
                                                    </Grid>

                                                    <Grid item xs={2}>
                                                        <Typography variant="caption" color="textSecondary">
                                                            {getTranslatedLabel("salesRequest.form.apartmentM2", "Apt m²")}
                                                        </Typography>
                                                        <Typography>{apt?.apartmentSpaceM2 ?? "-"}</Typography>
                                                    </Grid>

                                                    <Grid item xs={2}>
                                                        <Typography variant="caption" color="textSecondary">
                                                            {getTranslatedLabel("salesRequest.form.gardenM2", "Garden m²")}
                                                        </Typography>
                                                        <Typography>{apt?.gardenSpaceM2 ?? "-"}</Typography>
                                                    </Grid>

                                                    <Grid item xs={3}>
                                                        <Typography variant="caption" color="textSecondary">
                                                            {getTranslatedLabel("salesRequest.form.status", "Status")}
                                                        </Typography>
                                                        <Typography>{apt?.apartmentStatusDescription ?? "-"}</Typography>
                                                    </Grid>

                                                    <Grid item xs={2}/> {/* spacer */}
                                                </>
                                            );
                                        })()}
                                    </Grid>


                                    <Grid container spacing={1}>
                                        <Grid item xs={3}>
                                            <Field
                                                id="apartmentPricePerM2"
                                                name="apartmentPricePerM2"
                                                label={getTranslatedLabel("salesRequest.form.apartmentPriceM2", "Apt/m² *")}
                                                format="n2"
                                                min={0}
                                                component={FormNumericTextBox}
                                                validator={requiredValidator}
                                                onChange={(e: any) => {
                                                    handlePricePerM2Change(formRenderProps, "apartmentPricePerM2", e.value);
                                                }}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="gardenPricePerM2"
                                                name="gardenPricePerM2"
                                                label={getTranslatedLabel("salesRequest.form.gardenPriceM2", "Garden/m²")}
                                                format="n2"
                                                min={0}
                                                component={FormNumericTextBox}
                                                onChange={(e: any) => {
                                                    handlePricePerM2Change(formRenderProps, "gardenPricePerM2", e.value);
                                                }}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="discount"
                                                name="discount"
                                                label={getTranslatedLabel("salesRequest.form.discount", "Discount")}
                                                format="n2"
                                                min={0}
                                                component={FormNumericTextBox}
                                                onChange={(e: any) => {
                                                    handleDiscountChange(formRenderProps, e.value);
                                                }}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="totalPrice"
                                                name="totalPrice"
                                                label={getTranslatedLabel("salesRequest.form.totalPrice", "Total")}
                                                format="n2"
                                                min={0}
                                                validator={requiredValidator}
                                                component={FormNumericTextBox}
                                                disabled={true}
                                            />
                                        </Grid>
                                    </Grid>

                                    <Grid container spacing={1}>
                                        <Grid item xs={3}>
                                            <Field
                                                id="advancePayment"
                                                name="advancePayment"
                                                label={getTranslatedLabel("salesRequest.form.advance", "Advance")}
                                                format="n2"
                                                min={0}
                                                validator={requiredValidator}
                                                component={FormNumericTextBox}
                                                onChange={(e: any) => {
                                                    handleAdvanceChange(formRenderProps, e.value);
                                                }}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="numberOfInstallments"
                                                name="numberOfInstallments"
                                                label={getTranslatedLabel("salesRequest.form.installments", "Installments")}
                                                min={0}
                                                component={FormNumericTextBox}
                                            />
                                        </Grid>
                                        <Grid item xs={2}>
                                            <Field
                                                id="dateOfFirstInstallment"
                                                name="dateOfFirstInstallment"
                                                label={getTranslatedLabel("salesRequest.form.firstInstallmentDate", "First")}
                                                component={FormDatePicker}
                                            />
                                        </Grid>
                                        <Grid item xs={2}>
                                            <Field
                                                id="monthsBetweenInstallments"
                                                name="monthsBetweenInstallments"
                                                label={getTranslatedLabel("salesRequest.form.duration", "Months")}
                                                min={0}
                                                component={FormNumericTextBox}
                                            />
                                        </Grid>
                                        <Grid item xs={2}>
                                            <Field
                                                id="maintenanceDeposit"
                                                name="maintenanceDeposit"
                                                label={getTranslatedLabel("salesRequest.form.maintenanceDeposit", "Maintenance Deposit")}
                                                format="n2"
                                                min={0}
                                                component={FormNumericTextBox}
                                                onChange={(e: any) => {
                                                    formRenderProps.onChange("maintenanceDeposit", {value: e.value});
                                                    if (e.value !== null) setUserEditedMaintenance(true);
                                                }}
                                            />
                                        </Grid>
                                    </Grid>

                                    <Grid item xs={12} mt={0.5}>
                                        <Field
                                            id="comments"
                                            name="comments"
                                            label={getTranslatedLabel("salesRequest.form.comments", "Comments")}
                                            autoComplete="off"
                                            rows={2}
                                            component={FormTextArea}
                                        />
                                    </Grid>

                                    <div className="k-form-buttons" style={{marginTop: 8}}>
                                        <Grid container spacing={1}>
                                            {visited && errors?.VALIDATION_SUMMARY && (
                                                <Grid item xs={12}>
                                                    <div className="k-messagebox k-messagebox-error">
                                                        {errors.VALIDATION_SUMMARY}
                                                    </div>
                                                </Grid>
                                            )}
                                            <Grid item>
                                                <Button
                                                    size="small"
                                                    variant="contained"
                                                    type="submit"
                                                    color="success"
                                                    disabled={buttonFlag || isCreating || isUpdating}
                                                >
                                                    {getTranslatedLabel("general.submit", "Submit")}
                                                </Button>
                                            </Grid>
                                            <Grid item>
                                                <Button
                                                    size="small"
                                                    onClick={cancelEdit}
                                                    color="error"
                                                    variant="contained"
                                                >
                                                    {getTranslatedLabel("general.cancel", "Cancel")}
                                                </Button>
                                            </Grid>
                                            {editMode === 2 && (
                                                <Grid item>
                                                    <Button
                                                        variant="outlined"
                                                        color="primary"
                                                        onClick={() => setShowPaymentPlan(true)}
                                                    >
                                                        {getTranslatedLabel("salesRequest.form.viewPaymentPlan", "View Payment Plan")}
                                                    </Button>
                                                </Grid>
                                            )}
                                        </Grid>
                                    </div>

                                    {(buttonFlag || isCreating || isUpdating) && (
                                        <LoadingComponent
                                            message={getTranslatedLabel("salesRequest.form.processing", "Processing...")}
                                        />
                                    )}

                                    {showPaymentPlan && editMode === 2 && (
                                        <ModalContainer
                                            show={showPaymentPlan}
                                            onClose={() => setShowPaymentPlan(false)}
                                            width={850}
                                        >
                                            <PaymentPlanModal
                                                onClose={() => setShowPaymentPlan(false)}
                                                salesRequest={currentFormValues}   // ← always fresh
                                                apartment={apartmentForModal}
                                            />
                                        </ModalContainer>
                                    )}
                                </fieldset>
                            </FormElement>
                        );
                    }}
                />

                {/* --------------------------------------------------------------- */}
                {/*  New Customer Modal – now passes updateCustomerDropDown         */}
                {/* --------------------------------------------------------------- */}
                {showNewCustomer && (
                    <ModalContainer
                        show={showNewCustomer}
                        onClose={() => setShowNewCustomer(false)}
                        width={500}
                    >
                        <CreateCustomerModalForm
                            onClose={() => setShowNewCustomer(false)}
                            onUpdateCustomerDropDown={updateCustomerDropDown}
                        />
                    </ModalContainer>
                )}
                
            </Paper>
        </>
    );
}

/* ------------------------------------------------------------------ */
/* Memoisation – no partyInputRef in props                             */
/* ------------------------------------------------------------------ */
const arePropsEqual = (prev: Props, next: Props) => {
    return (
        prev.salesRequest?.salesRequestId === next.salesRequest?.salesRequestId &&
        prev.editMode === next.editMode &&
        prev.cancelEdit === next.cancelEdit &&
        prev.onSalesRequestCreated === next.onSalesRequestCreated
    );
};

export default React.memo(SalesRequestForm, arePropsEqual);