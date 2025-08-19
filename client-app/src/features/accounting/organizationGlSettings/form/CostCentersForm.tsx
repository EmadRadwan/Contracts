import { Field, Form, FormElement } from "@progress/kendo-react-form";
import React from "react";
import { Grid, Button } from "@mui/material";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";

interface CostCentersFormProps {
  onSubmit: (values: any) => void;
}

const CostCentersForm = ({ onSubmit }: CostCentersFormProps) => {
  return (
    <Form
      onSubmit={(values) => onSubmit(values)}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid container spacing={2} alignItems={"flex-end"}>
              <Grid container item xs={12}>
                <Grid item xs={6}>
                  <Field
                    name={"fromDate"}
                    id={"fromDate"}
                    label={"From Date"}
                    component={FormDatePicker}
                    validator={(value) => {
                      if (!value) {
                        return "From Date is required";
                      }
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
                  />
                </Grid>
              </Grid>
              <Grid container item xs={12}>
                <Grid item xs={6}>
                  <Field
                    name={"thruDate"}
                    id={"thruDate"}
                    label={"Thru Date"}
                    component={FormDatePicker}
                    validator={(value) => {
                      if (!value) {
                        return "Thru Date is required";
                      }
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
                  />
                </Grid>
              </Grid>
              <Grid container item xs={12}>
                <Grid item xs={6}>
                  <Field
                    name={"fiscalGlType"}
                    id={"fiscalGlType"}
                    label={"Fiscal GL Type"}
                    component={MemoizedFormDropDownList}
                    data={[
                      { text: "Actual", fiscalGlType: "Actual" },
                      { text: "Budget", fiscalGlType: "Budget" },
                      { text: "Plan", fiscalGlType: "Plan" },
                      { text: "Scenario", fiscalGlType: "Scenario" },
                      { text: "Forecast", fiscalGlType: "Forecast" },
                    ]}
                    textField="text"
                    dataItemKey="fiscalGlType"
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

export default CostCentersForm;
