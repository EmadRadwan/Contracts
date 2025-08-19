import React, { useEffect } from 'react';
import { Box, Grid, Paper, Typography } from "@mui/material";
import Button from "@mui/material/Button";
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import LoadingComponent from "../../../../app/layout/LoadingComponent";
import FormNumericTextBox from "../../../../app/common/form/FormNumericTextBox";
import { MemoizedFormDropDownList } from "../../../../app/common/form/MemoizedFormDropDownList";
import {
    useAppSelector,
    useAppDispatch,
    useCreateProductFacilityMutation,
    useUpdateProductFacilityMutation
} from "../../../../app/store/configureStore";
import { facilitiesSelectors, fetchFacilitiesAsync } from "../../../facilities/slice/FacilitySlice";
import { ProductFacility } from "../../../../app/models/product/productFacility";
import { requiredValidator } from "../../../../app/common/form/Validators";
import CatalogMenu from '../../menu/CatalogMenu';

interface Props {
    productFacility?: ProductFacility;
    editMode: number;
    cancelEdit: () => void;
}

export default function ProductFacilityForm({ productFacility, cancelEdit, editMode }: Props) {
    const { facilitiesLoaded } = useAppSelector(state => state.facility);
    const facilities = useAppSelector(facilitiesSelectors.selectAll);
    const selectedProduct = useAppSelector(state => state.productUi.selectedProduct);
    const dispatch = useAppDispatch();

    // REFACTOR: Replaced Redux slice actions with RTK Query mutations for creating and updating product facilities.
    // This leverages RTK Query's automatic handling of loading, error states, and caching, reducing boilerplate.
    const [createProductFacility, { isLoading: isCreating }] = useCreateProductFacilityMutation();
    const [updateProductFacility, { isLoading: isUpdating }] = useUpdateProductFacilityMutation();

    useEffect(() => {
        if (!facilitiesLoaded) dispatch(fetchFacilitiesAsync());
    }, [facilitiesLoaded, dispatch]);

    async function handleSubmitData(data: ProductFacility) {
        try {
            // REFACTOR: Use RTK Query mutations instead of agent calls and Redux dispatches.
            // This simplifies the API call logic and removes the need for manual state updates in Redux.
            if (editMode === 2) {
                await updateProductFacility(data).unwrap();
            } else {
                if (selectedProduct) {
                    data.productId = selectedProduct.productId;
                }
                await createProductFacility(data).unwrap();
            }
            cancelEdit();
        } catch (error) {
            console.log(error);
        }
    }

    // REFACTOR: Consolidated loading states from RTK Query's isLoading into a single buttonFlag variable.
    // This maintains the existing UI behavior while using RTK Query's loading states.
    const buttonFlag = isCreating || isUpdating;

    return (
        <>
            <CatalogMenu selectedMenuItem='/products' />
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container spacing={2}>
                    <Grid item xs={6}>
                        <Box display='flex' justifyContent='space-between'>
                            <Typography sx={{ p: 2 }} variant='h4' color={editMode === 2 ? "black" : "green"}>
                                {`${editMode === 2 ? "Edit Facility for" : "New Facility for"} ${selectedProduct?.productName}`}
                            </Typography>
                        </Box>
                    </Grid>
                </Grid>

                <Form
                    initialValues={editMode === 2 ? productFacility : undefined}
                    onSubmit={values => handleSubmitData(values as ProductFacility)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className={'k-form-fieldset'}>
                                <Grid item xs={3}>
                                    <Field
                                        id={"facilityId"}
                                        name={"facilityId"}
                                        label={"Product Facility *"}
                                        component={MemoizedFormDropDownList}
                                        dataItemKey={"facilityId"}
                                        textField={"facilityName"}
                                        data={facilities}
                                        validator={requiredValidator}
                                        disabled={editMode === 2}
                                    />
                                </Grid>
                                <Grid container space={2} justifyContent={"space-between"}>
                                    <Grid item xs={3}>
                                        <Field
                                            id={'minimumStock'}
                                            format="n2"
                                            name={'minimumStock'}
                                            label={'Minimum Stock'}
                                            component={FormNumericTextBox}
                                            validator={requiredValidator}
                                        />
                                    </Grid>

                                    <Grid item xs={3}>
                                        <Field
                                            id={'reorderQuantity'}
                                            format="n2"
                                            name={'reorderQuantity'}
                                            label={'Reorder Quantity'}
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

                                {buttonFlag && <LoadingComponent message='Processing Product Facility...' />}
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}