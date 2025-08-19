import React, { useEffect, useState } from 'react';
import { Grid, Paper, Typography, Button } from "@mui/material";
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { ProductPrice } from "../../../../app/models/product/productPrice";
import {
    useAppDispatch,
    useAppSelector,
    useCreateProductPriceMutation,
    useFetchCompanyBaseCurrencyQuery, useUpdateProductPriceMutation
} from "../../../../app/store/configureStore";
import { currenciesSelectors, fetchCurrenciesAsync } from "../../slice/currencySlice";
import { fetchProductPriceTypesAsync } from "../../slice/productPriceTypeSlice";
import { requiredValidator } from "../../../../app/common/form/Validators";
import CatalogMenu from '../../menu/CatalogMenu';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import { v4 as uuidv4 } from 'uuid';

interface Props {
    productPrice?: ProductPrice;
    editMode: number;
    cancelEdit: () => void;
}

export default function ProductPriceForm({ productPrice, cancelEdit, editMode }: Props) {
    const localizationKey = "product.prod-price.form";
    const selectedProductUi = useAppSelector(state => state.productUi.selectedProduct);
    const { currenciesLoaded } = useAppSelector(state => state.currency);
    const currencies = useAppSelector(currenciesSelectors.selectAll);
    const { productPriceTypesLoaded } = useAppSelector(state => state.productPriceType);
    const { data: baseCurrency, isLoading: isBaseCurrencyLoading } = useFetchCompanyBaseCurrencyQuery(undefined);
    const dispatch = useAppDispatch();
    const { getTranslatedLabel } = useTranslationHelper();
    const [createProductPrice, { isLoading: isCreating }] = useCreateProductPriceMutation();
    const [updateProductPrice, { isLoading: isUpdating }] = useUpdateProductPriceMutation();

    useEffect(() => {
        if (!currenciesLoaded) dispatch(fetchCurrenciesAsync());
        if (!productPriceTypesLoaded) dispatch(fetchProductPriceTypesAsync());
    }, [currenciesLoaded, productPriceTypesLoaded, dispatch]);

    const [buttonFlag, setButtonFlag] = useState(false);

    async function handleSubmitData(data: any) {
        try {
            let response: any;
            data.productPriceTypeId = 'DEFAULT_PRICE';
            if (editMode === 2 && productPrice?.productPriceId) {
                data.productPriceId = productPrice.productPriceId;
                data.fromDate = productPrice.fromDate; // Preserve original string
                // REFACTOR: Use RTK Query mutation hook instead of agent
                // Purpose: Leverages RTK Query's automatic cache invalidation and optimistic updates
                response = await updateProductPrice(data).unwrap();
            } else {
                if (selectedProductUi) {
                    data.productId = selectedProductUi.productId;
                }
                data.productPriceId = uuidv4();
                // REFACTOR: Use RTK Query mutation hook instead of agent
                // Purpose: Simplifies API interaction and integrates with RTK Query cache
                response = await createProductPrice(data).unwrap();
            }
            cancelEdit();
        } catch (error) {
            console.error('Submit error:', error);
        }
    }


    if (isBaseCurrencyLoading || !currenciesLoaded) {
        return <LoadingComponent message="Loading initial data..." />;
    }

    return (
        <>
            <CatalogMenu selectedMenuItem='/products' />
            <Paper elevation={5} sx={{ p: 2 }}>
                <Typography variant='h4' color={editMode === 2 ? "black" : "green"} mb={2}>
                    {`${editMode === 2 ? getTranslatedLabel(`${localizationKey}.title-edit`, "Edit Price for") : getTranslatedLabel(`${localizationKey}.title-new`, "New Price for")}: ${selectedProductUi?.productName}`}
                </Typography>
                <Form
                    initialValues={{
                        ...productPrice,
                        productPriceTypeId: 'DEFAULT_PRICE',
                        currencyUomId: editMode === 2 ? productPrice?.currencyUomId : baseCurrency?.currencyUomId,
                        fromDate: editMode === 2 && productPrice?.fromDate ? new Date(productPrice.fromDate) : new Date(),
                        rowVersion: editMode === 2 ? productPrice?.rowVersion : undefined
                    }}
                    onSubmit={values => handleSubmitData(values as ProductPrice)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <Field
                                id="productPriceId"
                                name="productPriceId"
                                component="input"
                                type="hidden"
                            />
                            <Grid container spacing={2}>
                                <Grid item xs={3}>
                                    <Field
                                        id="currencyUomId"
                                        name="currencyUomId"
                                        label={getTranslatedLabel(`${localizationKey}.currency`, "Currency *")}
                                        component={MemoizedFormDropDownList}
                                        dataItemKey="currencyUomId"
                                        textField="description"
                                        data={currencies}
                                        validator={requiredValidator}
                                        disabled={editMode === 2}
                                    />
                                </Grid>
                                <Grid item xs={3}>
                                    <Field
                                        id="fromDate"
                                        name="fromDate"
                                        label={getTranslatedLabel(`${localizationKey}.from`, "From Date *")}
                                        component={FormDatePicker}
                                        validator={requiredValidator}
                                        disabled={editMode === 2}
                                    />
                                </Grid>
                                <Grid item xs={3}>
                                    <Field
                                        id="thruDate"
                                        name="thruDate"
                                        label={getTranslatedLabel(`${localizationKey}.thru`, "To Date")}
                                        component={FormDatePicker}
                                    />
                                </Grid>
                                <Grid item xs={3}>
                                    <Field
                                        id="price"
                                        format="n2"
                                        name="price"
                                        label={getTranslatedLabel(`${localizationKey}.price`, "Price *")}
                                        component={FormNumericTextBox}
                                        validator={requiredValidator}
                                    />
                                </Grid>
                                <Grid item xs={12} display="flex" justifyContent="flex-end" alignItems="center">
                                    <Button
                                        variant="contained"
                                        type="submit"
                                        color="success"
                                        disabled={!formRenderProps.allowSubmit || isCreating || isUpdating}
                                        sx={{ mx: 1 }}
                                    >
                                        {getTranslatedLabel(`general.submit`, "Submit")}
                                    </Button>
                                    <Button
                                        onClick={cancelEdit}
                                        color="error"
                                        variant="contained"
                                        sx={{ mx: 1 }}
                                    >
                                        {getTranslatedLabel(`general.cancel`, "Cancel")}
                                    </Button>
                                </Grid>
                            </Grid>
                            {(isCreating || isUpdating) && <LoadingComponent message="Processing Product Price..." />}
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}