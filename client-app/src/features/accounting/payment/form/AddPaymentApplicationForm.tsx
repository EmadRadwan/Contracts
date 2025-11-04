import {Button, Grid as MuiGrid, Typography} from "@mui/material";
import {Field, Form, FormElement, FormRenderProps} from "@progress/kendo-react-form";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import {requiredValidator} from "../../../../app/common/form/Validators";
import {FormDropDownList} from "../../../../app/common/form/FormDropDownList";
import {Payment} from "../../../../app/models/accounting/payment";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import {useApplyPayment} from "../hook/useApplyPayment";

interface NotAppliedInvoice {
    invoiceId: string;
    amount: number;
    amountApplied: number;
    amountToApply: number;
}

interface AddPaymentApplicationFormProps {
    payment: Payment | undefined;
    notAppliedInvoices: NotAppliedInvoice[];
    notAppliedAmount: number;
    disabled: boolean;
    onCancel: () => void;
}

const AddPaymentApplicationForm: React.FC<AddPaymentApplicationFormProps> = ({
                                                                                 payment,
                                                                                 notAppliedInvoices,
                                                                                 notAppliedAmount,
                                                                                 disabled,
                                                                                 onCancel,
                                                                             }) => {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.payments.applications";

    const { handleSubmit, isLoading } = useApplyPayment({
        paymentId: payment?.paymentId ?? "",
        onSuccess: onCancel,               // close the modal on success
    });

    const amountAppliedValidator = (value: any, formValues: any) => {
        const selected = formValues?.invoiceId as NotAppliedInvoice | undefined;

        if (!value) return requiredValidator(value);
        const num = Number(value);

        if (num > notAppliedAmount) {
            return getTranslatedLabel(
                `${localizationKey}.validation.exceedsPayment`,
                `Cannot exceed payment remaining (${notAppliedAmount.toFixed(2)} ${payment?.currencyUomId})`
            );
        }

        if (selected && num > selected.amountToApply) {
            return getTranslatedLabel(
                `${localizationKey}.validation.exceedsInvoice`,
                `Cannot exceed invoice remaining (${selected.amountToApply.toFixed(2)} ${payment?.currencyUomId})`
            );
        }

        return "";
    };


    return (
        <MuiGrid item xs={12}>
            <Typography variant="h5">
                {getTranslatedLabel(`${localizationKey}.applyPayment`, "Apply Payment To")}
            </Typography>
            <Form
                onSubmit={handleSubmit}
                render={(formRenderProps: FormRenderProps) => {
                    const selectedInvoice = formRenderProps.valueGetter("invoiceId") as NotAppliedInvoice | undefined;

                    const maxOverall = selectedInvoice
                        ? Math.min(notAppliedAmount, selectedInvoice.amountToApply)
                        : 0;

                    return (
                        
                    <FormElement>
                        <fieldset>
                            <MuiGrid container spacing={2}>
                                <MuiGrid item xs={4}>
                                    <Field
                                        id="invoiceId"
                                        name="invoiceId"
                                        label={getTranslatedLabel(`${localizationKey}.invoiceId`, "Invoice ID")}
                                        component={FormDropDownList}
                                        data={notAppliedInvoices}
                                        dataItemKey="invoiceId"
                                        textField="invoiceId"
                                        validator={requiredValidator}
                                    />
                                </MuiGrid>
                                <MuiGrid item xs={4}>
                                    <Field
                                        id="amountApplied"
                                        name="amountApplied"
                                        label={getTranslatedLabel(`${localizationKey}.amountApplied`, "Amount to Apply")}
                                        component={FormNumericTextBox}
                                        format="n2"
                                        min={0}
                                        max={maxOverall}
                                        validator={amountAppliedValidator}
                                    />
                                </MuiGrid>
                            </MuiGrid>
                        </fieldset>
                        <div className="k-form-buttons">
                            <MuiGrid container spacing={2}>
                                <MuiGrid item>
                                    <Button
                                        type="submit"
                                        variant="contained"
                                        disabled={!formRenderProps.valid}
                                        sx={{mt: 2}}
                                    >
                                        {getTranslatedLabel(`${localizationKey}.apply`, "Apply Payment")}
                                    </Button>
                                </MuiGrid>
                                <MuiGrid item>
                                    <Button
                                        variant="outlined"
                                        color="error"
                                        onClick={onCancel}
                                        sx={{mt: 2}}
                                    >
                                        {getTranslatedLabel("general.cancel", "Cancel")}
                                    </Button>
                                </MuiGrid>
                            </MuiGrid>
                        </div>
                    </FormElement>
                    );
                }}
            />
        </MuiGrid>
    );
};

export default AddPaymentApplicationForm;