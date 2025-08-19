import React, { useState } from 'react';
import { Box, Button, Grid, Paper, Typography } from '@mui/material';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { toast } from 'react-toastify';
import {useAddRoutingTaskMutation, useUpdateRoutingTaskMutation} from "../../../app/store/apis";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import FormInput from "../../../app/common/form/FormInput";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import {requiredValidator} from "../../../app/common/form/Validators";
import {MemoizedFormDropDownList} from "../../../app/common/form/MemoizedFormDropDownList";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {useLocation} from "react-router-dom";
import RoutingTaskMenu from "../menu/RoutingTaskMenu";


interface WorkEffort {
    workEffortId: string | null;
    workEffortTypeId: string;
    workEffortName: string;
    workEffortPurposeTypeId: string | null;
    fixedAssetId: string | null;
    estimatedSetupMillis: number | null;
    estimatedMilliSeconds: number | null;
    estimateCalcMethod: string | null;
    currentStatusId: string | null;
}

interface Props {
    selectedRoutingTask: WorkEffort | undefined;
    editMode: number; // 1 for create, 2 for edit
    cancelEdit: () => void;
}

export default function EditRoutingTask({ selectedRoutingTask, editMode, cancelEdit }: Props) {
    //const { data: purposeTypes, isFetching: purposeTypesFetching, error: purposeTypesError } = useFetchWorkEffortPurposeTypesQuery({ filter: 'ROU%' });
    //const { data: fixedAssets, isFetching: fixedAssetsFetching, error: fixedAssetsError } = useFetchFixedAssetsQuery({ fixedAssetTypeId: 'GROUP_EQUIPMENT' });

    const [isProcessing, setIsProcessing] = useState(false);
    const [addRoutingTask, { isLoading: addLoading }] = useAddRoutingTaskMutation();
    const [updateRoutingTask, { isLoading: updateLoading }] = useUpdateRoutingTaskMutation();
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'manufacturing.routingTask.form';
    const location = useLocation(); 


    const purposeTypeItems = [
        { workEffortPurposeTypeId: null, description: '' },
        { workEffortPurposeTypeId: 'ROU_MANUFACTURING', description: 'Manufacturing' },
        { workEffortPurposeTypeId: 'ROU_ASSEMBLING', description: 'Assembling' },
        { workEffortPurposeTypeId: 'ROU_SUBCONTRACTING', description: 'Sub-contracting' },
    ];

    const fixedAssetItems = [
        { fixedAssetId: null, description: '' },
        { fixedAssetId: 'BLENDER_01', description: 'Blender 01 [BLENDER_01]' },
        { fixedAssetId: 'BLISTER_PACKAGING_MACHINE_01', description: 'Blister Packaging Machine 01 [BLISTER_PACKAGING_MACHINE_01]' },
        { fixedAssetId: 'BLISTER_PACKAGING_MACHINE_02', description: 'Blister Packaging Machine 02 [BLISTER_PACKAGING_MACHINE_02]' },
        { fixedAssetId: 'CAPSULE_FILLER_01', description: 'Capsule Filler 01 [CAPSULE_FILLER_01]' },
        { fixedAssetId: 'CAPSULE_FILLING_MACHINE_01', description: 'Capsule Filling Machine 01 [CAPSULE_FILLING_MACHINE_01]' },
        { fixedAssetId: 'CAPSULE_FILLING_MACHINE_02', description: 'Capsule Filling Machine 02 [CAPSULE_FILLING_MACHINE_02]' },
        { fixedAssetId: 'CAPSULE_SEALER_01', description: 'Capsule Sealer 01 [CAPSULE_SEALER_01]' },
        { fixedAssetId: 'COATING_MACHINE_01', description: 'Coating Machine 01 [COATING_MACHINE_01]' },
        { fixedAssetId: 'COATING_MACHINE_02', description: 'Coating Machine 02 [COATING_MACHINE_02]' },
    ];



    const handleSubmitData = async (data: WorkEffort) => {
        setIsProcessing(true);
        try {
            const payload = {
                workEffortId: editMode === 2 ? selectedRoutingTask?.workEffortId : null,
                workEffortTypeId: editMode === 1 ? 'ROU_TASK' : data.workEffortTypeId,
                workEffortName: data.workEffortName,
                workEffortPurposeTypeId: data.workEffortPurposeTypeId,
                fixedAssetId: data.fixedAssetId,
                // Convert minutes to milliseconds and send as numbers to match double? type
                estimatedSetupMillis: data.estimatedSetupMillis != null ? data.estimatedSetupMillis * 60000 : null,
                estimatedMilliSeconds: data.estimatedMilliSeconds != null ? data.estimatedMilliSeconds * 60000 : null,
                currentStatusId: editMode === 1 ? 'ROU_ACTIVE' : data.currentStatusId,
            };
            if (editMode === 2) {
                await updateRoutingTask(payload).unwrap();
                toast.success(getTranslatedLabel(`${localizationKey}.successUpdate`, 'Routing Task Updated Successfully!'));
            } else {
                await addRoutingTask(payload).unwrap();
                toast.success(getTranslatedLabel(`${localizationKey}.successCreate`, 'Routing Task Created Successfully!'));
            }
            cancelEdit();
        } catch (e: any) {
            const errorMsg =
                e.data?.error || e.data?.title || (e.data?.errors ? e.data.errors.join(', ') : e.message) || 'Operation failed.';
            console.error('Submission error:', { error: e, payload });
            toast.error(errorMsg);
        } finally {
            setIsProcessing(false);
        }
    };

    const initialValues = {
        workEffortId: selectedRoutingTask?.workEffortId || null,
        workEffortTypeId: editMode === 1 ? 'ROU_TASK' : selectedRoutingTask?.workEffortTypeId || 'ROU_TASK',
        workEffortName: selectedRoutingTask?.workEffortName || '',
        workEffortPurposeTypeId: selectedRoutingTask?.workEffortPurposeTypeId || 'ROU_MANUFACTURING',
        fixedAssetId: selectedRoutingTask?.fixedAssetId || null,
        estimatedSetupMillis: selectedRoutingTask?.estimatedSetupMillis != null ? selectedRoutingTask.estimatedSetupMillis / 60000 : null,
        estimatedMilliSeconds: selectedRoutingTask?.estimatedMilliSeconds != null ? selectedRoutingTask.estimatedMilliSeconds / 60000 : null,
        currentStatusId: editMode === 1 ? 'ROU_ACTIVE' : selectedRoutingTask?.currentStatusId || null,
    };

    const titleText =
        editMode === 2 && selectedRoutingTask?.workEffortId
            ? getTranslatedLabel(
                `${localizationKey}.editTitle`,
                `Edit Routing Task (${selectedRoutingTask.workEffortId})`
            )
            : getTranslatedLabel(`${localizationKey}.createTitle`, 'New Routing Task');


    return (
        <Paper elevation={5} className="div-container-withBorderCurved" style={{ padding: '16px' }}>
            <Grid container spacing={2}>
                <Grid item xs={8}>
                    <Box display="flex" justifyContent="space-between">
                        <Typography sx={{ p: 2 }} variant="h3" color={editMode === 2 ? 'black' : 'green'}>
                            {titleText}
                        </Typography>
                    </Box>
                </Grid>
            </Grid>
            {editMode === 2 && selectedRoutingTask?.workEffortId && (
                <RoutingTaskMenu workEffortId={selectedRoutingTask.workEffortId} selectedMenuItem={location.pathname} />
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
                                            <>
                                                <Field
                                                    name="workEffortTypeId"
                                                    component={FormInput}
                                                    type="hidden"
                                                    value="ROU_TASK"
                                                />
                                                <Field
                                                    name="currentStatusId"
                                                    component={FormInput}
                                                    type="hidden"
                                                    value="ROU_ACTIVE"
                                                />
                                            </>
                                        )}
                                        {editMode === 2 && (
                                            <Field
                                                name="workEffortId"
                                                component={FormInput}
                                                type="hidden"
                                                value={selectedRoutingTask?.workEffortId}
                                            />
                                        )}
                                        <Field
                                            id="workEffortName"
                                            name="workEffortName"
                                            label={getTranslatedLabel(`${localizationKey}.taskName`, 'Task Name')}
                                            component={FormInput}
                                            validator={requiredValidator}
                                        />
                                        <Field
                                            id="description"
                                            name="description"
                                            label={getTranslatedLabel(`${localizationKey}.description`, 'Description')}
                                            component={FormInput}
                                        />
                                        <Field
                                            id="estimatedSetupMillis"
                                            name="estimatedSetupMillis"
                                            label={getTranslatedLabel(`${localizationKey}.estimatedSetupMinutes`, 'Estimated Setup Minutes')}
                                            component={FormNumericTextBox}
                                        />
                                        <Field
                                            id="estimatedMilliSeconds"
                                            name="estimatedMilliSeconds"
                                            label={getTranslatedLabel(`${localizationKey}.estimatedUnitRunMinutes`, 'Estimated Unit Run Minutes')}
                                            component={FormNumericTextBox}
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                </Grid>
                                <Grid item container xs={6}>
                                    <Grid item xs={8}>
                                        <Field
                                            id="workEffortPurposeTypeId"
                                            name="workEffortPurposeTypeId"
                                            label={getTranslatedLabel(`${localizationKey}.taskPurpose`, 'Task Purpose')}
                                            component={MemoizedFormDropDownList}
                                            data={purposeTypeItems}
                                            dataItemKey="workEffortPurposeTypeId"
                                            textField="description"
                                            allowEmpty={true}
                                        />
                                        <Field
                                            id="fixedAssetId"
                                            name="fixedAssetId"
                                            label={getTranslatedLabel(`${localizationKey}.fixedAssetId`, 'Fixed Asset')}
                                            component={MemoizedFormDropDownList}
                                            data={fixedAssetItems}
                                            dataItemKey="fixedAssetId"
                                            textField="description"
                                            allowEmpty={true}
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
                                <LoadingComponent message={getTranslatedLabel(`${localizationKey}.processing`, 'Processing Routing Task...')} />
                            )}
                        </fieldset>
                    </FormElement>
                )}
            />
        </Paper>
    );
}