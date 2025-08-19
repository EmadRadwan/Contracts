import React from 'react'
import ReactDOM from "react-dom";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import {CSSTransition} from "react-transition-group";
import {Button, Grid, Paper} from "@mui/material";
import {requiredValidator} from "../../../../../app/common/form/Validators";
import FormInput from '../../../../../app/common/form/FormInput';
import {useFetchGlobalGlAccountsQuery} from '../../../../../app/store/configureStore';
import {FormComboBox} from '../../../../../app/common/form/FormComboBox';

interface Props {
    show: boolean
    selectedPaymentMethod?: any
    onClose: () => void
    onSubmit: (assignment: any) => void
    width: number
}

const PaymentMethodTypeForm = ({show, selectedPaymentMethod, width, onClose, onSubmit}: Props) => {
    const {data: accounts} = useFetchGlobalGlAccountsQuery(undefined)
    // const defaultAccountsFromUi = useSelector(defaultAccountsSelector)
    // console.log(defaultAccountsFromUi)
    const defaultAccounts = accounts?.map((account: any) => {
        return {
            defaultGlAccountId: account.glAccountId,
            accountName: account.accountName
        }
    })


    const handleSubmit = (data: any) => {
        if (!data.isValid) return
        data.values.paymentMethodTypeId = selectedPaymentMethod.paymentMethodTypeId
        console.log(data.values)
        onSubmit(data.values)
    }

    return ReactDOM.createPortal(
        <CSSTransition
            in={show}
            unmountOnExit
            timeout={{enter: 0, exit: 300}}
        >
            <div className="modal">
                <div className="modal-content" style={{width}} onClick={e => e.stopPropagation()}>
                    <Paper elevation={5} className={`div-container-withBorderCurved`}>
                        <Form
                            initialValues={selectedPaymentMethod}
                            // key={formRef.current.toString()}
                            onSubmitClick={values => handleSubmit(values)}
                            render={(formRenderProps) => (
                                <FormElement>
                                    <fieldset className={'k-form-fieldset'}>
                                        <Grid container spacing={2} direction={"column"} paddingLeft={2}
                                              paddingRight={2}>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"paymentMethodTypeDescription"}
                                                    name={"paymentMethodTypeDescription"}
                                                    label={"Payment Method Type *"}
                                                    autoComplete={"off"}
                                                    disabled
                                                    component={FormInput}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"defaultGlAccountId"}
                                                    name={"defaultGlAccountId"}
                                                    label={"Default Gl Account *"}

                                                    dataItemKey={"glAccountId"}
                                                    textField={"accountName"}
                                                    validator={requiredValidator}
                                                    // autoComplete={"off"}
                                                    component={FormComboBox}
                                                    data={defaultAccounts ? defaultAccounts : []}
                                                />
                                            </Grid>

                                        </Grid>
                                    </fieldset>

                                    <div className="k-form-buttons">
                                        <Grid container spacing={3} paddingLeft={2}>
                                            <Grid item xs={3}>
                                                <Button
                                                    variant="contained"
                                                    type={'submit'}
                                                    color="success"
                                                >
                                                    Update
                                                </Button>
                                            </Grid>
                                            <Grid item xs={2}>
                                                <Button onClick={() => onClose()}
                                                        color="error"
                                                        variant="contained"
                                                >
                                                    Cancel
                                                </Button>
                                            </Grid>
                                        </Grid>
                                    </div>
                                </FormElement>
                            )}
                        />
                    </Paper>
                </div>
            </div>

        </CSSTransition>,
        document.getElementById("root")!
    )
}

export default PaymentMethodTypeForm