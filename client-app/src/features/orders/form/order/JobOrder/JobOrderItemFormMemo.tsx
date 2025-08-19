import React, {useCallback, useEffect} from "react";
import {Field, Form, FormElement, KeyValue} from "@progress/kendo-react-form";
import ReactDOM from "react-dom";
import {CSSTransition} from "react-transition-group";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import useJobOrderItem from "../../../hook/useJobOrderItem";
import {useAppDispatch, useAppSelector,} from "../../../../../app/store/configureStore";
import {requiredValidator} from "../../../../../app/common/form/Validators";
import {setIsSelectedPriceZero} from "../../../../services/slice/quoteUiSlice";
import {OrderItem} from "../../../../../app/models/order/orderItem";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import {Paper} from "@mui/material";
import {
    FormMultiColumnComboBoxVirtualJobOrderServiceProduct
} from "../../../../../app/common/form/FormMultiColumnComboBoxVirtualJobOrderServiceProduct";
import {MemoizedFormDropDownList} from "../../../../../app/common/form/MemoizedFormDropDownList";

interface Props {
    orderItem?: any;
    editMode: number;
    show: boolean;
    onClose: () => void;
    setShowNewRateSpecification: (show: boolean) => void;
    orderFormEditMode: number;
    skip: boolean;
    width?: number;
}

export default function JobOrderItemForm({
                                             orderItem,
                                             editMode,
                                             show,
                                             onClose,
                                             orderFormEditMode,
                                             skip,
                                             width,
                                             setShowNewRateSpecification,
                                         }: Props) {
    const MyForm = React.useRef<any>();

    const {
        oItem,
        productPromotions,
        handleSubmitData,
        productPromotionsWithEmpty,
    } = useJobOrderItem({editMode, skip, orderItem});

    const selectedProduct = useAppSelector(
        (state) => state.jobOrderUi.selectedProduct,
    );
    const isServicePriceZero = useAppSelector(
        (state) => state.jobOrderUi.isSelectedServicePriceZero,
    );
    console.log("orderItem", orderItem);

    const dispatch = useAppDispatch();

    useEffect(() => {
        const closeOnEscapeKeyDown = (e: any) => {
            if ((e.charCode || e.keyCode) === 27) {
                //orderItemProduct = undefined;
                onClose();
            }
        };

        document.body.addEventListener("keydown", closeOnEscapeKeyDown);
        return function cleanup() {
            document.body.removeEventListener("keydown", closeOnEscapeKeyDown);
        };
    }, [onClose]);

    const productValidator = (values: any): KeyValue<string> | undefined => {
        if (
            values.productId?.productTypeId === "MARKETING_PKG" ||
            values.productId?.productTypeId === "SERVICE"
        ) {
            return;
        }
        const msgQuantityGreaterThanATP: KeyValue<string> = {
            VALIDATION_SUMMARY: "Quantity is greater than ATP.",
        };
        if (
            Object.keys(values).length > 0 &&
            values.productId != null &&
            values.quantity > 0
        ) {
            if (values.quantity > values.productId.availableToPromiseTotal) {
                return msgQuantityGreaterThanATP;
            }
        }

        return;
    };

    const quantityValidator = (value: any) => {
        if (selectedProduct?.productTypeId !== "MARKETING_PKG") {
            return requiredValidator(value);
        }
        return "";
    };

    const onProductChange = useCallback(
        (event) => {
            const product = event.value;
            console.log("product", product);
            if (product?.productTypeId === "MARKETING_PKG") {
                MyForm.current.onChange("quantity", {value: 1});
            }

            if (product?.productTypeId === "SERVICE" && product?.price === 0) {
                dispatch(setIsSelectedPriceZero(true));
            } else if (product?.productTypeId === "SERVICE" && product?.price > 0) {
                dispatch(setIsSelectedPriceZero(false));
            }
        },
        [dispatch],
    );

    return ReactDOM.createPortal(
        <CSSTransition in={show} unmountOnExit timeout={{enter: 0, exit: 300}}>
            <div className="modal">
                <div
                    className="modal-content"
                    style={{width: width}}
                    onClick={(e) => e.stopPropagation()}
                >
                    <Paper elevation={5} className={`div-container-withBorderCurved`}>
                        <Form
                            ref={MyForm}
                            initialValues={oItem}
                            validator={productValidator}
                            key={JSON.stringify(oItem)}
                            onSubmit={(values) => handleSubmitData(values as OrderItem)}
                            render={(formRenderProps) => (
                                <FormElement>
                                    {formRenderProps.visited &&
                                        formRenderProps.errors &&
                                        formRenderProps.errors.VALIDATION_SUMMARY && (
                                            <div className={"k-messagebox k-messagebox-error"}>
                                                {formRenderProps.errors.VALIDATION_SUMMARY}
                                            </div>
                                        )}
                                    <fieldset className={"k-form-fieldset"}>
                                        <Grid container spacing={2} alignItems={"flex-end"}>
                                            <Grid item xs={isServicePriceZero ? 8 : 12}>
                                                {" "}
                                                {/* Adjusted to 8 to accommodate the button */}
                                                <Field
                                                    id={"productId"}
                                                    name={"productId"}
                                                    label={"Product"}
                                                    component={
                                                        FormMultiColumnComboBoxVirtualJobOrderServiceProduct
                                                    }
                                                    autoComplete={"off"}
                                                    onChange={onProductChange}
                                                    validator={requiredValidator}
                                                    disabled={editMode === 2}
                                                    style={{width: "100%"}}
                                                />
                                            </Grid>

                                            {isServicePriceZero && (
                                                <Grid item xs={4}>
                                                    <Button
                                                        variant="contained"
                                                        onClick={() => setShowNewRateSpecification(true)}
                                                    >
                                                        Define Service Rate
                                                    </Button>
                                                </Grid>
                                            )}
                                        </Grid>

                                        {productPromotions &&
                                            productPromotions.length > 0 &&
                                            formRenderProps.valueGetter("productId") !==
                                            undefined && (
                                                <Grid item xs={7}>
                                                    <Field
                                                        id={"productPromoId"}
                                                        name={"productPromoId"}
                                                        label={"Promotions"}
                                                        component={MemoizedFormDropDownList}
                                                        dataItemKey={"productPromoId"}
                                                        textField={"promoText"}
                                                        data={productPromotionsWithEmpty || []}
                                                        disabled={editMode === 2}
                                                    />
                                                </Grid>
                                            )}

                                        <Field
                                            id={"quantity"}
                                            format="n0"
                                            min={1}
                                            name={"quantity"}
                                            label={"Quantity *"}
                                            component={FormNumericTextBox}
                                            validator={quantityValidator}
                                            disabled={
                                                orderFormEditMode > 2 ||
                                                selectedProduct?.productTypeId === "MARKETING_PKG" ||
                                                isServicePriceZero
                                            }
                                        />

                                        <div className="k-form-buttons">
                                            <Grid container rowSpacing={2}>
                                                <Grid item xs={3}>
                                                    <Button
                                                        variant="contained"
                                                        type={"submit"}
                                                        color="success"
                                                        disabled={!formRenderProps.allowSubmit}
                                                    >
                                                        {editMode === 2 ? "Update" : "Add"}
                                                    </Button>
                                                </Grid>
                                                <Grid item xs={2}>
                                                    <Button
                                                        onClick={() => {
                                                            onClose();
                                                        }}
                                                        variant="contained"
                                                        color="error"
                                                    >
                                                        Cancel
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
        document.getElementById("root")!,
    );
}

export const JobOrderItemFormMemo = React.memo(JobOrderItemForm);
