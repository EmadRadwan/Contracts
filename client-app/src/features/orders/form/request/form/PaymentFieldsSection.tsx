import React from "react";
import { Field } from "@progress/kendo-react-form";
import { Grid } from "@mui/material";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import FormDatePicker from "../../../../../app/common/form/FormDatePicker";
import FormTextArea from "../../../../../app/common/form/FormTextArea";
import { requiredValidator } from "../../../../../app/common/form/Validators";
import { FormRenderProps } from "@progress/kendo-react-form";

interface PaymentFieldsSectionProps {
    formRenderProps: FormRenderProps;
    onAdvanceChange: (form: FormRenderProps, value: number | null) => void;
    getTranslatedLabel: (key: string, fallback: string) => string;
}

export const PaymentFieldsSection: React.FC<PaymentFieldsSectionProps> = React.memo(({
                                                                                         formRenderProps,
                                                                                         onAdvanceChange,
                                                                                         getTranslatedLabel,
                                                                                     }) => {
    return (
        <>
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
                        onChange={(e: any) => onAdvanceChange(formRenderProps, e.value)}
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
                        hint={getTranslatedLabel("salesRequest.form.dateUpdateHint", "Changes will update the payment schedule dates immediately")}
                    />
                </Grid>

                <Grid item xs={2}>
                    <Field
                        id="monthsBetweenInstallments"
                        name="monthsBetweenInstallments"
                        label={getTranslatedLabel("salesRequest.form.duration", "Months")}
                        min={0}
                        component={FormNumericTextBox}
                        hint={getTranslatedLabel("salesRequest.form.dateUpdateHint", "Changes will update the payment schedule dates immediately")}
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
                        // No special handler — just updates directly
                    />
                </Grid>
            </Grid>

            <Grid item xs={12} mt={2}>
                <Field
                    id="comments"
                    name="comments"
                    label={getTranslatedLabel("salesRequest.form.comments", "Comments")}
                    autoComplete="off"
                    rows={3}
                    component={FormTextArea}
                />
            </Grid>
        </>
    );
});

PaymentFieldsSection.displayName = "PaymentFieldsSection";