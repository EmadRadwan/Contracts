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

let renderCount = 0;

/* ------------------------------------------------------------------ */
/* Props – removed partyInputRef (now internal)                       */

/* ------------------------------------------------------------------ */
interface Props {
    salesRequest?: SalesRequest;
    editMode: number; // 1 = create, 2 = edit
    cancelEdit: () => void;
    /** Now receives full created SalesRequest */
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

    const dispatch = useAppDispatch();               
    const formRef = useRef<FormRenderProps | null>(null);
    const [buttonFlag, setButtonFlag] = useState(false);
    const [selectedApartment, setSelectedApartment] = useState<SalesRequest | null>(null);

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
        // REFACTOR: Build field-by-field instead of spreading raw payload.
        // Why: 
        //   • Guarantees correct types (Date vs string, object vs id)
        //   • Supplies the *full* objects the Kendo combos need
        //   • Protects against future backend property changes
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

            // ----- payment plan ------------------------------------------------
            advancePayment: sr.advancePayment ?? null,
            numberOfInstallments: sr.numberOfInstallments ?? null,
            durationBetweenInstallments: sr.durationBetweenInstallments ?? null,

            // ----- dates – convert to real Date objects ------------------------
            saleDate: isoToDate(sr.saleDate),
            dateOfFirstInstallment: isoToDate(sr.dateOfFirstInstallment),

            // ----- free text ---------------------------------------------------
            comments: sr.comments ?? null,
        };
    }, [editMode, salesRequest]);
    
    
    // -----------------------------------------------------------------
    // Reset – clear form + combo selections
    // -----------------------------------------------------------------
    const handleResetForm = (formRenderProps: FormRenderProps) => {
        setButtonFlag(false);

        // 1. Reset the whole form (clears every field)
        formRenderProps.onFormReset();

        // 2. Explicitly null-out the combo fields (Kendo Form state)
        formRenderProps.onChange("productId",   { value: null });
        formRenderProps.onChange("fromPartyId", { value: null });

        // 3. Clear any *display-name* fields you keep separately
        formRenderProps.onChange("productIdName",   { value: "" });
        formRenderProps.onChange("fromPartyIdName", { value: "" });

        // 4. Reset any local UI state you manage yourself
        setSelectedApartment(null);

        // 5. (Optional) Force the combos to re-render with empty value
        //    – most Kendo combos respect `value={null}` when the prop changes.
    };

    // -----------------------------------------------------------------
// Helper – turn combo-box objects into simple strings
// -----------------------------------------------------------------
    const flattenComboValues = (data: any) => {
        const copy = {...data};

        // ---- productId ------------------------------------------------
        // Kendo ComboBox returns the whole selected apartment object
        if (copy.productId && typeof copy.productId === "object") {
            // REFACTOR: Extract only the key that the server needs
            // Why: Backend only wants the apartment ID (string)
            copy.productId = copy.productId.apartmentId ?? copy.productId.ProductId;
        }

        // ---- fromPartyId ---------------------------------------------
        // Party combo returns { fromPartyId: "19", fromPartyName: "…" }
        if (copy.fromPartyId && typeof copy.fromPartyId === "object") {
            // REFACTOR: Keep only the ID
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
                    "durationBetweenInstallments",
                ] as const;
                numericFields.forEach(field => {
                    if (copy[field] === "" || copy[field] == null) copy[field] = null;
                });
                return copy;
            };

            // 2. Flatten combo-box objects → plain strings
            // REFACTOR: Run *after* numeric normalisation so we don’t touch the objects
            const flattened = flattenComboValues(normalize(data));

            // 3. Wrap for the server (same pattern as CreateProduct)
            const payload = { salesRequestDto: { ...flattened } };

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

    const handleProductChange = useCallback(
        (formRenderProps: FormRenderProps, e: any) => {
            const apartment = e.value as SalesRequest | null;

            // Update local state for display
            setSelectedApartment(apartment);

            // REFACTOR: Update price fields using Kendo's onChange (native form sync)
            formRenderProps.onChange("apartmentPricePerM2", {
                value: apartment?.apartmentPricePerM2 ?? null,
            });
            formRenderProps.onChange("gardenPricePerM2", {
                value: apartment?.gardenPricePerM2 ?? null,
            });

            const aptM2 = toNumber(apartment?.apartmentSpaceM2);
            const aptPrice = toNumber(apartment?.apartmentPricePerM2);
            const gardenM2 = toNumber(apartment?.gardenSpaceM2);
            const gardenPrice = toNumber(apartment?.gardenPricePerM2);

            if (aptM2 !== null && aptPrice !== null && gardenM2 !== null && gardenPrice !== null) {
                const total = aptM2 * aptPrice + gardenM2 * gardenPrice;
                formRenderProps.onChange("totalPrice", {value: total});
            } else {
                // REFACTOR: Clear totalPrice when source data is incomplete
                formRenderProps.onChange("totalPrice", {value: null});
            }
        },
        []
    );

    const salesRequestValidator = (values: any): KeyValue<string> | undefined => {
        const t = getTranslatedLabel;                     // shortcut (defined later in render)

        const adv = Number(values.advancePayment ?? 0);
        const tot = Number(values.totalPrice ?? 0);
        const installments = values.numberOfInstallments;
        const firstInstDate = values.dateOfFirstInstallment;
        const duration = values.durationBetweenInstallments;

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


    return (
        <>
            <SalesRequestMenu selectedMenuItem="/sales-requests"/>
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
                        const {visited, errors} = formRenderProps;

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
                                                label={getTranslatedLabel("accounting.payments.form.from", "From *")}
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
                                        <Grid item xs={3}>
                                            <Typography variant="caption" color="textSecondary">
                                                {getTranslatedLabel("salesRequest.form.project", "Project")}
                                            </Typography>
                                            <Typography>{selectedApartment?.projectName ?? "-"}</Typography>
                                        </Grid>
                                        <Grid item xs={2}>
                                            <Typography variant="caption" color="textSecondary">
                                                {getTranslatedLabel("salesRequest.form.apartmentM2", "Apt m²")}
                                            </Typography>
                                            <Typography>{selectedApartment?.apartmentSpaceM2 ?? "-"}</Typography>
                                        </Grid>
                                        <Grid item xs={2}>
                                            <Typography variant="caption" color="textSecondary">
                                                {getTranslatedLabel("salesRequest.form.gardenM2", "Garden m²")}
                                            </Typography>
                                            <Typography>{selectedApartment?.gardenSpaceM2 ?? "-"}</Typography>
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Typography variant="caption" color="textSecondary">
                                                {getTranslatedLabel("salesRequest.form.status", "Status")}
                                            </Typography>
                                            <Typography>{selectedApartment?.apartmentStatusDescription ?? "-"}</Typography>
                                        </Grid>
                                        <Grid item xs={2}/>
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
                                        <Grid item xs={3}>
                                            <Field
                                                id="dateOfFirstInstallment"
                                                name="dateOfFirstInstallment"
                                                label={getTranslatedLabel("salesRequest.form.firstInstallmentDate", "First")}
                                                component={FormDatePicker}
                                            />
                                        </Grid>
                                        <Grid item xs={3}>
                                            <Field
                                                id="durationBetweenInstallments"
                                                name="durationBetweenInstallments"
                                                label={getTranslatedLabel("salesRequest.form.duration", "Days")}
                                                min={0}
                                                component={FormNumericTextBox}
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
                                            <Grid item>
                                                <Button
                                                    size="small"
                                                    onClick={() => handleResetForm(formRenderProps)}
                                                    color="primary"
                                                    variant="contained"
                                                    disabled={isCreating || isUpdating}
                                                >
                                                    {getTranslatedLabel("salesRequest.form.clear", "Clear")}
                                                </Button>
                                            </Grid>
                                        </Grid>
                                    </div>

                                    {(buttonFlag || isCreating || isUpdating) && (
                                        <LoadingComponent
                                            message={getTranslatedLabel("salesRequest.form.processing", "Processing...")}
                                        />
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
                            // REFACTOR: Pass the same callback used in PaymentForm
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