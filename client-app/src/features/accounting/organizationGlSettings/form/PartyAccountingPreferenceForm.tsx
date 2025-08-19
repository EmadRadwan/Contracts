import React, {useState} from 'react';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import FormTextArea from '../../../../app/common/form/FormTextArea';
import FormInput from '../../../../app/common/form/FormInput';
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import {MemoizedFormDropDownList} from "../../../../app/common/form/MemoizedFormDropDownList";
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import {Product} from "../../../../app/models/product/product";
import {
    useAddProductMutation,
    useAppDispatch,
    useFetchProductCategoriesQuery,
    useFetchProductTypesQuery
} from "../../../../app/store/configureStore";
import {Box, Paper, Typography} from "@mui/material";
import {toast} from "react-toastify";
import {requiredValidator} from "../../../../app/common/form/Validators";
import {FormDropDownTreeProductCategory} from "../../../../app/common/form/FormDropDownTreeProductCategory";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";

interface Props {
    product?: Product;
    editMode: number;
    cancelEdit: () => void;
}

export default function PartyAccountingPreferenceForm({product, cancelEdit, editMode}: Props) {

    const [createProduct, {isLoading}] = useAddProductMutation();


    const {data: productTypes} = useFetchProductTypesQuery(undefined)
    const [serviceFieldsFlag, setServiceFieldsFlag] = useState<boolean>(product?.productTypeId === "FINISHED_GOOD" || product?.productTypeId === "SERVICE_PRODUCT")
    const {data: productCategories} = useFetchProductCategoriesQuery(undefined);

    const dispatch = useAppDispatch();


    const [buttonFlag, setButtonFlag] = useState(false);
    console.log("product", product)

    const onProductTypeChange = React.useCallback(
        (event) => {
            setServiceFieldsFlag(event.value === ("GOOD" || "SERVICE"))
        }, []
    )

    async function handleSubmitData(data: any) {
        setButtonFlag(true)
        try {
            let response: any;
            if (editMode === 2) {
                //response = await agent.Products.updateProduct(data);
            } else {
                try {
                    const createdOrder = await createProduct(data).unwrap();

                } catch (error) {
                    toast.error("Failed to create product");

                }
            }
            //dispatch(setProduct(response));
            cancelEdit();
        } catch (error) {
            console.log(error)
        }
        setButtonFlag(false)
    }

    console.log(productCategories)

    return (
        <Paper elevation={5} className={`div-container-withBorderCurved`}>
            <Grid container spacing={2}>
                <Grid item xs={5}>
                    {<Box display='flex' justifyContent='space-between'>
                        <Typography color={product?.productName ? "black" : "green"} sx={{p: 2}}
                                    variant='h4'> {product?.productName ? product.productName : "New Product"} </Typography>
                    </Box>}
                </Grid>
                {editMode === 2 && <Grid item xs={7}>

                </Grid>}
            </Grid>


            <Form
                initialValues={editMode === 2 ? product : undefined}
                onSubmit={values => handleSubmitData(values)}
                render={(formRenderProps) => (

                    <FormElement>
                        <fieldset className={'k-form-fieldset'}>
                            <Grid container spacing={2}>
                                <Grid item xs={5}>
                                    <Field
                                        id={'productName'}
                                        name={'productName'}
                                        label={'Product Name *'}
                                        component={FormInput}
                                        autoComplete={"off"}
                                        validator={requiredValidator}
                                    />
                                </Grid>
                            </Grid>

                            <Grid container spacing={2}>
                                <Grid item xs={5}>
                                    <Field
                                        id={"productTypeId"}
                                        name={"productTypeId"}
                                        label={"Product Type *"}
                                        component={MemoizedFormDropDownList}
                                        dataItemKey={"productTypeId"}
                                        textField={"description"}
                                        data={productTypes ? productTypes : []}
                                        onChange={onProductTypeChange}
                                        validator={requiredValidator}
                                    />
                                </Grid>
                                <Grid item xs={5}>
                                    <Field
                                        id={"primaryProductCategoryId"}
                                        name={"primaryProductCategoryId"}
                                        label={"Product Category *"}
                                        data={productCategories ? productCategories : []}
                                        component={FormDropDownTreeProductCategory}
                                        dataItemKey={"productCategoryId"}
                                        textField={"text"}
                                        validator={requiredValidator}
                                        selectField={"selected"}
                                        expandField={"expanded"}
                                        onChange={e => console.log(e)}
                                    />
                                </Grid>
                            </Grid>
                            {serviceFieldsFlag && <Grid container spacing={2}>
                                <Grid item xs={5}>
                                    <Field
                                        id={"serviceLifeDays"}
                                        name={"serviceLifeDays"}
                                        label={"Service Life Days"}
                                        component={FormNumericTextBox}
                                        disabled={editMode > 1}
                                    />
                                </Grid>
                                <Grid item xs={5}>
                                    <Field
                                        id={"serviceLifeMileage"}
                                        name={"serviceLifeMileage"}
                                        label={"Service Life Mileage"}
                                        component={FormNumericTextBox}
                                        disabled={editMode > 1}
                                    />
                                </Grid>
                            </Grid>}

                            <Field
                                id={'comments'}
                                name={'comments'}
                                label={'Comments'}
                                autoComplete={"off"}
                                rows={3}
                                component={FormTextArea}
                            />
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


                            {buttonFlag && <LoadingComponent message='Processing Product...'/>}
                        </fieldset>

                    </FormElement>

                )}
            />

        </Paper>


    );
}


