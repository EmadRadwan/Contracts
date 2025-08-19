import React, { useState } from 'react';
import {Grid, Paper, Button, Typography, Box} from '@mui/material';
import { useParams } from 'react-router-dom';
import { toast } from 'react-toastify';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import FormDateTimePicker from "../../../app/common/form/FormDateTimePicker";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import { requiredValidator } from "../../../app/common/form/Validators";
import { useCreateRoutingProductLinkMutation, useUpdateRoutingProductLinkMutation, useDeleteRoutingProductLinkMutation } from "../../../app/store/apis";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import RoutingMenu from "../menu/RoutingMenu";
import {
    FormMultiColumnComboBoxVirtualFinishedProduct
} from "../../../app/common/form/FormMultiColumnComboBoxVirtualFinishedProduct";

// Interface for Routing Product Link data
interface RoutingProductLink {
    productId: string;
    productName: string;
    fromDate: string;
    thruDate: string | null;
    estimatedQuantity: number | null;
    workEffortGoodStdTypeId: string;
}

// Form component for creating/editing Routing Product Links
interface EditRoutingProductLinkProps {
    productLink?: RoutingProductLink | null;
    editMode: number; // 1: new, 2: edit
    cancelEdit: () => void;
}

export function EditRoutingProductLink({ productLink, editMode, cancelEdit }: EditRoutingProductLinkProps) {
    const { workEffortId } = useParams<{ workEffortId: string }>();
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'manufacturing.routingProductLink.form';
    const [isProcessing, setIsProcessing] = useState(false);
    const [addRoutingProductLink, { isLoading: addLoading }] = useCreateRoutingProductLinkMutation();
    const [updateRoutingProductLink, { isLoading: updateLoading }] = useUpdateRoutingProductLinkMutation();

    const initialValues: RoutingProductLink = productLink ? {
        productId: productLink.productId,
        productName: productLink.productName,
        fromDate: new Date(productLink.fromDate),
        thruDate: productLink.thruDate ? new Date(productLink.thruDate) : null,
        estimatedQuantity: productLink.estimatedQuantity,
        workEffortGoodStdTypeId: 'ROU_PROD_TEMPLATE',
    } : {
        productId: '',
        productName: '',
        fromDate: new Date(),
        thruDate: null,
        estimatedQuantity: null,
        workEffortGoodStdTypeId: 'ROU_PROD_TEMPLATE',
    };

    const handleSubmitData = async (data: RoutingProductLink) => {
        setIsProcessing(true);
        try {
            const payload = {
                workEffortId: workEffortId!,
                productId: typeof data.productId === 'string' ? data.productId : data.productId.productId,
                workEffortGoodStdTypeId: 'ROU_PROD_TEMPLATE',
                fromDate: data.fromDate || new Date(),
                thruDate: data.thruDate || null,
                estimatedQuantity: data.estimatedQuantity || null,
            };

            // REFACTOR: Use appropriate mutation based on edit mode
            // Purpose: Consistent API interaction for create/update, similar to EditRoutingTaskAssoc
            // Benefit: Simplifies state management and aligns with RTK Query patterns
            if (editMode === 2) {
                await updateRoutingProductLink(payload).unwrap();
                toast.success(getTranslatedLabel(`${localizationKey}.successUpdate`, 'Routing Product Link Updated Successfully!'));
            } else {
                await addRoutingProductLink(payload).unwrap();
                toast.success(getTranslatedLabel(`${localizationKey}.success`, 'Routing Product Link Added Successfully!'));
            }
            cancelEdit();
        } catch (e: any) {
            const errorMsg = e.data?.title || e.data?.errors?.join(', ') || 'Operation failed.';
            toast.error(errorMsg);
        } finally {
            setIsProcessing(false);
        }
    };

    return (
        <>
            <ManufacturingMenu />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container spacing={2}>
                    <Grid item xs={8}>
                        <Box display="flex" justifyContent="space-between">
                            <Typography sx={{ p: 2 }} variant="h3" color="green">
                                {getTranslatedLabel(`${localizationKey}.title`, 'New Routing Product Link')}
                            </Typography>
                        </Box>
                    </Grid>
                </Grid>
                <RoutingMenu workEffortId={workEffortId} selectedMenuItem="routingProductLink" />
                <Form
                    initialValues={initialValues}
                    onSubmit={(values) => handleSubmitData(values as RoutingProductLink)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className="k-form-fieldset">
                                <Grid container spacing={2}>
                                    <Grid item container xs={6}>
                                        <Grid item xs={8}>
                                            <Field
                                                name="productId"
                                                id="productId"
                                                label={getTranslatedLabel(`${localizationKey}.productId`, 'Product ID')}
                                                component={FormMultiColumnComboBoxVirtualFinishedProduct}
                                                validator={requiredValidator}
                                            />
                                            <Field
                                                id="fromDate"
                                                name="fromDate"
                                                label={getTranslatedLabel('common.fromDate', 'From Date')}
                                                component={FormDateTimePicker}
                                                validator={requiredValidator}
                                                disabled={editMode === 2} // Disable in edit mode
                                            />
                                        </Grid>
                                    </Grid>
                                    <Grid item container xs={6}>
                                        <Grid item xs={8}>
                                            <Field
                                                id="estimatedQuantity"
                                                name="estimatedQuantity"
                                                label={getTranslatedLabel('manufacturing.quantity', 'Quantity')}
                                                component={FormNumericTextBox}
                                            />
                                            <Field
                                                id="thruDate"
                                                name="thruDate"
                                                label={getTranslatedLabel('common.thruDate', 'Thru Date')}
                                                component={FormDateTimePicker}
                                            />
                                        </Grid>
                                    </Grid>
                                </Grid>
                                <div className="k-form-buttons">
                                    <Grid container spacing={2}>
                                        <Grid item xs={1}>
                                            <Button
                                                variant="contained"
                                                type="submit"
                                                color="success"
                                                disabled={!formRenderProps.allowSubmit || isProcessing}
                                            >
                                                {getTranslatedLabel(`${localizationKey}.addButton`, 'Submit')}
                                            </Button>
                                        </Grid>
                                        <Grid item xs={1}>
                                            <Button
                                                variant="outlined"
                                                color="secondary"
                                                onClick={cancelEdit}
                                                disabled={isProcessing}
                                            >
                                                {getTranslatedLabel('common.cancel', 'Cancel')}
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </div>
                                {isProcessing && (
                                    <LoadingComponent message={getTranslatedLabel(`${localizationKey}.processing`, 'Processing Routing Product Link...')} />
                                )}
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}