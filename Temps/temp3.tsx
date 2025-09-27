import React, { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { RootState, useAppDispatch, useAppSelector, useFetchFacilitiesQuery } from "../../../app/store/configureStore";
import { Field, Form, FormElement } from "@progress/kendo-react-form";
import { Box, Button, Collapse, Grid, IconButton, Paper, Typography } from "@mui/material";
import LoadingButton from "@mui/lab/LoadingButton";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { resetCertificateUi, setCertificateFormEditMode } from "../slice/certificateUiSlice";
import { RibbonContainer, Ribbon } from "react-ribbons";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import { requiredValidator } from "../../../app/common/form/Validators";
import { toast } from "react-toastify";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import ProjectMenu from "../menu/ProjectMenu";
import useProjectCertificate from "../hook/useProjectCertificate";
import { CertificateItemsListMemo } from "../dashboard/CertificateItemsList";
import { FormComboBoxVirtualProject } from "../../../app/common/form/FormComboBoxVirtualProject";
import { FormComboBoxVirtualContractor } from "../../../app/common/form/FormComboBoxVirtualContractor";
import FormInput from "../../../app/common/form/FormInput";
import { resetUiCertificateItems } from "../slice/certificateItemsUiSlice";
import { Menu, MenuItem } from '@mui/material';
import { CertificateStatus } from "../../../app/models/project/certificate";
import { MemoizedFormDropDownList2 } from "../../../app/common/form/MemoizedFormDropDownList2";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import { CertificateItemsListGroupedMemo } from "../dashboard/CertificateItemsListGrouped";
import { FormComboBoxVirtualSupplierMultiColumn } from "../../../app/common/form/FormComboBoxVirtualSupplierMultiColumn";
import { certificateReportSelector, certificateSubTotal } from "../slice/certificateSelectors";
import CertificatePDFDocument from "../report/CertificatePDFDocument";
import { useSelector } from "react-redux";

interface ProjectCertificateFormProps {
    editMode: number; // 0: view, 1: create, 2: edit (CREATED), 3: edit (APPROVED), 4: edit (COMPLETED)
    cancelEdit: () => void;
}

interface CertificateActionsMenuProps {
    workEffortId: string | undefined;
    currentStatusId: string | undefined;
    handleStatusUpdate: (action: string) => void;
    disabled: boolean;
}

const CertificateActionsMenu: React.FC<CertificateActionsMenuProps> = ({
                                                                           workEffortId,
                                                                           currentStatusId,
                                                                           handleStatusUpdate,
                                                                           disabled,
                                                                       }) => {
    const { user } = useAppSelector((state) => state.account);
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const { getTranslatedLabel } = useTranslationHelper();

    const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleClose = () => {
        setAnchorEl(null);
    };

    const handleApprove = () => {
        handleStatusUpdate('Approve Certificate');
        handleClose();
    };

    const handleComplete = () => {
        handleStatusUpdate('Complete Certificate');
        handleClose();
    };

    // Determine if actions are disabled based on certificate status
    const isApproveDisabled = !workEffortId || currentStatusId === CertificateStatus.APPROVED || currentStatusId === CertificateStatus.COMPLETE;
    const isCompleteDisabled = !workEffortId || currentStatusId === CertificateStatus.COMPLETE;

    // REFACTOR: Updated translation keys to match CertificatePDFDocument
    // Purpose: Ensures consistency in translation keys across form and report components
    // Improvement: Aligns with provided JSON structure under projects.certificate
    return (
        <>
            <Button
                variant="contained"
                color="primary"
                onClick={handleClick}
                disabled={disabled || !workEffortId}
                sx={{ mt: 2, mr: 2 }}
            >
                {getTranslatedLabel('projects.certificate.actions', 'Actions')}
            </Button>
            <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleClose}
                anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
                transformOrigin={{ vertical: 'top', horizontal: 'right' }}
            >
                {user?.roles?.includes('ApproveCertificate') && (
                    <MenuItem onClick={handleApprove} disabled={isApproveDisabled}>
                        {getTranslatedLabel('projects.certificate.approve', 'Approve Certificate')}
                    </MenuItem>
                )}
                {/*{user?.roles?.includes('CompleteCertificate') && (
          <MenuItem onClick={handleComplete} disabled={isCompleteDisabled}>
            {getTranslatedLabel('projects.certificate.complete', 'Complete Certificate')}
          </MenuItem>
        )}*/}
            </Menu>
        </>
    );
};

export default function ProjectCertificateForm({ editMode, cancelEdit }: ProjectCertificateFormProps) {
    const formRef = useRef<any>(null);
    const formRef2 = useRef<boolean>(false);
    const dispatch = useAppDispatch();
    const { getTranslatedLabel } = useTranslationHelper();
    const { currentCertificateType, selectedCertificate } = useAppSelector((state) => state.certificateUi);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [selectedMenuItem, setSelectedMenuItem] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const { language } = useAppSelector((state) => state.localization);
    const { user } = useAppSelector((state) => state.account);
    const [isFormCollapsed, setIsFormCollapsed] = useState(false);
    const reportData = useAppSelector(certificateReportSelector);
    const subtotal = useAppSelector(certificateSubTotal);

    const { formEditMode, setFormEditMode, handleCreate, isAddCertificateLoading, isUpdateCertificateLoading, isReceiveLoading } = useProjectCertificate({
        selectedMenuItem,
        formRef2,
        editMode,
        setIsLoading,
    });

    const { data: facilities = [] } = useFetchFacilitiesQuery(undefined);

    const renderSwitchStatus = useCallback(() => {
        const status = selectedCertificate?.currentStatusId || CertificateStatus.CREATED;
        const { language } = useAppSelector((state) => state.localization);

        // Purpose: Use descriptions from ProjectCertificateDto if available, fallback to static mapping
        // Context: Ensures consistency with backend status objects
        if (selectedCertificate?.statusDescription && selectedCertificate?.statusDescriptionArabic) {
            return {
                label: language === "ar" ? selectedCertificate.statusDescriptionArabic : selectedCertificate.statusDescription,
                backgroundColor: status === CertificateStatus.CREATED ? "blue" : status === CertificateStatus.APPROVED ? "yellow" : "green",
                foreColor: status === CertificateStatus.APPROVED ? "#000000" : "#ffffff"
            };
        }

        // Fallback mapping
        const statusLabels: { [key in CertificateStatus]: { en: string; ar: string } } = {
            [CertificateStatus.CREATED]: { en: "Created", ar: "تم الإنشاء" },
            [CertificateStatus.APPROVED]: { en: "Approved", ar: "تمت الموافقة" },
            [CertificateStatus.COMPLETE]: { en: "Complete", ar: "مكتمل" },
        };

        switch (status) {
            case CertificateStatus.CREATED:
                return {
                    label: language === "ar" ? statusLabels[CertificateStatus.CREATED].ar : statusLabels[CertificateStatus.CREATED].en,
                    backgroundColor: "blue",
                    foreColor: "#ffffff"
                };
            case CertificateStatus.APPROVED:
                return {
                    label: language === "ar" ? statusLabels[CertificateStatus.APPROVED].ar : statusLabels[CertificateStatus.APPROVED].en,
                    backgroundColor: "yellow",
                    foreColor: "#000000"
                };
            case CertificateStatus.COMPLETE:
                return {
                    label: language === "ar" ? statusLabels[CertificateStatus.COMPLETE].ar : statusLabels[CertificateStatus.COMPLETE].en,
                    backgroundColor: "green",
                    foreColor: "#ffffff"
                };
            default:
                return {
                    label: language === "ar" ? "غير معروف" : "Unknown",
                    backgroundColor: "gray",
                    foreColor: "#ffffff"
                };
        }
    }, [selectedCertificate]);

    const formKey = useMemo(() => formRef2.current.toString(), [formRef2.current]);

    const showSupplier = currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE";
    const showContractor = ["WORKMANSHIP_CONTRACTING_CERTIFICATE", "COMPANY_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType);

    const initialFormValues = useMemo(() => {
        if (editMode === 1 || !selectedCertificate?.workEffortId) {
            const now = new Date();
            const oneWeekAgo = new Date(now);
            oneWeekAgo.setDate(now.getDate() - 7);
            return {
                description: "",
                projectId: undefined,
                partyIdSupplier: undefined,
                partyIdContractor: undefined,
                estimatedStartDate: oneWeekAgo,
                estimatedCompletionDate: now,
                facilityId: undefined,
            };
        }
        return {
            description: selectedCertificate.description || "",
            projectId: selectedCertificate.projectId
                ? { projectId: selectedCertificate.projectId, projectName: selectedCertificate.projectName || "" }
                : undefined,
            partyIdSupplier: showSupplier && selectedCertificate.partyIdSupplier
                ? { fromPartyId: selectedCertificate.partyIdSupplier.fromPartyId || "", fromPartyName: selectedCertificate.partyIdSupplier.partyName || "" }
                : undefined,
            partyIdContractor: showContractor && selectedCertificate.partyIdContractor
                ? { fromPartyId: selectedCertificate.partyIdContractor.fromPartyId || "", fromPartyName: selectedCertificate.partyIdContractor.partyName || "" }
                : undefined,
            estimatedStartDate: selectedCertificate.estimatedStartDate ? new Date(selectedCertificate.estimatedStartDate) : null,
            estimatedCompletionDate: selectedCertificate.estimatedCompletionDate ? new Date(selectedCertificate.estimatedCompletionDate) : null,
            facilityId: selectedCertificate.facilityId,
        };
        // Purpose: Ensure contractor field is initialized for both WORKMANSHIP_CONTRACTING_CERTIFICATE and COMPANY_SUPPLY_SALE_CERTIFICATE
        // Improvement: Maintains consistency with form rendering and prevents missing initial values
    }, [editMode, selectedCertificate, showSupplier, showContractor]);

    const handleProjectChange = useCallback(
        (event: any, formRenderProps: any) => {
            const project = event.value;
            if (project?.facilityId && facilities.some((f: any) => f.facilityId === project.facilityId)) {
                // Update facilityId in the form to match the project's facilityId
                formRenderProps.onChange("facilityId", { value: project.facilityId });
            } else {
                // Clear facilityId if no valid facilityId is associated with the project
                formRenderProps.onChange("facilityId", { value: undefined });
            }
        },
        [facilities]
    );

    const handleSubmit = useCallback(
        async (formProps: any) => {
            if (!formProps.isValid) {
                // toast.error(getTranslatedLabel("projects.certificate.form.invalid", "Form is invalid"));
                return false;
            }
            if (isSubmitting) return false;
            setIsSubmitting(true);

            // Purpose: Ensure handleCreate receives the correct action type
            // Context: Matches the expected action for createCertificate or updateCertificate
            const action = editMode === 1 ? "Create Certificate" : "Update Certificate";
            setSelectedMenuItem(action);

            const certificateData = {
                values: {
                    workEffortTypeId: currentCertificateType,
                    description: formProps.values.description,
                    projectId: formProps.values.projectId?.projectId || formProps.values.projectId,
                    partyIdSupplier: formProps.values.partyIdSupplier?.fromPartyId,
                    partyIdContractor: formProps.values.partyIdContractor?.fromPartyId,
                    estimatedStartDate: formProps.values.estimatedStartDate,
                    estimatedCompletionDate: formProps.values.estimatedCompletionDate,
                    facilityId: formProps.values.facilityId,
                },
                selectedMenuItem: action,
            };

            try {
                await handleCreate(certificateData);
            } catch (error) {
                toast.error(getTranslatedLabel("projects.certificate.form.error", "Failed to save certificate"));
            } finally {
                setIsSubmitting(false);
                setSelectedMenuItem("");
            }
        },
        [isSubmitting, editMode, currentCertificateType, getTranslatedLabel, handleCreate]
    );

    // Purpose: Sends status update requests to useProjectCertificate hook.
    const handleStatusUpdate = useCallback(
        async (action: string) => {
            if (!selectedCertificate?.workEffortId) {
                toast.error(getTranslatedLabel("projects.certificate.noWorkEffortId", "No certificate selected"));
                return;
            }
            setIsSubmitting(true);

            // Purpose: Ensure status transitions use WEPR_CREATED, WEPR_APPROVED, WEPR_COMPLETE
            // Context: Aligns with editModeMap and backend expectations
            setSelectedMenuItem(action);
            const statusUpdate = {
                values: {
                    workEffortId: selectedCertificate.workEffortId,
                    currentStatusId: action === 'Approve Certificate' ? CertificateStatus.APPROVED : CertificateStatus.COMPLETE,
                },
                selectedMenuItem: action,
            };

            try {
                await handleCreate(statusUpdate);
                toast.success(
                    getTranslatedLabel(
                        action === 'Approve Certificate' ? 'projects.certificate.approved' : 'projects.certificate.completed',
                        action === 'Approve Certificate' ? 'Certificate approved' : 'Certificate completed'
                    )
                );
            } catch (error) {
                toast.error(getTranslatedLabel("projects.certificate.statusUpdate.error", "Failed to update certificate status"));
            } finally {
                setIsSubmitting(false);
                setSelectedMenuItem("");
            }
        },
        [handleCreate, selectedCertificate, getTranslatedLabel]
    );

    const handleCancel = useCallback(() => {
        formRef2.current = !formRef2.current;
        dispatch(resetCertificateUi());
        dispatch(setCertificateFormEditMode(0));
        dispatch(resetUiCertificateItems());
        cancelEdit();
        setSelectedMenuItem("");
    }, [dispatch, cancelEdit]);

    useEffect(() => {
        if (selectedCertificate?.workEffortId) {
            // Purpose: Ensure form reflects the latest selectedCertificate data
            // Context: Prevents stale form data when switching certificates
            formRef.current?.resetForm({ values: initialFormValues });
            formRef2.current = !formRef2.current; // Trigger form re-render
        }
    }, [selectedCertificate?.workEffortId, initialFormValues]);

    const getCertificateTypeDisplayText = (type: string) => {
        return type
            .split('_')
            .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
            .join(' ');
    };

    const status = renderSwitchStatus();

    const renderCertificateItems = () => {
        if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
            return (
                <CertificateItemsListGroupedMemo
                    editMode={editMode}
                    workEffortId={selectedCertificate?.workEffortId}
                    isFormCollapsed={isFormCollapsed}
                />
            );
        }
        return (
            <CertificateItemsListMemo
                editMode={editMode}
                workEffortId={selectedCertificate?.workEffortId}
                isFormCollapsed={isFormCollapsed}
            />
        );
    };

    // REFACTOR: Updated translation keys to match CertificatePDFDocument
    // Purpose: Ensures consistency in translation keys across form and report components
    // Improvement: Aligns with provided JSON structure under projects.certificate and projects.certificate.form
    return (
        <>
            <ProjectMenu
                onMenuSelect={(key) => {
                    if (key === "projectCertificates") {
                        cancelEdit(); // Trigger cancelEdit to switch to list view
                    }
                }}
            />
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container spacing={2} alignItems="center" position="relative">
                    <Grid item xs={11}>
                        <Box display="flex" alignItems="center" justifyContent="space-between" sx={{ paddingLeft: 3 }}>
                            <Typography
                                sx={{ fontWeight: "bold", fontSize: "18px", color: editMode === 1 ? "green" : "black" }}
                                variant="h6"
                            >
                                {selectedCertificate?.certificateNumber
                                    ? `${getTranslatedLabel("projects.certificate.form.title", "Project Certificate No")}: ${selectedCertificate.certificateNumber} (${getCertificateTypeDisplayText(currentCertificateType)})`
                                    : `${getTranslatedLabel("projects.certificate.form.new", "New Project Certificate")} (${getCertificateTypeDisplayText(currentCertificateType)})`}
                            </Typography>
                            <IconButton onClick={() => setIsFormCollapsed(!isFormCollapsed)}>
                                {isFormCollapsed ? <ExpandMoreIcon /> : <ExpandLessIcon />}
                            </IconButton>
                            {editMode >= 2 && (
                                <CertificateActionsMenu
                                    workEffortId={selectedCertificate?.workEffortId}
                                    currentStatusId={selectedCertificate?.currentStatusId}
                                    handleStatusUpdate={handleStatusUpdate}
                                    disabled={editMode < 2 || isSubmitting || isAddCertificateLoading || isUpdateCertificateLoading}
                                />
                            )}
                        </Box>
                    </Grid>
                    <Grid item xs={1}>
                        {editMode > 1 && (
                            <RibbonContainer>
                                <Ribbon
                                    side={language === "ar" ? "left" : "right"}
                                    type="corner"
                                    size="large"
                                    backgroundColor={status.backgroundColor}
                                    color={status.foreColor}
                                    fontFamily="sans-serif"
                                >
                                    {status.label}
                                </Ribbon>
                            </RibbonContainer>
                        )}
                    </Grid>
                </Grid>
                <Collapse in={!isFormCollapsed}>
                    <Form
                        ref={formRef}
                        initialValues={initialFormValues}
                        key={formKey}
                        onSubmitClick={handleSubmit}
                        render={(formRenderProps) => (
                            <FormElement>
                                <fieldset className="k-form-fieldset">
                                    <Grid container alignItems="start" justifyContent="start" spacing={1}>
                                        <Grid container spacing={1} alignItems="center" justifyContent="flex-start" sx={{ paddingLeft: 3 }}>
                                            <Grid item xs={2} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                                <Field
                                                    id="projectId"
                                                    name="projectId"
                                                    component={FormComboBoxVirtualProject}
                                                    label={getTranslatedLabel("projects.certificate.form.project", "Project")}
                                                    dataItemKey="projectId"
                                                    textField="ProjectName"
                                                    validator={requiredValidator}
                                                    disabled={editMode > 3}
                                                    onChange={(event: any) => handleProjectChange(event, formRenderProps)}
                                                />
                                            </Grid>
                                            {showSupplier && (
                                                <Grid item xs={showContractor ? 2 : 3} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                                    <Field
                                                        id="partyIdSupplier"
                                                        name="partyIdSupplier"
                                                        component={FormComboBoxVirtualSupplierMultiColumn}
                                                        label={getTranslatedLabel("projects.certificate.form.supplier", "Supplier *")}
                                                        valueField="fromPartyId"
                                                        textField="fromPartyName"
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
                                                        label={getTranslatedLabel("projects.certificate.form.contractor", "Contractor *")}
                                                        valueField="fromPartyId"
                                                        textField="fromPartyName"
                                                        validator={requiredValidator}
                                                        disabled={editMode > 3}
                                                    />
                                                </Grid>
                                            )}
                                            {["SUPPLY_PROCUREMENT_CERTIFICATE", "COMPANY_SUPPLY_SALE_CERTIFICATE", "CONTRACTOR_PURCHASE_CERTIFICATE"].includes(currentCertificateType) && (
                                                <Grid item xs={2} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                                    <Field
                                                        id="facilityId"
                                                        name="facilityId"
                                                        label={getTranslatedLabel("projects.certificate.form.facility", "Facility *")}
                                                        component={MemoizedFormDropDownList2}
                                                        data={facilities}
                                                        dataItemKey="facilityId"
                                                        textField="facilityName"
                                                        validator={requiredValidator}
                                                        disabled={editMode > 3}
                                                    />
                                                </Grid>
                                            )}
                                            <Grid item xs={showSupplier && showContractor ? 1.5 : 2} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                                <Field
                                                    name="estimatedStartDate"
                                                    id="estimatedStartDate"
                                                    label={getTranslatedLabel("projects.certificate.form.startDate", "Start Date")}
                                                    disabled={editMode > 1}
                                                    component={FormDatePicker}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={showSupplier && showContractor ? 1.5 : 2} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                                <Field
                                                    name="estimatedCompletionDate"
                                                    id="estimatedCompletionDate"
                                                    label={getTranslatedLabel("projects.certificate.form.completionDate", "Completion Date")}
                                                    disabled={editMode > 1}
                                                    component={FormDatePicker}
                                                    validator={requiredValidator}
                                                />
                                            </Grid>
                                            <Grid item xs={6.5} className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                                <Field
                                                    id="description"
                                                    name="description"
                                                    label={getTranslatedLabel("projects.certificate.form.description", "Description")}
                                                    component={FormInput}
                                                    validator={requiredValidator}
                                                    disabled={editMode > 3}
                                                />
                                            </Grid>
                                        </Grid>
                                    </Grid>
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
                                                            editMode === 1 ? "projects.certificate.form.create" : "projects.certificate.form.update",
                                                            editMode === 1 ? "Create Certificate" : "Update Certificate"
                                                        )}
                                                    </LoadingButton>
                                                </Grid>
                                            )}
                                            <Grid item>
                                                {/* REFACTOR: Restrict PDF export to CREATED status */}
                                                {/* Purpose: Only allow printing for certificates in CREATED status */}
                                                {/* Improvement: Enhances control by limiting PDF generation to specific state */}
                                                {reportData.items && reportData.items.length > 0 && selectedCertificate?.currentStatusId === CertificateStatus.CREATED ? (
                                                    <CertificatePDFDocument
                                                        certificate={reportData.certificate}
                                                        items={reportData.items}
                                                        getTranslatedLabel={getTranslatedLabel}
                                                        subtotal={subtotal}
                                                        isSubmitting={isSubmitting}
                                                        isAddCertificateLoading={isAddCertificateLoading}
                                                        isUpdateCertificateLoading={isUpdateCertificateLoading}
                                                        isReceiveLoading={isReceiveLoading}
                                                        certificateNumber={reportData.certificate.certificateNumber}
                                                        certificateType={currentCertificateType}
                                                    />
                                                ) : (
                                                    <Button
                                                        color="primary"
                                                        variant="outlined"
                                                        disabled
                                                        title={getTranslatedLabel(
                                                            'projects.certificate.print.disabled',
                                                            'Printing is only available for certificates in CREATED status'
                                                        )}
                                                    >
                                                        {getTranslatedLabel('projects.certificate.export', 'Export to PDF')}
                                                    </Button>
                                                )}
                                            </Grid>
                                            <Grid item>
                                                <Button
                                                    size="large"
                                                    color="error"
                                                    variant="outlined"
                                                    onClick={handleCancel}
                                                >
                                                    {getTranslatedLabel("projects.certificate.form.cancel", "Cancel")}
                                                </Button>
                                            </Grid>
                                        </Grid>
                                    </div>
                                </fieldset>
                            </FormElement>
                        )}
                    />
                </Collapse>
                <Grid item xs={12}>
                    {renderCertificateItems()}
                </Grid>
            </Paper>
            {(isAddCertificateLoading || isUpdateCertificateLoading) && (
                <LoadingComponent message={getTranslatedLabel("projects.certificate.form.saving", "Saving Certificate...")} />
            )}
            {isReceiveLoading && (
                <LoadingComponent message={getTranslatedLabel("projects.certificate.form.saving", "Approving Certificate...")} />
            )}
        </>
    );
}