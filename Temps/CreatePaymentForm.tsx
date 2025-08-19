import React, {useEffect, useState} from "react";
import usePayments from "../../../../app/hooks/usePayments";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";
import Grid from "@mui/material/Grid";
import {Box, Paper, Typography} from "@mui/material";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import FormInput from "../../../../app/common/form/FormInput";
import Button from "@mui/material/Button";
import {FormComboBoxVirtualCustomer} from "../../../../app/common/form/FormComboBoxVirtualCustomer";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {Menu, MenuItem, MenuSelectEvent} from "@progress/kendo-react-layout";
import {requiredValidator} from "../../../../app/common/form/Validators";
import {Payment} from "../../../../app/models/accounting/payment";
import {PaymentApplication} from "../../../../app/models/accounting/paymentApplication";
import {
    getTotalPaymentApplicationsValue,
    paymentApplicationsSelectors,
    resetSinglePaymentAll
} from "../slice/singlePaymentSlice";
import PaymentApplicationsList from "../dashboard/PaymentApplicationsList";

interface Props {
    paymentId?: string
    editMode: number;
    cancelEdit: () => void;
}

// editModes: [
//     {1: New},
//     {2: Created},
//     {3: Approved},
//     {4: Completed},
//     {5: Not Paid}
// ]

export default function CreatePaymentForm({paymentId, cancelEdit, editMode}: Props) {
    const [buttonFlag, setButtonFlag] = useState(false);
    const {selectedSinglePayment, selectSinglePayment} = usePayments();
    const [payment, setPaymentHeader] = useState<Payment>(selectedSinglePayment);
    const paymentApplications = useAppSelector(paymentApplicationsSelectors.selectAll);
    const nonDeletedItems = paymentApplications.filter(item => !item.isPaymentApplicationDeleted)
    const [showList, setShowList] = useState(false);
    const [formEditMode, setFormEditMode] = useState(editMode)
    const [formPaymentId, setFormPaymentId] = useState(paymentId)
    const totalPaymentApplicationsValue: any = useAppSelector(getTotalPaymentApplicationsValue)

    const formRef = React.useRef(null);
    const [isFromMenu, setIsFromMenu] = React.useState(false);

    console.log(selectedSinglePayment!)

    const dispatch = useAppDispatch();

    useEffect(() => {
        if (formEditMode > 1) {
            selectSinglePayment(formPaymentId!)
            setPaymentHeader(selectedSinglePayment)

        } else {
            // @ts-ignore
            setPaymentHeader(undefined)
        }

    }, [paymentId, selectedSinglePayment, payment, formEditMode])


    async function handleMenuSelect(e: MenuSelectEvent) {
        if (e.item.text === 'Approve Payment') {
            setIsFromMenu(true);
            setTimeout(() => {
                // @ts-ignore
                formRef.current.onSubmit();
            });
        }
        if (e.item.text === 'New Payment') {
            dispatch(resetSinglePaymentAll(null));
            setFormEditMode(1)
        }
        if (e.item.text === 'Complete Payment') {
            /* const response = await agent.Accounting.completePayment(payment);
             const updatedPayment: Payment = {...payment, statusDescription: response.statusDescription}
             dispatch(setSinglePayment(updatedPayment))
             setFormEditMode(4)*/
        }
    }

    function preparePaymentApplications(PaymentApplications: PaymentApplication[]) {
        const PaymentApplicationsForServer: PaymentApplication[] = [];
        PaymentApplications.forEach(item => {
            const newPaymentApplication: PaymentApplication = {
                paymentId: item.paymentId,
                paymentApplicationId: item.paymentApplicationId,
                isPaymentApplicationDeleted: item.isPaymentApplicationDeleted
            }

            PaymentApplicationsForServer.push(newPaymentApplication)
        })
        return PaymentApplicationsForServer;
    }


    async function handleSubmitData(data: any) {

        if (!data.isValid) {
            return false
        }
        setButtonFlag(true)


        try {
            let response: any;

            const items: PaymentApplication[] = preparePaymentApplications(paymentApplications);

            /*const payment: Payment = {
                paymentId: formEditMode === 2 ? data.values.paymentId : "PAYMENT-DUMMY",
                partyIdFrom: data.values.fromPartyId.fromPartyId,
                partyIdFromName: data.values.fromPartyId.fromPartyName,
                paymentMethodTypeId: data.values.paymentMethodTypeId,
                //grandTotal: totalPaymentApplicationsValue, 
                paymentApplications: items,
            }
*/
            /*if (isFromMenu) {
                setIsFromMenu(false);
                response = await agent.Accounting.approvePayment(payment);
                const updatedPayment: Payment = {...data.values, statusDescription: response.statusDescription}
                dispatch(setSinglePayment(updatedPayment))
                setFormEditMode(3)
                
                setButtonFlag(false)
                
                return
                 }

            if (formEditMode === 2) {
                response = await agent.Accounting.updatePayment(payment);
                const updatedPayment: Payment = {...payment, grandTotal: totalPaymentApplicationsValue }
                dispatch(setPayment(updatedPayment))
                
            } else {
                response = await agent.Accounting.createPayment(payment);

                const newPayment: Payment = {
                    ...data.values,
                    paymentId: response.paymentId,
                    statusDescription: response.statusDescription
                }
                
                dispatch(resetSinglePaymentApplication(null))
                PaymentApplications.forEach(item => {
                    const newItem = {...item, paymentId: response.paymentId}
                    dispatch(setSinglePaymentApplication(newItem))
                })
                
                dispatch(setSinglePayment(newPayment));
                
                selectSinglePayment(response.paymentId)
                setFormPaymentId(response.paymentId)

                setFormEditMode(2)
            }*/
        } catch (error) {
            console.log(error)
        }
        setButtonFlag(false)
    }

    function enableDisableSubmitButton(allowSubmit: boolean, formAllowSubmit: boolean): boolean {
        if (formAllowSubmit && nonDeletedItems.length > 0 || formEditMode === 1 && nonDeletedItems.length > 0) {
            return true
        } else {
            return allowSubmit && nonDeletedItems.length > 0;
        }

    }

    ////console.log('selectedSinglePayment', selectedSinglePayment)
    //console.log('formEditMode', formEditMode)


    return (
        <>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>

                <Grid container spacing={2}>
                    <Grid item xs={6}>
                        {formEditMode > 1 && <Box display='flex' justifyContent='space-between'>
                            <Typography sx={{p: 2}} variant='h4'> {payment && payment?.paymentId} </Typography>
                        </Box>}
                    </Grid>
                    <Grid item xs={6}>
                        <div className="col-md-6">
                            <Menu onSelect={handleMenuSelect}>
                                {formEditMode === 2 && <MenuItem text="Approve Payment"/>}
                                {/* <MenuItem text="Duplicate Payment"/> */}
                                {formEditMode > 1 && <MenuItem text="New Payment"/>}
                                {formEditMode === 3 && <MenuItem text="Complete Payment"/>}
                            </Menu>
                        </div>
                    </Grid>
                </Grid>

                <Form
                    ref={formRef}
                    initialValues={payment}
                    key={payment && payment.paymentId?.concat(payment.statusDescription!)}
                    onSubmitClick={values => handleSubmitData(values)}
                    render={(formRenderProps) => (

                        <FormElement>
                            <fieldset className={'k-form-fieldset'}>
                                <Grid container spacing={2} alignItems="flex-end"
                                >
                                    <Grid item xs={2}>
                                        <Field
                                            id={'paymentId'}
                                            name={'paymentId'}
                                            label={'Payment Number *'}
                                            component={FormInput}
                                            autoComplete={"off"}
                                            disabled={true}
                                        />
                                    </Grid>


                                    <Grid item xs={6}>
                                        <Button color={"secondary"} onClick={() => {
                                            setShowList(true);
                                        }} variant="contained"
                                                disabled={(formEditMode > 2 && formEditMode < 5)}>
                                            Payment Adjustments
                                        </Button>
                                    </Grid>

                                    <Grid item xs={2}>
                                        <Field
                                            id={'statusDescription'}
                                            name={'statusDescription'}
                                            label={'Status'}
                                            component={FormInput}
                                            autoComplete={"off"}
                                            disabled={true}
                                        />
                                    </Grid>

                                </Grid>

                                {/*<Grid item xs={3}>
                                    <Grid item xs={2}>
                                        <Grid item xs={2}>
                                            <Typography sx={{p: 2}} variant='h4'>{ payment&&payment.statusDescription} </Typography>
                                        </Grid>
                                    </Grid>
                                </Grid>*/}

                                <Grid item xs={3}>
                                    <Field
                                        id={"fromPartyId"}
                                        name={"fromPartyId"}
                                        label={"Customer"}
                                        component={FormComboBoxVirtualCustomer}
                                        autoComplete={"off"}
                                        validator={requiredValidator}
                                        disabled={formEditMode > 2}
                                    />
                                </Grid>

                                <Grid container justifyContent="center">
                                    <Grid item xs={10}>
                                        <PaymentApplicationsList paymentFormEditMode={formEditMode}/>
                                    </Grid>
                                </Grid>
                                <Grid container alignItems="flex-end" direction={"column"}>
                                    <Grid item xs={3}>
                                        <Grid container>
                                            <Typography sx={{p: 0}} variant='h6'>Sub Total </Typography>
                                            <Typography sx={{color: 'red', pl: 1}}
                                                        variant='h6'> {totalPaymentApplicationsValue} </Typography>
                                        </Grid>

                                    </Grid>

                                    <Grid item xs={3}>
                                        <Grid container>
                                            <Typography sx={{p: 0}} variant='h6'>Grand Total </Typography>
                                            <Typography sx={{color: 'red', pl: 1}}
                                                        variant='h6'> {totalPaymentApplicationsValue} </Typography>
                                        </Grid>

                                    </Grid>
                                </Grid>


                                <Grid container spacing={1}>
                                    <Grid item xs={1}>
                                        <Button
                                            sx={{m: 1}}
                                            variant="contained"
                                            type={'submit'}
                                            color="success"
                                            disabled={!enableDisableSubmitButton(formRenderProps.valueGetter('allowSubmit'), formRenderProps.allowSubmit) || buttonFlag || formEditMode > 2}
                                        >
                                            Submit
                                        </Button>
                                    </Grid>
                                    <Grid item xs={1}>
                                        <Button sx={{m: 1}} onClick={cancelEdit} color="error" variant="contained">
                                            Cancel
                                        </Button>
                                    </Grid>

                                </Grid>


                                {buttonFlag && <LoadingComponent message='Processing Payment...'/>}
                                <input type="hidden" name="allowSubmit" value="false"/>

                            </fieldset>

                        </FormElement>

                    )}
                />

            </Paper>


        </>

    );
}

