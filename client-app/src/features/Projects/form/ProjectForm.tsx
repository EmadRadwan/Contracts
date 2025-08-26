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

    const initialValues = editMode === 2 && project ? project : {
        ProjectNum: null,
        ProjectName: "",
        EstimatedStartDate: null,
        EstimatedCompletionDate: null,
        CurrentStatusId: "",
    };

    // This ensures EstimatedCompletionDate is not earlier than EstimatedStartDate, aligning with Kendo Form's best practices.
    const formValidator = (values: any) => {
        const errors: any = {};

        // Validate EstimatedStartDate
        if (!values.EstimatedStartDate || !(values.EstimatedStartDate instanceof Date) || isNaN(values.EstimatedStartDate.getTime())) {
            errors.EstimatedStartDate = getTranslatedLabel("project.projects.form.validation.startDate", "Please enter a valid start date.");
        }

        // Validate EstimatedCompletionDate (optional, but must be valid if provided)
        if (values.EstimatedCompletionDate && (!(values.EstimatedCompletionDate instanceof Date) || isNaN(values.EstimatedCompletionDate.getTime()))) {
            errors.EstimatedCompletionDate = getTranslatedLabel("project.projects.form.validation.completionDate", "Please enter a valid completion date.");
        }

        // Validate date chronology if both dates are provided
        if (values.EstimatedStartDate && values.EstimatedCompletionDate) {
            const startDate = new Date(values.EstimatedStartDate);
            const completionDate = new Date(values.EstimatedCompletionDate);
            if (
                startDate instanceof Date &&
                completionDate instanceof Date &&
                !isNaN(startDate.getTime()) &&
                !isNaN(completionDate.getTime()) &&
                completionDate < startDate
            ) {
                errors.EstimatedCompletionDate = getTranslatedLabel(
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
                response = await updateProject(data).unwrap();
                toast.success(getTranslatedLabel("project.projects.form.updateSuccess", "Project updated successfully"));
            } else {
                const newProject = { ...data, WorkEffortId: uuid(), WorkEffortTypeId: "PROJECT" };
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

            <Paper elevation={5} className={`div-container-withBorderCurved`}>
                {editMode === 1 && (
                    <Typography variant="h4" color={"green"}>
                        New Project
                    </Typography>
                )}
                <Form
                    initialValues={initialValues}
                    validator={formValidator}
                    onSubmit={(values) => handleSubmitData(values)}
                    render={(formRenderProps) => (
                        <FormElement>
                            <fieldset className={"k-form-fieldset"}>
                                <Grid container spacing={2}>
                                    <Grid item xs={3}>
                                        <Field
                                            id={"ProjectNum"}
                                            name={"ProjectNum"}
                                            label={getTranslatedLabel("project.projects.form.num", "Project Number")}
                                            component={FormInput}
                                            autoComplete={"off"}
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                    <Grid item xs={3}>
                                        <Field
                                            id={"ProjectName"}
                                            name={"ProjectName"}
                                            label={getTranslatedLabel("project.projects.form.name", "Project Name")}
                                            component={FormInput}
                                            autoComplete={"off"}
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                    
                                    <Grid item xs={3}>
                                        <Field
                                            id={"EstimatedStartDate"}
                                            name={"EstimatedStartDate"}
                                            label={getTranslatedLabel("project.projects.form.startDate", "Start Date")}
                                            component={FormDatePicker}
                                        />
                                    </Grid>
                                    <Grid item xs={3}>
                                        <Field
                                            id={"EstimatedCompletionDate"}
                                            name={"EstimatedCompletionDate"}
                                            label={getTranslatedLabel("project.projects.form.completionDate", "Completion Date")}
                                            component={FormDatePicker}
                                        />
                                    </Grid>
                                    <Grid item xs={3}>
                                        <Field
                                            id={"CurrentStatusId"}
                                            name={"CurrentStatusId"}
                                            label={getTranslatedLabel("project.projects.form.status", "Status")}
                                            component={MemoizedFormDropDownList}
                                            dataItemKey={"StatusId"}
                                            textField={"Description"} // Assuming StatusItem has a Description field
                                            data={[{ CurrentStatusId: "PRJ_ACTIVE", Description: "Active" }, { CurrentStatusId: "PRJ_COMPLETED", Description: "Completed" }]} // Placeholder, replace with useFetchStatusesQuery
                                            validator={requiredValidator}
                                        />
                                    </Grid>
                                </Grid>
                                <div className="k-form-buttons">
                                    <Grid container spacing={1}>
                                        <Grid item xs={2}>
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
                                {buttonFlag && <LoadingComponent message="Processing Project..." />}
                            </fieldset>
                        </FormElement>
                    )}
                />
            </Paper>
        </>
    );
}