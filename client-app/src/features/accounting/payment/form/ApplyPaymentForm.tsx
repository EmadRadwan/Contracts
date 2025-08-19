import { Button, Grid, Typography } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import React, { useEffect, useState } from "react";
import FormInput from "../../../../app/common/form/FormInput";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";

interface Props {
  selectedApplication: any;
  onClose: () => void;
  paymentId: string
  
}

const ApplyPaymentForm = ({ onClose, selectedApplication, paymentId }: Props) => {
  const MyForm = React.useRef<any>();
  const onSubmit = (values: any) => {
    console.log(values)
  }
  return (
    <>
        <Typography variant="h5" mb={2}>
            {`Apply payment to invoice ${selectedApplication.invoiceId}`}
        </Typography>
      <Form
        key={JSON.stringify(selectedApplication)}
        ref={MyForm}
        onSubmit={(values) => onSubmit(values)}
        render={(formRenderProps) => (
          <FormElement>
            <fieldset className={"k-form-fieldset"}>
              <Field
                id={"amount"}
                name={"amount"}
                label={"Amount to apply *"}
                component={FormNumericTextBox}
              />

              <div className="k-form-buttons">
                <Grid container rowSpacing={1}>
                  <Grid item xs={5}>
                    <Button
                      variant="contained"
                      type={"submit"}
                      color="success"
                    //   disabled={!formRenderProps.allowSubmit || buttonFlag}
                    >
                      {"Apply"}
                    </Button>
                  </Grid>
                  <Grid item xs={2}>
                    <Button
                      onClick={() => onClose()}
                      color="error"
                      variant="contained"
                    >
                      Cancel
                    </Button>
                  </Grid>
                </Grid>
              </div>
            </fieldset>
          </FormElement>
        )}
      />
    </>
  );
};

export default ApplyPaymentForm;
