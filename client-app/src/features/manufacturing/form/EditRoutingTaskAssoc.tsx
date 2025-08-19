import React, { useState } from 'react';
import { Box, Grid, Paper, Typography, Button } from '@mui/material';
import { Field, Form, FormElement } from '@progress/kendo-react-form';
import { toast } from 'react-toastify';
import { useParams } from 'react-router-dom';
import FormDateTimePicker from "../../../app/common/form/FormDateTimePicker";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import FormNumericTextBox from "../../../app/common/form/FormNumericTextBox";
import {requiredValidator} from "../../../app/common/form/Validators";
import RoutingMenu from "../menu/RoutingMenu";

import {FormComboBoxVirtualRoutingTasks} from "../../../app/common/form/FormComboBoxVirtualRoutingTasks";
import ManufacturingMenu from "../menu/ManufacturingMenu";
import {useCreateWorkEffortAssocMutation, useUpdateWorkEffortAssocMutation} from "../../../app/store/apis";

interface RoutingTaskAssoc {
    workEffortIdTo: string;
    fromDate: string;
    sequenceNum: number | null;
    thruDate: string | null;
}

interface Props {
    taskAssoc?: RoutingTaskAssoc | null;
    editMode: number; // 1: new, 2: edit
    cancelEdit: () => void;
}

export default function EditRoutingTaskAssoc({ taskAssoc, editMode, cancelEdit }: Props) {
    const { workEffortId } = useParams<{ workEffortId: string }>();
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = 'manufacturing.routingTaskAssoc.form';
    const [isProcessing, setIsProcessing] = useState(false);
    const [addRoutingTaskAssoc, { isLoading: addLoading }] = useCreateWorkEffortAssocMutation();
    const [updateRoutingTaskAssoc, { isLoading: updateLoading }] = useUpdateWorkEffortAssocMutation();

    const initialValues: RoutingTaskAssoc = taskAssoc
        ? {
            workEffortIdFrom: taskAssoc.workEffortIdFrom,
            workEffortIdTo: taskAssoc.workEffortIdTo,
            sequenceNum: taskAssoc.sequenceNum,
            fromDate: new Date(taskAssoc.fromDate),
            thruDate: taskAssoc.thruDate ? new Date(taskAssoc.thruDate) : null,
        }
        : {
            workEffortIdFrom: workEffortId!,
            workEffortIdTo: '',
            sequenceNum: null,
            fromDate: new Date(),
            thruDate: null,
        };



    const handleSubmitData = async (data: RoutingTaskAssoc) => {
        console.log('handleSubmitData', data)
        setIsProcessing(true);
        try {
            const payload = {
                workEffortIdFrom: workEffortId!,
                workEffortIdTo: typeof data.workEffortIdTo === 'string' ? data.workEffortIdTo : data.workEffortIdTo.workEffortIdTo,
                workEffortAssocTypeId: 'ROUTING_COMPONENT',
                fromDate: data.fromDate || new Date(),
                sequenceNum: data.sequenceNum || null,
                thruDate: data.thruDate || null,
            };

            if (editMode === 2) {
                // REFACTOR: Use update mutation for edit mode
                // Purpose: Handles updates in edit mode, similar to ProductPriceForm's updateProductPrice
                // Benefit: Consistent API interaction with RTK Query
                await updateRoutingTaskAssoc(payload).unwrap();
                toast.success(getTranslatedLabel(`${localizationKey}.successUpdate`, 'Routing Task Updated Successfully!'));
            } else {
                await addRoutingTaskAssoc(payload).unwrap();
                toast.success(getTranslatedLabel(`${localizationKey}.success`, 'Routing Task Added Successfully!'));
            }
            cancelEdit(); // Return to list view after submission
        } catch (e: any) {
            const errorMsg = e.data?.title || e.data?.errors?.join(', ') || 'Operation failed.';
            toast.error(errorMsg);
        } finally {
            setIsProcessing(false);
        }
    };

console.log('taskAssoc', taskAssoc)

    return (
        <>
            <ManufacturingMenu />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container spacing={2}>
                    <Grid item xs={8}>
                        <Box display="flex" justifyContent="space-between">
                            <Typography sx={{ p: 2 }} variant="h3" color="green">
                                {getTranslatedLabel(`${localizationKey}.title`, 'New Routing Task Association')}
                            </Typography>
                        </Box>
                    </Grid>
                </Grid>
                <RoutingMenu workEffortId={workEffortId} selectedMenuItem="routingTaskAssoc" />
                <Form
                    initialValues={initialValues}
                    onSubmit={(values) => handleSubmitData(values as RoutingTaskAssoc, 'N')}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className="k-form-fieldset">
                                <Grid container spacing={2}>
                                    <Grid item container xs={6}>
                                        <Grid item xs={8}>
                                            <Field
                                                name="workEffortIdTo"
                                                id="workEffortIdTo"
                                                label={getTranslatedLabel(`${localizationKey}.routingTaskId`, 'Routing Task ID')}
                                                component={FormComboBoxVirtualRoutingTasks}
                                                validator={requiredValidator}
                                            />
                                            <Field
                                                id="fromDate"
                                                name="fromDate"
                                                label={getTranslatedLabel('common.fromDate', 'From Date')}
                                                component={FormDateTimePicker}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                    </Grid>
                                    <Grid item container xs={6}>
                                        <Grid item xs={8}>
                                            <Field
                                                id="sequenceNum"
                                                name="sequenceNum"
                                                label={getTranslatedLabel('common.sequenceNum', 'Sequence Number')}
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
        </>
    );
}