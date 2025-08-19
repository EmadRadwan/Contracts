import {Field, Form, FormElement, KeyValue} from "@progress/kendo-react-form";
import {OrderItem} from "../../../../../app/models/order/orderItem";
import {
    FormMultiColumnComboBoxVirtualSalesProduct
} from "../../../../../app/common/form/FormMultiColumnComboBoxVirtualSalesProduct";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import {requiredValidator} from "../../../../../app/common/form/Validators";
import {MemoizedFormDropDownList} from "../../../../../app/common/form/MemoizedFormDropDownList";
import useSalesOrderItem from "../../../hook/useSalesOrderItem";
import * as React from 'react';
import LoadingButton from "@mui/lab/LoadingButton";
import {setProductId} from "../../../slice/sharedOrderUiSlice";
import {useAppDispatch} from "../../../../../app/store/configureStore";
import {Typography} from "@mui/material";
import {useTranslationHelper} from "../../../../../app/hooks/useTranslationHelper";
import { useGetProductPriceQuery} from "../../../../../app/store/apis";


interface Props {
    orderItem?: any;
    editMode: number;
    onClose: () => void;
    orderFormEditMode: number;
}


function SalesOrderItemForm({
                                orderItem,
                                editMode,
                                onClose,
                                orderFormEditMode,
                            }: Props) {

    const [showQuantityWarning, setShowQuantityWarning] = React.useState(false);
    const [quantity, setQuantity] = React.useState<number | undefined>(orderItem?.quantity);

    const MyForm = React.useRef<any>();
    const dispatch = useAppDispatch();
    const [selectedProduct, setSelectedProduct] = React.useState(undefined)
    const localizationKey = "order.so.items.form"
    const {getTranslatedLabel} = useTranslationHelper()


    React.useEffect(() => {
        if (editMode === 2 && orderItem?.productId && !selectedProduct) {
            setSelectedProduct({
                productId: typeof orderItem.productId === 'string' ? orderItem.productId : orderItem.productId.productId,
                // Include other necessary fields if required by FormMultiColumnComboBoxVirtualSalesProduct
                productName: orderItem.productName,
                uomDescription: orderItem.uomDescription,
                availableToPromiseTotal: orderItem.availableToPromiseTotal,
                quantityOnHandTotal: orderItem.quantityOnHandTotal,
                colorDescription: orderItem.colorDescription, //  Added colorDescription to pre-populate in edit mode
                productFeatureId: orderItem.productFeatureId, //  Added productFeatureId for edit mode

            });
        }
    }, [editMode, orderItem, selectedProduct]);

    
    const { data: productPrice, isLoading: isPriceLoading } = useGetProductPriceQuery(
        selectedProduct?.productId,
        { skip: !selectedProduct?.productId }
    );
    

    React.useEffect(() => {
        if (productPrice && quantity && quantity < productPrice.quantityIncluded) {
            setShowQuantityWarning(true);
        } else {
            setShowQuantityWarning(false);
        }
    }, [productPrice, quantity]);

    const {
        productPromotions,
        handleSubmitData,
        productPromotionsWithEmpty,
        processOrderItemLoading,
        processOrderItemFetching,
        processOrderItemError,
        processOrderItemData,
    } = useSalesOrderItem({editMode, orderItem, productPrice});


    const [loading, setLoading] = React.useState(processOrderItemLoading || processOrderItemFetching);
    const [formKey, setFormKey] = React.useState<number>(Math.random());
    
    React.useEffect(() => {
        setLoading(processOrderItemLoading || processOrderItemFetching);
    }, [processOrderItemLoading, processOrderItemFetching]);

    React.useEffect(() => {
        if (processOrderItemData && processOrderItemData!.status === 'Success') {
            setFormKey(Math.random())
            setSelectedProduct(undefined);
            setQuantity(undefined);
        }
    }, [orderItem, processOrderItemData]);



   
    
    if (editMode === 2) {
        dispatch(setProductId(orderItem.productId.productId));
    }

    const productValidator = (values: any): KeyValue<string> | undefined => {
        console.log("Validating product form with values:", values);
        const msgQuantityGreaterThanATP: KeyValue<string> = {
            VALIDATION_SUMMARY: `${getTranslatedLabel(`${localizationKey}.validation.quantityGreaterThanATP`, "Quantity is greater than ATP.")}`
        };

        if (Object.keys(values).length > 0 && values.productId != null && values.quantity > 0) {
            if (values.productId.availableToPromiseTotal > 0 && values.quantity > values.productId.availableToPromiseTotal) {
                return msgQuantityGreaterThanATP;
            }
        }

        return;
    };
    
    console.log('orderItem:', orderItem);

    return (
        <React.Fragment>
            <Form
                ref={MyForm}
                initialValues={processOrderItemData && processOrderItemData!.status === 'Success' ? undefined : orderItem}
                validator={productValidator}
                key={formKey}

                onSubmit={(values) => {
                    const submitValues = {
                        ...values,
                        productId: values.productId || selectedProduct,
                        productFeatureId: values.productId?.productFeatureId || selectedProduct?.productFeatureId, // REFACTOR: Include productFeatureId in form submission
                    };
                    handleSubmitData(submitValues as OrderItem);
                    setSelectedProduct(undefined);
                    setShowQuantityWarning(false);
                    setQuantity(undefined);
                }}
                render={(formRenderProps) => (
                        <FormElement>
                            {processOrderItemError && (
                                <div className={"k-messagebox k-messagebox-error"}>
                                    {processOrderItemError}
                                </div>
                            )}

                            {formRenderProps.visited &&
                                formRenderProps.errors &&
                                formRenderProps.errors.VALIDATION_SUMMARY && (
                                    <div className={"k-messagebox k-messagebox-error"}>
                                        {formRenderProps.errors.VALIDATION_SUMMARY}
                                    </div>
                                )}

                            {showQuantityWarning && (
                                <Typography variant="body2" color="warning.main" sx={{mb: 2}}>
                                    {getTranslatedLabel(`${localizationKey}.validation.quantityLessThanIncluded`, "Quantity must be equal to or greater than Quantity Included.")}
                                </Typography>
                            )}

                            <fieldset className={"k-form-fieldset"}>
                                <Grid container spacing={2} alignItems={"flex-end"}>
                                    <Grid item xs={6}>
                                        <Field
                                            id={"productId"}
                                            name={"productId"}
                                            label={getTranslatedLabel(`${localizationKey}.product`, "Product")}
                                            component={FormMultiColumnComboBoxVirtualSalesProduct}
                                            autoComplete={"off"}
                                            validator={requiredValidator}
                                            onChange={e => setSelectedProduct(e.value)}
                                            disabled={editMode === 2}
                                        />
                                    </Grid>
                                    
                                </Grid>

                                {productPromotions &&
                                    productPromotions.length > 0 &&
                                    formRenderProps.valueGetter("productId") !== undefined && (
                                        <Grid item xs={9}>
                                            <Field
                                                id={"productPromoId"}
                                                name={"productPromoId"}
                                                label={getTranslatedLabel(`${localizationKey}.promos`, "Promotions")}
                                                component={MemoizedFormDropDownList}
                                                dataItemKey={"productPromoId"}
                                                textField={"promoText"}
                                                data={productPromotionsWithEmpty || []}
                                                disabled={orderFormEditMode > 3}
                                            />
                                        </Grid>
                                    )}

                                <Grid item container xs={12} spacing={2} alignItems={"flex-end"}>
                                    <Grid item xs={6}> {/* Adjusted xs to make space for the checkbox */}
                                        <Field
                                            id={"quantity"}
                                            format="n0"
                                            min={1}
                                            name={"quantity"}
                                            label={getTranslatedLabel(`${localizationKey}.quantity`, "Quantity *")}
                                            component={FormNumericTextBox}
                                            validator={requiredValidator}
                                            disabled={orderFormEditMode > 3}
                                            value={quantity}
                                            onChange={(e) => setQuantity(e.value)}
                                        />
                                    </Grid>

                                    {selectedProduct && (
                                        <Grid item xs={6} container direction="column" spacing={1} mt={2}>
                                            {isPriceLoading ? (
                                                <Typography variant="body1" component="div">
                                                    {getTranslatedLabel(`general.loading`, "Loading...")}
                                                </Typography>
                                            ) : (
                                                <>
                                                    <Grid item>
                                                        <Typography variant="body1" component="div">
                                                            <strong>{getTranslatedLabel(`${localizationKey}.uom`, "Unit of Measure")}:</strong> {selectedProduct?.uomDescription}
                                                        </Typography>
                                                    </Grid>
                                                    <Grid item>
                                                        <Typography variant="body1" component="div">
                                                            <strong>{getTranslatedLabel(`${localizationKey}.color`, "Color")}:</strong>{" "}
                                                            {selectedProduct?.colorDescription || "N/A"} {/* REFACTOR: Added color display */}
                                                           
                                                        </Typography>
                                                    </Grid>
                                                    {productPrice && (
                                                        <>
                                                            <Grid item>
                                                                <Typography variant="body1" component="div">
                                                                    <strong>{getTranslatedLabel(`${localizationKey}.price`, "Price")}:</strong> {productPrice.price.toFixed(2)}
                                                                </Typography>
                                                            </Grid>
                                                            <Grid item>
                                                                <Typography variant="body1" component="div">
                                                                    <strong>{getTranslatedLabel(`${localizationKey}.quantityIncluded`, "Quantity Included")}:</strong> {productPrice.quantityIncluded}
                                                                </Typography>
                                                            </Grid>
                                                            <Grid item>
                                                                <Typography variant="body1" component="div">
                                                                    <strong>{getTranslatedLabel(`${localizationKey}.piecesIncluded`, "Pieces Included")}:</strong> {productPrice.piecesIncluded}
                                                                </Typography>
                                                            </Grid>
                                                        </>
                                                    )}
                                                </>
                                            )}
                                        </Grid>
                                    )}
                                </Grid>

                                <div className="k-form-buttons">
                                    <Grid container spacing={2}>
                                        <Grid item>
                                            <LoadingButton
                                                size="large"
                                                type={"submit"}
                                                loading={loading}
                                                variant="outlined"
                                                disabled={!formRenderProps.allowSubmit}
                                            >
                                                {loading ? getTranslatedLabel(`general.processing`, "Processing") : (editMode === 1 ? getTranslatedLabel(`general.add`, "Add") : getTranslatedLabel(`general.update`, "Update"))}
                                            </LoadingButton>
                                        </Grid>
                                        <Grid item>
                                            <Button
                                                onClick={() => {
                                                    onClose();
                                                }}
                                                size="large"
                                                color="error"
                                                variant="outlined"
                                            >
                                                <span>{getTranslatedLabel(`general.cancel`, "cancel")}</span>
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


export const SalesOrderItemFormMemo = React.memo(SalesOrderItemForm);


