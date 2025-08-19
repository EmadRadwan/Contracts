import { Button, Grid } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import React from "react";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import { requiredValidator } from "../../../../app/common/form/Validators";

interface BalanceSheetFormProps {
  onSubmit: (values: any) => void;
}

const BalanceSheetForm = ({ onSubmit }: BalanceSheetFormProps) => {
  return (
    <Form
      onSubmit={(values) => onSubmit(values)}
      initialValues={{ glFiscalTypeId: "ACTUAL" }}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid container spacing={2} alignItems={"flex-end"}>
              <Grid container item xs={12} spacing={2}>
                <Grid item xs={6}>
                  <Field
                    name={"glFiscalTypeId"}
                    id={"glFiscalTypeId"}
                    label={"GL Fiscal Type"}
                    component={MemoizedFormDropDownList}
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
                <Grid item xs={6}>
                  <Field
                    name={"thruDate"}
                    id={"thruDate"}
                    label={"Thru Date"}
                    component={FormDatePicker}
                    validator={requiredValidator}
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

export default BalanceSheetForm;
