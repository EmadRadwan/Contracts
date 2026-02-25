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
        if (finalTotal == null || finalTotal <= 0) return;

        const currentAdvance = formRef.current.valueGetter("advancePayment") || 0;
        const expectedAdvance = finalTotal * defaultAdvancePercent;

        const currentMaintenance = formRef.current.valueGetter("maintenanceDeposit") || 0;
        const expectedMaintenance = finalTotal * defaultMaintenancePercent;

        // Only update if values are significantly different
        if (
            Math.abs(currentAdvance - expectedAdvance) > 0.01 ||
            Math.abs(currentMaintenance - expectedMaintenance) > 0.01
        ) {
            autoSetDerivedFields(formRef.current, finalTotal);
        }
    }, [defaultAdvancePercent, defaultMaintenancePercent]);

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
                isChequesDelivered: true,
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
            isChequesDelivered: sr.isChequesDelivered ?? null,
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
        installments: Array<{ dueDate: string; amount: number; isAdvance: boolean }>
    ) => {
        setCustomInstallments(installments);

        const actualAdvanceSum = installments
            .filter(inst => inst.isAdvance)
            .reduce((sum, inst) => sum + inst.amount, 0);

        if (formRef.current) {
            const currentAdvance = formRef.current.valueGetter("advancePayment");
            // Only update if different (prevents re-render loop)
            const roundedAdvanceSum = Math.round(actualAdvanceSum);
            if (Math.abs(roundedAdvanceSum - (currentAdvance || 0)) > 0.01) {
                formRef.current.onChange("advancePayment", { value: roundedAdvanceSum });
            }
        }

        setShowPaymentPlan(false);
        toast.success("Payment plan applied and advance updated");
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

            if (advance < total && total > 0) {
                const numberOfInstallments = data.numberOfInstallments ?? 0;
                const dateOfFirstInstallment = data.dateOfFirstInstallment;
                const monthsBetweenInstallments = data.monthsBetweenInstallments ?? 0;

                const missingFields: string[] = [];

                if (!numberOfInstallments || numberOfInstallments <= 0) {
                    missingFields.push("Number of Installments");
                }
                if (!dateOfFirstInstallment) {
                    missingFields.push("Date of First Installment");
                }
                if (!monthsBetweenInstallments || monthsBetweenInstallments <= 0) {
                    missingFields.push("Months Between Installments");
                }

                if (missingFields.length > 0) {
                    toast.error(
                        `For installment-based sales, please fill in: ${missingFields.join(", ")}.`
                    );
                    setButtonFlag(false);
                    return;
                }
            }


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


    const salesRequestValidator = useCallback((values: any): KeyValue<string> | undefined => {
        const t = getTranslatedLabel;

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

        // All other validation (payment plan, advance match, etc.) moved to handleSubmitData
        return;
    }, [getTranslatedLabel]);


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
                    //key={editMode}
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

                        const totalPrice = valueGetter("totalPrice");
                        const advancePayment = valueGetter("advancePayment");
                        const numberOfInstallments = valueGetter("numberOfInstallments");
                        const dateOfFirstInstallment = valueGetter("dateOfFirstInstallment");
                        const monthsBetweenInstallments = valueGetter("monthsBetweenInstallments");

                        // Determine if payment is partial or full
                        const isPartialPayment =
                            totalPrice > 0 &&
                            advancePayment != null &&
                            advancePayment < totalPrice;

                        const areDefaultFieldsFilled =
                            numberOfInstallments > 0 &&
                            dateOfFirstInstallment != null &&
                            monthsBetweenInstallments > 0;

                        // Show button only when:
                        // - Total and advance are set
                        // - AND if partial payment → default fields are filled
                        const canOpenPaymentPlan =
                            totalPrice > 0 &&
                            advancePayment != null &&
                            (!isPartialPayment || areDefaultFieldsFilled);

                        // -----------------------------------------------------------------
                        // 2. Apartment for the legacy prop
                        // -----------------------------------------------------------------
                        const apartmentForModal = currentFormValues.apartmentId
                            ? {productName: currentFormValues.apartmentName ?? "Unknown Unit"}
                            : undefined;

                        
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