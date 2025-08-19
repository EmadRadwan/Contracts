import { Button, Grid } from '@mui/material'
import { Field, Form, FormElement } from '@progress/kendo-react-form'
import React, { useState } from 'react'
import FormInput from '../../../../app/common/form/FormInput'
import { requiredValidator } from '../../../../app/common/form/Validators'
import FormNumericTextBox from '../../../../app/common/form/FormNumericTextBox'
import { FormMultiColumnComboBoxVirtualPaymentApplication } from '../../../../app/common/form/FormMultiColumnComboBoxVirtualPaymentApplication'

interface Props {
    onClose: () => void
}

const InvoicePaymentApplicationForm = ({onClose}: Props) => {
    const [buttonFlag, setButtonFlag] = useState(false)
  return (
    <>
        <Form
            onSubmit={(values) => console.log(values)}
            render={(formRenderProps) => (
                <FormElement>
                    <fieldset className="k-form-fieldset">
                        <Grid container>
                            <Grid item xs={8}>
                                <Field 
                                    name='paymentId'
                                    id="paymentId"
                                    label="Payment Id *"
                                    component={FormMultiColumnComboBoxVirtualPaymentApplication}
                                    validator={requiredValidator}
                                />
                            </Grid>
                            <Grid item xs={8}>
                                <Field 
                                    name='amount'
                                    id="amount"
                                    label="Amount to apply *"
                                    format={"n"}
                                    component={FormNumericTextBox}
                                    validator={requiredValidator}
                                />
                            </Grid>
                        </Grid>
                    </fieldset>
                    <div className="k-form-buttons">
                      <Button
                        variant="contained"
                        type={"submit"}
                        color="success"
                        disabled={!formRenderProps.allowSubmit || buttonFlag}
                      >
                        Submit
                      </Button>
                      <Button
                        onClick={onClose}
                        color="error"
                        variant="contained"
                      >
                        Cancel
                      </Button>
                    </div>
                </FormElement>
            )} 
        />
    </>
  )
}

export default InvoicePaymentApplicationForm