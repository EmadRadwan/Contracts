import React, {useCallback, useEffect, useMemo, useRef, useState} from "react";
import {RootState, useAppDispatch, useAppSelector, useFetchFacilitiesQuery} from "../../../app/store/configureStore";
import {Field, Form, FormElement, FormRenderProps} from "@progress/kendo-react-form";
import {Box, Button, Checkbox, Collapse, FormControlLabel, Grid, IconButton, Paper, Typography} from "@mui/material";
import LoadingButton from "@mui/lab/LoadingButton";
import {useTranslationHelper} from "../../../app/hooks/useTranslationHelper";
import {resetCertificateUi, setCertificateFormEditMode, setCurrentFacilityId} from "../slice/certificateUiSlice";
import {RibbonContainer, Ribbon} from "react-ribbons";
import LoadingComponent from "../../../app/layout/LoadingComponent";
import {requiredValidator} from "../../../app/common/form/Validators";
import {toast} from "react-toastify";
import FormDatePicker from "../../../app/common/form/FormDatePicker";
import ProjectMenu from "../menu/ProjectMenu";
import useProjectCertificate from "../hook/useProjectCertificate";
import {CertificateItemsListMemo} from "../dashboard/CertificateItemsList";
import {FormComboBoxVirtualProject} from "../../../app/common/form/FormComboBoxVirtualProject";
import {FormComboBoxVirtualContractor} from "../../../app/common/form/FormComboBoxVirtualContractor";
import FormInput from "../../../app/common/form/FormInput";
import {resetUiCertificateItems} from "../slice/certificateItemsUiSlice";
import {Menu, MenuItem} from '@mui/material';
import {CertificateStatus} from "../../../app/models/project/certificate";
import {MemoizedFormDropDownList2} from "../../../app/common/form/MemoizedFormDropDownList2";
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import {CertificateItemsListGroupedMemo} from "../dashboard/CertificateItemsListGrouped";
import {FormComboBoxVirtualSupplierMultiColumn} from "../../../app/common/form/FormComboBoxVirtualSupplierMultiColumn";
import {
    certificateSubTotal, displayCertificateItemsSelector, supplyCertificateReportSelector,
    workmanshipCertificateReportSelector
} from "../slice/certificateSelectors";
import CertificatesListModal from "../dashboard/CertificatesListModal";
import {certificateItemsApi} from "../../../app/store/apis/certificateItemsApi";
import {WorkmanshipCertificateExcel} from "../report/WorkmanshipCertificateExcel";
import {SupplyCertificateExcel} from "../report/SupplyCertificateExcel";
import {Can} from "../../account/Can";
import {ReviewCommentsDialog} from "./ReviewCommentsDialog";
import {useDeleteProjectCertificateMutation} from "../../../app/store/apis/projectsApi";
import {ConfirmDialog} from "./ConfirmDialog";
import ModalContainer from "../../../app/common/modals/ModalContainer";
import CreatePartyModalForm from "../../parties/form/CreatePartyModalForm";

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
    const {user} = useAppSelector((state) => state.account);
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const open = Boolean(anchorEl);
    const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
    const [deleteLoading, setDeleteLoading] = useState(false);
    const [deleteCertificate] = useDeleteProjectCertificateMutation();
    const dispatch = useAppDispatch();
    const {getTranslatedLabel} = useTranslationHelper();


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

    const handleReset = () => {
        handleStatusUpdate('Reset Certificate');
        handleClose();
    };

    // Determine if actions are disabled based on certificate status
    const isApproveDisabled = !workEffortId || currentStatusId === CertificateStatus.APPROVED || currentStatusId === CertificateStatus.COMPLETE;
    const isResetDisabled = !workEffortId || currentStatusId === CertificateStatus.CREATED;

    const handleDeleteClick = () => {
        setDeleteDialogOpen(true);
    };

    const handleDeleteConfirm = async () => {
        if (!workEffortId) return;

        setDeleteLoading(true);
        try {
            await deleteCertificate(workEffortId).unwrap();
            toast.success(getTranslatedLabel(
                'projects.certificate.deleted',
                'Certificate deleted successfully'
            ));

            // Reset UI and go back to list
            dispatch(resetCertificateUi());
            dispatch(setCertificateFormEditMode(0));
            dispatch(resetUiCertificateItems());

            // Optional: refetch list if needed
            // refetchCertificates();

            setDeleteDialogOpen(false);
        } catch (err: any) {
            toast.error(
                err?.data?.message ||
                getTranslatedLabel('projects.certificate.deleteFailed', 'Failed to delete certificate')
            );
        } finally {
            setDeleteLoading(false);
        }
    };
    
    return (
        <>
            <Button
                variant="contained"
                color="primary"
                onClick={handleClick}
                disabled={disabled || !workEffortId}
                sx={{mt: 2, mr: 2}}
            >
                {getTranslatedLabel('projects.certificate.actions', 'Actions')}
            </Button>
            <Menu
                anchorEl={anchorEl}
                open={open}
                onClose={handleClose}
                anchorOrigin={{vertical: 'bottom', horizontal: 'right'}}
                transformOrigin={{vertical: 'top', horizontal: 'right'}}
            >
                <Can perform="ReviewCertificate">
                    {/* Mark as Ready for Approval - only from Created or Requires Edit */}
                    {currentStatusId && [CertificateStatus.CREATED, CertificateStatus.REQUIRES_EDIT].includes(currentStatusId) && (
                        <MenuItem onClick={() => handleStatusUpdate('MarkReadyForApproval')}>
                            {getTranslatedLabel('projects.certificate.markAsReadyForApproval', 'Mark as Ready for Approval')}
                        </MenuItem>
                    )}

                    {/* Mark as Requires Editing - allowed from Created, Requires Edit, or Ready for Approval */}
                    {currentStatusId &&
                        [CertificateStatus.CREATED, CertificateStatus.REQUIRES_EDIT, CertificateStatus.READY_FOR_APPROVAL].includes(currentStatusId) && (
                            <MenuItem onClick={() => handleStatusUpdate('MarkRequiresEdit')}>
                                {getTranslatedLabel('projects.certificate.markAsRequiresEdit', 'Mark as Requires Editing')}
                            </MenuItem>
                        )}
                </Can>

                <Can perform="ApproveCertificate">
                    <MenuItem onClick={handleApprove} disabled={isApproveDisabled}>
                        {getTranslatedLabel('projects.certificate.approve', 'Approve Certificate')}
                    </MenuItem>
                </Can>

                <Can perform="ResetCertificate">
                    <MenuItem onClick={handleReset} disabled={isResetDisabled}>
                        {getTranslatedLabel('projects.certificate.reset', 'Reset Certificate')}
                    </MenuItem>
                </Can>

                <Can perform="DeleteCertificate">
                        <MenuItem
                            onClick={() => {
                                handleClose();
                                handleDeleteClick();
                            }}
                            sx={{ color: 'error.main' }}
                        >
                            {getTranslatedLabel('projects.certificate.delete', 'Delete Certificate')}
                        </MenuItem>
                </Can>
               
            </Menu>

            <ConfirmDialog
                open={deleteDialogOpen}
                title={getTranslatedLabel('projects.certificate.confirmDeleteTitle', 'Delete Certificate')}
                message={getTranslatedLabel(
                    'projects.certificate.confirmDeleteMessage',
                    'Are you sure you want to delete this certificate and all related data (items, purchase order, etc.)? This action cannot be undone.'
                )}
                confirmText={getTranslatedLabel('common.delete', 'Delete')}
                cancelText={getTranslatedLabel('common.cancel', 'Cancel')}
                onConfirm={handleDeleteConfirm}
                onCancel={() => setDeleteDialogOpen(false)}
                loading={deleteLoading}
                danger
            />
        </>
    );
};


export default function ProjectCertificateForm({editMode, cancelEdit}: ProjectCertificateFormProps) {
    const formRef = useRef<any>(null);
    const formRef2 = useRef<boolean>(false);
    const dispatch = useAppDispatch();
    const {getTranslatedLabel} = useTranslationHelper();
    const {currentCertificateType, selectedCertificate} = useAppSelector((state) => state.certificateUi);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [selectedMenuItem, setSelectedMenuItem] = useState("");
    const [isLoading, setIsLoading] = useState(false);
    const {language} = useAppSelector((state) => state.localization);
    const {user} = useAppSelector((state) => state.account);
    const [isFormCollapsed, setIsFormCollapsed] = useState(false);
    const workmanshipReportData = useAppSelector(workmanshipCertificateReportSelector);
    const supplyReportData = useAppSelector(supplyCertificateReportSelector);
    const subtotal = useAppSelector(certificateSubTotal);
    const [showCertificatesModal, setShowCertificatesModal] = useState(false);
    const [showPDF, setShowPDF] = useState(false);
    const currentFacilityId = useAppSelector((state: RootState) =>
        state.certificateUi.selectedCertificate.currentFacilityId
        ?? state.certificateUi.selectedCertificate.facilityId
    );
    const [deliverToSite, setDeliverToSite] = useState(true);
    const [showNewCustomer, setShowNewCustomer] = useState(false);

    const handleFacilityChange = useCallback((event: any, formRenderProps: FormRenderProps) => {
        const newFacilityId = event.value;
        formRenderProps.onChange("facilityId", { value: newFacilityId });
        dispatch(setCurrentFacilityId(newFacilityId));
    }, [dispatch]);
    const [reviewDialogOpen, setReviewDialogOpen] = useState(false);
    const [pendingReviewAction, setPendingReviewAction] = useState<string>("");
    const certificateItems = useAppSelector(displayCertificateItemsSelector);
    
    console.log('selectedCertificate', selectedCertificate)

    const [contractorId, setContractorId] = useState<string | undefined>(
        selectedCertificate?.partyIdContractor?.fromPartyId
    );
    const [supplierId, setSupplierId] = useState<string | undefined>(
        selectedCertificate?.partyIdSupplier?.fromPartyId
    );
    const formRenderPropsRef = useRef<FormRenderProps | null>(null);

    // Ensure certificate items are fetched every time the form opens
    useEffect(() => {
        if (selectedCertificate?.workEffortId) {
            dispatch(certificateItemsApi.endpoints.fetchCertificateItems.initiate(selectedCertificate.workEffortId));
        }
    }, [selectedCertificate?.workEffortId, dispatch]);
    
    

    const {
        formEditMode,
        setFormEditMode,
        handleCreate,
        isAddCertificateLoading, isResetLoading, isIssueMaterialsLoading,
        isUpdateCertificateLoading, isReceiveLoading
    } = useProjectCertificate({
        selectedMenuItem,
        formRef2,
        editMode,
        setIsLoading,
    });
    const {data: facilities = []} = useFetchFacilitiesQuery(undefined);


    const renderSwitchStatus = useCallback(() => {
        const currentStatus = selectedCertificate?.currentStatusId || CertificateStatus.CREATED;
        const { language } = useAppSelector((state) => state.localization);

        // Define color configuration centrally (kept unchanged for visual consistency)
        const statusConfig: Record<CertificateStatus, { bg: string; fg: string }> = {
            [CertificateStatus.CREATED]: { bg: "#1976d2", fg: "#ffffff" }, // blue
            [CertificateStatus.REQUIRES_EDIT]: { bg: "#ff9800", fg: "#ffffff" }, // orange
            [CertificateStatus.READY_FOR_APPROVAL]: { bg: "#4fc3f7", fg: "#000000" }, // light blue
            [CertificateStatus.APPROVED]: { bg: "#ffeb3b", fg: "#000000" }, // yellow
            [CertificateStatus.COMPLETE]: { bg: "#4caf50", fg: "#ffffff" }, // green
        };

        // Get color config with fallback
        const config = statusConfig[currentStatus] || { bg: "#757575", fg: "#ffffff" };

        // FALLBACK: Hardcoded labels only when no backend description is available at all
        const fallbackLabels: Record<CertificateStatus, { en: string; ar: string }> = {
            [CertificateStatus.CREATED]: { en: "Created", ar: "تم الإنشاء" },
            [CertificateStatus.REQUIRES_EDIT]: { en: "Requires Editing", ar: "يتطلب تعديل" },
            [CertificateStatus.READY_FOR_APPROVAL]: { en: "Ready for Approval", ar: "جاهز للموافقة" },
            [CertificateStatus.APPROVED]: { en: "Approved", ar: "تمت الموافقة" },
            [CertificateStatus.COMPLETE]: { en: "Complete", ar: "مكتمل" },
        };

        const fallback = fallbackLabels[currentStatus] || { en: "Unknown", ar: "غير معروف" };
        const label = language === "ar" ? fallback.ar : fallback.en;

        return { label, backgroundColor: config.bg, foreColor: config.fg };
    }, [selectedCertificate, language]);


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
            projectId: selectedCertificate.projectId ? {
                projectId: selectedCertificate.projectId,
                projectName: selectedCertificate.projectName || ""
            } : undefined,
            partyIdSupplier: showSupplier && selectedCertificate.partyIdSupplier ? {
                fromPartyId: selectedCertificate.partyIdSupplier.fromPartyId || "",
                fromPartyName: selectedCertificate.partyIdSupplier.partyName || "",
            } : undefined,
            partyIdContractor: showContractor && selectedCertificate.partyIdContractor ? {
                fromPartyId: selectedCertificate.partyIdContractor.fromPartyId || "",
                fromPartyName: selectedCertificate.partyIdContractor.partyName || "",
            } : undefined,
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
            const newFacilityId = project?.facilityId && facilities.some((f: any) => f.facilityId === project.facilityId)
                ? project.facilityId
                : undefined;

            formRenderProps.onChange("facilityId", { value: newFacilityId });
            dispatch(setCurrentFacilityId(newFacilityId)); // ← Important!
        },
        [facilities, dispatch]
    );

    const handleSubmit = useCallback(
        async (formProps: any) => {
            if (!formProps.isValid) {
                //toast.error(getTranslatedLabel("certificate.form.invalid", "Form is invalid"));
                return false;
            }
            if (isSubmitting) return false;
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
                toast.error(getTranslatedLabel("certificate.form.error", "Failed to save certificate"));
            } finally {
                setIsSubmitting(false);
                setSelectedMenuItem("");
            }
        },
        [isSubmitting, editMode, currentCertificateType, getTranslatedLabel, handleCreate]
    );

    const handleStatusUpdate = useCallback(
        async (action: string) => {
            if (!selectedCertificate?.workEffortId) {
                toast.error(getTranslatedLabel("certificate.noWorkEffortId", "No certificate selected"));
                return;
            }

            // Direct actions (Approve / Complete) – no comments needed
            if (action === "Approve Certificate" || action === "Complete Certificate" || action === "Reset Certificate") {
                setIsSubmitting(true);
                setSelectedMenuItem(action);

                const statusUpdate = {
                    values: {
                        workEffortId: selectedCertificate.workEffortId,
                        currentStatusId:
                            action === "Approve Certificate" ? CertificateStatus.APPROVED : 
                            action === "Complete Certificate" ? CertificateStatus.COMPLETE :
                            CertificateStatus.CREATED,
                        deliverToSite:
                            currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE" ? deliverToSite : undefined,
                    },
                    selectedMenuItem: action,
                };

                try {
                    await handleCreate(statusUpdate);
                } finally {
                    setIsSubmitting(false);
                    setSelectedMenuItem("");
                }
                return;
            }

            // Review actions – open dialog to collect comments
            if (action === "MarkReadyForApproval" || action === "MarkRequiresEdit") {
                setPendingReviewAction(action);
                setReviewDialogOpen(true);
                return;
            }

            // Fallback
            toast.error("Unknown action");
        },
        [
            handleCreate,
            selectedCertificate,
            getTranslatedLabel,
            currentCertificateType,
            deliverToSite,
        ]
    );

    const handleReviewConfirm = useCallback(
        async (comments: string) => {
            setIsSubmitting(true);
            setSelectedMenuItem(pendingReviewAction);

            const statusUpdate = {
                values: {
                    workEffortId: selectedCertificate!.workEffortId,
                    comments: comments || undefined, // send only if provided
                },
                selectedMenuItem: pendingReviewAction,
            };

            try {
                await handleCreate(statusUpdate);
            } finally {
                setIsSubmitting(false);
                setSelectedMenuItem("");
                setReviewDialogOpen(false);
                setPendingReviewAction("");
            }
        },
        [handleCreate, selectedCertificate, pendingReviewAction]
    );

    const handleCancel = useCallback(() => {
        formRef2.current = !formRef2.current;
        dispatch(resetCertificateUi());
        dispatch(setCertificateFormEditMode(0));
        dispatch(resetUiCertificateItems());
        cancelEdit();
        setSelectedMenuItem("");
        setShowPDF(false);
    }, [dispatch, cancelEdit]);

    const handleCloseCertificatesModal = useCallback(() => {
        setShowCertificatesModal(false);
    }, []);

    useEffect(() => {
        if (selectedCertificate?.workEffortId) {
            formRef.current?.resetForm({values: initialFormValues});
            formRef2.current = !formRef2.current; 
        }
    }, [selectedCertificate?.workEffortId, initialFormValues]);

    useEffect(() => {
        return () => {
            formRenderPropsRef.current = null; // Clean up on unmount
        };
    }, []);

    const getCertificateTypeLabel = (type: string) => {
        switch (type) {
            case "SUPPLY_PROCUREMENT_CERTIFICATE":
                return getTranslatedLabel("projects.certificate.list.supplyProcurement", "Supply Procurement");
            case "WORKMANSHIP_CONTRACTING_CERTIFICATE":
                return getTranslatedLabel("projects.certificate.list.workmanshipContracting", "Workmanship Contracting");
            case "COMPANY_SUPPLY_SALE_CERTIFICATE":
                return getTranslatedLabel("projects.certificate.list.companySupplySale", "Company Supply Sale");
            default:
                return getTranslatedLabel("projects.certificate.form.unknown", "Unknown Certificate Type");
        }
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

    
    const renderCertificateReport = () => {

        if (!selectedCertificate?.certificateNumber) return null;

        // Use the same transformation you had in transformResponse + extra fields
        const formattedItems = certificateItems.map(item => ({
            ...item,
            // Ensure these fields exist for Excel components (safe fallback)
            code: item.productIdObject?.productId ? `${item.productIdObject.productId}/1` : item.code || 'N/A',
            productName: item.productName || 'N/A',
            description: item.description || '',
            quantity: item.quantity || 0,
            uomName: item.uomName || item.quantityUomObject?.description || 'N/A',
            unitPrice: item.unitPrice || 0,
            materialPrice: item.materialPrice || 0,
            laborPrice: item.laborPrice || 0,
            displayTotal: item.totalAmount || item.net || 0,
            discount: item.discount || 0,
            deductions: item.deductions || 0,
            transportationExpenses: item.transportationExpenses || 0,
            gratuities: item.gratuities || 0,
            insurance: item.insurance || 0,
            additionalInsurance: item.additionalInsurance || 0,
            net: item.net || 0,
            deserved: item.deserved || 0,
            deductionDescription: item.deductionDescription || '',
            achievementPercentage: typeof item.achievementPercentage === 'string'
                ? parseFloat(item.achievementPercentage.replace('%', '')) || 0
                : item.achievementPercentage || 0,
            formattedProcurementDate: item.procurementDate
                ? new Date(item.procurementDate).toLocaleDateString("en-GB")
                : 'N/A',
            // For grouped workmanship
            productSubtotal: item.productSubtotal || item.net || 0,
            isLastInGroup: item.isLastInGroup || false,
            mainItemDescription: item.mainItemDescription || '',
            discountNote: item.discountNote || '',
        }));

        const commonProps = {
            getTranslatedLabel,
            subtotal,
            isSubmitting,
            isAddCertificateLoading,
            isUpdateCertificateLoading,
            isReceiveLoading,
            // No more isFetching from hook — we can infer from certificateItems length
            isFetching: certificateItems.length === 0 && selectedCertificate?.workEffortId != null,
        };

        // Workmanship Certificate
        if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
            if (formattedItems.length === 0) return null;

            return (
                <WorkmanshipCertificateExcel
                    certificate={selectedCertificate}
                    items={formattedItems}
                    {...commonProps}
                    key={`${selectedCertificate.workEffortId}-workmanship`}
                />
            );
        }

        // Supply / Company Sale Certificates
        if (["SUPPLY_PROCUREMENT_CERTIFICATE", "COMPANY_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType)) {
            if (formattedItems.length === 0) return null;

            return (
                <SupplyCertificateExcel
                    certificate={selectedCertificate}
                    items={formattedItems}
                    {...commonProps}
                    key={`${selectedCertificate.workEffortId}-supply`}
                />
            );
        }

        return null;
    };
    
    
    const handleContractorChange = useCallback(
        (event: any) => {
            const contractor = event.value;
            setContractorId(contractor?.fromPartyId);
        },
        []
    );

    const handleSupplierChange = useCallback(
        (event: any) => {
            const supplier = event.value;
            setSupplierId(supplier?.fromPartyId);
        },
        []
    );
    

    return (
        <>
            <ProjectMenu onMenuSelect={(key) => {
                if (key === "projectCertificates") {
                    cancelEdit(); // Trigger cancelEdit to switch to list view
                }
            }}/>
            <Paper elevation={5} className="div-container-withBorderCurved">
                <Grid container spacing={2} alignItems="center" position="relative">
                    <Grid item xs={11}>
                        <Box display="flex" alignItems="center" justifyContent="space-between" sx={{paddingLeft: 3}}>
                            <Typography
                                sx={{fontWeight: "bold", fontSize: "18px", color: editMode === 1 ? "green" : "black"}}
                                variant="h6"
                            >
                                {selectedCertificate?.certificateNumber
                                    ? `${getTranslatedLabel("projects.certificate.form.title", "Project Certificate No")}: ${selectedCertificate.certificateNumber} (${getCertificateTypeLabel(currentCertificateType)})`
                                    : `(${getCertificateTypeLabel(currentCertificateType)})`}
                            </Typography>
                            <Button
                                variant="outlined"
                                color="primary"
                                onClick={() => setShowCertificatesModal(true)}
                                disabled={!contractorId && !supplierId}
                                sx={{mr: 2}}
                            >
                                {getTranslatedLabel('projects.certificate.form.viewOld', 'View Certificates')}
                            </Button>
                            <IconButton onClick={() => setIsFormCollapsed(!isFormCollapsed)}>
                                {isFormCollapsed ? <ExpandMoreIcon/> : <ExpandLessIcon/>}
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
                        render={(formRenderProps: FormRenderProps) => {
                            formRenderPropsRef.current = formRenderProps;
                            return (
                                <FormElement>
                                    <fieldset className="k-form-fieldset">
                                        <Grid container alignItems="start" justifyContent="start" spacing={1}>
                                            <Grid container spacing={1} alignItems="flex-end" justifyContent="flex-start"
                                                  sx={{paddingLeft: 3}}>
                                                <Grid item xs={3}
                                                      className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
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
                                                    <Grid item xs={showContractor ? 2 : 3}
                                                          className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                                        <Field
                                                            id="partyIdSupplier"
                                                            name="partyIdSupplier"
                                                            component={FormComboBoxVirtualSupplierMultiColumn}
                                                            label={getTranslatedLabel("projects.certificate.form.supplier", "Supplier *")}
                                                            valueField="fromPartyId"
                                                            textField="fromPartyName"
                                                            validator={requiredValidator}
                                                            disabled={editMode > 3}
                                                            onChange={handleSupplierChange}
                                                        />
                                                    </Grid>
                                                )}
                                                {showContractor && (
                                                    <Grid item xs={showSupplier ? 2 : 3}
                                                          className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                                        <Field
                                                            id="partyIdContractor"
                                                            name="partyIdContractor"
                                                            component={FormComboBoxVirtualContractor}
                                                            label={getTranslatedLabel("projects.certificate.form.contractor", "Contractor *")}
                                                            valueField="fromPartyId"
                                                            textField="fromPartyName"
                                                            validator={requiredValidator}
                                                            disabled={editMode > 3}
                                                            onChange={handleContractorChange}
                                                        />
                                                    </Grid>
                                                )}
                                                <Grid item xs={1}>
                                                    <Button
                                                        color="secondary"
                                                        onClick={() => setShowNewCustomer(true)}
                                                        variant="outlined"
                                                    >
                                                        +
                                                    </Button>
                                                </Grid>
                                                {["SUPPLY_PROCUREMENT_CERTIFICATE", "COMPANY_SUPPLY_SALE_CERTIFICATE", "CONTRACTOR_PURCHASE_CERTIFICATE"].includes(currentCertificateType) && (
                                                    <Grid item xs={2}
                                                          className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
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
                                                            onChange={(e: any) => handleFacilityChange(e, formRenderPropsRef.current!)}
                                                        />
                                                    </Grid>
                                                )}
                                                <Grid item xs={showSupplier && showContractor ? 1.5 : 2}
                                                      className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                                    <Field
                                                        name="estimatedStartDate"
                                                        id="estimatedStartDate"
                                                        label={getTranslatedLabel("projects.certificate.form.startDate", "Start Date")}
                                                        disabled={editMode > 3}
                                                        component={FormDatePicker}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid>
                                                <Grid item xs={showSupplier && showContractor ? 1.5 : 2}
                                                      className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
                                                    <Field
                                                        name="estimatedCompletionDate"
                                                        id="estimatedCompletionDate"
                                                        label={getTranslatedLabel("projects.certificate.form.completionDate", "Completion Date")}
                                                        disabled={editMode > 3}
                                                        component={FormDatePicker}
                                                        validator={requiredValidator}
                                                    />
                                                </Grid>
                                                <Grid item xs={6.5}
                                                      className={editMode > 3 ? "grid-disabled" : "grid-normal"}>
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
                                        {currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE" && editMode === 2 && (
                                            <Grid item xs={12} sx={{ pl: 3, mt: 1 }}>
                                                <FormControlLabel
                                                    control={
                                                        <Checkbox
                                                            checked={deliverToSite}
                                                            onChange={(e) => setDeliverToSite(e.target.checked)}
                                                            color="primary"
                                                        />
                                                    }
                                                    label={getTranslatedLabel(
                                                        "projects.certificate.deliverToSite",
                                                        "Deliver to project site (receive directly at site instead of main warehouse)"
                                                    )}
                                                />
                                            </Grid>
                                        )}
                                        <div className="k-form-buttons">
                                            <Grid container spacing={2}>
                                                {(editMode === 1 || editMode === 2) && (
                                                    <Grid item>
                                                        <LoadingButton
                                                            size="large"
                                                            type="submit"
                                                            loading={isSubmitting || isAddCertificateLoading || isUpdateCertificateLoading}
                                                            variant="contained"
                                                            color="primary"
                                                        >
                                                            {getTranslatedLabel(
                                                                editMode === 1 ? "projects.certificate.form.create" : "projects.certificate.form.update",
                                                                editMode === 1 ? "Create Certificate" : "Update Certificate"
                                                            )}
                                                        </LoadingButton>
                                                    </Grid>
                                                )}
                                                <Grid item>
                                                    {renderCertificateReport()}
                                                </Grid>
                                                <Grid item>
                                                    <Button size="large" color="error" variant="outlined"
                                                            onClick={handleCancel}>
                                                        {getTranslatedLabel("projects.certificate.form.cancel", "Cancel")}
                                                    </Button>
                                                </Grid>
                                            </Grid>
                                        </div>
                                    </fieldset>
                                </FormElement>
                            );
                        }}
                    />
                </Collapse>
                <Grid item xs={12}>
                    {renderCertificateItems()}
                </Grid>
            </Paper>
            {(isAddCertificateLoading || isUpdateCertificateLoading || isReceiveLoading || isIssueMaterialsLoading || isResetLoading) && (
                <LoadingComponent
                    message={getTranslatedLabel("projects.certificate.form.saving", "Saving Certificate...")}/>
            )}
            <CertificatesListModal
                show={showCertificatesModal}
                onClose={handleCloseCertificatesModal}
                contractorId={contractorId}
                supplierId={supplierId}
                certificateType={currentCertificateType}
            />

            <ReviewCommentsDialog
                open={reviewDialogOpen}
                action={pendingReviewAction}
                onClose={() => {
                    setReviewDialogOpen(false);
                    setPendingReviewAction("");
                }}
                onConfirm={handleReviewConfirm}
            />

            {showNewCustomer && (
                <ModalContainer
                    show={showNewCustomer}
                    onClose={() => setShowNewCustomer(false)}
                    width={500}
                >
                    <CreatePartyModalForm
                        onClose={() => setShowNewCustomer(false)}
                    />
                </ModalContainer>
            )}
        </>
    );
}