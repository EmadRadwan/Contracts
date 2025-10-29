import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import {setSelectedPayment} from "../../slice/accountingSharedUiSlice";
import usePayment from "../hook/usePayment";
import {
    useAppDispatch,
    useAppSelector,
    useFetchCompaniesQuery,
    useFetchCurrenciesQuery,
    useFetchPaymentMethodsQuery,
    useFetchPaymentTypesQuery,
} from "../../../../app/store/configureStore";
import {useCallback, useEffect, useMemo, useRef, useState} from "react";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import CreateCustomerModalForm from "../../../parties/form/CreateCustomerModalForm";
import {Payment} from "../../../../app/models/accounting/payment";
import ModalContainer from "../../../../app/common/modals/ModalContainer";
import {setPaymentType} from "../slice/paymentsUiSlice";
import {Grid, Paper} from "@mui/material";
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

// -----------------------------------------------------------------------------
// Props & constants
// -----------------------------------------------------------------------------
interface Props {
    selectedPayment?: Payment;
    editMode: number;
    cancelEdit: () => void;
}

const PAYMENT_STATUSES = {
    RECEIVED: "PMNT_RECEIVED",
    SENT: "PMNT_SENT",
    CANCELLED: "PMNT_CANCELLED",
    CONFIRMED: "PMNT_CONFIRMED",
    NOT_PAID: "PMNT_NOT_PAID",
};

/**
 * REFACTOR: Updated filter lists to match the exact PAYMENT_TYPE_ID values
 * returned by the API.  This guarantees that only real payment types appear
 * in the UI and prevents "no valid payment types" errors.
 */
const PAYMENT_TYPE_FILTERS = {
    incoming: [
        "RECEIPT",
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
    ],
    outgoing: [
        "DISBURSEMENT",
        "TAX_PAYMENT",
        "SALES_TAX_PAYMENT",
        "PAYROLL_TAX_PAYMENT",
        "INCOME_TAX_PAYMENT",
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
    ],
};

export default function PaymentForm({
                                        selectedPayment,
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

    const {
        payment,
        setPayment,
        formEditMode,
        setFormEditMode,
        handleCreate, handleUpdate, handleStatusChange,
        isLoading: apiLoading,
    } = usePayment({
        selectedMenuItem: "",
        editMode,
        selectedPayment,
        setIsLoading,
        formFieldsUpdated: false,
        companies
    });

    // -------------------------------------------------------------------------
    // REFACTOR: Filter payment types using the exact ID strings from the API.
    // -------------------------------------------------------------------------
    const filteredPaymentTypes = useMemo(() => {
        if (!paymentTypes || !Array.isArray(paymentTypes)) return [];

        const filterKey = paymentType === 1 ? "incoming" : "outgoing";
        const allowedIds = PAYMENT_TYPE_FILTERS[filterKey];

        return paymentTypes.filter((type) =>
            allowedIds.includes(type.paymentTypeId)
        );
    }, [paymentTypes, paymentType]);

    // -------------------------------------------------------------------------
    // Synchronize selected payment & cleanup
    // -------------------------------------------------------------------------
    useEffect(() => {
        if (selectedPayment) {
            setPayment(selectedPayment);
        } else {
            setPayment(undefined);
        }
        return () => {
            dispatch(setSelectedPayment(undefined));
        };
    }, [selectedPayment, setPayment, dispatch]);

    // -------------------------------------------------------------------------
    // Focus party input & reset invalid payment type
    // -------------------------------------------------------------------------
    useEffect(() => {
        if (formEditMode === 1 && partyInputRef.current) {
            partyInputRef.current.focus();
        }

        if (
            formRef.current?.values?.paymentTypeId &&
            filteredPaymentTypes.length > 0
        ) {
            const isValid = filteredPaymentTypes.some(
                (t) => t.paymentTypeId === formRef.current.values.paymentTypeId
            );
            if (!isValid) {
                formRef.current.onChange("paymentTypeId", {value: "", valid: false});
            }
        }
    }, [formEditMode, filteredPaymentTypes]);

    // -------------------------------------------------------------------------
    // Handlers
    // -------------------------------------------------------------------------
    const handleNewPayment = useCallback(
        (newPaymentType: number) => {
            dispatch(setPaymentType(newPaymentType));
            setPayment(undefined);
            setFormEditMode(1);
            formRef.current?.reset();
            formRef.current?.setValues({
                paymentId: "",
                paymentTypeId: "",
                paymentMethodId: "",
                statusId: PAYMENT_STATUSES.NOT_PAID,
                partyIdFrom: newPaymentType === 1 ? "" : formRef.current?.values.organizationPartyId || "",
                partyIdFromName: "",
                partyIdTo: newPaymentType === 2 ? "" : formRef.current?.values.organizationPartyId || "",
                partyIdToName: "",
                amount: 0,
                effectiveDate: new Date(),
                paymentRefNum: "",
                organizationPartyId: formRef.current?.values.organizationPartyId || "",
                isDepositWithDrawPayment: "Y",
                finAccountTransTypeId: "DEPOSIT",
                isDisbursement: newPaymentType === 2,
            });
        },
        [dispatch, setFormEditMode, setPayment]
    );

    const handleCancelForm = useCallback(() => {
        setPayment(undefined);
        setFormEditMode(1);
        dispatch(setPaymentType(1));
        cancelEdit();
    }, [setPayment, setFormEditMode, dispatch, cancelEdit]);

    const handleCancelApplications = useCallback(() => {
        setShowPaymentApplicationsList(false);
    }, []);

    const handleMenuSelect = useCallback(
        (e: any) => {
            const data = e.item.data;
            const formValues = formRef.current?.values;
            const isValid = formRef.current?.isValid();

            if (data === "update") {
                handleCreate({values: formValues, isValid, menuItem: "Update Payment"});
            } else if (data === "receive") {
                handleStatusChange({values: formValues, isValid, menuItem: "Status to Received"});
            } else if (data === "send") {
                handleStatusChange({values: formValues, isValid, menuItem: "Status to Sent"});
            } else if (data === "cancel") {
                handleStatusChange({values: formValues, isValid, menuItem: "Status to Cancelled"});
            } else if (data === "confirm") {
                handleStatusChange({values: formValues, isValid, menuItem: "Status to Confirmed"});
            } else if (data === "incoming") {
                handleNewPayment(1);
            } else if (data === "outgoing") {
                handleNewPayment(2);
            } else if (data === "transactions") {
                setShowTransactionsList(true);
            } else if (data === "applications") {
                setShowPaymentApplicationsList(true);
            }
        },
        [handleCreate, handleStatusChange, handleNewPayment]
    );

    const updateCustomerDropDown = useCallback(
        (newCustomer: { partyId: string; description: string }) => {
            const fieldName = paymentType === 1 ? "partyIdFrom" : "partyIdTo";
            formRef.current?.onChange(fieldName, {value: newCustomer.partyId, valid: true});
            formRef.current?.onChange(`${fieldName}Name`, {value: newCustomer.description, valid: true});
            dispatch(setCustomerId(newCustomer.partyId));
        },
        [dispatch, paymentType]
    );

    const getAvailableStatusTransitions = useMemo(
        () => (payment?: Payment) => {
            if (!payment) {
                return {toSent: false, toReceived: false, toCancelled: false, toConfirmed: false};
            }
            return {
                toSent: payment.isDisbursement && payment.statusId === PAYMENT_STATUSES.NOT_PAID,
                toReceived: !payment.isDisbursement && payment.statusId === PAYMENT_STATUSES.NOT_PAID,
                toCancelled: payment.statusId === PAYMENT_STATUSES.NOT_PAID,
                toConfirmed: [PAYMENT_STATUSES.RECEIVED, PAYMENT_STATUSES.SENT].includes(payment.statusId),
            };
        },
        []
    );

    // -------------------------------------------------------------------------
    // Render form based on mode
    // -------------------------------------------------------------------------
    const renderForm = () => {
        if (formEditMode === 1) {
            return paymentType === 1 ? (
                <NewPaymentIn
                    formRef={formRef}
                    partyInputRef={partyInputRef}
                    companies={companies}
                    filteredPaymentTypes={filteredPaymentTypes}
                    paymentMethods={paymentMethods}
                    getTranslatedLabel={getTranslatedLabel}
                    setShowNewCustomer={setShowNewCustomer}
                    onCreate={handleCreate}
                    handleCancelForm={handleCancelForm}
                />
            ) : (
                <NewPaymentOut
                    formRef={formRef}
                    partyInputRef={partyInputRef}
                    companies={companies}
                    filteredPaymentTypes={filteredPaymentTypes}
                    paymentMethods={paymentMethods}
                    getTranslatedLabel={getTranslatedLabel}
                    setShowNewCustomer={setShowNewCustomer}
                    onCreate={handleCreate}
                    handleCancelForm={handleCancelForm}
                />
            );
        }

        return (
            <EditPaymentForm
                payment={payment}
                paymentType={paymentType}
                formEditMode={formEditMode}
                formRef={formRef}
                companies={companies}
                filteredPaymentTypes={filteredPaymentTypes}
                paymentMethods={paymentMethods}
                getTranslatedLabel={getTranslatedLabel}
                onUpdate={handleUpdate}
                currencies={currencies}
                handleCancelForm={handleCancelForm}
            />
        );
    };

    // -------------------------------------------------------------------------
    // Loading / empty states
    // -------------------------------------------------------------------------
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

    // -------------------------------------------------------------------------
    // Main render
    // -------------------------------------------------------------------------
    return (
        <RibbonContainer>
            <AccountingMenu selectedMenuItem="/payments"/>
            <Paper elevation={5} className="div-container-withBorderCurved">
                <PaymentHeader
                    payment={payment}
                    paymentType={paymentType}
                    formEditMode={formEditMode}
                    language={language}
                    getTranslatedLabel={getTranslatedLabel}
                />
                <Grid container spacing={2} alignItems="center">
                    <Grid item xs={12} sx={{display: 'flex', justifyContent: 'flex-end'}}>
                        <PaymentActions
                            payment={payment}
                            formEditMode={formEditMode}
                            getTranslatedLabel={getTranslatedLabel}
                            handleMenuSelect={handleMenuSelect}
                            getAvailableStatusTransitions={getAvailableStatusTransitions}
                            isFormValid={formRef.current?.isValid() ?? false}
                        />
                    </Grid>
                </Grid>

                {renderForm()}

                {(isLoading || apiLoading) && (
                    <LoadingComponent
                        message={getTranslatedLabel(
                            `${localizationKey}.loading`,
                            "Processing Payment..."
                        )}
                    />
                )}

                {showNewCustomer && (
                    <ModalContainer show={showNewCustomer} onClose={() => setShowNewCustomer(false)} width={500}>
                        <CreateCustomerModalForm
                            onClose={() => setShowNewCustomer(false)}
                            onUpdateCustomerDropDown={updateCustomerDropDown}
                        />
                    </ModalContainer>
                )}

                {showTransactionsList && (
                    <ModalContainer show={showTransactionsList} onClose={() => setShowTransactionsList(false)} width={950}>
                        <PaymentTransactionsList
                            onClose={() => setShowTransactionsList(false)}
                            paymentId={payment?.paymentId || ""}
                        />
                    </ModalContainer>
                )}

                {showPaymentApplicationsList && payment?.paymentId && (
                    <ModalContainer show={showPaymentApplicationsList} onClose={handleCancelApplications} width={950}>
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