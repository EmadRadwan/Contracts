import React, {useEffect, useState} from 'react';
import {Box, Grid, Paper, Typography} from "@mui/material";
import Button from "@mui/material/Button";
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import FormDatePicker from "../../../../app/common/form/FormDatePicker";
import {useAppDispatch, useAppSelector} from "../../../../app/store/configureStore";
import agent from "../../../../app/api/agent";
import {ProductCategoryMember} from "../../../../app/models/product/productCategoryMember";
import {
    addProductCategoryMember,
    fetchProductCategoriesAsync,
    updateProductCategoryMember
} from "../../slice/productCategorySlice";
import {requiredValidator} from "../../../../app/common/form/Validators";
import CatalogMenu from '../../menu/CatalogMenu';

interface Props {
    productCategoryMember?: ProductCategoryMember;
    editMode: number;
    cancelEdit: () => void;
}


export default function ProductCategoryForm({productCategoryMember, cancelEdit, editMode}: Props) {

    const {productCategoriesLoaded, productCategories} = useAppSelector(state => state.productCategory);

    const selectedProduct = useAppSelector(state => state.productUi.selectedProduct);

    console.log('productCategories', productCategories);
    console.log('productCategoryMember', productCategoryMember);

    const dispatch = useAppDispatch();

    useEffect(() => {
        if (!productCategoriesLoaded) dispatch(fetchProductCategoriesAsync());
    }, [productCategoriesLoaded, dispatch]);


    const [buttonFlag, setButtonFlag] = useState(false);

    async function handleSubmitData(data: any) {
        setButtonFlag(true)
        try {
            let response: any;
            if (editMode === 2) {
                response = await agent.ProductCategories.updateProductCategoryMember(data);
                dispatch(updateProductCategoryMember(response));

            } else {
                if (selectedProduct) {
                    data.productId = selectedProduct.productId
                }
                response = await agent.ProductCategories.createProductCategoryMember(data);
                dispatch(addProductCategoryMember(response));
            }

            cancelEdit();
        } catch (error) {
            console.log(error)
        }
        setButtonFlag(false)
    }


    return (
        <>
        <CatalogMenu selectedMenuItem='/products' />
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container spacing={2}>
                    <Grid item xs={6}>
                        <Box display='flex' justifyContent='space-between'>
                            <Typography sx={{p: 2}} variant='h4' color={editMode === 2 ? "black" : "green"}>
                                {`${editMode === 2 ? "Edit Category for" : "New Category for"} ${selectedProduct?.productName}`}
                            </Typography>
                        </Box>
                    </Grid>
                </Grid>
                <Form
                    initialValues={editMode === 2 ? productCategoryMember : undefined}
                    onSubmit={values => handleSubmitData(values as ProductCategoryMember)}
                    render={(formRenderProps) => (
    
                        <FormElement>
                            <fieldset className={'k-form-fieldset'}>
                                <Grid item xs={3}>
                                    <Field
                                        id={"productCategoryId"}
                                        name={"productCategoryId"}
                                        label={"Product Category *"}
                                        component={MemoizedFormDropDownList}
                                        dataItemKey={"productCategoryId"}
                                        textField={"description"}
                                        data={productCategories}
                                        validator={requiredValidator}
                                        disabled={editMode === 2}
                                    />
                                </Grid>
    
    
                                <Grid item xs={3}>
                                    <Field
                                        id={'fromDate *'}
                                        name={'fromDate'}
                                        label={'From Date'}
                                        component={FormDatePicker}
                                        disabled={editMode === 2}
                                        validator={requiredValidator}
                                    />
                                </Grid>
    
    
                                <Grid item xs={3}>
                                    <Field
                                        id={'thruDate'}
                                        name={'thruDate'}
                                        label={'To Date'}
                                        component={FormDatePicker}
                                    />
                                </Grid>
    
    
                                <div className="k-form-buttons">
                                    <Grid container rowSpacing={2}>
                                        <Grid item xs={1}>
                                            <Button
                                                variant="contained"
                                                type={'submit'}
                                                color='success'
                                                disabled={!formRenderProps.allowSubmit || buttonFlag}
                                            >
                                                Submit
                                            </Button>
                                        </Grid>
                                        <Grid item xs={1}>
                                            <Button onClick={cancelEdit} color='error' variant="contained">
                                                Cancel
                                            </Button>
                                        </Grid>
    
                                    </Grid>
                                </div>
                                {buttonFlag && <LoadingComponent message='Processing Product Category...'/>}
    
                            </fieldset>
    
                        </FormElement>
    
                    )}
                />
    
            </Paper>
        </>
    );
}


