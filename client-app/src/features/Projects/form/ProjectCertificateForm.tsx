import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useAppDispatch, useAppSelector } from "../../../app/store/configureStore";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import {Box, Button, Grid, Paper, Typography} from "@mui/material";
import LoadingButton from "@mui/lab/LoadingButton";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { resetCertificateUi, setCertificateFormEditMode } from "../slice/certificateUiSlice";

import LoadingComponent from "../../../app/layout/LoadingComponent";
import { requiredValidator } from "../../../app/common/form/Validators";
import { toast } from "react-toastify";
import {useAddProjectCertificateMutation} from "../../../app/store/apis/projectsApi";
import {FormComboBoxVirtualSupplier} from "../../../app/common/form/FormComboBoxVirtualSupplier";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import ProjectMenu from "../menu/ProjectMenu";
import useProjectCertificate from "../hook/useProjectCertificate";
import {CertificateItemsListMemo} from "../dashboard/CertificateItemsList";
import {FormComboBoxVirtualProject} from "../../../app/common/form/FormComboBoxVirtualProject";
import {FormComboBoxVirtualContractor} from "../../../app/common/form/FormComboBoxVirtualContractor";
import FormInput from "../../../app/common/form/FormInput";
import {resetUiCertificateItems} from "../slice/certificateItemsUiSlice";


interface ProjectCertificateFormProps {
  selectedCertificate?: {
    workEffortId: string;
    projectNum: string;
    projectName: string;
      partyIdSupplier?: string;
      partyIdContractor?: string; 
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

    const initialFormValues = useMemo(() => {
        if (editMode === 1) {
            const now = new Date();
            const oneWeekAgo = new Date(now);
            oneWeekAgo.setDate(now.getDate() - 7);
            return {
                description: "",
                projectId: "",
                partyIdSupplier: "",
                partyIdContractor: "",
                estimatedStartDate: oneWeekAgo,
                estimatedCompletionDate: now,
            };
        }
        return {
            description: selectedCertificate?.description || "",
            projectId: selectedCertificate?.projectName || "",
            partyIdSupplier: currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE" || currentCertificateType === "EXTERNAL_SUPPLY_SALE_CERTIFICATE" ? selectedCertificate?.partyIdSupplier || "" : "",
            partyIdContractor: currentCertificateType === "COMPANY_SUPPLY_SALE_CERTIFICATE" || currentCertificateType === "CONTRACTOR_PURCHASE_CERTIFICATE" || currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE" || currentCertificateType === "EXTERNAL_SUPPLY_SALE_CERTIFICATE" ? selectedCertificate?.partyIdContractor || "" : "",
            estimatedStartDate: selectedCertificate?.estimatedStartDate ? new Date(selectedCertificate.estimatedStartDate) : null,
            estimatedCompletionDate: selectedCertificate?.estimatedCompletionDate ? new Date(selectedCertificate.estimatedCompletionDate) : null,
        };
    }, [editMode, selectedCertificate, currentCertificateType]);



    const formKey = useMemo(() => formRef2.current.toString(), [formRef2.current]);
    const showSupplier = ["SUPPLY_PROCUREMENT_CERTIFICATE", "EXTERNAL_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType);
    const showContractor = ["COMPANY_SUPPLY_SALE_CERTIFICATE", "CONTRACTOR_PURCHASE_CERTIFICATE", "WORKMANSHIP_CONTRACTING_CERTIFICATE", "EXTERNAL_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType);

    // In ProjectCertificateForm.tsx
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
                values: {
                    workEffortTypeId: currentCertificateType,
                    description: formProps.values.description,
                    projectId: formProps.values.projectId,
                    partyIdSupplier: formProps.values.partyIdSupplier || undefined,
                    partyIdContractor: formProps.values.partyIdContractor || undefined,
                    estimatedStartDate: formProps.values.estimatedStartDate || null,
                    estimatedCompletionDate: formProps.values.estimatedCompletionDate || null,
                },
                selectedMenuItem: editMode === 1 ? "Create Certificate" : "Update Certificate",
            };

            try {
                await handleCreate(certificateData);
                dispatch(setCertificateFormEditMode(0));
                cancelEdit();
            } catch (error) {
                toast.error(getTranslatedLabel("certificate.form.error", "Failed to save certificate"));
            } finally {
                setIsSubmitting(false);
            }
        },
        [handleCreate, currentCertificateType, editMode, dispatch, cancelEdit, getTranslatedLabel]
    );

    const handleCancel = useCallback(() => {
        formRef2.current = !formRef2.current; // Update formRef2 to force formKey change
        dispatch(resetCertificateUi());
        dispatch(setCertificateFormEditMode(0));
        dispatch(resetUiCertificateItems()); // Clear certificateItemsList data
        cancelEdit();
    }, [dispatch, cancelEdit]);


    // Purpose: Initialize form with selected certificate data
  // Context: Matches PurchaseOrderForm's selectedOrder logic
  useEffect(() => {
    if (selectedCertificate) {
      formRef2.current = !formRef2.current;
    }
  }, [selectedCertificate]);

    const getCertificateTypeDisplayText = (type: string) => {
        return type
            .split('_')
            .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
            .join(' ');
    };

  if (isAddCertificateLoading ) {
    return <LoadingComponent message={getTranslatedLabel("certificate.form.saving", "Saving Certificate...")} />;
  }

    return (
        <>
            <ProjectMenu />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container spacing={2} alignItems="center" position="relative">
                    <Grid item xs={10}>
                        <Box display="flex" justifyContent="space-between">
                            <Typography
                                sx={{ fontWeight: "bold", paddingLeft: 3, fontSize: "18px", color: editMode === 1 ? "green" : "black" }}
                                variant="h6"
                            >
                                {selectedCertificate?.projectNum
                                    ? `${getTranslatedLabel("certificate.form.title", "Project Certificate No")}: ${selectedCertificate.projectNum} (${getCertificateTypeDisplayText(currentCertificateType)})`
                                    : `${getTranslatedLabel("certificate.form.new", "New Project Certificate")} (${getCertificateTypeDisplayText(currentCertificateType)})`}
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
                                    <Grid container spacing={2} alignItems="center" justifyContent="flex-start" sx={{ paddingLeft: 3 }}>
                                        {/* REFACTOR: Reduced xs values for all fields except description */}
                                        {/* Purpose: Minimize space usage while keeping description at xs=6 */}
                                        {/* Context: Adjusts layout for EXTERNAL_SUPPLY_SALE_CERTIFICATE (both supplier and contractor) and other types */}
                                        <Grid item xs={2} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                            <Field
                                                id="projectId"
                                                name="projectId"
                                                component={FormComboBoxVirtualProject}
                                                label={getTranslatedLabel("certificate.form.project", "Project")}
                                                dataItemKey="projectId"
                                                textField="ProjectName"
                                                validator={requiredValidator}
                                                disabled={editMode > 3}
                                            />
                                        </Grid>
                                        {showSupplier && (
                                            <Grid item xs={showContractor ? 2 : 3} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                                <Field
                                                    id="partyIdSupplier"
                                                    name="partyIdSupplier"
                                                    component={FormComboBoxVirtualSupplier}
                                                    label={getTranslatedLabel("certificate.form.supplier", "Supplier *")}
                                                    valueField="partyId"
                                                    textField="partyName"
                                                    validator={requiredValidator}
                                                    disabled={editMode > 3}
                                                />
                                            </Grid>
                                        )}
                                        {showContractor && (
                                            <Grid item xs={showSupplier ? 2 : 3} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                                <Field
                                                    id="partyIdContractor"
                                                    name="partyIdContractor"
                                                    component={FormComboBoxVirtualContractor}
                                                    label={getTranslatedLabel("certificate.form.contractor", "Contractor *")}
                                                    valueField="partyId"
                                                    textField="partyName"
                                                    validator={requiredValidator}
                                                    disabled={editMode > 3}
                                                />
                                            </Grid>
                                        )}
                                        <Grid item xs={showSupplier && showContractor ? 2 : 3} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                            <Field
                                                name="estimatedStartDate"
                                                id="estimatedStartDate"
                                                label={getTranslatedLabel("certificate.form.startDate", "Start Date")}
                                                disabled={editMode > 1}
                                                component={FormDatePicker}
                                                validator={requiredValidator}
                                            />
                                        </Grid>
                                        <Grid item xs={showSupplier && showContractor ? 2 : 3} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                            <Field
                                                name="estimatedCompletionDate"
                                                id="estimatedCompletionDate"
                                                label={getTranslatedLabel("certificate.form.completionDate", "Completion Date")}
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
                                                component={FormInput}
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
                                                    loading={isSubmitting || isAddCertificateLoading}
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