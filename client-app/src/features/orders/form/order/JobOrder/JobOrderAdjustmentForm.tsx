import React, {useEffect, useState} from "react";
import {v4 as uuid} from "uuid";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {useSelector} from "react-redux";
import {toast} from "react-toastify";
import {useFetchOrderAdjustmentTypesQuery} from "../../../../../app/store/apis";
import {useAppDispatch, useAppSelector} from "../../../../../app/store/configureStore";
import {jobOrderLevelAdjustments, jobOrderSubTotal, setUiJobOrderAdjustments} from "../../../slice/jobOrderUiSlice";
import {OrderAdjustment} from "../../../../../app/models/order/orderAdjustment";
import {MemoizedFormDropDownList} from "../../../../../app/common/form/MemoizedFormDropDownList";
import {requiredValidator} from "../../../../../app/common/form/Validators";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";

interface Props {
    jobOrderAdjustment?: any;
    editMode: number;
    show: boolean;
    onClose: () => void;
}

export default function JobOrderAdjustmentForm({
                                                   jobOrderAdjustment,
                                                   editMode,
                                                   show,
                                                   onClose
                                               }: Props) {


    const [oAdjustment, setOAdjustment] = useState(jobOrderAdjustment);

    const {data: orderAdjustmentTypesData} = useFetchOrderAdjustmentTypesQuery(undefined);
    const [buttonFlag, setButtonFlag] = useState(false);
    const dispatch = useAppDispatch();
    const uiJobOrderLevelAdjustments: any = useSelector(jobOrderLevelAdjustments)
    const {user} = useAppSelector(state => state.account);
    const roles = [...user!.roles!];
    const roleWithPercentage = roles.sort((a, b) => b.PercentageAllowed - a.PercentageAllowed).find(role => role.Name.includes('AddAdjustments'));
    const subTotal: any = useSelector(jobOrderSubTotal)


    const defaultOrderAdjustmentType = orderAdjustmentTypesData?.find((x: any) => x.description === 'Discount');


    const MyForm = React.useRef<any>()
    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };

    useEffect(() => {
        const initialFormValues: OrderAdjustment = {
            orderAdjustmentId: '',
            orderAdjustmentTypeId: defaultOrderAdjustmentType ? defaultOrderAdjustmentType?.orderAdjustmentTypeId : '',
        };

        if (jobOrderAdjustment !== undefined && editMode === 2) {
            setOAdjustment(jobOrderAdjustment);
        }

        if (orderAdjustmentTypesData && editMode === 1) {
            // set default values for new record using oAdjustment state by selecting Discount as default from orderAdjustmentTypesData
            // and update oAdjustment state
            setOAdjustment(initialFormValues);
        }

    }, [defaultOrderAdjustmentType, editMode, jobOrderAdjustment, orderAdjustmentTypesData]);


    useEffect(() => {
        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, []);


    const percentageValidator = (value: any) => {
        if (value === null || value === undefined || value === "") {
            return "Percentage is required";
        } else if (value < 0 || value > 100) {
            return "Percentage should be between 0 and 100";
        } else {
            return "";
        }
    };

    const onAmountChange = React.useCallback(
        (event) => {
            if (!event.value) return;

            const newPercentage = (event.value / subTotal) * 100;
            MyForm.current.onChange('sourcePercentage', {value: newPercentage});


        },
        [subTotal]
    );

    const onPercentageChange = React.useCallback(
        (event) => {
            if (!event.value) return;
            const newAmount = (event.value / 100) * subTotal;
            MyForm.current.onChange('amount', {value: newAmount});
        },
        [subTotal]
    );


    async function handleSubmitData(data: any) {
        // calculate the allowed percentage based on the role
        // property roleWithPercentage.AllowedPercentage
        // and return error if the sourcePercentage field is more than the allowed percentage
        if (roleWithPercentage && roleWithPercentage.PercentageAllowed) {
            if (data.sourcePercentage > roleWithPercentage.PercentageAllowed) {
                toast.error(`Percentage should be less than ${roleWithPercentage.PercentageAllowed} %`)
                return false;
            }
        }


        setButtonFlag(true)
        let newOrderAdjustment: OrderAdjustment;

        try {
            if (editMode === 2) {
                newOrderAdjustment = {
                    ...oAdjustment,
                    ...data,
                };
            } else {
                newOrderAdjustment = {
                    ...data,
                    orderAdjustmentId: uuid(),
                    orderId: "ORDER-DUMMY",
                    orderItemSeqId: "_NA_",
                    orderAdjustmentTypeDescription: orderAdjustmentTypesData!.find((x: any) => x.orderAdjustmentTypeId === data.orderAdjustmentTypeId)?.description,
                    isAdjustmentDeleted: false,
                    isManual: 'Y',
                };
            }
            // update orderAdjustments based on editMode
            if (editMode === 1) {
                if (uiJobOrderLevelAdjustments) {
                    dispatch(setUiJobOrderAdjustments([...uiJobOrderLevelAdjustments!, newOrderAdjustment]))
                } else {

                    dispatch(setUiJobOrderAdjustments([newOrderAdjustment]))
                }
            } else if (editMode === 2) {
                // edit existing orderItem
                const newOrderAdjustments = uiJobOrderLevelAdjustments?.map((item: OrderAdjustment) => {
                    if (item.orderAdjustmentId === item?.orderAdjustmentId) {
                        return newOrderAdjustment;
                    } else {
                        return item;
                    }
                });
                dispatch(setUiJobOrderAdjustments(newOrderAdjustments!));

            }

            onClose();


        } catch (error) {
            console.log(error)
        }
        setButtonFlag(false)
    }


    ////console.log('orderAdjustment', orderAdjustment);

    return ReactDOM.createPortal(
        <CSSTransition
            in={show}
            unmountOnExit
            timeout={{enter: 0, exit: 300}}
        >
            <div className="modal">
                <div className="modal-content" onClick={e => e.stopPropagation()}>
                    <div className="div-container-withBorderBoxBlue">
                        <Form
                            initialValues={oAdjustment}
                            key={JSON.stringify(oAdjustment)}
                            ref={MyForm}
                            onSubmit={values => handleSubmitData(values as OrderAdjustment)}
                            render={(formRenderProps) => (

                                <FormElement>
                                    <fieldset className={'k-form-fieldset'}>

                                        <Field
                                            id={"orderAdjustmentTypeId"}
                                            name={"orderAdjustmentTypeId"}
                                            label={"Adjustment Type *"}
                                            component={MemoizedFormDropDownList}
                                            dataItemKey={"orderAdjustmentTypeId"}
                                            textField={"description"}
                                            data={orderAdjustmentTypesData}
                                            validator={requiredValidator}
                                            disabled={editMode === 2}
                                        />


                                        <Field
                                            id={'amount'}
                                            format="n2"
                                            name={'amount'}
                                            label={'Adjustment Amount *'}
                                            component={FormNumericTextBox}
                                            min={.1}
                                            validator={requiredValidator}
                                            onChange={onAmountChange}
                                        />

                                        <Field
                                            id={'sourcePercentage'}
                                            format="n2"
                                            min={.1}
                                            name={'sourcePercentage'}
                                            label={'Adjustment Percent *'}
                                            component={FormNumericTextBox}
                                            validator={percentageValidator}
                                            onChange={onPercentageChange}
                                        />

                                        <div className="k-form-buttons">
                                            <Grid container rowSpacing={2}>
                                                <Grid item xs={2}>
                                                    <Button
                                                        variant="contained"
                                                        type={'submit'}
                                                        disabled={!formRenderProps.allowSubmit || buttonFlag}
                                                    >
                                                        {editMode === 2 ? 'Update' : 'Add'}
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={2}>
                                                    <Button onClick={() => onClose()} variant="contained">
                                                        Cancel
                                                    </Button>
                                                </Grid>


                                            </Grid>
                                        </div>

                                    </fieldset>

                                </FormElement>

                            )}
                        />
                    </div>

                </div>
            </div>
        </CSSTransition>,
        document.getElementById("root")!
    );
}
