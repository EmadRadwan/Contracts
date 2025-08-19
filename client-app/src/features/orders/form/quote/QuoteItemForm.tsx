import React, {useCallback} from "react";
import {Field, Form, FormElement, KeyValue} from "@progress/kendo-react-form";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import useQuoteItem from "../../hook/useQuoteItem";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";
import {requiredValidator} from "../../../../app/common/form/Validators";
import {
    FormMultiColumnComboBoxVirtualQuoteServiceProduct
} from "../../../../app/common/form/FormMultiColumnComboBoxVirtualQuoteServiceProduct";
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import {QuoteItem} from "../../../../app/models/order/quoteItem";
import {setIsSelectedPriceZero} from "../../slice/quoteItemsUiSlice";
import {setProductId} from "../../slice/sharedOrderUiSlice";
import LoadingButton from "@mui/lab/LoadingButton";
import { Typography } from "@mui/material";
import { FormMultiColumnComboBoxVirtualSalesProduct } from "../../../../app/common/form/FormMultiColumnComboBoxVirtualSalesProduct";

interface Props {
    quoteItem?: any;
    editMode: number;
    onClose: () => void;
    quoteFormEditMode: number;
}

export default function QuoteItemForm({
                                          quoteItem,
                                          editMode,
                                          onClose,
                                          quoteFormEditMode,
                                      }: Props) {
    const MyForm = React.useRef<any>();
    const [formKey, setFormKey] = React.useState(1);
    const [selectedProduct, setSelectedProduct] = React.useState(undefined)

    const {
        productPromotions,
        handleSubmitData,
        productPromotionsWithEmpty,
        processQuoteItemLoading,
        processQuoteItemFetching,
        processQuoteItemError,
        processQuoteItemData,
    } = useQuoteItem({editMode, quoteItem, setFormKey});
    const dispatch = useAppDispatch();

    const [loading, setLoading] = React.useState(processQuoteItemLoading || processQuoteItemFetching);

    React.useEffect(() => {
        setLoading(processQuoteItemLoading || processQuoteItemFetching);
    }, [processQuoteItemLoading, processQuoteItemFetching]);
    
    if (editMode === 2) {
        dispatch(setProductId(quoteItem.productId.productId));
    }

    const {selectProductOrService} = useAppSelector(
        (state) => state.sharedOrderUi
    );
    // const isServicePriceZero = useAppSelector(
    //     (state) => state.quoteItemsUi.isSelectedServicePriceZero,
    // );


    const productValidator = (values: any): KeyValue<string> | undefined => {
        if (
            values.productId?.productTypeId === "MARKETING_PKG" ||
            values.productId?.productTypeId === "SERVICE_PRODUCT"
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
        if (selectProductOrService?.productTypeId !== "MARKETING_PKG") {
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
            } else {
                setSelectedProduct(product)
                if (product?.listPrice) {
                    MyForm.current.onChange("price", {value: product?.listPrice})
                }
            }

            if (product?.productTypeId === "SERVICE_PRODUCT" && product?.price === 0) {
                dispatch(setIsSelectedPriceZero(true));
            } else if (product?.productTypeId === "SERVICE_PRODUCT" && product?.price > 0) {
                dispatch(setIsSelectedPriceZero(false));
            }
        },
        [dispatch],
    );

    return (
        <React.Fragment>
            <Form
                ref={MyForm}
                initialValues={processQuoteItemData && processQuoteItemData!.status === 'Success' ? undefined : quoteItem}
                validator={productValidator}
                key={formKey}
                onSubmit={(values) => handleSubmitData(values as QuoteItem)}
                render={(formRenderProps) => (
                    <FormElement>
                        {console.log("MyForm:", MyForm.current)}
                        {processQuoteItemError && (
                            <div className={"k-messagebox k-messagebox-error"}>
                                {processQuoteItemError}
                            </div>
                        )}
                        {formRenderProps.visited &&
                            formRenderProps.errors &&
                            formRenderProps.errors.VALIDATION_SUMMARY && (
                                <div className={"k-messagebox k-messagebox-error"}>
                                    {formRenderProps.errors.VALIDATION_SUMMARY}
                                </div>
                            )}
                        <fieldset className={"k-form-fieldset"}>
                            <Grid container spacing={2} alignItems={"flex-end"}>
                                <Grid item xs={12}>
                                    {" "}
                                    {/* Adjusted to 8 to accommodate the button */}
                                    <Field
                                        id={"productId"}
                                        name={"productId"}
                                        label={"Product"}
                                        component={
                                            FormMultiColumnComboBoxVirtualSalesProduct
                                        }
                                        autoComplete={"off"}
                                        onChange={onProductChange}
                                        validator={requiredValidator}
                                        disabled={editMode === 2}
                                        style={{width: "100%"}}
                                    />
                                </Grid>
                                <Grid item xs={6}>
                                    {selectedProduct && (
                                        <Grid item xs={12}><div>List Price: {selectedProduct?.listPrice}</div></Grid>
                                    )}
                                </Grid>

                                {/* {isServicePriceZero && (
                                    <Grid item xs={4}>
                                        <Button
                                            variant="contained"
                                            onClick={() => setShowNewRateSpecification(true)}
                                        >
                                            Define Service Rate
                                        </Button>
                                    </Grid>
                                )} */}
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

                            <Grid item container xs={12} spacing={2} alignItems={"flex-end"}>
                                <Grid item xs={9}>
                                    <Field
                                        id={"quantity"}
                                        format="n0"
                                        min={1}
                                        name={"quantity"}
                                        label={"Quantity *"}
                                        component={FormNumericTextBox}
                                        validator={quantityValidator}
                                        disabled={
                                            quoteFormEditMode > 2 ||
                                            selectProductOrService?.productTypeId === "MARKETING_PKG"
                                        }
                                    />
                                </Grid>
                                {selectedProduct && (
                                    <Grid item xs={3}>
                                        <Typography variant="h6" color={"blue"} fontWeight={"bold"}>{selectedProduct?.uomDescription!}</Typography>
                                    </Grid>
                                )}
                            </Grid>
                            <Grid item xs={9}>
                                    <Field
                                        id={"price"}
                                        format="n1"
                                        // min={1}
                                        name={"price"}
                                        label={"Price *"}
                                        component={FormNumericTextBox}
                                        disabled={
                                            quoteFormEditMode > 2 ||
                                            selectProductOrService?.productTypeId === "MARKETING_PKG"
                                        }
                                    />
                                </Grid>

                            <div className="k-form-buttons">
                                <Grid container rowSpacing={2}>
                                    <Grid item xs={5}>
                                        <LoadingButton
                                            size="large"
                                            type={"submit"}
                                            loading={loading}
                                            variant="outlined"
                                            disabled={!formRenderProps.allowSubmit}
                                        >
                                            {loading ? "Processing" : (editMode === 2 ? "Update" : "Add")}
                                        </LoadingButton>
                                    </Grid>
                                    <Grid item xs={2}>
                                        <Button
                                            onClick={() => {
                                                onClose();
                                            }}
                                            size="large"
                                            color="error"
                                            variant="outlined"
                                        >
                                            <span>Cancel</span>
                                        </Button>
                                    </Grid>
                                </Grid>
                            </div>
                        </fieldset>
                    </FormElement>
                )}
            />
        </React.Fragment>
    );
}

export const QuoteItemFormMemo = React.memo(QuoteItemForm);
