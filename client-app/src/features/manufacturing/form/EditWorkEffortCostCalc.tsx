import React, { useState } from 'react';
import { Box, Button, Grid, Paper, Typography } from '@mui/material';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { toast } from 'react-toastify';
import {requiredValidator} from "../../../app/common/form/Validators";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import FormInput from "../../../app/common/form/FormInput";
import {useCreateWorkEffortCostCalcMutation, useUpdateWorkEffortCostCalcMutation} from "../../../app/store/apis";

// REFACTOR: Define interface for WorkEffortCostCalc, aligning with OFBiz form fields
interface Props {
    costCalc: WorkEffortCostCalc | undefined;
    editMode: number; // 1 for create, 2 for edit
    cancelEdit: () => void;
    workEffortId: string;
    workEffortName: string; // REFACTOR: Added to display routing task context
}

// REFACTOR: Props interface for the component
interface Props {
    costCalc: WorkEffortCostCalc | undefined;
    editMode: number; // 1 for create, 2 for edit
    cancelEdit: () => void;
    workEffortId: string;
}

const costComponentTypes = [
    { costComponentTypeId: 'FOH_GENERAL', description: 'Manufacturing Overhead - General' },
    { costComponentTypeId: 'LABOR_COST', description: 'Labor Cost' },
];

// REFACTOR: Static data for costComponentCalcs dropdown
// Purpose: Temporarily replaces Redux-based data fetching with static list
const costComponentCalcs = [
    { costComponentCalcId: 'DIRECT_LABOR_HOUR', description: 'Direct Labor Cost for Hour (50 EGP/hr)' },
    { costComponentCalcId: 'FOH_GENERAL_HOUR', description: 'Factory Overhead (FOH) for General Tasks (70 EGP/hr)' },
];


// REFACTOR: EditWorkEffortCostCalc component for creating/editing WorkEffortCostCalc records
export function EditWorkEffortCostCalc({ costCalc, editMode, cancelEdit, workEffortId, workEffortName }: Props) {
    const [isProcessing, setIsProcessing] = useState(false);
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'manufacturing.routingTaskCosts.form';
    
    // REFACTOR: RTK Query mutations for create/edit
    const [createWorkEffortCostCalc, { isLoading: createLoading }] = useCreateWorkEffortCostCalcMutation();
    const [updateWorkEffortCostCalc, { isLoading: updateLoading }] = useUpdateWorkEffortCostCalcMutation();


    const initialValues = {
        workEffortId: costCalc?.workEffortId || workEffortId,
        costComponentTypeId: costCalc?.costComponentTypeId || '',
        costComponentCalcId: costCalc?.costComponentCalcId || '',
        fromDate: costCalc?.fromDate || new Date(),
    };

    // REFACTOR: Handle form submission
    // Purpose: Submits create or update request based on editMode
    const handleSubmitData = async (data: WorkEffortCostCalc) => {
        setIsProcessing(true);
        try {
            const payload = { ...data };
            if (editMode === 2) {
                await updateWorkEffortCostCalc(payload).unwrap();
                toast.success(getTranslatedLabel(`${localizationKey}.successUpdate`, 'Routing Task Cost Updated Successfully!'));
            } else {
                await createWorkEffortCostCalc(payload).unwrap();
                toast.success(getTranslatedLabel(`${localizationKey}.successCreate`, 'Routing Task Cost Created Successfully!'));
            }
            cancelEdit();
        } catch (e: any) {
            const errorMsg = e.data?.error || e.data?.title || (e.data?.errors ? e.data.errors.join(', ') : e.message) || 'Operation failed.';
            console.error('Submission error:', { error: e, payload });
            toast.error(errorMsg);
        } finally {
            setIsProcessing(false);
        }
    };

    // REFACTOR: Set form title based on mode
    // Purpose: Matches OFBiz form title behavior
    const titleText =
        editMode === 2 && costCalc?.costComponentCalcId
            ? getTranslatedLabel(`${localizationKey}.editTitle`, `Edit Routing Task Cost (${costCalc.costComponentCalcId})`)
            : getTranslatedLabel(`${localizationKey}.createTitle`, 'New Routing Task Cost');



    return (
        <Paper elevation={5} className="div-container-withBorderCurved" style={{ padding: '16px' }}>
            <Grid container spacing={2}>
                <Grid item xs={12}>
                    <Box display="flex" justifyContent="space-between">
                        <Typography sx={{ p: 2 }} variant="h3" color={editMode === 2 ? 'black' : 'green'}>
                            {titleText}
                        </Typography>
                    </Box>
                    <Typography sx={{ p: 2 }} variant="body1">
                        {getTranslatedLabel(
                            `${localizationKey}.context`,
                            `Routing Task: ${workEffortId} - ${workEffortName}`
                        )}
                    </Typography>
                </Grid>
            </Grid>
            <Form
                initialValues={initialValues}
                onSubmit={(values) => handleSubmitData(values as WorkEffortCostCalc)}
                render={(formRenderProps) => (
                    <FormElement>
                        <fieldset className="k-form-fieldset">
                            <Grid container spacing={2}>
                                {/* REFACTOR: Hidden workEffortId field */}
                                <Grid item xs={12}>
                                    <Field
                                        name="workEffortId"
                                        component={FormInput}
                                        type="hidden"
                                        value={workEffortId}
                                    />
                                </Grid>
                                {/* REFACTOR: Row 1 - costComponentTypeId dropdown */}
                                <Grid item xs={6}>
                                    <Field
                                        id="costComponentTypeId"
                                        name="costComponentTypeId"
                                        label={getTranslatedLabel(`${localizationKey}.costComponentType`, 'Cost Component Type *')}
                                        component={MemoizedFormDropDownList}
                                        data={costComponentTypes}
                                        dataItemKey="costComponentTypeId"
                                        textField="description"
                                        validator={requiredValidator}
                                        disabled={editMode === 2}
                                    />
                                </Grid>
                                {/* REFACTOR: Row 2 - costComponentCalcId dropdown */}
                                <Grid item xs={6}>
                                    <Field
                                        id="costComponentCalcId"
                                        name="costComponentCalcId"
                                        label={getTranslatedLabel(`${localizationKey}.costComponentCalc`, 'Cost Component Calc *')}
                                        component={MemoizedFormDropDownList}
                                        data={costComponentCalcs}
                                        dataItemKey="costComponentCalcId"
                                        textField="description"
                                        validator={requiredValidator}
                                        disabled={editMode === 2}
                                    />
                                </Grid>
                                {/* REFACTOR: Row 3 - fromDate */}
                                <Grid item xs={6}>
                                    <Field
                                        id="fromDate"
                                        name="fromDate"
                                        label={getTranslatedLabel(`${localizationKey}.fromDate`, 'From Date *')}
                                        component={FormDatePicker}
                                        validator={requiredValidator}
                                        format="yyyy-MM-dd"
                                        disabled={editMode === 2}
                                    />
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
                                            {getTranslatedLabel(`${localizationKey}.submit`, 'Submit')}
                                        </Button>
                                    </Grid>
                                    <Grid item xs={1}>
                                        <Button
                                            onClick={cancelEdit}
                                            color="error"
                                            variant="contained"
                                        >
                                            {getTranslatedLabel(`${localizationKey}.cancel`, 'Cancel')}
                                        </Button>
                                    </Grid>
                                </Grid>
                            </div>
                            {isProcessing && (
                                <LoadingComponent
                                    message={getTranslatedLabel(`${localizationKey}.processing`, 'Processing Routing Task Cost...')}
                                />
                            )}
                        </fieldset>
                    </FormElement>
                )}
            />
        </Paper>
    );
}
// REFACTOR: Ensure component is memoized to prevent unnecessary re-renders
export default React.memo(EditWorkEffortCostCalc);