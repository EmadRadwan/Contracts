import { FormElement, Form, Field } from "@progress/kendo-react-form";
import React from "react";
import { Grid, Button } from "@mui/material";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { requiredValidator } from "../../../../app/common/form/Validators";
import { MemoizedFormDropDownList2 } from "../../../../app/common/form/MemoizedFormDropDownList2";

interface ComparativeBalanceSheetFormProps {
  onSubmit: (values: any) => void;
}

const ComparativeBalanceSheetForm = ({
  onSubmit,
}: ComparativeBalanceSheetFormProps) => {
  return (
    <Form
      onSubmit={(values) => onSubmit(values)}
      initialValues={{period1GlFiscalTypeId: "ACTUAL", period2GlFiscalTypeId: "ACTUAL"}}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid container spacing={2} alignItems={"flex-end"}>
              <Grid container item xs={6} spacing={2}>
                <Grid item xs={10}>
                  <Field
                    name={"period1ThruDate"}
                    id={"period1ThruDate"}
                    label={"Period 1 Thru Date"}
                    component={FormDatePicker}
                  />
                </Grid>
                <Grid item xs={10}>
                  <Field
                    name={"period1GlFiscalTypeId"}
                    id={"period1GlFiscalTypeId"}
                    label={"GL Fiscal Type"}
                    component={MemoizedFormDropDownList2}
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
              </Grid>
              <Grid container item xs={6} spacing={2}>
                <Grid item xs={10}>
                  <Field
                    name={"period2ThruDate"}
                    id={"period2ThruDate"}
                    label={"Period 2 Thru Date"}
                    component={FormDatePicker}
                  />
                </Grid>
                <Grid item xs={10}>
                <Field
                    name={"period2GlFiscalTypeId"}
                    id={"period2GlFiscalTypeId"}
                    label={"GL Fiscal Type"}
                    component={MemoizedFormDropDownList2}
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
              </Grid>
            </Grid>
            <Grid container item xs={12} spacing={2} mt={2}>
              <Grid item xs={12}>
                <Button variant="contained" type="submit" color="success">
                  Generate Report
                </Button>
              </Grid>
            </Grid>
          </fieldset>
        </FormElement>
      )}
    />
  );
};

export default ComparativeBalanceSheetForm;
