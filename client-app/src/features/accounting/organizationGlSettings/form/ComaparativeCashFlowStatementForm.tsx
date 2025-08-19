import { Field, Form, FormElement } from '@progress/kendo-react-form';
import React from 'react'
import { Grid, Button } from '@mui/material';
import FormDatePicker from '../../../../app/common/form/FormDatePicker';
import { requiredValidator } from '../../../../app/common/form/Validators';
import { MemoizedFormDropDownList2 } from '../../../../app/common/form/MemoizedFormDropDownList2';

interface ComaparativeCashFlowStatementFormProps {
  onSubmit: (values: any) => void;
}

const ComaparativeCashFlowStatementForm = ({onSubmit}: ComaparativeCashFlowStatementFormProps) => {
  return (
    <Form
    onSubmit={(values) => onSubmit(values)}
    render={(formRenderProps) => (
      <FormElement>
        <fieldset className={"k-form-fieldset"}>
          <Grid container spacing={2} alignItems={"flex-end"}>
            <Grid container item xs={6} spacing={2}>
                <Grid item xs={10}>
                <Field
                  name={"fromDate1"}
                  id={"fromDate1"}
                  label={"Period 1 From Date *"}
                  component={FormDatePicker}
                  validator={requiredValidator}
                />
              </Grid>
              <Grid item xs={10}>
                <Field
                  name={"thruDate1"}
                  id={"thruDate1"}
                  label={"Period 1 Thru Date *"}
                  component={FormDatePicker}
                  validator={requiredValidator}
                />
              </Grid>
              <Grid item xs={10}>
                <Field
                  name={"fiscalGlType1"}
                  id={"fiscalGlType1"}
                  label={"Period 1 Fiscal GL Type"}
                  component={MemoizedFormDropDownList2}
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
            <Grid container item xs={6} spacing={2}>
            <Grid item xs={10}>
                <Field
                  name={"fromDate2"}
                  id={"fromDate2"}
                  label={"Period 2 From Date *"}
                  component={FormDatePicker}
                  validator={requiredValidator}
                />
              </Grid>
              <Grid item xs={10}>
                <Field
                  name={"thruDate2"}
                  id={"thruDate2"}
                  label={"Period 2 Thru Date *"}
                  component={FormDatePicker}
                  validator={requiredValidator}
                />
              </Grid>
              <Grid item xs={10}>
                <Field
                  name={"fiscalGlType2"}
                  id={"fiscalGlType2"}
                  label={"Period 2 Fiscal GL Type"}
                  component={MemoizedFormDropDownList2}
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
  )
}

export default ComaparativeCashFlowStatementForm