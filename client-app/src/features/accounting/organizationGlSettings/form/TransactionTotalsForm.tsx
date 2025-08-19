import React from "react";
import { Button, Grid } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";
import { useTranslationHelper } from "../../../../app/hooks/useTranslationHelper";

interface TransactionTotalsFormProps {
  onSubmit: (values: any) => void;
}

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

const TransactionTotalsForm = ({ onSubmit }: TransactionTotalsFormProps) => {
  const {getTranslatedLabel} = useTranslationHelper()
  const localizationKey = "accounting.orgGL.reports.transaction-totals.form"
  return (
    <Form
      onSubmit={(values) => onSubmit(values)}
      initialValues={{glFiscalTypeId: "ACTUAL"}}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid container spacing={2} alignItems={"flex-end"}>
              <Grid container item xs={12} spacing={2}>
                <Grid item xs={3}>
                  <Field
                    name={"selectedMonth"}
                    id={"selectedMonth"}
                    label={getTranslatedLabel(`${localizationKey}.month`, "Month")}
                    component={MemoizedFormDropDownList2}
                    data={months}
                    textField="text"
                    dataItemKey="month"
                    onChange={(e) => {
                      formRenderProps.onChange("fromDate", {
                        value: null,
                      });
                      formRenderProps.onChange("thruDate", {
                        value: null,
                      });
                    }
                  }
                  />
                </Grid>
                <Grid item xs={3}>
                  <Field
                    name={"glFiscalTypeId"}
                    id={"glFiscalTypeId"}
                    label={getTranslatedLabel(`${localizationKey}.fiscalType`, "GL Fiscal Type *")}
                    component={MemoizedFormDropDownList}
                    validator={requiredValidator}
                    data={[
                      { text: "Actual", glFiscalTypeId: "ACTUAL" },
                      { text: "Budget", glFiscalTypeId: "BUDGET" },
                      { text: "Plan", glFiscalTypeId: "PLAN" },
                      { text: "Scenario", glFiscalTypeId: "SCENARIO" },
                      { text: "Forecast", glFiscalTypeId: "FORECAST" },
                    ]}
                    textField="text"
                    dataItemKey="glFiscalTypeId"
                  />
                </Grid>
                <Grid item xs={3}>
                  <Field
                    name={"fromDate"}
                    id={"fromDate"}
                    label={getTranslatedLabel(`${localizationKey}.fromDate`, "From Date")}
                    component={FormDatePicker}
                    validator={(value) => {
                      const thruDate = formRenderProps.valueGetter("thruDate");
                      if (
                        thruDate &&
                        value &&
                        new Date(value) > new Date(thruDate)
                      ) {
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
                <Grid item xs={3}>
                  <Field
                    name={"thruDate"}
                    id={"thruDate"}
                    label={getTranslatedLabel(`${localizationKey}.thruDate`, "Thru Date")}
                    component={FormDatePicker}
                    validator={(value) => {
                      const fromDate = formRenderProps.valueGetter("fromDate");
                      if (
                        fromDate &&
                        value &&
                        new Date(value) < new Date(fromDate)
                      ) {
                        return "Thru Date cannot be before From Date";
                      }
                      return "";
                    }}
                    onChange={(e) => {
                      formRenderProps.onChange("month", { value: null });
                      formRenderProps.onChange("thruDate", e);
                    }}
                  />
                </Grid>
              </Grid>
            </Grid>
            <Grid container item xs={12} spacing={2} mt={2}>
              <Grid item xs={12}>
                <Button variant="contained" type="submit" color="success">
                  {getTranslatedLabel(`${localizationKey}.generate`, "Generate Report")}
                </Button>
              </Grid>
            </Grid>
          </fieldset>
        </FormElement>
      )}
    />
  );
};

export default TransactionTotalsForm;
