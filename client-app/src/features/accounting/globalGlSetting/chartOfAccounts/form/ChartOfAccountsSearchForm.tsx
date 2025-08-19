import {Button, Grid, Paper} from "@mui/material";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import React, {useEffect} from "react";
import ReactDOM from "react-dom";
import {FormInput} from "semantic-ui-react";
import {CSSTransition} from "react-transition-group";


interface Props {
    params: any
    show: boolean
    width: number
    onClose: () => void
    onSubmit: (params: any) => void
}

const ChartOfAccountsSearchForm = ({params, width, onSubmit, show, onClose}: Props) => {
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

    const sortOptions = [
        {value: 'accountNumAsc', label: 'Account Number Asc'},
        {value: 'AccountNumDesc', label: 'Account Number Desc'},
    ]

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
                                        <Grid container spacing={2} direction={"row"} paddingLeft={2} paddingRight={2}>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"id"}
                                                    name={"id"}
                                                    label={"Account Number"}
                                                    autoComplete={"off"}
                                                    component={FormInput}
                                                />
                                            </Grid>
                                            <Grid item xs={6}>
                                                <Field
                                                    id={"name"}
                                                    name={"name"}
                                                    label={"Name"}
                                                    autoComplete={"off"}
                                                    component={FormInput}
                                                />
                                            </Grid>
                                        </Grid>
                                    </fieldset>

                                    <div className="k-form-buttons">
                                        <Grid container paddingLeft={2}>
                                            <Grid item xs={2}>
                                                <Button
                                                    variant="contained"
                                                    type={'submit'}
                                                    color="success"
                                                >
                                                    Find
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

export default ChartOfAccountsSearchForm