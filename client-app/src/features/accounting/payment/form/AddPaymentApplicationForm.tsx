import {Button, Grid as MuiGrid, Typography} from "@mui/material";
import {Field, Form, FormElement, FormRenderProps} from "@progress/kendo-react-form";
import {useTranslationHelper} from "../../../../app/hooks/useTranslationHelper";
import {requiredValidator} from "../../../../app/common/form/Validators";
import {FormDropDownList} from "../../../../app/common/form/FormDropDownList";
import {Payment} from "../../../../app/models/accounting/payment";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";


interface AddPaymentApplicationFormProps {
    payment: Payment | undefined;
    notAppliedInvoices: NotAppliedInvoice[];
    notAppliedAmount: number;
    disabled: boolean;
    onSubmit: (values: any) => void;
    onCancel: () => void;
}

const AddPaymentApplicationForm: React.FC<AddPaymentApplicationFormProps> = ({
                                                                                 payment,
                                                                                 notAppliedInvoices,
                                                                                 notAppliedAmount,
                                                                                 disabled,
                                                                                 onSubmit,
                                                                                 onCancel,
                                                                             }) => {
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = "accounting.payments.applications";

    return (
        <MuiGrid item xs={12}>
            <Typography variant="h5">
                {getTranslatedLabel(`${localizationKey}.applyPayment`, "Apply Payment To")}
            </Typography>
            <Form
                onSubmit={onSubmit}
                render={(formRenderProps: FormRenderProps) => (
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
                                        max={notAppliedAmount}
                                        validator={requiredValidator}
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
                                        disabled={!formRenderProps.valid || disabled}
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
                )}
            />
        </MuiGrid>
    );
};

export default AddPaymentApplicationForm;