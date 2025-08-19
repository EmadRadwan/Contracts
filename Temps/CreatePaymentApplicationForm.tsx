import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";
import usePayments from "../../../../app/hooks/usePayments";
import React, {useEffect, useState} from "react";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import {
    FormMultiColumnComboBoxVirtualSalesProduct
} from "../../../../app/common/form/FormMultiColumnComboBoxVirtualSalesProduct";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {requiredValidator} from "../../../../app/common/form/Validators";
import {paymentApplicationsSelectors, setSinglePayment, setSinglePaymentApplication} from "../slice/singlePaymentSlice";
import {PaymentApplication} from "../../../../app/models/accounting/paymentApplication";
import {Paper} from "@mui/material";

interface Props {
    paymentApplication?: any;
    editMode: number;
    show: boolean;
    onClose: () => void;
    paymentFormEditMode: number
}

export default function CreatePaymentApplicationForm({
                                                         paymentApplication,
                                                         editMode,
                                                         show,
                                                         onClose,
                                                         paymentFormEditMode
                                                     }: Props) {

    const {selectedSinglePayment} = usePayments();
    const paymentApplications = useAppSelector(paymentApplicationsSelectors.selectAll);
    const nonDeletedItems = paymentApplications.filter(item => !item.isPaymentApplicationDeleted)


    const dispatch = useAppDispatch();

    const [buttonFlag, setButtonFlag] = useState(false);

    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };


    useEffect(() => {
        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, []);

    async function handleSubmitData(data: any) {
        setButtonFlag(true)
        try {
            let response: any;
            if (editMode === 2) {


                data.subTotal = ((data.quantity * data.productId.priceWithTax))
                data.unitPrice = data.subTotal

                dispatch(setSinglePaymentApplication(data));
            } else {
                let newPaymentApplication: PaymentApplication;
                /* newPaymentApplication = {
                     inventoryItemId: data.productId.inventoryItem,
                     isProductDeleted: false,
                     paymentApplicationSeqId: (paymentApplications.length + 1).toString().padStart(2, '0'),
                     productId: data.productId,
                     productName: data.productId.productName,
                     quantity: data.quantity,
                     unitListPrice: data.productId.priceWithTax,
 
                 };*/
                /*if (selectedSinglePayment) {
                    newPaymentApplication.paymentId = selectedSinglePayment.paymentId
                } else {
                    newPaymentApplication.paymentId = "PAYMENT-DUMMY"
                }*/


                /*newPaymentApplication.subTotal = ((newPaymentApplication.quantity! * newPaymentApplication.unitListPrice) )
                newPaymentApplication.unitPrice = newPaymentApplication.subTotal*/

                //dispatch(setSinglePaymentApplication(newPaymentApplication));
            }
            dispatch(setSinglePayment({...selectedSinglePayment, allowSubmit: true}));

            onClose();

        } catch (error) {
            console.log(error)
        }
        setButtonFlag(false)
    }


    /*const productValidator = (values: any): KeyValue<string> | undefined => {
        const msgProductAlreadyExist: KeyValue<string> = {VALIDATION_SUMMARY: "Product Already Exists."}
        const msgQuantityGreaterThanATP: KeyValue<string> = {VALIDATION_SUMMARY: "Quantity is greater than ATP."}
        let productAlreadyExist: boolean = false
        if (Object.keys(values).length > 0 && values.productId != null && editMode != 2) {
            nonDeletedItems.forEach((product) => {
                if (product.productId.productId === values.productId.productId) {
                    productAlreadyExist = true
                }
            })
        }
        
        if (Object.keys(values).length > 0 && values.productId != null && values.quantity > 0) {
            if (values.quantity > values.productId.quantityOnHandTotal){
                return msgQuantityGreaterThanATP
            }
        }

        if (productAlreadyExist) {
            return msgProductAlreadyExist
        }
        return
    };*/


    return ReactDOM.createPortal(
        <CSSTransition
            in={show}
            unmountOnExit
            timeout={{enter: 0, exit: 300}}
        >
            <div className="modal">
                <div className="modal-content" style={{width: 300}} onClick={e => e.stopPropagation()}>
                    <Paper elevation={5} className={`div-container-withBorderCurved`}>
                        <Form
                            initialValues={editMode === 2 ? paymentApplication : undefined}
                            //validator={productValidator}
                            onSubmit={values => handleSubmitData(values as PaymentApplication)}
                            render={(formRenderProps) => (

                                <FormElement>
                                    {formRenderProps.visited &&
                                        formRenderProps.errors &&
                                        formRenderProps.errors.VALIDATION_SUMMARY && (
                                            <div className={"k-messagebox k-messagebox-error"}>
                                                {formRenderProps.errors.VALIDATION_SUMMARY}
                                            </div>
                                        )}
                                    <fieldset className={'k-form-fieldset'}>

                                        <Field
                                            id={"productId"}
                                            name={"productId"}
                                            label={"Product"}
                                            component={FormMultiColumnComboBoxVirtualSalesProduct}
                                            autoComplete={"off"}
                                            validator={requiredValidator}
                                            disabled={editMode === 2}
                                        />

                                        <Field
                                            id={'quantity'}
                                            format="n0"
                                            min={1}
                                            name={'quantity'}
                                            label={'Quantity *'}
                                            component={FormNumericTextBox}
                                            validator={requiredValidator}
                                            disabled={paymentFormEditMode > 2}
                                        />

                                        <div className="k-form-buttons">
                                            <Grid container rowSpacing={2}>
                                                <Grid item xs={4}>
                                                    <Button
                                                        variant="contained"
                                                        type={'submit'}
                                                        color="success"
                                                        disabled={!formRenderProps.allowSubmit || buttonFlag}
                                                    >
                                                        {editMode === 2 ? 'Update' : 'Add'}
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={4}>
                                                    <Button onClick={() => onClose()} color="error" variant="contained">
                                                        Cancel
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={4}>
                                                    <Button onClick={() => {
                                                        const data = {...paymentApplication}
                                                        data.isProductDeleted = true
                                                        dispatch(setSinglePayment({
                                                            ...selectedSinglePayment,
                                                            allowSubmit: true
                                                        }));
                                                        dispatch(setSinglePaymentApplication(data))


                                                        onClose()
                                                    }} variant="outlined" color="secondary"
                                                            disabled={editMode != 2 || paymentFormEditMode > 2}
                                                    >
                                                        Remove
                                                    </Button>
                                                </Grid>

                                            </Grid>
                                        </div>

                                    </fieldset>

                                </FormElement>

                            )}
                        />
                    </Paper>

                </div>
            </div>
        </CSSTransition>,
        document.getElementById("root")!
    );
}