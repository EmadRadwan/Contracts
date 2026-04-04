import { Button, Grid } from '@mui/material';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import React from 'react'
import FormDatePicker from '../../../../app/common/form/FormDatePicker';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import { MemoizedFormDropDownList2 } from '../../../../app/common/form/MemoizedFormDropDownList2';

const months = [
    { text: "", month: null },
    { text: "January", month: 1 },
    { text: "February", month: 2 },
    { text: "March", month: 3 },
    { text: "April", month: 4 },
    { text: "May", month: 5 },
    { text: "June", month: 6 },
    { text: "July", month: 7 },
    { text: "August", month: 8 },
    { text: "September", month: 9 },
    { text: "October", month: 10 },
    { text: "November", month: 11 },
    { text: "December", month: 12 },
  ];

interface IncomeStatementFormProps {
    onSubmit: (values: any) => void;
}

const IncomeStatementForm = ({onSubmit}: IncomeStatementFormProps) => {
  const {getTranslatedLabel} = useTranslationHelper()
  const localizationKey = "accounting.orgGL.reports.income-statement.form"

  const now = new Date();
  const firstDayOfYear = new Date(now.getFullYear(), 0, 1);

    return (
        <Form
            initialValues={{
                glFiscalTypeId: "ACTUAL",
                fromDate: firstDayOfYear,
                thruDate: now,
                selectedMonth: null,   // explicitly add this
            }}
            ignoreModified={true}
            onSubmit={(values) => onSubmit(values)}   // ← Use onSubmit here
            render={(formRenderProps) => (
                <FormElement>
                    <fieldset className={"k-form-fieldset"}>
                        <Grid container spacing={2} alignItems={"flex-end"}>
                            <Grid container item xs={12} spacing={2}>
                                <Grid item xs={4}>
                                    <Field
                                        name={"selectedMonth"}
                                        id={"selectedMonth"}
                                        label={getTranslatedLabel(`${localizationKey}.month`, "Month")}
                                        component={MemoizedFormDropDownList2}
                                        data={months}
                                        textField="text"
                                        dataItemKey="month"
                                        onChange={(e) => {
                                            formRenderProps.onChange("fromDate", { value: null });
                                            formRenderProps.onChange("thruDate", { value: null });
                                            formRenderProps.onChange("selectedMonth", e);
                                        }}
                                    />
                                </Grid>
                                <Grid item xs={4}>
                                    <Field
                                        name={"fromDate"}
                                        id={"fromDate"}
                                        label={getTranslatedLabel(`${localizationKey}.fromDate`, "From Date")}
                                        component={FormDatePicker}
                                        validator={(value) => {
                                            const thruDate = formRenderProps.valueGetter("thruDate");
                                            if (thruDate && value && new Date(value) > new Date(thruDate)) {
                                                return "From Date cannot be after Thru Date";
                                            }
                                            return "";
                                        }}
                                        onChange={(e) => {
                                            formRenderProps.onChange("selectedMonth", { value: null });
                                            formRenderProps.onChange("fromDate", e);
                                        }}
                                    />
                                </Grid>
                                <Grid item xs={4}>
                                    <Field
                                        name={"thruDate"}
                                        id={"thruDate"}
                                        label={getTranslatedLabel(`${localizationKey}.thruDate`, "Thru Date")}
                                        component={FormDatePicker}
                                        validator={(value) => {
                                            const fromDate = formRenderProps.valueGetter("fromDate");
                                            if (fromDate && value && new Date(value) < new Date(fromDate)) {
                                                return "Thru Date cannot be before From Date";
                                            }
                                            return "";
                                        }}
                                        onChange={(e) => {
                                            formRenderProps.onChange("selectedMonth", { value: null });
                                            formRenderProps.onChange("thruDate", e);
                                        }}
                                    />
                                </Grid>
                            </Grid>
                        </Grid>

                        <Grid container item xs={12} spacing={2} mt={2}>
                            <Grid item xs={12}>
                                <Button
                                    variant="contained"
                                    type="submit"           // Important: keep type="submit"
                                    color="success"
                                    disabled={!formRenderProps.allowSubmit}   // Optional but recommended
                                >
                                    {getTranslatedLabel("general.generate", "Generate Report")}
                                </Button>
                            </Grid>
                        </Grid>
                    </fieldset>
                </FormElement>
            )}
        />
    );
};
export default IncomeStatementForm