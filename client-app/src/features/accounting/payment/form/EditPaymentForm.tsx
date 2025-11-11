import {chequeValidator, requiredValidator} from "../../../../app/common/form/Validators";
import {
    Field,
    Form,
    FormElement,
    FormRenderProps,
} from "@progress/kendo-react-form";
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import {Button, Grid, Skeleton, Typography} from "@mui/material";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import {Payment} from "../../../../app/models/accounting/payment";
import FormInput from "../../../../app/common/form/FormInput";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import React, {useCallback, useEffect, useMemo, useState} from "react";
import {RootState, useAppSelector, useFetchPaymentAcctTransEntriesQuery} from "../../../../app/store/configureStore";
import {
    useFetchGlAccountOrganizationHierarchyLovQuery,
    useFetchPaymentApplicationsForPaymentQuery
} from "../../../../app/store/apis";
import {FormDropDownTreeGlAccount2} from "../../../../app/common/form/FormDropDownTreeGlAccount2";
import {PaymentExcelTechnical} from "../report/PaymentExcelTechnical";
import {PaymentExcelParty} from "../report/PaymentExcelParty";

interface EditPaymentFormProps {
    onValidityChange?: (valid: boolean) => void;
    filteredPaymentTypes: any[];
    paymentMethods?: any[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    onUpdate: (data: { values: any; menuItem: string }) => void;
    payment: Payment;
    paymentType: number;
    formEditMode: number;
    currencies: any[];
    handleCancelForm: () => void;
    debugForm?: boolean;
}

const EditPaymentForm: React.FC<EditPaymentFormProps> = ({
                                                             filteredPaymentTypes,
                                                             paymentMethods,
                                                             getTranslatedLabel,
                                                             onUpdate,
                                                             payment,
                                                             paymentType,
                                                             currencies,
                                                             handleCancelForm, debugForm
}) => {
    const localizationKey = "accounting.payments.form";
    const CASH_PAYMENT_METHOD_ID = "CASH";
    const nonEditableStatuses = ['PMNT_RECEIVED', 'PMNT_SENT', 'PMNT_CONFIRMED' /*, 'PMNT_CANCELLED' */];
    const isFormDisabled = payment && nonEditableStatuses.includes(payment.statusId);
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";
    const companyName = useAppSelector((state: RootState) => state.accountingSharedUi.selectedAccountingCompanyName);
    const { data: glAccounts, isLoading: isLoadingGlAccounts } = useFetchGlAccountOrganizationHierarchyLovQuery(companyId, {
        skip: !companyId,
    });


    const {
        data: paymentApplications = [],
        isLoading: isAppsLoading,
        isFetching: isAppsFetching,
    } = useFetchPaymentApplicationsForPaymentQuery(payment?.paymentId ?? "", {
        skip: !payment?.paymentId,
    });

    const {
        data: acctTransEntryData = [],
        isLoading: isTransLoading,
        isFetching: isTransFetching,
    } = useFetchPaymentAcctTransEntriesQuery(payment?.paymentId ?? "", {
        skip: !payment?.paymentId,
    });

    const excelApplications = useMemo(
        () =>
            paymentApplications.map((app: any) => ({
                invoiceId: app.invoiceId,
                toPaymentId: app.toPaymentId,
                billingAccountId: app.billingAccountId,
                taxAuthGeoId: app.taxAuthGeoId,
                amountApplied: app.amountApplied ?? 0,
            })),
        [paymentApplications]
    );

    const excelTransactions = useMemo(
        () =>
            acctTransEntryData.map((t: any) => ({
                acctgTransId: t.acctgTransId,
                acctgTransEntrySeqId: t.acctgTransEntrySeqId,
                glAccountId: t.glAccountId,
                glAccountName: t.glAccountTypeDescription || t.glAccountName || "",
                debitCreditFlag: t.debitCreditFlag,
                origAmount: t.origAmount ?? 0,
                currency: t.origCurrencyUomId || "",
                transactionDate: t.transactionDate,
            })),
        [acctTransEntryData]
    );

    const isExcelFetching = isAppsFetching || isTransFetching;

    
    
    const statusDesc = useMemo(() => ({
        'PMNT_NOT_PAID': 'Not Paid',
        'PMNT_RECEIVED': 'Received',
        'PMNT_SENT': 'Sent',
        'PMNT_CONFIRMED': 'Confirmed',
        'PMNT_CANCELLED': 'Cancelled',
    }[payment?.statusId] || payment?.statusId), [payment?.statusId]);

    // Memoize the mapped currencies to avoid recomputation on every render
    const mappedCurrencies = useMemo(() => {
        if (!currencies) return [];
        return currencies.map((currency) => ({
            actualCurrencyUomId: currency.currencyUomId,
            description: currency.description,
        }));
    }, [currencies]);

    

    const paymentTypeDesc = useMemo(() => {
        if (!payment?.paymentTypeId) return "";
        return (
            filteredPaymentTypes.find(
                (pt: any) => pt.paymentTypeId === payment.paymentTypeId
            )?.description ?? payment.paymentTypeId
        );
    }, [filteredPaymentTypes, payment?.paymentTypeId]);

    const paymentRow = useMemo(() => {
        if (!payment) return null;

        const isCash = payment.paymentMethodId === "CASH";
        return {
            paymentId: payment.paymentId ?? "",
            paymentType: paymentTypeDesc,
            fromParty: paymentType === 1 ? payment.partyIdFromName ?? "" : payment.partyIdToName ?? "",
            toParty: paymentType === 1 ? payment.partyIdToName ?? "" : payment.partyIdFromName ?? "",
            amount: payment.amount ?? 0,
            currency: payment.currencyUomId ?? "",
            effectiveDate: payment.effectiveDate ?? "",
            status: statusDesc,
            paymentMethod:
                paymentMethods?.find((m) => m.paymentMethodId === payment.paymentMethodId)
                    ?.description ?? "",
            chequeNumber: isCash ? "" : payment.chequeNumber,
            chequeDate: isCash ? undefined : payment.chequeDate,
        };
    }, [
        payment,
        paymentTypeDesc,
        paymentType,
        statusDesc,
        paymentMethods,
    ]);

    // Initialize form with payment values
    const initialValues = useMemo(() => {
        if (!payment) return {};
        const isCash = payment.paymentMethodId === CASH_PAYMENT_METHOD_ID;
        return {
            paymentId: payment.paymentId ?? undefined,
            paymentTypeId: payment.paymentTypeId ?? undefined,
            paymentMethodId: payment.paymentMethodId ?? undefined,
            statusId: payment.statusId ?? undefined,
            fromPartyId: payment.fromPartyId ?? undefined,
            partyIdFromName: payment.partyIdFromName ?? '',
            partyIdTo: payment.partyIdTo ?? undefined,
            partyIdToName: payment.partyIdToName ?? '',
            amount: payment.amount ?? undefined,
            currencyUomId: payment.currencyUomId ?? undefined,
            effectiveDate: payment.effectiveDate ? new Date(payment.effectiveDate) : undefined,
            comments: payment.comments ?? '',
            finAccountTransId: payment.finAccountTransId ?? undefined,
            overrideGlAccountId: payment.overrideGlAccountId ?? undefined,
            isDepositWithDrawPayment: payment.isDepositWithDrawPayment ?? undefined,
            finAcctTransTypeId: payment.finAcctTransTypeId ?? undefined,
            isDisbursement: payment.isDisbursement ?? undefined,
            actualCurrencyUomId: '',
            actualCurrencyAmount: undefined,
            chequeNumber: isCash ? '' : (payment.chequeNumber ?? ''),
            chequeDate: isCash ? null : (payment.chequeDate ? new Date(payment.chequeDate) : null),
        };
    }, [payment]);

    if (!payment) {
        return (
            <Typography variant="h6" sx={{pl: 2}}>
                {getTranslatedLabel(
                    `${localizationKey}.noPayment`,
                    "No payment selected for editing."
                )}
            </Typography>
        );
    }
    
    

    // Handle form submission     
    const handleSubmit = (values: any) => {
        if (isFormDisabled) return;
        onUpdate({
            values,
            menuItem: "Update Payment",
        });
    };




    return (
            <Grid container>
                <Form
                    initialValues={initialValues}
                    onSubmit={handleSubmit}
                    key={payment.paymentId}
                    render={(formRenderProps: FormRenderProps) => {
                        const {
                            valid,
                            validator, onSubmit,
                            errors,
                            touched,
                            visited,
                            valueGetter,
                            onChange,
                        } = formRenderProps;

                        
                        // REFACTOR: Custom handler for payment method change
                        const handlePaymentMethodChange = (event: any) => {
                            const selectedMethodId = event.value;
                            const isCash = selectedMethodId === CASH_PAYMENT_METHOD_ID;

                            // Update the payment method
                            onChange('paymentMethodId', { value: selectedMethodId });

                            // Clear cheque fields if CASH is selected
                            if (isCash) {
                                onChange('chequeNumber', { value: '' });
                                onChange('chequeDate', { value: null });
                            }
                        };

                        
                        return (
                        <FormElement>
                            <fieldset
                                className={`k-form-fieldset ${isFormDisabled ? 'grid-disabled' : 'grid-normal'}`}
                                aria-disabled={isFormDisabled}
                            >
                                <Grid container spacing={1} padding={2}>
                                   

                                    {/* Hidden Fields */}
                                    <Field name="statusId" component="input" type="hidden" />
                                    <Field name="paymentTypeId" component="input" type="hidden" />
                                    <Field name="partyIdFrom" component="input" type="hidden" />
                                    <Field name="partyIdTo" component="input" type="hidden" />
                                    <Field name="currencyUomId" component="input" type="hidden" />
                                    <Field name="finAccountTransId" component="input" type="hidden" />
                                    <Field name="isDepositWithDrawPayment" component="input" type="hidden" />
                                    <Field name="finAcctTransTypeId" component="input" type="hidden" />
                                    <Field name="isDisbursement" component="input" type="hidden" />
                                    <Field name="paymentPreferenceId" component="input" type="hidden" />
                                    <Field name="paymentGatewayResponseId" component="input" type="hidden" />

                                    {/* Section 2: Party Details */}
                                    <Grid item xs={12}>
                                        <Grid container spacing={1} alignItems="flex-end">
                                            <Grid item xs={3}>
                                                <Typography variant="h6" sx={{ pl: 2, pb: 1 }}>
                                                    {getTranslatedLabel(
                                                        paymentType === 1 ? `${localizationKey}.from` : `${localizationKey}.to`,
                                                        paymentType === 1 ? "From Party" : "To Party"
                                                    )}
                                                </Typography>
                                                <Typography variant="h6" sx={{ pl: 2 }}>
                                                    <strong style={{ color: "blue" }}>
                                                        {paymentType === 1 ? payment.partyIdFromName : payment.partyIdToName || "N/A"}
                                                    </strong>
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Typography variant="h6" sx={{ pl: 2, pb: 1 }}>
                                                    {getTranslatedLabel(
                                                        paymentType === 1 ? `${localizationKey}.to` : `${localizationKey}.from`,
                                                        paymentType === 1 ? "To Party" : "From Party"
                                                    )}
                                                </Typography>
                                                <Typography variant="h6" sx={{ pl: 2 }}>
                                                    <strong style={{ color: "blue" }}>
                                                        {paymentType === 1 ? payment.partyIdToName : payment.partyIdFromName || "N/A"}
                                                    </strong>
                                                </Typography>
                                            </Grid>
                                        </Grid>
                                    </Grid>

                                    {/* Section 1: Core Details */}
                                    <Grid item xs={12}>
                                        <Grid container spacing={1} alignItems="flex-end">
                                            <Grid item xs={3}>
                                                <Typography variant="h6" sx={{ pl: 2, pb: 1 }}>
                                                    {getTranslatedLabel(
                                                        `${localizationKey}.paymentType`,
                                                        "Payment Type"
                                                    )}
                                                </Typography>
                                                <Typography variant="h6" sx={{ pl: 2 }}>
                                                    <strong style={{ color: "blue" }}>{paymentTypeDesc}</strong>
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={3}>
                                                <Field
                                                    id="paymentMethodId"
                                                    name="paymentMethodId"
                                                    label={getTranslatedLabel(
                                                        `${localizationKey}.paymentMethod`,
                                                        "Payment Method *"
                                                    )}
                                                    component={MemoizedFormDropDownList}
                                                    dataItemKey="paymentMethodId"
                                                    textField="description"
                                                    data={paymentMethods || []}
                                                    validator={requiredValidator}
                                                    onChange={handlePaymentMethodChange}
                                                />
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Field
                                                    id="chequeNumber"
                                                    name="chequeNumber"
                                                    label={getTranslatedLabel(`${localizationKey}.chequeNumber`, "Cheque Number")}
                                                    component={FormInput}
                                                    autoComplete="off"
                                                    validator={(value, getter) => chequeValidator(value, getter, undefined, formRenderProps)}
                                                />
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Field
                                                    id="chequeDate"
                                                    name="chequeDate"
                                                    label={getTranslatedLabel(`${localizationKey}.chequeDate`, "Cheque Date")}
                                                    component={FormDatePicker}
                                                    format="yyyy-MM-dd"
                                                    validator={(value, getter) => chequeValidator(value, getter, undefined, formRenderProps)}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>

                                    <Grid item xs={12}></Grid>
                                    <Grid item xs={12}>
                                        <Grid container spacing={1} alignItems="flex-end">
                                            <Grid item xs={2}>
                                                <Field
                                                    id="amount"
                                                    label={getTranslatedLabel(
                                                        `${localizationKey}.amount`,
                                                        "Amount *"
                                                    )}
                                                    format="n2"
                                                    min={0}
                                                    name="amount"
                                                    component={FormNumericTextBox}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>

                                            <Grid item xs={2}>
                                                <Typography variant="h6" sx={{ pl: 2, pb: 1 }}>
                                                    {getTranslatedLabel(
                                                        `${localizationKey}.currency`,
                                                        "Currency"
                                                    )}
                                                </Typography>
                                                <Typography variant="h6" sx={{ pl: 2 }}>
                                                    <strong style={{ color: "blue" }}>{payment.currencyUomId}</strong>
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={4}>
                                                {isLoadingGlAccounts ? (
                                                    <Skeleton variant="rounded" height={56} />
                                                ) : (
                                                    <Field
                                                        id="overrideGlAccountId"
                                                        name="overrideGlAccountId"
                                                        label={getTranslatedLabel(`${localizationKey}.overrideGlAccountId`, "Override GL Account")}
                                                        data={glAccounts || []}
                                                        component={FormDropDownTreeGlAccount2}
                                                        dataItemKey="glAccountId"
                                                        textField="text"
                                                        selectField="selected"
                                                        expandField="expanded"
                                                    />
                                                )}
                                            </Grid>
                                           
                                        </Grid>
                                    </Grid>

                                    {/* Section 4: Metadata */}
                                    <Grid item xs={12}>
                                        <Grid container spacing={1} alignItems="flex-end">
                                            <Grid item xs={4}>
                                                <Field
                                                    id="effectiveDate"
                                                    name="effectiveDate"
                                                    label={getTranslatedLabel(
                                                        `${localizationKey}.effectiveDate`,
                                                        "Effective Date *"
                                                    )}
                                                    component={FormDatePicker}
                                                    format="yyyy-MM-dd HH:mm:ss"
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            
                                            <Grid item xs={4}>
                                                <Field
                                                    id="comments"
                                                    name="comments"
                                                    label={getTranslatedLabel(
                                                        `${localizationKey}.comments`,
                                                        "Comments"
                                                    )}
                                                    component={FormTextArea}
                                                    autoComplete="off"
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
                                    
                                </Grid>
                            </fieldset>
                            <div className="k-form-buttons">
                                <Grid container spacing={2}>
                                    <Grid item xs={2}>
                                        <Button
                                            type="submit"
                                            variant="contained"
                                            disabled={!formRenderProps.valid || isFormDisabled}
                                            sx={{ mt: 2, mr: 1 }}
                                        >
                                            {getTranslatedLabel(
                                                `${localizationKey}.update`,
                                                "Update Payment"
                                            )}
                                        </Button>
                                    </Grid>
                                    {paymentRow && (
                                        <Grid item xs={2}>
                                            <PaymentExcelTechnical
                                                companyName={companyName ?? "N/A"}
                                                payment={paymentRow}
                                                applications={excelApplications}
                                                transactions={excelTransactions}
                                                getTranslatedLabel={getTranslatedLabel}
                                                isFetching={isExcelFetching}
                                            />
                                        </Grid>
                                    )}

                                    {paymentRow && (
                                            <Grid item xs={2}>
                                                <PaymentExcelParty
                                                    companyName={companyName ?? "N/A"}
                                                    payment={paymentRow}
                                                    getTranslatedLabel={getTranslatedLabel}
                                                    isFetching={isExcelFetching}
                                                />
                                            </Grid>
                                    )}
                                    <Grid item xs={1}>
                                        <Button
                                            sx={{ mt: 2 }}
                                            onClick={handleCancelForm}
                                            color="error"
                                            variant="contained"
                                        >
                                            {getTranslatedLabel("general.cancel", "Cancel")}
                                        </Button>
                                    </Grid>
                                </Grid>
                            </div>
                        </FormElement>
                        );
                    }}
                />
            </Grid>
    );
};

export default EditPaymentForm;