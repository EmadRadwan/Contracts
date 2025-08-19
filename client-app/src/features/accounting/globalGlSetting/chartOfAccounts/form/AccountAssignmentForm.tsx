import React, {useEffect} from "react";
import ReactDOM from "react-dom";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import {CSSTransition} from "react-transition-group";
import {Button, Grid, Paper} from "@mui/material";
import {MemoizedFormDropDownList} from "../../../../../app/common/form/MemoizedFormDropDownList";
import {requiredValidator} from "../../../../../app/common/form/Validators";
import FormDatePicker from "../../../../../app/common/form/FormDatePicker";

interface Props {
    show: boolean
    selectedAccount?: any
    onClose: () => void
    onSubmit: (assignment: any) => void
    width: number
}

const AccountAssignmentForm = ({show, selectedAccount, width, onClose, onSubmit}: Props) => {

    const accounts = [
        {name: "Accounts Payable", id: "100000"},
        {name: "Accounts Receivable", id: "100001"},
        {name: "Loans Payable", id: "100002"},
        {name: "General Checking Account", id: "100003"},
        {name: "Petty Cash", id: "100004"},
    ]

    const parties = [
        {fromPartyId: "200000", fromPartyName: "Dev Team 1"},
        {fromPartyId: "200001", fromPartyName: "R & D"},
        {fromPartyId: "200002", fromPartyName: "Human Resources"},
        {fromPartyId: "200003", fromPartyName: "Legal"},
        {fromPartyId: "200004", fromPartyName: "Sales"},
    ]

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
    }, [closeOnEscapeKeyDown]);

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
                            initialValues={{}}
                            // key={formRef.current.toString()}
                            onSubmitClick={values => onSubmit(values)}
                            render={(formRenderProps) => (
                                <FormElement>
                                    <fieldset className={'k-form-fieldset'}>
                                        <Grid container spacing={2} direction={"column"} paddingLeft={2}
                                              paddingRight={2}>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"accountId"}
                                                    name={"accountId"}
                                                    label={"Account Number *"}
                                                    autoComplete={"off"}
                                                    dataItemKey={"id"}
                                                    textField={"name"}
                                                    validator={requiredValidator}
                                                    component={MemoizedFormDropDownList}
                                                    data={accounts}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"partyId"}
                                                    name={"partyId"}
                                                    label={"Party *"}
                                                    autoComplete={"off"}
                                                    dataItemKey={"fromPartyId"}
                                                    textField={"fromPartyName"}
                                                    validator={requiredValidator}
                                                    component={MemoizedFormDropDownList}
                                                    data={parties}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"fromDate"}
                                                    name={"fromDate"}
                                                    label={"From Date *"}
                                                    autoComplete={"off"}
                                                    validator={requiredValidator}
                                                    component={FormDatePicker}

                                                />
                                            </Grid>
                                        </Grid>
                                    </fieldset>

                                    <div className="k-form-buttons">
                                        <Grid container spacing={3} paddingLeft={2}>
                                            <Grid item xs={2}>
                                                <Button
                                                    variant="contained"
                                                    type={'submit'}
                                                    color="success"
                                                >
                                                    Submit
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

export default AccountAssignmentForm