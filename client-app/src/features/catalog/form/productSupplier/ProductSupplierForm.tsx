import React, { useEffect, useState } from 'react';
import { Box, Grid, Paper, Typography } from "@mui/material";
import Button from "@mui/material/Button";
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import { useAppSelector } from "../../../../app/store/configureStore";
import { FormComboBoxVirtualSupplier } from "../../../../app/common/form/FormComboBoxVirtualSupplier";
import CatalogMenu from '../../menu/CatalogMenu';
import { requiredValidator } from "../../../../app/common/form/Validators";
import {
    useCreateProductSupplierMutation,
    useGetCurrenciesQuery,
    useGetQuantitiesQuery, useUpdateProductSupplierMutation
} from "../../../../app/store/apis";

interface Props {
    supplierProduct?: SupplierProductDto;
    editMode: number;
    cancelEdit: () => void;
}

export default function ProductSupplierForm({ supplierProduct, editMode, cancelEdit }: Props) {
    const selectedProduct = useAppSelector(state => state.productUi.selectedProduct);
    const [buttonFlag, setButtonFlag] = useState(false);

    // REFACTOR: Replaced Redux slice with RTK Query for fetching currencies, quantities, and suppliers
    // Improves data management with automatic caching and reduces boilerplate
    const { data: currencies = [], isLoading: currenciesLoading } = useGetCurrenciesQuery();
    const { data: quantities = [], isLoading: quantitiesLoading } = useGetQuantitiesQuery();
    const [createProductSupplier, { isLoading: createLoading, error: createError }] = useCreateProductSupplierMutation();
    const [updateProductSupplier, { isLoading: updateLoading, error: updateError }] = useUpdateProductSupplierMutation();

    // REFACTOR: Initialize form values with SupplierProductDto for edit mode
    // Ensures form fields align with DTO structure and handles date formatting
    const initialValues = editMode === 2 && supplierProduct
        ? {
            fromPartyId: supplierProduct.fromPartyId,
            currencyUomId: supplierProduct.currencyUomId, // Map to ID if needed
            quantityUomId: selectedProduct?.quantityUomId,
            minimumOrderQuantity: supplierProduct.minimumOrderQuantity,
            availableFromDate: supplierProduct.availableFromDate,
            availableThruDate: supplierProduct.availableThruDate,
            lastPrice: supplierProduct.lastPrice
        }
        : { quantityUomId: selectedProduct?.quantityUomId };
    
    console.log("initialValues", initialValues);
    console.log("supplierProduct", supplierProduct);

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        try {
            
            data.productId = selectedProduct?.productId || "";
            data.partyId = data.fromPartyId.fromPartyId; 
           

            console.log("data", data);

            let response;
            if (editMode === 2) {
                response = await updateProductSupplier(data).unwrap();
            } else {
                response = await createProductSupplier(data).unwrap();
            }

            cancelEdit();
        } catch (error) {
            console.error('Error submitting supplier:', error);
        }
        setButtonFlag(false);
    }

    if (currenciesLoading || quantitiesLoading) {
        return <LoadingComponent message="Loading form data..." />;
    }

    return (
        <>
            <CatalogMenu selectedMenuItem='/products' />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container spacing={2}>
                    <Grid item xs={6}>
                        <Box display='flex' justifyContent='space-between'>
                            <Typography sx={{ p: 2 }} variant='h4' color={editMode === 2 ? "black" : "green"}>
                                {`${editMode === 2 ? "Edit Supplier for" : "New Supplier for"} ${selectedProduct?.productName}`}
                            </Typography>
                        </Box>
                    </Grid>
                </Grid>
                <Form
                    initialValues={initialValues}
                    onSubmit={values => handleSubmitData(values)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className={'k-form-fieldset'}>
                                <Grid container spacing={2}>
                                    <Grid item xs={3}>
                                        <Field
                                            id="fromPartyId"
                                            name="fromPartyId"
                                            label="Supplier"
                                            component={FormComboBoxVirtualSupplier}
                                            autoComplete="off"
                                            validator={requiredValidator}
                                            dataItemKey="fromPartyId"
                                            textField="fromPartyName"
                                        />
                                    </Grid>

                                    <Grid item xs={3}>
                                        <Field
                                            id="currencyUomId"
                                            name="currencyUomId"
                                            label="Currency *"
                                            component={MemoizedFormDropDownList}
                                            dataItemKey="currencyUomId"
                                            textField="description"
                                            data={currencies}
                                            validator={requiredValidator}
                                            //disabled={editMode === 2}
                                        />
                                    </Grid>

                                    <Grid item xs={3}>
                                        <Field
                                            id="quantityUomId"
                                            name="quantityUomId"
                                            label="Quantity Uom *"
                                            component={MemoizedFormDropDownList}
                                            dataItemKey="quantityUomId"
                                            textField="description"
                                            data={quantities}
                                            validator={requiredValidator}
                                            //disabled
                                        />
                                    </Grid>

                                    <Grid item xs={3}>
                                        <Field
                                            id="minimumOrderQuantity"
                                            name="minimumOrderQuantity"
                                            label="Minimum Order Quantity *"
                                            component={FormNumericTextBox}
                                            //validator={requiredValidator}
                                            disabled={editMode === 2}
                                        />
                                    </Grid>

                                    <Grid item xs={3}>
                                        <Field
                                            id="availableFromDate"
                                            name="availableFromDate"
                                            label="Available From Date *"
                                            component={FormDatePicker}
                                            validator={requiredValidator}
                                            //disabled={editMode === 2}
                                        />
                                    </Grid>

                                    <Grid item xs={3}>
                                        <Field
                                            id="availableThruDate"
                                            name="availableThruDate"
                                            label="Available To Date"
                                            component={FormDatePicker}
                                        />
                                    </Grid>

                                    <Grid item xs={3}>
                                        <Field
                                            id="lastPrice"
                                            name="lastPrice"
                                            label="Last Price *"
                                            format="n2"
                                            component={FormNumericTextBox}
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                </Grid>

                                <div className="k-form-buttons">
                                    <Grid container rowSpacing={2}>
                                        <Grid item xs={1}>
                                            <Button
                                                variant="contained"
                                                type="submit"
                                                color="success"
                                                disabled={!formRenderProps.allowSubmit || buttonFlag || createLoading || updateLoading}
                                            >
                                                Submit
                                            </Button>
                                        </Grid>
                                        <Grid item xs={1}>
                                            <Button onClick={cancelEdit} color="error" variant="contained">
                                                Cancel
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </div>

                                {(buttonFlag || createLoading || updateLoading) && (
                                    <LoadingComponent message="Processing Product Supplier..." />
                                )}
                                
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}