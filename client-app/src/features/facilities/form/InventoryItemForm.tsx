import React, {useEffect, useState} from 'react';
import Button from '@mui/material/Button';
import Grid from '@mui/material/Grid';
import {Field, Form, FormElement} from '@progress/kendo-react-form';

import {Box, Paper, Typography} from "@mui/material";
import {InventoryItem} from "../../../app/models/facility/inventoryItem";
import {
    useAppSelector,
    useFetchCurrenciesQuery,
    useFetchFacilitiesQuery,
    useFetchFacilityLocationsLovQuery, useFetchProductFeatureColorsQuery
} from "../../../app/store/configureStore";
import {requiredValidator} from "../../../app/common/form/Validators";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import {
    FormMultiColumnComboBoxVirtualInventoryItemProduct
} from "../../../app/common/form/FormMultiColumnComboBoxVirtualInventoryItemProduct";
import {useTranslationHelper} from '../../../app/hooks/useTranslationHelper';
import FacilityMenu from '../menu/FacilityMenu';
import FormInput from '../../../app/common/form/FormInput';
import {MemoizedFormDropDownList2} from '../../../app/common/form/MemoizedFormDropDownList2';
import {
    useCreateInventoryItemMutation,
    useFetchProductFeatureSizesQuery,
    useUpdateInventoryItemMutation
} from "../../../app/store/apis";
import {toast} from "react-toastify";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import LoadingComponent from "../../../app/layout/LoadingComponent";

interface Props {
    inventoryItem?: InventoryItem
    editMode: number;
    cancelEdit: () => void;
}

interface FormData {
    inventoryItemId?: string; // Required for update
    productId: { productId: string; productName: string } | string; // Allow both types for flexibility
    lotId?: string;
    containerId?: string;
    facilityId: string;
    locationSeqId?: string;
    currencyUomId?: string;
    datetimeReceived?: Date;
    expireDate?: Date;
    unitCost?: number;
    statusId?: string; // Added for update
    ownerPartyId?: string; // Added for update
    colorFeatureId?: string;
    sizeFeatureId?: string;
}

export default function InventoryItemForm({inventoryItem, cancelEdit, editMode}: Props) {
    const {getTranslatedLabel} = useTranslationHelper();

    const {data: facilities, error, isFetching, isLoading} = useFetchFacilitiesQuery(undefined);
    const {data: facilityLocations} = useFetchFacilityLocationsLovQuery(undefined)
    const {data: currencies} = useFetchCurrenciesQuery(undefined)
    const {selectedFacilityId} = useAppSelector(state => state.facilityInventoryUi);
    const { data: productFeatureColors } = useFetchProductFeatureColorsQuery(undefined);
    const { data: productFeatureSizes } = useFetchProductFeatureSizesQuery(undefined);

    const formInitialValues = {
        ...inventoryItem,
        productId: inventoryItem?.productIdObject || inventoryItem?.productId || "", // Prioritize productIdObject for edit mode
    };

    const mapProductFeatureColors = (productFeatureColors: any[]) => {
        // Why: Ensures "No Color" appears at the top of the dropdown for user convenience
        const sortedColors = [...productFeatureColors].sort((a, b) => {
            if (a.productColorId === "COLOR_NO_COLOR") return -1;
            if (b.productColorId === "COLOR_NO_COLOR") return 1;
            return 0;
        });

        return sortedColors.map(color => ({
            ...color,
            colorFeatureId: color.productColorId,
            productColorId: undefined,
        }));
    };
    
    const mapProductFeatureSizes = (productFeatureSizes: any[]) => {
        return productFeatureSizes.map(size => ({
            ...size,
            sizeFeatureId: size.productSizeId,
            productSizeId: undefined,
        }));
    };
    const transformedColors = mapProductFeatureColors(productFeatureColors ?? []);
    const transformedSizes = mapProductFeatureSizes(productFeatureSizes ?? []);

    const [createInventoryItem, {isLoading: isCreating}] = useCreateInventoryItemMutation();
    const [updateInventoryItem, {isLoading: isUpdating}] = useUpdateInventoryItemMutation();

   
    const handleResetForm = () => {
        formRenderProps?.form.reset();
    };
    
    async function handleSubmitData(data: FormData) {
        try {
            if (editMode === 2 && data.inventoryItemId) {
                const updateRequest = {
                    inventoryItemId: data.inventoryItemId,
                    productId: typeof data.productId === 'object' ? data.productId.productId : data.productId,
                    lotId: data.lotId,
                    facilityId: data.facilityId,
                    statusId: data.statusId,
                    ownerPartyId: data.ownerPartyId,
                    unitCost: data.unitCost,
                    colorFeatureId: data.colorFeatureId,
                    sizeFeatureId: data.sizeFeatureId
                };

                const result = await updateInventoryItem(updateRequest).unwrap();
                toast.success(`${getTranslatedLabel('facility.items.form.updateSuccess', 'Inventory item')} ${data.inventoryItemId} ${getTranslatedLabel('general.updated', 'updated successfully')}`);
                cancelEdit();
            } else {
                // Create mode
                const createRequest = {
                    productId: typeof data.productId === 'object' ? data.productId.productId : data.productId,
                    lotId: data.lotId,
                    containerId: data.containerId,
                    facilityId: data.facilityId,
                    locationSeqId: data.locationSeqId,
                    currencyUomId: data.currencyUomId,
                    datetimeReceived: data.datetimeReceived,
                    expireDate: data.expireDate,
                    unitCost: data.unitCost,
                    colorFeatureId: data.colorFeatureId,
                    sizeFeatureId: data.sizeFeatureId
                };

                const result = await createInventoryItem(createRequest).unwrap();
                toast.success(`${getTranslatedLabel('facility.items.form.createSuccess', 'Inventory item created successfully')}`);
                cancelEdit();
            }
        } catch (error: any) {
            // Why: Simplifies code and ensures consistent user feedback.
            toast.error(`An error occurred while ${editMode === 2 ? 'updating' : 'creating'} the inventory item`);
        }
    }


    return (
        <>
            <FacilityMenu/>
            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                <Grid container spacing={2}>
                    <Grid item xs={6}>
                        {editMode === 2 && <Box display='flex' justifyContent='space-between'>
                            <Typography sx={{p: 2}}
                                        variant='h4'>{getTranslatedLabel('facility.items.form.title', 'Inventory Item')}: {inventoryItem?.inventoryItemId} </Typography>
                        </Box>}
                    </Grid>

                </Grid>


                <Form
                    initialValues={formInitialValues}
                    key={JSON.stringify(formInitialValues)}
                    onSubmit={values => handleSubmitData(values)}
                    render={(formRenderProps) => (

                        <FormElement>
                            <fieldset className={'k-form-fieldset'}>
                                <Grid container>
                                    <Grid item container xs={12} spacing={2}>
                                        
                                        <Grid item xs={4}>
                                            <Field
                                                id={"productId"}
                                                name={"productId"}
                                                label={getTranslatedLabel('facility.items.form.product', 'Product')}
                                                component={FormMultiColumnComboBoxVirtualInventoryItemProduct}
                                                autoComplete={"off"}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'facilityId'}
                                                name={'facilityId'}
                                                label={getTranslatedLabel('facility.items.form.facility', 'Facility *')}
                                                component={MemoizedFormDropDownList2}
                                                data={facilities ?? []}
                                                dataItemKey={"facilityId"}
                                                textField={"facilityName"}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item container xs={12} spacing={2}>
                                            <Grid item xs={4}>
                                                <Field
                                                    id={"colorFeatureId"}
                                                    name={"colorFeatureId"}
                                                    label={getTranslatedLabel('facility.items.form.color', 'Color')}
                                                    component={MemoizedFormDropDownList2}
                                                    data={transformedColors}
                                                    dataItemKey={"colorFeatureId"}
                                                    textField={"description"}
                                                />
                                            </Grid>
                                            <Grid item xs={4}>
                                                <Field
                                                    id={"sizeFeatureId"}
                                                    name={"sizeFeatureId"}
                                                    label={getTranslatedLabel('facility.items.form.size', 'Size')}
                                                    component={MemoizedFormDropDownList2}
                                                    data={transformedSizes}
                                                    dataItemKey={"sizeFeatureId"}
                                                    textField={"description"}
                                                />
                                            </Grid>
                                        </Grid>
                                        <Grid item container xs={12} spacing={2}>
                                            <Grid item xs={4}>
                                                <Field
                                                    id={'unitCost'}
                                                    name={'unitCost'}
                                                    format="n2"                // show 2 decimal places
                                                    decimals={2}               // limit input to 2 decimal places
                                                    label={getTranslatedLabel('facility.items.form.unitCost', 'Unit Cost')}
                                                    component={FormNumericTextBox}
                                                    inputProps={{ type: 'number', step: '0.01' }}
                                                    autoComplete={'off'}
                                                    disabled={isCreating}
                                                />
                                            </Grid>
                                        </Grid>
                                        <Grid item container xs={12} spacing={2}>
                                            <Grid item xs={4}>
                                                <Field
                                                    id={"lotId"}
                                                    name={"lotId"}
                                                    label={getTranslatedLabel('facility.items.form.lot', 'Lot')}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                            <Grid item xs={4}>
                                                <Field
                                                    id={"containerId"}
                                                    name={"containerId"}
                                                    label={getTranslatedLabel('facility.items.form.container', 'Container')}
                                                    component={FormInput}
                                                    autoComplete={"off"}
                                                />
                                            </Grid>
                                        </Grid>
                                        
                                    </Grid>
                                    <Grid item container xs={12} spacing={2}>
                                        
                                        <Grid item xs={4}>
                                            <Field
                                                id={'locationSeqId'}
                                                name={'locationSeqId'}
                                                label={getTranslatedLabel('facility.items.form.location', 'Facility Location')}
                                                component={MemoizedFormDropDownList2}
                                                data={facilityLocations ?? []}
                                                dataItemKey={"locationSeqId"}
                                                textField={"description"}
                                            />
                                        </Grid>
                                    </Grid>
                                    
                                    <Grid item container xs={12} spacing={2}>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'datetimeReceived'}
                                                name={'datetimeReceived'}
                                                label={getTranslatedLabel('facility.items.form.received', 'Datetime Received *')}
                                                component={FormDatePicker}
                                            />
                                        </Grid>
                                        <Grid item xs={4}>
                                            <Field
                                                id={'expireDate'}
                                                name={'expireDate'}
                                                label={getTranslatedLabel('facility.items.form.expire', 'Expire Date *')}
                                                component={FormDatePicker}
                                            />
                                        </Grid>
                                        
                                    </Grid>
                                    
                                </Grid>


                                <div className="k-form-buttons">
                                    <Grid container rowSpacing={2}>
                                        <Grid item xs={1}>
                                            <Button
                                                variant="contained"
                                                type={"submit"}
                                                color="success"
                                                disabled={!formRenderProps.allowSubmit}
                                            >
                                                {getTranslatedLabel("general.submit", "Submit")}
                                            </Button>
                                        </Grid>
                                        <Grid item xs={1}>
                                            <Button onClick={cancelEdit} color="error" variant="contained">
                                                {getTranslatedLabel("general.cancel", "Cancel")}
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </div>

                                
                            </fieldset>

                        </FormElement>

                    )}
                />

            </Paper>
            {isCreating || isUpdating && (
                <LoadingComponent
                    message={getTranslatedLabel(
                        'facility.items.form.processing',
                        'Processing Inventory Item'
                    )}
                />
            )}
        </>

    );
}


