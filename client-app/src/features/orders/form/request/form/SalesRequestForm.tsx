import React, {useCallback, useEffect, useMemo, useRef, useState} from "react";
import {Form, FormElement, FormRenderProps, KeyValue} from "@progress/kendo-react-form";

import {Paper} from "@mui/material";
import {toast} from "react-toastify";
import {SalesRequest} from "../../../../../app/models/order/SalesRequest";
import {
    useAddSalesRequestMutation,
    useGetSalesRequestInstallmentsQuery,
    useUpdateSalesRequestMutation
} from "../../../../../app/store/apis/salesRequestApi";
import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper";
import ModalContainer from "../../../../../app/common/modals/ModalContainer";
import CreateCustomerModalForm from "../../../../parties/form/CreateCustomerModalForm";
import {useAppDispatch, useAppSelector} from "../../../../../app/store/configureStore";
import SalesRequestMenu from "../menu/SalesRequestMenu";
import PaymentPlanModal from "../dashboard/PaymentPlanModal";
import DefaultPercentagesModal from "../dashboard/DefaultPercentagesModal";
import {PricingSection} from "./PricingSection";
import {normalizeNumeric} from "../../../../../app/util/utils";
import {ApartmentHeaderSection} from "./ApartmentHeaderSection";
import {PaymentFieldsSection} from "./PaymentFieldsSection";
import {FormActionsSection} from "./FormActionsSection";
import {SalesRequestHeader} from "./SalesRequestHeader";
import {useSalesRequestCalculations} from "../hook/useSalesRequestCalculations";

const APARTMENT_AVAILABLE = "APARTMENT_AVAILABLE";

interface Props {
    salesRequest?: SalesRequest;
    editMode: number; // 1 = create, 2 = edit
    cancelEdit: () => void;
    onSalesRequestCreated?: (createdRequest: SalesRequest) => void;
    onSalesRequestUpdated?: (updated: SalesRequest) => void;
    onSalesRequestDeleted?: () => void;
}

function SalesRequestForm({
                              salesRequest,
                              editMode,
                              cancelEdit,
                              onSalesRequestCreated, onSalesRequestUpdated, onSalesRequestDeleted
                          }: Props) {

    const [createSR, {isLoading: isCreating}] = useAddSalesRequestMutation();
    const [updateSR, {isLoading: isUpdating}] = useUpdateSalesRequestMutation();
    const [showNewCustomer, setShowNewCustomer] = useState(false);
    const {getTranslatedLabel} = useTranslationHelper();
    const [showPaymentPlan, setShowPaymentPlan] = useState(false);
    const [showCalculatorModal, setShowCalculatorModal] = useState(false);
    const [customInstallments, setCustomInstallments] = useState<Array<{ dueDate: string; amount: number }>>([]);

    const dispatch = useAppDispatch();
    const formRef = useRef<FormRenderProps | null>(null);
    const [buttonFlag, setButtonFlag] = useState(false);
    const [selectedApartment, setSelectedApartment] = useState<SalesRequest | null>(null);
    const [defaultAdvancePercent, setDefaultAdvancePercent] = useState(0.10);
    const [defaultMaintenancePercent, setDefaultMaintenancePercent] = useState(0.08);
    const [showDefaultsModal, setShowDefaultsModal] = useState(false);
    const {language} = useAppSelector((state) => state.localization);

    const isEditMode = editMode === 2 || editMode === 3; // edit or approved view
    const salesRequestId = salesRequest?.salesRequestId;

    const {
        data: savedInstallments = [],
        isLoading: installmentsLoading,
        isError: installmentsError,
    } = useGetSalesRequestInstallmentsQuery(salesRequestId!, {
        skip: !isEditMode || !salesRequestId,
    });

    const {
        autoSetDerivedFields,
        handleProductChange,
        handlePricePerM2Change,
        handleDiscountChange,
        handleAdvanceChange,
    } = useSalesRequestCalculations({
        defaultAdvancePercent,
        defaultMaintenancePercent,
    });

    // -----------------------------------------------------------------
    // Internal ref for party input (no longer passed from parent)
    // -----------------------------------------------------------------
    const partyInputRef = useRef<HTMLInputElement>(null);
   
    useEffect(() => {
        if (savedInstallments.length > 0) {
            const mapped = savedInstallments.map(inst => ({
                dueDate: inst.dueDate,
                amount: inst.amount,
                isAdvance: inst.isAdvance,
            }));
            setCustomInstallments(mapped);
        } else if (isEditMode) {
            // No saved installments → start with empty (user must create one)
            setCustomInstallments([]);
        }
        // Create mode → already empty from initial state
    }, [savedInstallments, isEditMode]);

    useEffect(() => {
        if (!formRef.current) return;

        const finalTotal = formRef.current.valueGetter("totalPrice");
        if (finalTotal == null) return;

        // REFACTOR: Recalculate derived fields whenever default % change
        // This runs only when user changes defaults via modal
        autoSetDerivedFields(formRef.current, finalTotal);
    }, [defaultAdvancePercent, defaultMaintenancePercent, autoSetDerivedFields]);


    const isoToDate = (iso: string | null | undefined): Date | null => {
        if (!iso) return null;
        const d = new Date(iso);
        return isNaN(d.getTime()) ? null : d;   // safety – malformed strings → null
    };

    // -----------------------------------------------------------------
    // Initial values
    // -----------------------------------------------------------------
    const today = new Date();
    const formInitialValues = useMemo(() => {
        if (editMode === 1) {
            return {
                saleDate: today, // ← Default to today when creating new
                dateOfFirstInstallment: null,
                // All other fields start empty/null
                fromPartyId: null,
                partyIdEmployee: null,
                productId: null,
                advancePayment: null,
                totalPrice: null,
                // ... etc
            };
        }

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

        const employeeObj = sr.employeePartyId // assuming backend sends the string ID
            ? {
                fromPartyId: sr.employeePartyId,
                fromPartyName: sr.employeeName ?? "",        // use denormalized name if available
                fromPartyPhone: sr.employeePhone ?? "",
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
                apartmentSpaceM2: normalizeNumeric(sr.apartmentSpaceM2),
                gardenSpaceM2: normalizeNumeric(sr.gardenSpaceM2),
                gardenPricePerM2: normalizeNumeric(sr.gardenPricePerM2),
                apartmentPricePerM2: normalizeNumeric(sr.apartmentPricePerM2),
                apartmentStatusId: sr.apartmentStatusId ?? "",
                apartmentStatusDescription: sr.apartmentStatusDescription ?? "",
                reservedBySalesRequestId: sr.apartmentReservedBySalesRequestId ?? null,
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

            employeePartyId: employeeObj,
            employeePartyIdName: sr.employeeName ?? "", // optional display field


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
            apartmentPricePerM2: normalizeNumeric(sr.apartmentPricePerM2),
            gardenPricePerM2: normalizeNumeric(sr.gardenPricePerM2),
            discount: normalizeNumeric(sr.discount),
            totalPrice: normalizeNumeric(sr.totalPrice),
            advancePayment: normalizeNumeric(sr.advancePayment),
            maintenanceDeposit: normalizeNumeric(sr.maintenanceDeposit),

            // ----- payment plan ------------------------------------------------
            numberOfInstallments: sr.numberOfInstallments ?? null,
            monthsBetweenInstallments: sr.monthsBetweenInstallments ?? null,

            // ----- dates – convert to real Date objects ------------------------
            saleDate: isoToDate(sr.saleDate),
            dateOfFirstInstallment: isoToDate(sr.dateOfFirstInstallment),

            // ----- free text ---------------------------------------------------
            comments: sr.comments ?? null,
            statusId: sr.statusId ?? null,
            statusDescription: sr.statusDescription ?? null,
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

        if (copy.employeePartyId && typeof copy.employeePartyId === "object") {
            copy.employeePartyId = copy.employeePartyId.fromPartyId;
        }

        return copy;
    };

    const handleApplyPaymentPlan = useCallback((
        installments: Array<{ dueDate: string; amount: number; isAdvance: boolean }>  // ← updated type
    ) => {
        setCustomInstallments(installments);
        setShowPaymentPlan(false);
        toast.success(getTranslatedLabel("salesRequest.form.paymentPlanApplied", "Payment plan applied successfully"));
    }, [getTranslatedLabel]);


    // -----------------------------------------------------------------
// Submit
// -----------------------------------------------------------------
    async function handleSubmitData(data: any) {
        console.log('from submit')
        setButtonFlag(true);
        try {

            const advance = Number(data.advancePayment ?? 0);
            const total = Number(data.totalPrice ?? 0);

            if (customInstallments.length > 0) {
                const tolerance = 0.01;

                // Extract advance and regular parts from custom plan
                const advanceRows = customInstallments.filter(inst => inst.isAdvance);
                const regularRows = customInstallments.filter(inst => !inst.isAdvance);

                const advanceSum = advanceRows.reduce((sum, inst) => sum + inst.amount, 0);
                const regularSum = regularRows.reduce((sum, inst) => sum + inst.amount, 0);
                const grandTotal = advanceSum + regularSum;

                // 1. Grand total must match totalPrice
                if (Math.abs(grandTotal - total) > tolerance) {
                    toast.error(
                        getTranslatedLabel(
                            "salesRequest.form.validation.planTotalMismatch",
                            "The payment plan total does not match the sales total price. Please rebuild the plan."
                        )
                    );
                    setButtonFlag(false);
                    return;
                }

                // 2. Advance sum must match advancePayment
                if (Math.abs(advanceSum - advance) > tolerance) {
                    toast.error(
                        getTranslatedLabel(
                            "salesRequest.form.validation.planAdvanceMismatch",
                            "The advance amount in the payment plan does not match the advance payment. Please rebuild the plan."
                        )
                    );
                    setButtonFlag(false);
                    return;
                }

                // 3. For partial payment: number of regular installments must match form field
                if (advance < total && total > 0) {
                    const expectedInstallments = data.numberOfInstallments ?? 0;
                    if (regularRows.length !== expectedInstallments) {
                        toast.error(
                            getTranslatedLabel(
                                "salesRequest.form.validation.installmentCountMismatch",
                                `The payment plan has ${regularRows.length} regular installment(s), but ${expectedInstallments} were specified. Please rebuild the plan.`
                            )
                        );
                        setButtonFlag(false);
                        return;
                    }
                }
            }

            if (total > 0 && customInstallments.length === 0) {
                toast.error(
                    getTranslatedLabel(
                        "salesRequest.form.validation.customPlanRequired",
                        "You must create and apply a custom payment plan using the Payment Plan button."
                    )
                );
                setButtonFlag(false);
                return; // Block submission
            }

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
            const payload = {
                salesRequestDto: {
                    ...flattened,
                    ...(customInstallments.length > 0 && {
                        customInstallments: customInstallments.map((inst, idx) => ({
                            installmentNumber: idx + 1,
                            dueDate: inst.dueDate,
                            amount: inst.amount,
                            isAdvance: inst.isAdvance,  // ← send the flag
                        })),
                    }),
                },
            };

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
        if (aptM2 == null || aptPrice == null) return null;

        const apartmentTotal = aptM2 * aptPrice;

        // Only add garden if both gardenM2 and gardenPrice exist and are positive
        if (gardenM2 != null && gardenPrice != null && gardenM2 > 0 && gardenPrice > 0) {
            return apartmentTotal + (gardenM2 * gardenPrice);
        }

        return apartmentTotal;
    }, []);


    const salesRequestValidator = (values: any): KeyValue<string> | undefined => {
        const t = getTranslatedLabel;
        console.log('from validator')

        // -----------------------------------------------------------------
        // Existing apartment availability check (unchanged)
        // -----------------------------------------------------------------
        const apt = values.productId;
        const currentSalesRequestId = values.salesRequestId;

        const aptStatusId = typeof apt === "object" ? apt?.apartmentStatusId : null;

        if (aptStatusId && aptStatusId !== APARTMENT_AVAILABLE) {
            const reservedByThisRequest = apt.reservedBySalesRequestId === currentSalesRequestId;

            if (!reservedByThisRequest) {
                return {
                    VALIDATION_SUMMARY: t(
                        "salesRequest.form.validation.apartmentNotAvailable",
                        "Cannot proceed: this apartment is already SOLD or RESERVED by another sales request."
                    )
                };
            }
        }

        const advance = Number(values.advancePayment ?? 0);
        const total = Number(values.totalPrice ?? 0);

        // -----------------------------------------------------------------
        // Case 1: Partial payment (advance < total) → default fields AND custom plan required
        // -----------------------------------------------------------------
        if (advance < total && total > 0) {
            const missingDefault: string[] = [];

            if (!values.numberOfInstallments || values.numberOfInstallments <= 0) {
                missingDefault.push(t("salesRequest.form.installments", "Number of Installments"));
            }
            if (!values.dateOfFirstInstallment) {
                missingDefault.push(t("salesRequest.form.firstInstallmentDate", "First Installment Date"));
            }
            if (!values.monthsBetweenInstallments || values.monthsBetweenInstallments <= 0) {
                missingDefault.push(t("salesRequest.form.duration", "Months Between Installments"));
            }

            if (missingDefault.length > 0) {
                return {
                    VALIDATION_SUMMARY: t(
                        "salesRequest.form.validation.missingDefaultFields",
                        "For partial payment, the following fields are required to generate the initial payment plan: {0}."
                    ).replace("{0}", missingDefault.join(", "))
                };
            }

            // Custom plan still mandatory – user must open modal and apply (even if just accepting defaults)
            if (customInstallments.length === 0) {
                return {
                    VALIDATION_SUMMARY: t(
                        "salesRequest.form.validation.customPlanRequired",
                        "You must open the Payment Plan modal and apply a plan (even if you keep the defaults)."
                    )
                };
            }
        }

        // -----------------------------------------------------------------
        // Case 2: Full payment (advance >= total) → custom plan still required (for advance splitting)
        // -----------------------------------------------------------------
        if (advance >= total && total > 0) {
            if (customInstallments.length === 0) {
                return {
                    VALIDATION_SUMMARY: t(
                        "salesRequest.form.validation.customPlanRequiredFull",
                        "Even for full payment, you must create a custom payment plan (e.g., to split the advance payment)."
                    )
                };
            }

            // Prevent leftover default fields in full payment scenario
            if (
                values.numberOfInstallments > 0 ||
                values.dateOfFirstInstallment != null ||
                values.monthsBetweenInstallments > 0
            ) {
                return {
                    VALIDATION_SUMMARY: t(
                        "salesRequest.form.validation.clearDefaultFieldsFull",
                        "Full payment detected – please clear the installment fields (they are not used)."
                    )
                };
            }
        }

        if (customInstallments.length > 0) {
            const advance = Number(values.advancePayment ?? 0);
            const total = Number(values.totalPrice ?? 0);
            const tolerance = 0.01;

            const advanceSum = customInstallments
                .filter(i => i.isAdvance)
                .reduce((s, i) => s + i.amount, 0);
            const regularSum = customInstallments
                .filter(i => !i.isAdvance)
                .reduce((s, i) => s + i.amount, 0);

            if (Math.abs(advanceSum + regularSum - total) > tolerance) {
                return {
                    VALIDATION_SUMMARY: t(
                        "salesRequest.form.validation.planTotalMismatch",
                        "Payment plan total does not match sales price. Reopen the plan to update."
                    )
                };
            }

            if (Math.abs(advanceSum - advance) > tolerance) {
                return {
                    VALIDATION_SUMMARY: t(
                        "salesRequest.form.validation.planAdvanceMismatch",
                        "Advance in payment plan does not match advance payment. Reopen to fix."
                    )
                };
            }

            if (advance < total && total > 0) {
                const expected = values.numberOfInstallments ?? 0;
                const actual = customInstallments.filter(i => !i.isAdvance).length;
                if (actual !== expected) {
                    return {
                        VALIDATION_SUMMARY: t(
                            "salesRequest.form.validation.installmentCountMismatch",
                            `Payment plan has ${actual} regular installments, expected ${expected}. Reopen plan.`
                        )
                    };
                }
            }
        }

        // All good
        return;
    };


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

                        const selectedApartmentObj = valueGetter("productId");

                        const apt = selectedApartmentObj;
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

                        const canOpenPaymentPlan =
                            !!valueGetter("totalPrice") &&
                            valueGetter("advancePayment") < valueGetter("totalPrice") &&
                            valueGetter("totalPrice") > 0;


                        // -----------------------------------------------------------------
                        // 2. Apartment for the legacy prop
                        // -----------------------------------------------------------------
                        const apartmentForModal = currentFormValues.apartmentId
                            ? {productName: currentFormValues.apartmentName ?? "Unknown Unit"}
                            : undefined;

                        const statusId = formRenderProps.valueGetter("statusId") as string | undefined;
                        const statusDescription = formRenderProps.valueGetter("statusDescription") as string | undefined;


                        return (
                            <>
                               
                                <SalesRequestHeader
                                    salesRequest={salesRequest}
                                    editMode={editMode}
                                    defaultAdvancePercent={defaultAdvancePercent}
                                    defaultMaintenancePercent={defaultMaintenancePercent}
                                    onOpenDefaultsModal={() => setShowDefaultsModal(true)}
                                    language={language}
                                    getTranslatedLabel={getTranslatedLabel}
                                    onSalesRequestUpdated={onSalesRequestUpdated}
                                    onSalesRequestDeleted={cancelEdit}
                                    disabledActions={isCreating || isUpdating || buttonFlag}
                                />
                                <FormElement>
                                    <fieldset className="k-form-fieldset">

                                        <ApartmentHeaderSection
                                            formRenderProps={formRenderProps}
                                            selectedApartment={selectedApartment}
                                            onProductChange={(form, e) => handleProductChange(form, e, setSelectedApartment)}
                                            showNewCustomer={showNewCustomer}
                                            setShowNewCustomer={setShowNewCustomer}
                                            getTranslatedLabel={getTranslatedLabel}
                                            partyInputRef={partyInputRef}
                                            editMode={editMode}
                                        />
                                        <PricingSection
                                            formRenderProps={formRenderProps}
                                            selectedApartment={selectedApartment}
                                            onPricePerM2Change={handlePricePerM2Change}
                                            onDiscountChange={handleDiscountChange}
                                            autoSetDerivedFields={autoSetDerivedFields}
                                            showCalculatorModal={showCalculatorModal}
                                            setShowCalculatorModal={setShowCalculatorModal}
                                            getTranslatedLabel={getTranslatedLabel}
                                        />
                                        <PaymentFieldsSection
                                            formRenderProps={formRenderProps}
                                            onAdvanceChange={handleAdvanceChange}
                                            getTranslatedLabel={getTranslatedLabel}
                                        />

                                        <FormActionsSection
                                            formRenderProps={formRenderProps}
                                            customInstallmentsLength={customInstallments.length}
                                            canOpenPaymentPlan={canOpenPaymentPlan}
                                            onOpenPaymentPlan={() => setShowPaymentPlan(true)}
                                            buttonFlag={buttonFlag}
                                            isCreating={isCreating}
                                            isUpdating={isUpdating}
                                            editMode={editMode}
                                            onCancel={cancelEdit}
                                            getTranslatedLabel={getTranslatedLabel}
                                        />


                                        {showPaymentPlan && (
                                            <ModalContainer show={showPaymentPlan}
                                                            onClose={() => setShowPaymentPlan(false)} width={950}>
                                                <PaymentPlanModal
                                                    onClose={() => setShowPaymentPlan(false)}
                                                    salesRequest={currentFormValues}
                                                    apartment={apartmentForModal}
                                                    onApply={handleApplyPaymentPlan}
                                                    isPreview={customInstallments.length > 0}
                                                    initialInstallments={customInstallments}  // ← Key new prop for two-way sync
                                                />
                                            </ModalContainer>
                                        )}

                                        {showDefaultsModal && (
                                            <DefaultPercentagesModal
                                                open={showDefaultsModal}
                                                onClose={() => setShowDefaultsModal(false)}
                                                advancePercent={defaultAdvancePercent}
                                                maintenancePercent={defaultMaintenancePercent}
                                                onSave={(adv, maint) => {
                                                    setDefaultAdvancePercent(adv);
                                                    setDefaultMaintenancePercent(maint);
                                                }}
                                            />
                                        )}
                                    </fieldset>
                                </FormElement>

                            </>
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

export default React.memo(SalesRequestForm);