import { Field, Form, FormElement } from '@progress/kendo-react-form';
import React from 'react'
import { Grid, Button } from '@mui/material';
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

interface ComparativeIncomeStatementFormProps {
  onSubmit: (values: any) => void;
}

const ComparativeIncomeStatementForm = ({onSubmit}: ComparativeIncomeStatementFormProps) => {
  const {getTranslatedLabel} = useTranslationHelper()
  const localizationKey = "accounting.orgGL.reports.comparative-income-statement.form"

  const now = new Date();
  const firstDayOfYear = new Date(now.getFullYear(), 0, 1);
  const firstDayOfPrevYear = new Date(now.getFullYear() - 1, 0, 1);
  const endOfPrevYear = new Date(now.getFullYear() - 1, 11, 31);

  return (
    <Form
        initialValues={{
            glFiscalTypeId1: "ACTUAL",
            fromDate1: firstDayOfYear,
            thruDate1: now,
            selectedMonth1: null,
            glFiscalTypeId2: "ACTUAL",
            fromDate2: firstDayOfPrevYear,
            thruDate2: endOfPrevYear,
            selectedMonth2: null,
        }}
        ignoreModified={true}
        onSubmit={(values) => onSubmit(values)}
        render={(formRenderProps) => (
            <FormElement>
                <fieldset className={"k-form-fieldset"}>
                    <Grid container spacing={4}>
                        {/* Period 1 */}
                        <Grid item xs={12} md={6}>
                            <Grid container spacing={2}>
                                <Grid item xs={12}>
                                    <strong>{getTranslatedLabel(`${localizationKey}.period1`, "Period 1")}</strong>
                                </Grid>
                                <Grid item xs={12}>
                                    <Field
                                        name={"selectedMonth1"}
                                        id={"selectedMonth1"}
                                        label={getTranslatedLabel(`${localizationKey}.month`, "Month")}
                                        component={MemoizedFormDropDownList2}
                                        data={months}
                                        textField="text"
                                        dataItemKey="month"
                                        onChange={(e) => {
                                            formRenderProps.onChange("fromDate1", { value: null });
                                            formRenderProps.onChange("thruDate1", { value: null });
                                            formRenderProps.onChange("selectedMonth1", e);
                                        }}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    <Field
                                        name={"fromDate1"}
                                        id={"fromDate1"}
                                        label={getTranslatedLabel(`${localizationKey}.fromDate`, "From Date")}
                                        component={FormDatePicker}
                                        onChange={(e) => {
                                            formRenderProps.onChange("selectedMonth1", { value: null });
                                            formRenderProps.onChange("fromDate1", e);
                                        }}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    <Field
                                        name={"thruDate1"}
                                        id={"thruDate1"}
                                        label={getTranslatedLabel(`${localizationKey}.thruDate`, "Thru Date")}
                                        component={FormDatePicker}
                                        onChange={(e) => {
                                            formRenderProps.onChange("selectedMonth1", { value: null });
                                            formRenderProps.onChange("thruDate1", e);
                                        }}
                                    />
                                </Grid>
                            </Grid>
                        </Grid>

                        {/* Period 2 */}
                        <Grid item xs={12} md={6}>
                            <Grid container spacing={2}>
                                <Grid item xs={12}>
                                    <strong>{getTranslatedLabel(`${localizationKey}.period2`, "Period 2")}</strong>
                                </Grid>
                                <Grid item xs={12}>
                                    <Field
                                        name={"selectedMonth2"}
                                        id={"selectedMonth2"}
                                        label={getTranslatedLabel(`${localizationKey}.month`, "Month")}
                                        component={MemoizedFormDropDownList2}
                                        data={months}
                                        textField="text"
                                        dataItemKey="month"
                                        onChange={(e) => {
                                            formRenderProps.onChange("fromDate2", { value: null });
                                            formRenderProps.onChange("thruDate2", { value: null });
                                            formRenderProps.onChange("selectedMonth2", e);
                                        }}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    <Field
                                        name={"fromDate2"}
                                        id={"fromDate2"}
                                        label={getTranslatedLabel(`${localizationKey}.fromDate`, "From Date")}
                                        component={FormDatePicker}
                                        onChange={(e) => {
                                            formRenderProps.onChange("selectedMonth2", { value: null });
                                            formRenderProps.onChange("fromDate2", e);
                                        }}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    <Field
                                        name={"thruDate2"}
                                        id={"thruDate2"}
                                        label={getTranslatedLabel(`${localizationKey}.thruDate`, "Thru Date")}
                                        component={FormDatePicker}
                                        onChange={(e) => {
                                            formRenderProps.onChange("selectedMonth2", { value: null });
                                            formRenderProps.onChange("thruDate2", e);
                                        }}
                                    />
                                </Grid>
                            </Grid>
                        </Grid>
                    </Grid>

                    <Grid container item xs={12} spacing={2} mt={2}>
                        <Grid item xs={12}>
                            <Button
                                variant="contained"
                                type="submit"
                                color="success"
                                disabled={!formRenderProps.allowSubmit}
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

export default ComparativeIncomeStatementForm;