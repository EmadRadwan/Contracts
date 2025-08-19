import { Button, Grid, Typography } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import React from "react";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import { requiredValidator } from "../../../../app/common/form/Validators";

interface Props {
    onSubmit: (values: any) => void
}

const AssignPaymentForm = ({onSubmit}: Props) => {
  const formRef = React.useRef<any>();
  const [formKey, setFormKey] = React.useState(Math.random());
  return (
    <>
    <hr/>
    <Typography variant="h6">
        Assign payment to (select one):
    </Typography>
    <Form
      ref={formRef}
      key={formKey}
      onSubmit={(values) => onSubmit(values)}
      render={(formRenderProps) => (
        <FormElement>
          <fieldset className={"k-form-fieldset"}>
            <Grid container>
              <Grid item container xs={3} spacing={2}>
                <Grid item xs={12}>
                    <Field
                      id={"invoiceId"}
                      format="n2"
                      min={0}
                      name={"invoiceId"}
                      label={"Invoice Id"}
                      component={FormNumericTextBox}
                    />
                </Grid>
                <Grid item xs={12}>
                    <Field
                      id={"paymentId"}
                      format="n2"
                      min={0}
                      name={"paymentId"}
                      label={"Payment Id"}
                      component={FormNumericTextBox}
                    />
                </Grid>
                <Grid item xs={12}>
                    <Field
                      id={"billingAccountId"}
                      format="n2"
                      min={0}
                      name={"billingAccountId"}
                      label={"Billing Account Id"}
                      component={FormNumericTextBox}
                    />
                </Grid>
                <Grid item xs={12}>
                    <Field
                      id={"taxAuthGeoId"}
                      format="n2"
                      min={0}
                      name={"taxAuthGeoId"}
                      label={"Tax Auth Geo Id"}
                      component={FormNumericTextBox}
                    />
                </Grid>
                <Grid item xs={12}>
                    <Field
                      id={"amount"}
                      format="n2"
                      min={0}
                      name={"amount"}
                      label={"Amount to apply *"}
                      validator={requiredValidator}
                      component={FormNumericTextBox}
                    />
                </Grid>
              </Grid>
            </Grid>

            <Grid container spacing={1}>
              <Grid item xs={1}>
                <Button
                  sx={{ mt: 1 }}
                  color="success"
                  type="submit"
                  variant="contained"
                >
                  Apply
                </Button>
              </Grid>
            </Grid>
          </fieldset>
        </FormElement>
      )}
    />
    </>
  );
};

export default AssignPaymentForm;
