import React, {useEffect, useState} from "react";
import {v4 as uuid} from "uuid";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {useSelector} from "react-redux";
import {toast} from "react-toastify";
import {useFetchQuoteAdjustmentTypesQuery} from "../../../../app/store/apis";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";
import {quoteLevelAdjustments, quoteSubTotal} from "../../slice/quoteSelectors";
import {QuoteAdjustment} from "../../../../app/models/order/quoteAdjustment";
import {setUiQuoteAdjustments} from "../../slice/quoteAdjustmentsUiSlice";
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import {requiredValidator} from "../../../../app/common/form/Validators";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";

interface Props {
    quoteAdjustment?: any;
    editMode: number;
    onClose: () => void;
}

export default function QuoteAdjustmentForm({
                                                quoteAdjustment,
                                                editMode,
                                                onClose,
                                            }: Props) {


    const [jQuoteAdjustment, setJQuoteAdjustment] = useState(quoteAdjustment);

    const {data: quoteAdjustmentTypesData} = useFetchQuoteAdjustmentTypesQuery(undefined);
    console.log('quoteAdjustmentTypesData', quoteAdjustmentTypesData);
    const [buttonFlag, setButtonFlag] = useState(false);
    const dispatch = useAppDispatch();
    const uiQuoteLevelAdjustments: any = useSelector(quoteLevelAdjustments);
    const {user} = useAppSelector(state => state.account);
    const roles = [...user!.roles!];
    const roleWithPercentage = roles.sort((a, b) => b.PercentageAllowed - a.PercentageAllowed).find(role => role.Name.includes('AddAdjustments'));
    const subTotal: any = useSelector(quoteSubTotal)


    const defaultQuoteAdjustmentType = quoteAdjustmentTypesData?.find((x: any) => x.description === 'Discount');
    console.log('defaultQuoteAdjustmentType', defaultQuoteAdjustmentType)


    const MyForm = React.useRef<any>()


    useEffect(() => {
        const initialFormValues: QuoteAdjustment = {
            quoteAdjustmentId: '',
            quoteAdjustmentTypeId: defaultQuoteAdjustmentType ? defaultQuoteAdjustmentType?.quoteAdjustmentTypeId : '',
        };

        if (quoteAdjustment !== undefined && editMode === 2) {
            setJQuoteAdjustment(quoteAdjustment);
        }

        if (quoteAdjustmentTypesData && editMode === 1) {
            setJQuoteAdjustment(initialFormValues);
        }

    }, [quoteAdjustmentTypesData, editMode, quoteAdjustment, quoteAdjustmentTypesData]);


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
            MyForm.current.onChange('sourcePercentage', {value: parseFloat(newPercentage.toFixed(2))});


        },
        [subTotal]
    );

    const onPercentageChange = React.useCallback(
        (event) => {
            if (!event.value) return;
            const newAmount = (event.value / 100) * subTotal;
            MyForm.current.onChange('amount', {value: parseFloat(newAmount.toFixed(2))});
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
        let newQuoteAdjustment: QuoteAdjustment;

        try {
            if (editMode === 2) {
                newQuoteAdjustment = {
                    ...jQuoteAdjustment,
                    ...data,
                };
            } else {
                newQuoteAdjustment = {
                    ...data,
                    quoteAdjustmentId: uuid(),
                    quoteId: "QUOTE-DUMMY",
                    quoteItemSeqId: "_NA_",
                    quoteAdjustmentTypeDescription: quoteAdjustmentTypesData!.find((x: any) => x.quoteAdjustmentTypeId === data.quoteAdjustmentTypeId)?.description,
                    isAdjustmentDeleted: false,
                    isManual: 'Y',
                };
            }
            if (editMode === 1) {
                if (uiQuoteLevelAdjustments) {
                    dispatch(setUiQuoteAdjustments([...uiQuoteLevelAdjustments!, newQuoteAdjustment]))
                } else {

                    dispatch(setUiQuoteAdjustments([newQuoteAdjustment]))
                }
            } else if (editMode === 2) {
                const newQuoteAdjustment = uiQuoteLevelAdjustments?.map((item: QuoteAdjustment) => {
                    if (item.quoteAdjustmentId === item?.quoteAdjustmentId) {
                        return newQuoteAdjustment;
                    } else {
                        return item;
                    }
                });
                dispatch(setUiQuoteAdjustments(newQuoteAdjustment!));

            }

            onClose();


        } catch (error) {
            console.log(error)
        }
        setButtonFlag(false)
    }


    return <React.Fragment>
        <Form
            initialValues={jQuoteAdjustment}
            key={JSON.stringify(jQuoteAdjustment)}
            ref={MyForm}
            onSubmit={values => handleSubmitData(values as QuoteAdjustment)}
            render={(formRenderProps) => (

                <FormElement>
                    <fieldset className={'k-form-fieldset'}>

                        <Field
                            id={"quoteAdjustmentTypeId"}
                            name={"quoteAdjustmentTypeId"}
                            label={"Adjustment Type *"}
                            component={MemoizedFormDropDownList}
                            dataItemKey={"quoteAdjustmentTypeId"}
                            textField={"description"}
                            data={quoteAdjustmentTypesData}
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
                                <Grid item xs={3}>
                                    <Button
                                        variant="contained"
                                        type={'submit'}
                                        color="success"
                                        disabled={!formRenderProps.allowSubmit || buttonFlag}
                                    >
                                        {editMode === 2 ? 'Update' : 'Add'}
                                    </Button>
                                </Grid>
                                <Grid item xs={2}>
                                    <Button onClick={() => onClose()} color="error" variant="contained">
                                        Cancel
                                    </Button>
                                </Grid>


                            </Grid>
                        </div>

                    </fieldset>

                </FormElement>

            )}
        />
    </React.Fragment>
}
