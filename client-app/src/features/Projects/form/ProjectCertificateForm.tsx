import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useAppDispatch, useAppSelector } from "../../../app/store/configureStore";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import {Box, Button, Grid, Paper, Typography} from "@mui/material";
import LoadingButton from "@mui/lab/LoadingButton";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { resetCertificateUi, setCertificateFormEditMode } from "../slice/certificateUiSlice";

import LoadingComponent from "../../../app/layout/LoadingComponent";
import { FormTextArea } from "../../../app/common/form/FormTextArea";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import { requiredValidator } from "../../../app/common/form/Validators";
import { toast } from "react-toastify";
import {useAddProjectCertificateMutation} from "../../../app/store/apis/projectsApi";
import {FormComboBoxVirtualSupplier} from "../../../app/common/form/FormComboBoxVirtualSupplier";
import {FormComboBoxVirtualCustomer} from "../../../app/common/form/FormComboBoxVirtualCustomer";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import ProjectMenu from "../menu/ProjectMenu";
import useProjectCertificate from "../hook/useProjectCertificate";
import {CertificateItemsListMemo} from "../dashboard/CertificateItemsList";


interface ProjectCertificateFormProps {
  selectedCertificate?: {
    workEffortId: string;
    projectNum: string;
    projectName: string;
    partyId: string;
    partyName: string;
    description: string;
    estimatedStartDate: string;
    estimatedCompletionDate: string;
    statusDescription: string;
  };
  editMode: number; // 0: view, 1: create, 2: edit (CREATED), 3: edit (APPROVED), 4: edit (COMPLETED)
  cancelEdit: () => void;
}

export default function ProjectCertificateForm({ selectedCertificate, editMode, cancelEdit }: ProjectCertificateFormProps) {
  const formRef = useRef<any>(null);
  const formRef2 = useRef<boolean>(false);
  const dispatch = useAppDispatch();
  const { getTranslatedLabel } = useTranslationHelper();
  const { currentCertificateType } = useAppSelector((state) => state.certificateUi);
  const [addProjectCertificate, { isLoading: isAdding }] = useAddProjectCertificateMutation();
  //const [updateProjectCertificate, { isLoading: isUpdating }] = useUpdateProjectCertificateMutation();
  const [isSubmitting, setIsSubmitting] = useState(false);
    const [selectedMenuItem, setSelectedMenuItem] = useState("");
    const [isLoading, setIsLoading] = useState(false);

    const {
        certificate,
        setCertificate,
        formEditMode,
        setFormEditMode,
        handleCreate,
        isAddCertificateLoading,
        isUpdateCertificateLoading,
    } = useProjectCertificate({
        selectedMenuItem,
        formRef2,
        editMode,
        selectedCertificate,
        setIsLoading,
    });
    const initialFormValues = useMemo(
        () => ({
            description: selectedCertificate?.description || "",
            projectId: selectedCertificate?.projectName || "",
            partyId: selectedCertificate?.partyId || "",
            estimatedStartDate: selectedCertificate?.estimatedStartDate ? new Date(selectedCertificate.estimatedStartDate) : null,
            estimatedCompletionDate: selectedCertificate?.estimatedCompletionDate
                ? new Date(selectedCertificate.estimatedCompletionDate)
                : null,
        }),
        [selectedCertificate]
    );

  // Purpose: Placeholder for dropdowns, dynamic based on certificate type
  // Context: Replace with API calls (e.g., useFetchProjectsQuery, useFetchPartiesQuery)
  const projects = [
    { id: "PROJ1", name: "Project Alpha" },
    { id: "PROJ2", name: "Project Beta" },
  ];
  const parties = currentCertificateType === "PROCUREMENT_CERTIFICATE"
    ? [
        { id: "SUPP1", name: "Supplier A" },
        { id: "SUPP2", name: "Supplier B" },
      ]
    : [
        { id: "CONT1", name: "Contractor X" },
        { id: "CONT2", name: "Contractor Y" },
      ];

  const formKey = useMemo(() => formRef2.current.toString(), [formRef2.current]);

  const handleSubmit = useCallback(
    async (formProps: any) => {
      if (!formProps.isValid) {
        toast.error(getTranslatedLabel("certificate.form.invalid", "Form is invalid"));
        setIsSubmitting(false);
        return false;
      }
      if (isSubmitting) return false;
      setIsSubmitting(true);

      const certificateData = {
        work_effort_type_id: currentCertificateType,
        description: formProps.values.description,
        project_name: formProps.values.projectId,
        party_id: formProps.values.partyId,
        estimated_start_date: formProps.values.estimatedStartDate || null,
        estimated_completion_date: formProps.values.estimatedCompletionDate || null,
      };

      try {
        if (editMode === 1) {
          // Create mode
          const result = await addProjectCertificate(certificateData).unwrap();
          dispatch(setCertificateFormEditMode(0));
          // TODO: Navigate to items list after creation
          // navigate(`/projectCertificates/${result.work_effort_id}/items`);
        } else if (editMode >= 2) {
          // Edit mode
         /* await updateProjectCertificate({
            work_effort_id: selectedCertificate!.workEffortId,
            ...certificateData,
          }).unwrap();*/
          dispatch(setCertificateFormEditMode(0));
        }
        cancelEdit();
      } catch (error) {
        toast.error(getTranslatedLabel("certificate.form.error", "Failed to save certificate"));
      } finally {
        setIsSubmitting(false);
      }
    },
    [addProjectCertificate, currentCertificateType, editMode, dispatch, cancelEdit, selectedCertificate, getTranslatedLabel]
  );

  // Purpose: Reset form and return to list view
  // Context: Matches PurchaseOrderForm's cancel logic
  const handleCancel = useCallback(() => {
    dispatch(resetCertificateUi());
    dispatch(setCertificateFormEditMode(0));
    formRef2.current = !formRef2.current;
    cancelEdit();
  }, [dispatch, cancelEdit]);
  

  // Purpose: Initialize form with selected certificate data
  // Context: Matches PurchaseOrderForm's selectedOrder logic
  useEffect(() => {
    if (selectedCertificate) {
      formRef2.current = !formRef2.current;
    }
  }, [selectedCertificate]);

  if (isAdding ) {
    return <LoadingComponent message={getTranslatedLabel("certificate.form.saving", "Saving Certificate...")} />;
  }

  return (
      <>
          <ProjectMenu/>
          <Paper elevation={5} className="div-container-withBorderCurved">
              <Grid container spacing={2} alignItems="center" position="relative">
                  <Grid item xs={10}>
                      <Box display="flex" justifyContent="space-between">
                          <Typography
                              sx={{ fontWeight: "bold", paddingLeft: 3, fontSize: "18px", color: editMode === 1 ? "green" : "black" }}
                              variant="h6"
                          >
                              {selectedCertificate?.projectNum
                                  ? `${getTranslatedLabel("certificate.form.title", "Project Certificate No")}: ${selectedCertificate.projectNum}`
                                  : getTranslatedLabel("certificate.form.new", "New Project Certificate")}
                          </Typography>
                      </Box>
                  </Grid>
              </Grid>
              <Form
                  ref={formRef}
                  initialValues={initialFormValues}
                  key={formKey}
                  onSubmitClick={handleSubmit}
                  render={(formRenderProps) => (
                      <FormElement>
                          <fieldset className="k-form-fieldset">
                              <Grid container alignItems="start" justifyContent="start" spacing={1}>
                                  <Grid container spacing={2} alignItems="center" justifyContent="flex-start" xs={12} sx={{ paddingLeft: 3 }}>
                                      <Grid item xs={3} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                          <Field
                                              id="projectId"
                                              name="projectId"
                                              component={MemoizedFormDropDownList2}
                                              data={projects}
                                              label={getTranslatedLabel("certificate.form.project", "Project")}
                                              dataItemKey="id"
                                              textField="name"
                                              validator={requiredValidator}
                                              disabled={editMode > 3}
                                          />
                                      </Grid>

                                      <Grid item xs={3} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                          <Field
                                              id="partyId"
                                              name="partyId"
                                              component={
                                                  currentCertificateType === "PROCUREMENT_CERTIFICATE"
                                                      ? FormComboBoxVirtualSupplier
                                                      : FormComboBoxVirtualCustomer
                                              }
                                              data={parties}
                                              label={getTranslatedLabel(
                                                  "certificate.form.party",
                                                  currentCertificateType === "PROCUREMENT_CERTIFICATE" ? "Supplier" : "Contractor"
                                              )}
                                              valueField="partyId"
                                              textField="partyName"
                                              validator={requiredValidator}
                                              disabled={editMode > 3}
                                          />
                                      </Grid>
                                      <Grid item xs={3} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                          <Field
                                              name="estimatedStartDate"
                                              id="estimatedStartDate"
                                              label={getTranslatedLabel("certificate.form.startDate", "Start Date")}
                                              disabled={editMode > 1}
                                              component={FormDatePicker}
                                              validator={requiredValidator}
                                          />
                                      </Grid>
                                      <Grid item xs={3} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                          <Field
                                              name="estimatedCompletionDate"
                                              id="estimatedCompletionDate"
                                              label={getTranslatedLabel("certificate.form.startDate", "Start Date")}
                                              disabled={editMode > 1}
                                              component={FormDatePicker}
                                              validator={requiredValidator}
                                          />
                                      </Grid>
                                      <Grid item xs={6} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                          <Field
                                              id="description"
                                              name="description"
                                              label={getTranslatedLabel("certificate.form.description", "Description")}
                                              component={FormTextArea}
                                              validator={requiredValidator}
                                              disabled={editMode > 3}
                                          />
                                      </Grid>
                                      <Grid item xs={12}>
                                          <Grid container spacing={1} alignItems="center" sx={{ ml: 1, mt: 3 }}>
                                              <Grid item xs={12}>
                                                  <CertificateItemsListMemo
                                                      editMode={formEditMode}
                                                      workEffortId={certificate?.workEffortId}
                                                  />
                                              </Grid>
                                          </Grid>
                                      </Grid>
                                  </Grid>
                              </Grid>

                              {/* Purpose: Submit or cancel the form */}
                              {/* Context: Disables submit during loading, matches PurchaseOrderForm */}
                              <div className="k-form-buttons">
                                  <Grid container spacing={2}>
                                      {(editMode === 1 || editMode === 2) && (
                                          <Grid item>
                                              <LoadingButton
                                                  size="large"
                                                  type="submit"
                                                  loading={isSubmitting || isAdding}
                                                  variant="contained"
                                                  onClick={() => formRef.current?.onSubmit()}
                                              >
                                                  {getTranslatedLabel(
                                                      editMode === 1 ? "certificate.form.create" : "certificate.form.update",
                                                      editMode === 1 ? "Create Certificate" : "Update Certificate"
                                                  )}
                                              </LoadingButton>
                                          </Grid>
                                      )}
                                      <Grid item>
                                          <Button size="large" color="error" variant="outlined" onClick={handleCancel}>
                                              {getTranslatedLabel("certificate.form.cancel", "Cancel")}
                                          </Button>
                                      </Grid>
                                  </Grid>
                              </div>
                          </fieldset>
                      </FormElement>
                  )}
              />
          </Paper>
      </>
    
  );
}