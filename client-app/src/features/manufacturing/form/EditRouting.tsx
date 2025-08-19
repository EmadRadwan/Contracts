import React, {useState} from 'react';
import {Box, Button, Grid, Paper, Typography} from '@mui/material';
import {Field, Form, FormElement} from '@progress/kendo-react-form';
import {toast} from 'react-toastify';
import {useAddRoutingMutation, useUpdateRoutingMutation} from "../../../app/store/apis";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import FormInput from "../../../app/common/form/FormInput";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import {requiredValidator} from "../../../app/common/form/Validators";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import CatalogMenu from "../../catalog/menu/CatalogMenu";
import {useLocation, useParams} from "react-router-dom";
import RoutingMenu from "../menu/RoutingMenu";
import ManufacturingMenu from "../menu/ManufacturingMenu";

interface WorkEffort {
    workEffortId: string | null;
    workEffortTypeId: string;
    workEffortName: string;
    description: string | null;
    quantityToProduce: number | null;
    currentStatusId: string | null;
}

interface Props {
    selectedRouting: WorkEffort | null;
    editMode: number; // 1 for create, 2 for edit
    cancelEdit: () => void;
}

export default function EditRouting({selectedRouting, editMode, cancelEdit}: Props) {
    const [isProcessing, setIsProcessing] = useState(false);
    const [addRouting, {isLoading: addLoading}] = useAddRoutingMutation();
    const [updateRouting, {isLoading: updateLoading}] = useUpdateRoutingMutation();
    const {getTranslatedLabel} = useTranslationHelper();
    const localizationKey = 'manufacturing.routing.form';
    const location = useLocation();
    const {workEffortId} = useParams<{ workEffortId: string }>();


    const effectiveEditMode = editMode || (workEffortId ? 2 : 1);
    const effectiveSelectedRouting = workEffortId ? selectedRouting || {workEffortId} : selectedRouting;

    // REFACTOR: Initialize form with URL-derived workEffortId, ensuring correct create/edit mode
    const initialValues: WorkEffort = {
        workEffortId: effectiveSelectedRouting?.workEffortId || null,
        workEffortTypeId: 'ROUTING',
        workEffortName: effectiveSelectedRouting?.workEffortName || '',
        description: effectiveSelectedRouting?.description || null,
        quantityToProduce: effectiveSelectedRouting?.quantityToProduce || 1,
        currentStatusId: effectiveSelectedRouting?.currentStatusId || 'ROU_ACTIVE',
    };

    const handleSubmitData = async (data: Partial<WorkEffort>) => {
        try {
            const payload: WorkEffort = {
                workEffortId: effectiveEditMode === 2 ? effectiveSelectedRouting?.workEffortId || null : null,
                workEffortTypeId: effectiveEditMode === 1 ? 'ROUTING' : data.workEffortTypeId || 'ROUTING',
                workEffortName: data.workEffortName || '',
                description: data.description || null,
                quantityToProduce: data.quantityToProduce || 1,
                currentStatusId: data.currentStatusId || 'ROU_ACTIVE',
            };
            if (effectiveEditMode === 2) {
                await updateRouting(payload).unwrap();
                toast.success('Routing Updated Successfully!');
            } else {
                await addRouting(payload).unwrap();
                toast.success('Routing Created Successfully!');
            }
            cancelEdit();
        } catch (e: any) {
            const errorMsg = e.data?.title || e.data?.errors?.join(', ') || 'Operation failed.';
            toast.error(errorMsg);
        }
    };

    return (
        <>
            <ManufacturingMenu />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container spacing={2}>
                    <Grid item xs={8}>
                        <Box display="flex" justifyContent="space-between">
                            <Typography sx={{p: 2}} variant="h3" color={effectiveEditMode === 2 ? 'black' : 'green'}>
                                {effectiveEditMode === 2
                                    ? `Edit Routing [ID ${effectiveSelectedRouting?.workEffortId}] ${effectiveSelectedRouting?.workEffortName}`
                                    : 'New Routing'}
                            </Typography>

                        </Box>
                    </Grid>
                </Grid>
                {effectiveEditMode === 2 && effectiveSelectedRouting?.workEffortId && (
                    <RoutingMenu workEffortId={effectiveSelectedRouting.workEffortId}
                                 selectedMenuItem={location.pathname}/>
                )}
                <Form
                    initialValues={initialValues}
                    onSubmit={(values) => handleSubmitData(values as WorkEffort)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className="k-form-fieldset">
                                <Grid container spacing={2}>
                                    <Grid item container xs={6}>
                                        <Grid item xs={8}>
                                            {editMode === 1 && (
                                                <Field
                                                    name="workEffortTypeId"
                                                    component={FormInput}
                                                    type="hidden"
                                                    value="ROUTING"
                                                />
                                            )}
                                            {editMode === 2 && (
                                                <Field
                                                    name="workEffortId"
                                                    component={FormInput}
                                                    type="hidden"
                                                    value={selectedRouting?.workEffortId}
                                                />
                                            )}
                                            <Field
                                                id="workEffortName"
                                                name="workEffortName"
                                                label={getTranslatedLabel(`${localizationKey}.routingName`, 'Routing Name')}
                                                component={FormInput}
                                                validator={requiredValidator}
                                                //disabled={isFieldDisabled}
                                            />
                                            <Field
                                                id="description"
                                                name="description"
                                                label={getTranslatedLabel(`${localizationKey}.description`, 'Description')}
                                                component={FormInput}
                                                //disabled={isFieldDisabled}
                                            />
                                            <Field
                                                id="quantityToProduce"
                                                name="quantityToProduce"
                                                label={getTranslatedLabel(`${localizationKey}.quantityToProduce`, 'Quantity to Produce')}
                                                component={FormNumericTextBox}
                                                validator={requiredValidator}
                                                //disabled={isFieldDisabled}
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
                                                Submit
                                            </Button>
                                        </Grid>
                                        <Grid item xs={1}>
                                            <Button
                                                onClick={cancelEdit}
                                                color="error"
                                                variant="contained"
                                            >
                                                Cancel
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </div>
                                {isProcessing && (
                                    <LoadingComponent message="Processing Routing..."/>
                                )}
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}