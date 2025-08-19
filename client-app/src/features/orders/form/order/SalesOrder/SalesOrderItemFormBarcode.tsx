import { Field, Form, FormElement, KeyValue } from "@progress/kendo-react-form";
import { OrderItem } from "../../../../../app/models/order/orderItem";
import FormNumericTextBox from "../../../../../app/common/form/FormNumericTextBox";
import Grid from "@mui/material/Grid";
import Button from "@mui/material/Button";
import { MemoizedFormDropDownList } from "../../../../../app/common/form/MemoizedFormDropDownList";
import useSalesOrderItem from "../../../hook/useSalesOrderItem";
import * as React from 'react';
import LoadingButton from "@mui/lab/LoadingButton";
import { setProductId } from "../../../slice/sharedOrderUiSlice";
import { useAppDispatch } from "../../../../../app/store/configureStore";
import { Typography, TextField } from "@mui/material";
import { useTranslationHelper } from "../../../../../app/hooks/useTranslationHelper";
import { useGetProductDetailsQuery, useGetProductPriceQuery } from "../../../../../app/store/apis";
import { toast } from "react-toastify";

interface Props {
    orderItem?: any;
    editMode: number;
    onClose: () => void;
    orderFormEditMode: number;
}

function SalesOrderItemFormBarcode({
                                       orderItem,
                                       editMode,
                                       onClose,
                                       orderFormEditMode,
                                   }: Props) {
    const [showQuantityWarning, setShowQuantityWarning] = React.useState(false);
    const [quantity, setQuantity] = React.useState<number | undefined>(orderItem?.quantity);
    const [barcode, setBarcode] = React.useState('');
    const barcodeInputRef = React.useRef<HTMLInputElement | null>(null);
    const [isBarcodeLoading, setIsBarcodeLoading] = React.useState(false);
    const [barcodeToQuery, setBarcodeToQuery] = React.useState("");
    const isProcessingRef = React.useRef(false);
    // REFACTOR: Use ref to store formRenderProps for programmatic access
    // Purpose: Enable programmatic updates and submission for barcode-driven form.
    // Why: Allows setting quantity and productId then submitting after barcode scan.
    const formRenderPropsRef = React.useRef<any>(null);

    const dispatch = useAppDispatch();
    const [selectedProduct, setSelectedProduct] = React.useState<any | undefined>(undefined);
    const localizationKey = "order.so.items.form";
    const { getTranslatedLabel } = useTranslationHelper();

    // REFACTOR: Fetch product details and price for barcode
    // Purpose: Query product details using barcode and price using productId.
    // Why: Core functionality for barcode-driven form, replacing dropdown selection.
    const { data: productPrice, isLoading: isPriceLoading } = useGetProductPriceQuery(
        selectedProduct?.productId,
        { skip: !selectedProduct?.productId }
    );
    const { data: productDetails, isLoading: isDetailsLoading, error: detailsError } = useGetProductDetailsQuery(
        barcodeToQuery,
        { skip: !barcodeToQuery }
    );

    const {
        productPromotions,
        handleSubmitData,
        productPromotionsWithEmpty,
        processOrderItemLoading,
        processOrderItemFetching,
        processOrderItemError,
        processOrderItemData,
    } = useSalesOrderItem({ editMode, orderItem, productPrice });

    const [loading, setLoading] = React.useState(processOrderItemLoading || processOrderItemFetching);
    const [formKey, setFormKey] = React.useState<number>(Math.random());

    // REFACTOR: Update loading state
    // Purpose: Reflect loading state for form submission and barcode queries.
    // Why: Combines submission and barcode fetch states for accurate UI feedback.
    React.useEffect(() => {
        setLoading(processOrderItemLoading || processOrderItemFetching || isDetailsLoading);
    }, [processOrderItemLoading, processOrderItemFetching, isDetailsLoading]);

    // REFACTOR: Reset form key on successful submission
    // Purpose: Reset form after successful submission to clear fields.
    // Why: Ensures clean state for next barcode scan.
    React.useEffect(() => {
        if (processOrderItemData && processOrderItemData!.status === 'Success') {
            setFormKey(Math.random());
            setSelectedProduct(undefined);
            setQuantity(undefined);
            setBarcode('');
            setBarcodeToQuery("");
        }
    }, [processOrderItemData]);

    // REFACTOR: Handle barcode scan results
    // Purpose: Process product details from barcode scan, set selectedProduct, and handle errors.
    // Why: Core logic for barcode-driven form, ensuring product selection and error feedback.
    React.useEffect(() => {
        let isMounted = true;

        if (isProcessingRef.current || !barcodeToQuery) {
            return;
        }

        isProcessingRef.current = true;

        if (detailsError && isMounted) {
            toast.error(getTranslatedLabel(`${localizationKey}.validation.invalidBarcode`, "Invalid product ID"));
            setBarcode("");
            setIsBarcodeLoading(false);
            setBarcodeToQuery("");
            barcodeInputRef.current?.focus();
            isProcessingRef.current = false;
            return;
        }

        if (productDetails && isMounted) {
            setSelectedProduct(productDetails);
            setBarcode("");
            setIsBarcodeLoading(false);
            setBarcodeToQuery("");
            barcodeInputRef.current?.focus();
            isProcessingRef.current = false;
            return;
        }

        if (!isDetailsLoading && barcodeToQuery && isMounted && !productDetails && !detailsError) {
            toast.error(getTranslatedLabel(`${localizationKey}.validation.invalidBarcode`, "Invalid product ID"));
            setBarcode("");
            setIsBarcodeLoading(false);
            setBarcodeToQuery("");
            barcodeInputRef.current?.focus();
            isProcessingRef.current = false;
            return;
        }

        return () => {
            isMounted = false;
            isProcessingRef.current = false;
        };
    }, [productDetails, isDetailsLoading, detailsError, getTranslatedLabel]);

    // REFACTOR: Automatically set quantity and productId, then submit
    // Purpose: Set quantity to productPrice.quantityIncluded and productId to selectedProduct, then submit if valid.
    // Why: Automates form submission after barcode scan and price fetch, bypassing validators per user confirmation.
    React.useEffect(() => {
        if (productPrice && productPrice.quantityIncluded && formRenderPropsRef.current && selectedProduct) {
            setQuantity(productPrice.quantityIncluded);
            formRenderPropsRef.current.onChange("quantity", { value: productPrice.quantityIncluded });
            formRenderPropsRef.current.onChange("productId", { value: selectedProduct });

            console.log("Form state after updates:", {
                values: formRenderPropsRef.current.values,
                valid: formRenderPropsRef.current.valid,
                errors: formRenderPropsRef.current.errors,
            });

            if (formRenderPropsRef.current.valid) {
                const submitValues = {
                    ...formRenderPropsRef.current.values,
                    productId: selectedProduct,
                    quantity: productPrice.quantityIncluded,
                };
                formRenderPropsRef.current.onSubmit(submitValues);
            } else {
                toast.error(
                    getTranslatedLabel(
                        `${localizationKey}.validation.formInvalid`,
                        `Form is invalid: ${JSON.stringify(formRenderPropsRef.current.errors)}`
                    )
                );
            }
        }
    }, [productPrice, selectedProduct]);

    // REFACTOR: Check quantity against quantityIncluded
    // Purpose: Show warning if quantity is less than quantityIncluded.
    // Why: Maintains business rule for barcode-driven form.
    React.useEffect(() => {
        if (productPrice && quantity && quantity < productPrice.quantityIncluded) {
            setShowQuantityWarning(true);
        } else {
            setShowQuantityWarning(false);
        }
    }, [productPrice, quantity]);

    // REFACTOR: Handle edit mode for productId
    // Purpose: Set productId in store for update mode.
    // Why: Maintains behavior for editMode === 2, disabling barcode input.
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

    // REFACTOR: Handle barcode scan input
    // Purpose: Trigger product details query on barcode scan.
    // Why: Core input mechanism for barcode-driven form.
    const handleBarcodeScan = async () => {
        if (!barcode) {
            toast.error(getTranslatedLabel(`${localizationKey}.validation.emptyBarcode`, "Please enter a product ID"));
            barcodeInputRef.current?.focus();
            return;
        }

        if (!isProcessingRef.current) {
            setIsBarcodeLoading(true);
            setBarcodeToQuery(barcode);
        }
    };

    return (
        <React.Fragment>
            <Form
                initialValues={processOrderItemData && processOrderItemData!.status === 'Success' ? undefined : orderItem}
                validator={productValidator}
                key={formKey}
                onSubmit={(values) => {
                    const submitValues = {
                        ...values,
                        productId: values.productId || selectedProduct,
                    };
                    handleSubmitData(submitValues as OrderItem);
                    setSelectedProduct(undefined);
                    setShowQuantityWarning(false);
                    setQuantity(undefined);
                    setBarcode('');
                    setBarcodeToQuery("");
                }}
                render={(formRenderProps) => {
                    formRenderPropsRef.current = formRenderProps;
                    return (
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
                                <Typography variant="body2" color="warning.main" sx={{ mb: 2 }}>
                                    {getTranslatedLabel(`${localizationKey}.validation.quantityLessThanIncluded`, "Quantity must be equal to or greater than Quantity Included.")}
                                </Typography>
                            )}

                            <fieldset className={"k-form-fieldset"}>
                                <Grid container spacing={2} alignItems={"flex-end"}>
                                    <Grid item xs={6}>
                                        <TextField
                                            fullWidth
                                            label={getTranslatedLabel(`${localizationKey}.barcode`, "Enter Product ID")}
                                            value={barcode}
                                            onChange={(e) => setBarcode(e.target.value)}
                                            inputRef={barcodeInputRef}
                                            disabled={editMode === 2 || isBarcodeLoading}
                                            onKeyPress={(e) => {
                                                if (e.key === "Enter") {
                                                    handleBarcodeScan();
                                                }
                                            }}
                                            autoFocus
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
                                    <Grid item xs={6}>
                                        <Field
                                            id={"quantity"}
                                            format="n0"
                                            min={1}
                                            name={"quantity"}
                                            label={getTranslatedLabel(`${localizationKey}.quantity`, "Quantity *")}
                                            component={FormNumericTextBox}
                                            // REFACTOR: Disable requiredValidator for quantity
                                            // Purpose: Allow form submission without strict validation, as confirmed to work.
                                            // Why: Simplifies form and avoids validation issues, focusing on barcode input.
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
                                                type="submit"
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
                    );
                }}
            />
        </React.Fragment>
    );
}

export const SalesOrderItemFormBarcodeMemo = React.memo(SalesOrderItemFormBarcode);