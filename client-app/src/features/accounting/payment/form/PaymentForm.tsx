import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import usePayment from "../hook/usePayment";
import {
    useAppDispatch,
    useAppSelector,
    useFetchCompaniesQuery, useFetchCurrenciesQuery,
    useFetchPaymentMethodsQuery,
    useFetchPaymentTypesQuery,
} from "../../../../app/store/configureStore";
import {useCallback, useMemo, useRef, useState} from "react";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {Payment} from "../../../../app/models/accounting/payment";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import {setFormEditMode, setPaymentType} from "../slice/paymentsUiSlice";
import { Grid, Paper} from "@mui/material";
import PaymentTransactionsList from "../../transaction/dashboard/PaymentTransactionsList";
import {RibbonContainer} from "react-ribbons";
import PaymentHeader from "./PaymentHeader";
import PaymentActions from "./PaymentActions";
import NewPaymentOut from "./NewPaymentOut";
import NewPaymentIn from "./NewPaymentIn";
import EditPaymentForm from "./EditPaymentForm";
import AccountingMenu from "../../invoice/menu/AccountingMenu";
import EditPaymentApplications from "./EditPaymentApplications";
import {setCustomerId} from "../../../orders/slice/sharedOrderUiSlice";
import CreatePartyModalForm from "../../../parties/form/CreatePartyModalForm";

// Props interface for the PaymentForm component
interface Props {
    editMode: number;
    cancelEdit: () => void;
}

// Constants for payment statuses
const PAYMENT_STATUSES = {
    RECEIVED: "PMNT_RECEIVED",
    SENT: "PMNT_SENT",
    CANCELLED: "PMNT_CANCELLED",
    CONFIRMED: "PMNT_CONFIRMED",
    NOT_PAID: "PMNT_NOT_PAID",
};

// Constants for payment type filters
const PAYMENT_TYPE_FILTERS = {
    incoming: [
        "CUSTOMER_PAYMENT",
        "CUSTOMER_DEPOSIT",
        "INTEREST_RECEIPT",
        "GC_DEPOSIT",
        // New incoming types that exist in the system
        "RECEIPT_ADVANCE_PAYMENT",
        "RECEIPT_CHECK_REPLACEMENT",
        "RECEIPT_DUE_INSTALLMENT",
        "RECEIPT_MAINTENANCE_AMOUNT",
        "RECEIPT_PARTIAL_PAYMENT",
        "RECEIPT_RETURNED_CHECK",
        "PROPERTY_RENTAL_INCOME"
    ],
    outgoing: [
        "TAX_PAYMENT",
        "SALES_TAX_PAYMENT",
        "PAYROLL_TAX_PAYMENT",
        "INCOME_TAX_PAYMENT" as const,
        "VENDOR_PAYMENT",
        "VENDOR_PREPAY",
        "PAY_CHECK",
        "PAYROL_PAYMENT",
        "CUSTOMER_REFUND",
        "GC_WITHDRAWAL",
        "COMMISSION_PAYMENT",
        // New outgoing types that exist in the system
        "ADVANCE_TO_VENDOR_CONTRACTOR",
        "CONTRACTOR_INSTALLMENT",
        "PERMANENT_CUSTODY",
        "TEMP_ADVANCE",
        "VENDOR_INVOICE_PAYMENT",
        // REFACTOR: Added newly introduced disbursement types from the provided list
        // Purpose: Expand the outgoing filter to include all valid disbursement-based payment types
        // Improves: Completeness — ensures users can select any active system payment type
        // Context: These IDs were confirmed to have PARENT_TYPE_ID = "DISBURSEMENT"
        "CHECK_REPLACEMENT",
        "DEBTORS_ADVANCE",
        "DUE_INSTALLMENT",
        "EMPLOYEE_ADVANCE",
        "EMPLOYEE_LONG_TERM_ADVANCE",
        "EQUIPMENT_EXPENSES",
        "LABOR_WAGES",
        "LAND_PURCHASE",
        "MATERIAL_PURCHASE",
        "MISC_EXPENSES",
        "PARTIAL_PAYMENT",
        "ADVERTISING_EXPENSES",
        "VEHICLE_FUEL",
        "ALLOWANCES_BONUSES",
        "TRANSPORTATION",
        "MAINTENANCE_REPAIR",
        "VEHICLE_OIL_CHANGE",
        "CLEANING_SUPPLIES",
        "BUFFET_HOSPITALITY",
        "HOSPITALITY_PR",
        "GOV_LICENSE_FEES",
        "PHOTOCOPIER_SUPPLIES",
        "BANK_FEES",
        "FINANCING_EXPENSES",
        "LOAN_INTEREST",
        "COMMUNICATIONS_INTERNET",
        "OFFICE_SUPPLIES",
        "RENT_REVENUE_SHARE_SETTLEMENT"
    ],
};

export default function PaymentForm({
                                        cancelEdit,
                                        editMode,
                                    }: Props) {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.payments.form";
    const dispatch = useAppDispatch();
    const {language} = useAppSelector((state) => state.localization);
    const paymentType = useAppSelector((state) => state.paymentsUi.paymentType);

    const {data: companies} = useFetchCompaniesQuery(undefined);
    const {data: paymentTypes, isLoading: paymentTypesLoading} = useFetchPaymentTypesQuery(undefined);
    const {data: paymentMethods} = useFetchPaymentMethodsQuery(undefined);
    const {data: currencies, isLoading: isCurrenciesLoading} = useFetchCurrenciesQuery(undefined);

    const [showNewCustomer, setShowNewCustomer] = useState(false);
    const [showTransactionsList, setShowTransactionsList] = useState(false);
    const [showPaymentApplicationsList, setShowPaymentApplicationsList] = useState(false);
    const [isLoading, setIsLoading] = useState(false);
    const formRef = useRef<any>(null);
    const partyInputRef = useRef<HTMLInputElement>(null);
    const selectedPayment = useAppSelector((s) => s.accountingSharedUi.selectedPayment);
    
    const {
        payment,
        handleCreate, handleDuplicate,
        handleUpdate,
        handleStatusChange,
        isLoading: hookLoading, handleReset
    } = usePayment({
        editMode,
        selectedPayment,
        setIsLoading,
        companies,
    });

    // Compute filtered payment types
    const filteredPaymentTypes = useMemo(() => {
        if (!paymentTypes || !Array.isArray(paymentTypes)) return [];

        const filterKey = paymentType === 1 ? "incoming" : "outgoing";
        const allowedIds = PAYMENT_TYPE_FILTERS[filterKey];

        return paymentTypes.filter((type) =>
            allowedIds.includes(type.paymentTypeId)
        );
    }, [paymentTypes, paymentType]);


    
    const handleCancelApplications = useCallback(() => {
        setShowPaymentApplicationsList(false);
    }, []);

    const handleMenuSelect = (e: any) => {
        const action = e.item.data;
        // the forms call the appropriate handler with {values, isValid, menuItem}
        // we simply forward – the actual logic lives in usePayment
        if (action === "update") {
            // formRef is inside child components – they call handleUpdate directly
        } else if (["receive"].includes(action)) {
            handleStatusChange({
                menuItem: "Status to Received",
            });
        } else if (["send"].includes(action)) {
            handleStatusChange({
                menuItem: "Status to Sent",
            });
        } else if (action === "incoming") {
            dispatch(setPaymentType(1));
            dispatch(setFormEditMode(1));
        } else if (action === "outgoing") {
            dispatch(setPaymentType(2));
            dispatch(setFormEditMode(1));
        } else if (action === "transactions") {
            setShowTransactionsList(true);
        } else if (action === "applications") {
            setShowPaymentApplicationsList(true);
        }
        else if (action === "duplicate") {
            handleDuplicate();
        }
    };

    // Update customer dropdown
    const updateCustomerDropDown = useCallback(
        (newCustomer: { partyId: string; description: string }) => {
            const fieldName = paymentType === 1 ? "partyIdFrom" : "partyIdTo";
            formRef.current?.onChange(fieldName, {
                value: newCustomer.partyId,
                valid: true,
            });
            formRef.current?.onChange(`${fieldName}Name`, {
                value: newCustomer.description,
                valid: true,
            });
            dispatch(setCustomerId(newCustomer.partyId));
        },
        [dispatch, paymentType]
    );

    // Determine available status transitions
    const getAvailableStatusTransitions = useMemo(
        () => (payment?: Payment) => {
            if (!payment) {
                return {
                    toSent: false,
                    toReceived: false,
                    toCancelled: false,
                    toConfirmed: false,
                };
            }
            return {
                toSent:
                    payment.isDisbursement &&
                    payment.statusId === PAYMENT_STATUSES.NOT_PAID,
                toReceived:
                    !payment.isDisbursement &&
                    payment.statusId === PAYMENT_STATUSES.NOT_PAID,
                toCancelled: payment.statusId === PAYMENT_STATUSES.NOT_PAID,
                toConfirmed: [
                    PAYMENT_STATUSES.RECEIVED,
                    PAYMENT_STATUSES.SENT,
                ].includes(payment.statusId),
            };
        },
        []
    );
    


    // Render the appropriate form
    const renderForm = () => {
        const DEBUG_FORM = true;
        if (editMode === 1) {
            if (paymentType === 1) {
                return (
                    <NewPaymentIn
                        partyInputRef={partyInputRef}
                        companies={companies}
                        filteredPaymentTypes={filteredPaymentTypes}
                        paymentMethods={paymentMethods}
                        getTranslatedLabel={getTranslatedLabel}
                        setShowNewCustomer={setShowNewCustomer}
                        onCreate={handleCreate}
                        handleCancelForm={cancelEdit}
                    />
                );
            } else {
                return (
                    <NewPaymentOut
                        partyInputRef={partyInputRef}
                        companies={companies}
                        filteredPaymentTypes={filteredPaymentTypes}
                        paymentMethods={paymentMethods}
                        getTranslatedLabel={getTranslatedLabel}
                        setShowNewCustomer={setShowNewCustomer}
                        onCreate={handleCreate}
                        handleCancelForm={cancelEdit}
                    />
                );
            }
        }
        return (
            <EditPaymentForm
                payment={payment}
                paymentType={paymentType}
                formEditMode={editMode}
                companies={companies}
                filteredPaymentTypes={filteredPaymentTypes}
                paymentMethods={paymentMethods}
                getTranslatedLabel={getTranslatedLabel}
                onUpdate={handleUpdate}
                currencies={currencies}
                handleCancelForm={cancelEdit}
                debugForm={DEBUG_FORM}
            />
        );
    };


    // Handle loading and error states
    if (paymentTypesLoading) {
        return (
            <LoadingComponent
                message={getTranslatedLabel("loading", "Loading payment...")}
            />
        );
    }

    if (!filteredPaymentTypes.length) {
        return (
            <div>
                {getTranslatedLabel(
                    `${localizationKey}.noPaymentTypes`,
                    "No valid payment types available for this direction."
                )}
            </div>
        );
    }
    
    return (
        <RibbonContainer>
            <AccountingMenu selectedMenuItem="/payments"/>
            <Paper elevation={5} className="div-container-withBorderCurved">
                <PaymentHeader
                    payment={payment}
                    paymentType={paymentType}
                    formEditMode={editMode}
                    language={language}
                    getTranslatedLabel={getTranslatedLabel}
                />
                <Grid container spacing={2} alignItems="center">
                    <Grid item xs={12} sx={{display: 'flex', justifyContent: 'flex-end'}}>
                        {/* {JSON.stringify(payment?.statusId)} */}
                        <PaymentActions
                            payment={payment}
                            formEditMode={editMode}
                            getTranslatedLabel={getTranslatedLabel}
                            handleMenuSelect={handleMenuSelect}
                            getAvailableStatusTransitions={getAvailableStatusTransitions}
                            handleReset={handleReset}
                        />
                    </Grid>
                </Grid>
                {renderForm()}

                

                {(isLoading || hookLoading) && (
                    <LoadingComponent
                        message={getTranslatedLabel(
                            `${localizationKey}.loading`,
                            "Processing Payment..."
                        )}
                    />
                )}
                {showNewCustomer && (
                    <ModalContainer
                        show={showNewCustomer}
                        onClose={() => setShowNewCustomer(false)}
                        width={500}
                    >
                        <CreatePartyModalForm
                            onClose={() => setShowNewCustomer(false)}
                            onUpdateCustomerDropDown={updateCustomerDropDown}
                        />
                    </ModalContainer>
                )}

                {showTransactionsList && (
                    <ModalContainer
                        show={showTransactionsList}
                        onClose={() => setShowTransactionsList(false)}
                        width={950}
                    >
                        <PaymentTransactionsList
                            onClose={() => setShowTransactionsList(false)}
                            paymentId={payment?.paymentId || ""}
                        />
                    </ModalContainer>
                )}

                {showPaymentApplicationsList && payment?.paymentId && (
                    <ModalContainer
                        show={showPaymentApplicationsList}
                        onClose={handleCancelApplications}
                        width={950}
                    >
                        <EditPaymentApplications
                            payment={payment}
                            paymentId={payment.paymentId}
                            onClose={handleCancelApplications}
                        />
                    </ModalContainer>
                )}
            </Paper>
        </RibbonContainer>
    );
}