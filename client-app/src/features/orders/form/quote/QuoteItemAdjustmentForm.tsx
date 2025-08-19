import React, {useEffect, useState} from "react";
import {v4 as uuid} from "uuid";
import {Field, Form, FormElement} from "@progress/kendo-react-form";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {useSelector} from "react-redux";
import {QuoteAdjustment} from "../../../../app/models/order/quoteAdjustment";
import {setUiQuoteAdjustments} from "../../slice/quoteAdjustmentsUiSlice";
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import {requiredValidator} from "../../../../app/common/form/Validators";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import {quoteItemSubTotal} from "../../slice/quoteSelectors";
import {useFetchQuoteAdjustmentTypesQuery} from "../../../../app/store/apis";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";

interface Props {
    quoteItem?: any;
    quoteAdjustment?: any;
    editMode: number;
    show: boolean;
    onClose: () => void;
    width?: number;
}

export default function QuoteItemAdjustmentForm({
                                                    quoteItem,
                                                    quoteAdjustment,
                                                    editMode,
                                                    show,
                                                    onClose,
                                                    width,
                                                }: Props) {
    console.log("width", width);
    const [oAdjustment, setOAdjustment] = useState(quoteAdjustment);

    const {data: quoteAdjustmentTypesData} =
        useFetchQuoteAdjustmentTypesQuery(undefined);
    const [buttonFlag, setButtonFlag] = useState(false);
    const dispatch = useAppDispatch();
    const {user} = useAppSelector((state) => state.account);
    const roles = [...user!.roles!];
    const roleWithPercentage = roles
        .sort((a, b) => b.PercentageAllowed - a.PercentageAllowed)
        .find((role) => role.Name.includes("AddAdjustments"));
    const MyForm = React.useRef<any>();
    const itemSubTotal: any = useSelector(quoteItemSubTotal);
    const closeOnEscapeKeyDown = (e: any) => {
        if ((e.charCode || e.keyCode) === 27) {
            onClose();
        }
    };
    const defaultQuoteAdjustmentType = quoteAdjustmentTypesData?.find(
        (x: any) => x.description === "Discount",
    );

    useEffect(() => {
        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, []);

    useEffect(() => {
        const initialFormValues: QuoteAdjustment = {
            quoteAdjustmentId: "",
            quoteAdjustmentTypeId: defaultQuoteAdjustmentType
                ? defaultQuoteAdjustmentType?.quoteAdjustmentTypeId
                : "",
        };

        if (quoteAdjustment !== undefined && editMode === 2) {
            setOAdjustment(quoteAdjustment);
        }

        if (quoteAdjustmentTypesData && editMode === 1) {
            // set default values for new record using oAdjustment state by selecting Discount as default from quoteAdjustmentTypesData
            // and update oAdjustment state
            setOAdjustment(initialFormValues);
        }
    }, [
        defaultQuoteAdjustmentType,
        editMode,
        quoteAdjustment,
        quoteAdjustmentTypesData,
    ]);

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        let newQuoteAdjustment: QuoteAdjustment;
        try {
            if (editMode === 2) {
                newQuoteAdjustment = {
                    ...oAdjustment,
                    ...data,
                };
            } else {
                newQuoteAdjustment = {
                    ...data,
                    quoteAdjustmentId: uuid(),
                    amount: data.amount,
                    correspondingProductId: quoteItem.productId,
                    correspondingProductName: quoteItem.productName,
                    quoteAdjustmentTypeDescription: quoteAdjustmentTypesData!.find(
                        (x: any) => x.quoteAdjustmentTypeId === data.quoteAdjustmentTypeId,
                    )?.description,
                    isAdjustmentDeleted: false,
                    quoteAdjustmentTypeId: data.quoteAdjustmentTypeId,
                    quoteId: quoteItem.quoteId,
                    quoteItemSeqId: quoteItem.quoteItemSeqId,
                    isManual: "Y",
                };
            }

            // send adjustment to parent
            dispatch(setUiQuoteAdjustments([newQuoteAdjustment]));
        } catch (error) {
            console.log(error);
        }

        onClose();

        setButtonFlag(false);
    }

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

            const newPercentage = (event.value / itemSubTotal) * 100;
            MyForm.current.onChange("sourcePercentage", {
                value: newPercentage.toFixed(2),
            });
        },
        [itemSubTotal],
    );

    const onPercentageChange = React.useCallback(
        (event) => {
            if (!event.value) return;
            const newAmount = (event.value / 100) * itemSubTotal;
            MyForm.current.onChange("amount", {value: newAmount.toFixed(2)});
        },
        [itemSubTotal],
    );

    return ReactDOM.createPortal(
        <CSSTransition in={show} unmountOnExit timeout={{enter: 0, exit: 300}}>
            <div className="modal">
                <div
                    className="modal-content"
                    style={{width: width}}
                    onClick={(e) => e.stopPropagation()}
                >
                    <div className="div-container-withBorderBoxBlack">
                        <Form
                            initialValues={oAdjustment}
                            key={JSON.stringify(oAdjustment)}
                            ref={MyForm}
                            onSubmit={(values) => handleSubmitData(values as QuoteAdjustment)}
                            render={(formRenderProps) => (
                                <FormElement>
                                    <fieldset className={"k-form-fieldset"}>
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
                                            id={"amount"}
                                            format="n0"
                                            name={"amount"}
                                            min={0.1}
                                            label={"Adjustment Amount *"}
                                            component={FormNumericTextBox}
                                            onChange={onAmountChange}
                                            validator={requiredValidator}
                                        />

                                        <Field
                                            id={"sourcePercentage"}
                                            format="n2"
                                            name={"sourcePercentage"}
                                            label={"Adjustment Percent *"}
                                            component={FormNumericTextBox}
                                            validator={percentageValidator}
                                            onChange={onPercentageChange}
                                        />

                                        <div className="k-form-buttons">
                                            <Grid container rowSpacing={2}>
                                                <Grid item xs={2}>
                                                    <Button
                                                        variant="contained"
                                                        type={"submit"}
                                                        disabled={
                                                            !formRenderProps.allowSubmit || buttonFlag
                                                        }
                                                    >
                                                        {editMode === 2 ? "Update" : "Add"}
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
        document.getElementById("root")!,
    );
}
