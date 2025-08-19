import {requiredValidator} from "../../../../app/common/form/Validators";
import {
    Field,
    Form,
    FormElement,
    FormRenderProps,
} from "@progress/kendo-react-form";
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import {Button, Grid, Typography} from "@mui/material";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import FormTextArea from "../../../../app/common/form/FormTextArea";
import {Payment} from "../../../../app/models/accounting/payment";
import FormInput from "../../../../app/common/form/FormInput";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import {useMemo} from "react";

interface EditPaymentFormProps {
    formRef: React.MutableRefObject<any>;
    filteredPaymentTypes: any[];
    paymentMethods?: any[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    onUpdate: (data: { values: any; isValid: boolean; menuItem: string }) => void;
    payment: Payment;
    paymentType: number;
    formEditMode: number;
    currencies: any[];
    handleCancelForm: () => void;
}

const EditPaymentForm: React.FC<EditPaymentFormProps> = ({
                                                             formRef,
                                                             filteredPaymentTypes,
                                                             paymentMethods,
                                                             getTranslatedLabel,
                                                             onUpdate,
                                                             payment,
                                                             paymentType,
                                                             currencies,
                                                             handleCancelForm
                                                         }) => {

    const nonEditableStatuses = ['PMNT_RECEIVED', 'PMNT_SENT', 'PMNT_CONFIRMED' /*, 'PMNT_CANCELLED' */];
    const isFormDisabled = payment && nonEditableStatuses.includes(payment.statusId);

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

    const localizationKey = "accounting.payments.form";

    // Guard clause: return message if no payment is provided
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
            isValid: formRef.current?.isValid(),
            menuItem: "Update Payment",
        });
    };

    // Lookup descriptions for read-only fields
    const paymentTypeDesc = filteredPaymentTypes.find(
        (pt: any) => pt.paymentTypeId === payment.paymentTypeId
    )?.description || payment.paymentTypeId;

    // Initialize form with payment values
    const initialValues = {
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
        paymentRefNum: payment.paymentRefNum ?? '',
        comments: payment.comments ?? '',
        finAccountTransId: payment.finAccountTransId ?? undefined,
        overrideGlAccountId: payment.overrideGlAccountId ?? undefined,
        paymentPreferenceId: payment.paymentPreferenceId ?? undefined,
        paymentGatewayResponseId: payment.paymentGatewayResponseId ?? undefined,
        isDepositWithDrawPayment: payment.isDepositWithDrawPayment ?? undefined,
        finAcctTransTypeId: payment.finAcctTransTypeId ?? undefined,
        isDisbursement: payment.isDisbursement ?? undefined,
        actualCurrencyUomId: '',
    };


    return (
            <Grid container>
                <Form
                    ref={formRef}
                    initialValues={initialValues}
                    onSubmit={handleSubmit}
                    render={(formRenderProps: FormRenderProps) => (
                        <FormElement>
                            {/* REFACTOR: Apply grid-disabled/grid-normal classes to fieldset, inspired by PurchaseOrderForm
                            This disables user interactions at the container level for non-editable statuses */}
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
                                            <Grid item xs={4}>
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
                                            <Grid item xs={4}>
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
                                            <Grid item xs={4}>
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
                                            <Grid item xs={4}>
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

                                            <Grid item xs={2}>
                                                <Field
                                                    id="actualCurrencyUomId"
                                                    label={getTranslatedLabel(
                                                        `${localizationKey}.actualCurrencyUomId`,
                                                        "Actual Currency"
                                                    )}
                                                    name="actualCurrencyUomId"
                                                    component={MemoizedFormDropDownList}
                                                    dataItemKey="actualCurrencyUomId"
                                                    textField="description"
                                                    data={mappedCurrencies || []}
                                                />
                                            </Grid>

                                            <Grid item xs={2}>
                                                <Field
                                                    id="actualCurrencyAmount"
                                                    name="actualCurrencyAmount"
                                                    label={getTranslatedLabel(
                                                        `${localizationKey}.actualCurrencyAmount`,
                                                        "Actual Currency Amount"
                                                    )}
                                                    format="n2"
                                                    min={0}
                                                    component={FormNumericTextBox}
                                                />
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
                                                    id="paymentRefNum"
                                                    name="paymentRefNum"
                                                    label={getTranslatedLabel(
                                                        `${localizationKey}.paymentRefNum`,
                                                        "Payment Reference Number"
                                                    )}
                                                    component={FormInput}
                                                    autoComplete="off"
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
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>

                                    {/* Section 5: Accounting Details */}
                                    <Grid item xs={12}>
                                        <Grid container spacing={1} alignItems="flex-end">
                                            <Grid item xs={2}>
                                                <Typography variant="h6" sx={{ pl: 2, pb: 1 }}>
                                                    {getTranslatedLabel(
                                                        `${localizationKey}.finAccountTransId`,
                                                        "Fin Account Trans ID"
                                                    )}
                                                </Typography>
                                            </Grid>
                                            <Grid item xs={10}>
                                                <Typography variant="h6" sx={{ pl: 2 }}>
                                                    <strong style={{ color: "blue" }}>
                                                        {payment.finAccountTransId || "N/A"}
                                                    </strong>
                                                </Typography>
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
                    )}
                />
            </Grid>
    );
};

export default EditPaymentForm;