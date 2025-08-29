import React, { useState } from "react";
import { Button, Grid, Paper, Typography } from "@mui/material";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { MemoizedFormDropDownList } from "../../../app/common/form/MemoizedFormDropDownList";
import FormInput from "../../../app/common/form/FormInput";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import { v4 as uuid } from "uuid";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import {WorkEffort} from "../../../app/models/manufacturing/workEffort";
import {useAppDispatch} from "../../../app/store/configureStore";
import { requiredValidator} from "../../../app/common/form/Validators";
import {toast} from "react-toastify";
import {useAddProjectMutation, useUpdateProjectMutation} from "../../../app/store/apis/projectsApi";
import ProjectMenu from "../menu/ProjectMenu";

interface Props {
    project?: WorkEffort;
    editMode: number;
    cancelEdit: () => void;
}

export default function ProjectForm({ project, cancelEdit, editMode }: Props) {
    const dispatch = useAppDispatch();
    const [addProject, { isLoading: isCreating }] = useAddProjectMutation();
    const [updateProject, { isLoading: isUpdating }] = useUpdateProjectMutation();
    const [buttonFlag, setButtonFlag] = useState(false);
    const { getTranslatedLabel } = useTranslationHelper();

    // REFACTOR: Updated initialValues to use workEffortId instead of ProjectNum and lowercase field names.
    // Ensures consistency with field naming convention and aligns with backend workEffortId usage.
    const initialValues = editMode === 2 && project ? {
        workEffortId: project.workEffortId,
        projectName: project.projectName,
        estimatedStartDate: project.estimatedStartDate,
        estimatedCompletionDate: project.estimatedCompletionDate,
        currentStatusId: project.currentStatusId,
    } : {
        workEffortId: null, // Set to null in create mode as it’s backend-generated
        projectName: "",
        estimatedStartDate: null,
        estimatedCompletionDate: null,
        currentStatusId: "",
    };

    console.log('project', project);
    console.log('editMode', editMode);

    // REFACTOR: Updated formValidator to use lowercase field names for consistency.
    // Maintains date validation logic while aligning with new field naming convention.
    const formValidator = (values: any) => {
        const errors: any = {};
        // Validate estimatedStartDate
        if (!values.estimatedStartDate || !(values.estimatedStartDate instanceof Date) || isNaN(values.estimatedStartDate.getTime())) {
            errors.estimatedStartDate = getTranslatedLabel("project.projects.form.validation.startDate", "Please enter a valid start date.");
        }
        // Validate estimatedCompletionDate (optional, but must be valid if provided)
        if (values.estimatedCompletionDate && (!(values.estimatedCompletionDate instanceof Date) || isNaN(values.estimatedCompletionDate.getTime()))) {
            errors.estimatedCompletionDate = getTranslatedLabel("project.projects.form.validation.completionDate", "Please enter a valid completion date.");
        }
        // Validate date chronology if both dates are provided
        if (values.estimatedStartDate && values.estimatedCompletionDate) {
            const startDate = new Date(values.estimatedStartDate);
            const completionDate = new Date(values.estimatedCompletionDate);
            if (
                startDate instanceof Date &&
                completionDate instanceof Date &&
                !isNaN(startDate.getTime()) &&
                !isNaN(completionDate.getTime()) &&
                completionDate < startDate
            ) {
                errors.estimatedCompletionDate = getTranslatedLabel(
                    "project.projects.form.validation.dateOrder",
                    "Completion date cannot be earlier than start date."
                );
            }
        }
        return errors;
    };

    async function handleSubmitData(data: any) {
        setButtonFlag(true);
        try {
            let response: WorkEffort;
            if (editMode === 2) {
                // REFACTOR: Use workEffortId in payload for update, aligning with new field name.
                response = await updateProject(data).unwrap();
                toast.success(getTranslatedLabel("project.projects.form.updateSuccess", "Project updated successfully"));
            } else {
                // REFACTOR: Exclude workEffortId from payload in create mode, as it’s backend-generated.
                // Maintains consistency with previous ProjectNum exclusion logic.
                const { workEffortId, ...newProjectData } = data;
                const newProject = {
                    ...newProjectData,
                    workEffortId: uuid(),
                    workEffortTypeId: "PROJECT",
                };
                response = await addProject(newProject).unwrap();
                toast.success(getTranslatedLabel("project.projects.form.createSuccess", "Project created successfully"));
            }
            cancelEdit();
        } catch (error) {
            toast.error(getTranslatedLabel("project.projects.form.error", "Failed to process project"));
            console.error(error);
        } finally {
            setButtonFlag(false);
        }
    }

    return (
        <>
            <ProjectMenu selectedMenuItem={"projects"} />
            <Paper elevation={5} className={`div-container-withBorderCurved`} style={{ padding: '16px' }}>
                {editMode === 1 && (
                    <Typography variant="h4" color={"green"} sx={{ mb: 2 }}>
                        New Project
                    </Typography>
                )}
                {editMode === 2 && project?.workEffortId && (
                    // REFACTOR: Updated to use workEffortId in title for edit mode display.
                    <Typography variant="h4" color={"black"} sx={{ mb: 2 }}>
                        {getTranslatedLabel("project.projects.form.editTitle", `Edit Project (${project.workEffortId})`)}
                    </Typography>
                )}
                <Form
                    initialValues={initialValues}
                    validator={formValidator}
                    onSubmit={(values) => handleSubmitData(values)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className={"k-form-fieldset"}>
                                {editMode === 2 && project?.workEffortId && (
                                    // REFACTOR: Updated to use workEffortId field with lowercase naming.
                                    <Grid container spacing={2}>
                                        <Grid item xs={4}>
                                            <Field
                                                name="workEffortId"
                                                component={FormInput}
                                                type="hidden"
                                                value={project.workEffortId}
                                            />
                                        </Grid>
                                    </Grid>
                                )}
                                <Grid container spacing={2}>
                                    <Grid item xs={4} sx={{ maxWidth: '300px' }}>
                                        <Field
                                            id={"projectName"}
                                            name={"projectName"}
                                            label={getTranslatedLabel("project.projects.form.name", "Project Name")}
                                            component={FormInput}
                                            autoComplete={"off"}
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                </Grid>
                                <Grid container spacing={2}>
                                    <Grid item xs={4} sx={{ maxWidth: '300px' }}>
                                        <Field
                                            id={"estimatedStartDate"}
                                            name={"estimatedStartDate"}
                                            label={getTranslatedLabel("project.projects.form.startDate", "Start Date")}
                                            component={FormDatePicker}
                                        />
                                    </Grid>
                                </Grid>
                                <Grid container spacing={2}>
                                    <Grid item xs={4} sx={{ maxWidth: '300px' }}>
                                        <Field
                                            id={"estimatedCompletionDate"}
                                            name={"estimatedCompletionDate"}
                                            label={getTranslatedLabel("project.projects.form.completionDate", "Completion Date")}
                                            component={FormDatePicker}
                                        />
                                    </Grid>
                                </Grid>
                                <Grid container spacing={2}>
                                    <Grid item xs={4} sx={{ maxWidth: '300px' }}>
                                        <Field
                                            id={"currentStatusId"}
                                            name={"currentStatusId"}
                                            label={getTranslatedLabel("project.projects.form.status", "Status")}
                                            component={MemoizedFormDropDownList}
                                            dataItemKey={"currentStatusId"}
                                            textField={"Description"}
                                            data={[
                                                { currentStatusId: "WEPR_IN_PROGRESS", Description: "Active" },
                                                { currentStatusId: "WEPR_COMPLETE", Description: "Completed" },
                                            ]}
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                </Grid>
                                <div className="k-form-buttons">
                                    <Grid container spacing={1}>
                                        <Grid item xs={1}>
                                            <Button
                                                type={"submit"}
                                                color="success"
                                                variant="contained"
                                                disabled={!formRenderProps.allowSubmit || buttonFlag}
                                            >
                                                Submit
                                            </Button>
                                        </Grid>
                                        <Grid item xs={2}>
                                            <Button onClick={cancelEdit} variant="contained" color="error">
                                                Cancel
                                            </Button>
                                        </Grid>
                                    </Grid>
                                </div>
                                {buttonFlag && (
                                    <Grid container spacing={2}>
                                        <Grid item xs={4}>
                                            <LoadingComponent message="Processing Project..." />
                                        </Grid>
                                    </Grid>
                                )}
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}