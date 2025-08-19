import React from 'react'
import { Order } from '../../../../../app/models/order/order'
import { Field, Form, FormElement } from '@progress/kendo-react-form'
import { Button, Grid, Paper } from '@mui/material'
import { MemoizedFormDropDownList2 } from '../../../../../app/common/form/MemoizedFormDropDownList2'
import { useFetchOrderPaymentPreferenceQuery, useFetchPaymentMethodsQuery, useReceiveOfflinePaymentMutation } from '../../../../../app/store/apis'
import FormNumericTextBox from '../../../../../app/common/form/FormNumericTextBox'
import { requiredValidator } from '../../../../../app/common/form/Validators'
import FormInput from '../../../../../app/common/form/FormInput'
import OrderPaymentPreferenceList from './OrderPaymentPreferenceList'
import LoadingComponent from '../../../../../app/layout/LoadingComponent'
import { toast } from 'react-toastify'


interface ReceivePaymentFormProps {
    cancelEdit: () => void
    selectedOrder: Order
}

const ReceivePaymentForm = ({cancelEdit, selectedOrder}: ReceivePaymentFormProps) => {
    
    const {data: paymentMethods} = useFetchPaymentMethodsQuery(undefined);
    
    const [
        receiveOfflinePayment,
        {isLoading}
    ] = useReceiveOfflinePaymentMutation()
    
    const handleSubmit = async (data: any) => {
        if (!data.isValid) return
        console.log(data)

        const {values} = data
        const newOfflinePayment = {
            orderId: values.orderId,
            partyId: values.fromPartyId.fromPartyId,
            paymentDetails: [{
                paymentMethodId: values.paymentMethodId,
                amount: values.amount,
                reference: values.refernece
            }]
        }

        try {
            const receiveOfflinePaymentResult = await receiveOfflinePayment(newOfflinePayment).unwrap()
            console.log(receiveOfflinePaymentResult)
            cancelEdit()
        } catch (e) {
            console.error(e)
            toast.error("Something went wrong!")
        }
    }
  return (
    <>
        <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <Grid container flexDirection={"column"} spacing={2}>
                <Grid item>
                    <OrderPaymentPreferenceList orderId={selectedOrder?.orderId} />
                </Grid>
                <Grid item>
                    <Form
                        key={JSON.stringify(selectedOrder)}
                        initialValues={selectedOrder ?? null}
                        onSubmitClick={(values) => handleSubmit(values)}
                        render={(formRenderProps) => (
                            <FormElement>
                                <fieldset className={"k-form-fieldset"}>
                                    <Grid container spacing={2}>
                                        <Grid item container>
                                            <Grid item xs={12}>
                                                <Field
                                                    id={"paymentMethodId"}
                                                    name={"paymentMethodId"}
                                                    label={"Payment Method *"}
                                                    component={MemoizedFormDropDownList2}
                                                    dataItemKey={"paymentMethodId"}
                                                    textField={"description"}
                                                    data={paymentMethods ?? []}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid container item spacing={2}>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"amount"}
                                                        name={"amount"}
                                                        label={"Amount *"}
                                                        component={FormNumericTextBox}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid>
                                                <Grid item xs={6}>
                                                    <Field
                                                        id={"reference"}
                                                        name={"reference"}
                                                        label={"Reference"}
                                                        component={FormInput}
                                                    />
                                                </Grid>
                                            </Grid>
                                        </Grid>
                                        <Grid item>
                                            <Button
                                                sx={{ mt: 1 }}
                                                type='submit'
                                                color="success"
                                                variant="contained"
                                            >
                                                Submit
                                            </Button>
                                        </Grid>
                                        <Grid item>
                                            <Button
                                                sx={{ mt: 1 }}
                                                onClick={cancelEdit}
                                                color="error"
                                                variant="contained"
                                            >
                                                Cancel
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </fieldset>
                            </FormElement>
                        )}
                    />
                </Grid>
            </Grid>
            {isLoading && (
                <LoadingComponent message='Processing Payment' />
            )}
        </Paper>
    </>
  )
}

export default ReceivePaymentForm