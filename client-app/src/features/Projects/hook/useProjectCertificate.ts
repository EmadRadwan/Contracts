import React, {useCallback, useState} from "react";
import {toast} from "react-toastify";
import {Certificate, CertificateStatus} from "../../../app/models/project/certificate";
import {CertificateItem} from "../../../app/models/project/certificateItem";
import {
    useAddProjectCertificateMutation, useApprovePOForCertificateMutation,
    useFetchProjectCertificatesQuery,
    useIssueMaterialsForCertificateMutation,
    useProcessWorkEffortCertificateMutation, useReviewCertificateMutation,
    useUpdateProjectCertificateMutation
} from "../../../app/store/apis/projectsApi";
import {setProcessedCertificateItems} from "../slice/certificateItemsUiSlice";
import {setCertificateFormEditMode, setSelectedCertificate} from "../slice/certificateUiSlice";
import {useAppDispatch, useAppSelector} from "../../../app/store/configureStore";
import {useSelector} from "react-redux";
import {nonDeletedCertificateItemsSelector} from "../slice/certificateSelectors";
import {useReceiveInventoryFromPurchaseOrderMutation} from "../../../app/store/apis";

type UseProjectCertificateProps = {
    selectedMenuItem: string;
    formRef2: React.MutableRefObject<boolean>;
    editMode: number; // 1: create, 2: edit
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
};



const useProjectCertificate = ({
                                   selectedMenuItem,
                                   formRef2,
                                   editMode,
                                   setIsLoading,
                               }: UseProjectCertificateProps) => {
    const dispatch = useAppDispatch();
    const [addProjectCertificate, {isLoading: isAddCertificateLoading}] = useAddProjectCertificateMutation();
    const [updateProjectCertificate, {isLoading: isUpdateCertificateLoading}] = useUpdateProjectCertificateMutation();
    const nonDeletedCertificateItems: CertificateItem[] = useSelector(nonDeletedCertificateItemsSelector);
    const {
        currentCertificateType,
        selectedCertificate,
        certificateFormEditMode
    } = useAppSelector((state) => state.certificateUi);
    const {refetch} = useFetchProjectCertificatesQuery({skip: 0, take: 6});
    const [receiveInventoryFromPurchaseOrder, {isLoading: isReceiveLoading}] = useReceiveInventoryFromPurchaseOrderMutation();
    const [processWorkEffortCertificate, {isLoading: isProcessCertificateLoading}] = useProcessWorkEffortCertificateMutation();
    const [issueMaterialsForCertificate, {isLoading: isIssueMaterialsLoading}] = useIssueMaterialsForCertificateMutation(); // REFACTOR: Added mutation hook for issuing materials
    const [reviewCertificate, {isLoading: isReviewLoading}] = useReviewCertificateMutation();
    const [approvePOForCertificate, { isLoading: isApprovingPO }] = useApprovePOForCertificateMutation();
    
    const formEditMode = certificateFormEditMode;
    const setFormEditMode = useCallback((mode: number) => {
        dispatch(setCertificateFormEditMode(mode));
    }, [dispatch]);


    // Purpose: Ensures certificate items maintain complete data for form rendering
    // Context: Prevents loss of object structure needed for FormComboBox components
    const certificateItemsFlat = useCallback(() => {
        return nonDeletedCertificateItems.map((item: CertificateItem) => ({
            ...item,
            productId: typeof item.productId === "object" ? item.productId.productId : item.productId,
            uomId: typeof item.uomId === "object" ? item.uomId.UomId : item.uomId,
            description: item.description || "",
            deductionDescription: item.deductionDescription || "", // Include deductionDescription
            procurementDate: item.procurementDate ? new Date(item.procurementDate).toISOString() : undefined,
            achievementPercentage:
                typeof item.achievementPercentage === "string"
                    ? Math.round(parseFloat(item.achievementPercentage.replace("%", "")) || 0)
                    : Math.round(item.achievementPercentage || 0),
        }));
    }, [nonDeletedCertificateItems]);


    const validateCertificate = useCallback(
        (cert: Certificate, items: CertificateItem[]) => {
            let isHeaderValid = true;

            // Validate header based on certificate type
            if (currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE") {
                isHeaderValid = !!cert.partyIdSupplier && (editMode !== 2 || cert.workEffortId);
                if (!cert.partyIdSupplier) toast.error("Supplier ID is required");
            } else if (["WORKMANSHIP_CONTRACTING_CERTIFICATE", "COMPANY_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType)) {
                isHeaderValid = !!cert.partyIdContractor && (editMode !== 2 || cert.workEffortId);
                if (!cert.partyIdContractor) toast.error("Contractor ID is required");
            }

            // Purpose: Clarifies condition and avoids redundant toast messages
            // Improvement: Consolidates logic for editMode === 2 check to prevent duplicate errors
            if (editMode === 2 && !cert.workEffortId) {
                isHeaderValid = false;
                toast.error("Work Effort ID is required");
            }

            // Validate certificate items
            const isItemsValid = items.every((item) => {
                const achievementPercentage =
                    typeof item.achievementPercentage === "string"
                        ? Math.round(parseFloat(item.achievementPercentage.replace("%", "")) || 0)
                        : Math.round(item.achievementPercentage || 0);


                const isValid =
                    !!item.productId &&
                    !!item.description &&
                    item.quantity > 0 &&
                    item.unitPrice >= 0 &&
                    (currentCertificateType !== "WORKMANSHIP_CONTRACTING_CERTIFICATE" ||
                        (achievementPercentage !== null &&
                            achievementPercentage >= 1 &&
                            achievementPercentage <= 100));

                // Purpose: Provide clearer feedback for invalid fields
                // Improvement: Pinpoints which field caused the validation failure
                if (!isValid) {
                    if (!item.productId) toast.error("Invalid certificate item: Product ID is required");
                    else if (!item.description) toast.error("Invalid certificate item: Description is required");
                    else if (item.quantity <= 0) toast.error("Invalid certificate item: Quantity must be greater than 0");
                    else if (item.unitPrice < 0) toast.error("Invalid certificate item: Unit price cannot be negative");
                    else if (
                        currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE" &&
                        (achievementPercentage === null || achievementPercentage < 1 || achievementPercentage > 100)
                    ) {
                        toast.error("Invalid certificate item: Completion percentage must be between 1 and 100");
                    }
                }

                return isValid;
            });

            // REFACTOR: Add check for empty items array
            // Purpose: Provide specific feedback when no items are present
            // Improvement: Enhances user experience with clear error messaging
            if (items.length === 0) {
                toast.error("At least one valid certificate item is required");
            }

            return isHeaderValid && isItemsValid && items.length > 0;
        },
        [editMode, currentCertificateType]
    );

    const createCertificate = useCallback(
        async (newCertificate: Certificate) => {
            const items = certificateItemsFlat();
            if (items.length === 0) {
                toast.error("Certificate must have at least one item.");
                return;
            }
            /*if (!validateCertificate(newCertificate, nonDeletedCertificateItems)) {
                return;
            }*/

            const certificateData = {
                workEffortId: newCertificate.workEffortId,
                certificateCategory: currentCertificateType,
                description: newCertificate.description,
                projectId: newCertificate.projectId,
                partyIdSupplier: typeof newCertificate.partyIdSupplier === "object"
                    ? newCertificate.partyIdSupplier.fromPartyId
                    : newCertificate.partyIdSupplier,
                partyIdContractor: typeof newCertificate.partyIdContractor === "object"
                    ? newCertificate.partyIdContractor.fromPartyId
                    : newCertificate.partyIdContractor,
                estimatedStartDate: newCertificate.estimatedStartDate
                    ? new Date(newCertificate.estimatedStartDate).toISOString()
                    : null,
                estimatedCompletionDate: newCertificate.estimatedCompletionDate
                    ? new Date(newCertificate.estimatedCompletionDate).toISOString()
                    : null,
                facilityId: newCertificate.facilityId,
                certificateItems: items,
            };

            try {
                const createdCertificate = await addProjectCertificate(certificateData).unwrap();
                // Purpose: Ensure form re-renders with complete objects for FormComboBox components
                // Context: API returns workEffortId and certificateNumber; other fields are from input
                dispatch(
                    setSelectedCertificate({
                        workEffortId: createdCertificate.workEffortId,
                        certificateNumber: createdCertificate.certificateNumber,
                        projectId: newCertificate.projectId,
                        projectName: newCertificate.projectName || createdCertificate.projectName || "",
                        partyIdSupplier: newCertificate.partyIdSupplier
                            ? {
                                fromPartyId:
                                    typeof newCertificate.partyIdSupplier === "object"
                                        ? newCertificate.partyIdSupplier.fromPartyId
                                        : newCertificate.partyIdSupplier,
                                partyName: newCertificate.partyIdSupplier?.partyName || createdCertificate.partyNameSupplier || "",
                            }
                            : undefined,
                        partyIdContractor: newCertificate.partyIdContractor
                            ? {
                                fromPartyId:
                                    typeof newCertificate.partyIdContractor === "object"
                                        ? newCertificate.partyIdContractor.fromPartyId
                                        : newCertificate.partyIdContractor,
                                partyName:
                                    newCertificate.partyIdContractor?.partyName || createdCertificate.partyNameContractor || "",
                            }
                            : undefined,
                        description: newCertificate.description,
                        estimatedStartDate: newCertificate.estimatedStartDate
                            ? new Date(newCertificate.estimatedStartDate).toISOString()
                            : null,
                        estimatedCompletionDate: newCertificate.estimatedCompletionDate
                            ? new Date(newCertificate.estimatedCompletionDate).toISOString()
                            : null,
                        statusDescription: createdCertificate.statusDescription,
                        currentStatusId: createdCertificate.currentStatusId || CertificateStatus.CREATED,
                        statusDescriptionArabic: createdCertificate.statusDescriptionArabic || "",
                        relatedOrderId: createdCertificate.relatedOrderId || "",
                        facilityId: newCertificate.facilityId,
                        facilityName: createdCertificate.facilityName || "",
                    })
                );
                dispatch(setCertificateFormEditMode(2));
                dispatch(setProcessedCertificateItems(createdCertificate.certificateItems || []));
                formRef2.current = !formRef2.current;
                toast.success("Certificate and items created successfully");
                return {workEffortId: createdCertificate.workEffortId};
            } catch (error: any) {
                // console.error("Failed to create certificate:", error);
                toast.error(error?.data?.message || "Failed to create certificate and items");
                throw error;
            }
        },
        [addProjectCertificate, dispatch, formRef2, certificateItemsFlat, nonDeletedCertificateItems, currentCertificateType, refetch]
    );

    const updateCertificate = useCallback(
        async (newCertificate: Certificate) => {
            const items = certificateItemsFlat();
            if (items.length === 0) {
                toast.error("Certificate must have at least one item.");
                return;
            }
            if (!validateCertificate(newCertificate, nonDeletedCertificateItems)) {
                return;
            }
            if (!newCertificate.workEffortId) {
                toast.error("Work Effort ID is required for updating certificate");
                return;
            }
            const certificateData = {
                WorkEffortId: newCertificate.workEffortId,
                CertificateCategory: currentCertificateType,
                Description: newCertificate.description,
                ProjectId: newCertificate.projectId,
                PartyIdSupplier: typeof newCertificate.partyIdSupplier === "object" ? newCertificate.partyIdSupplier.fromPartyId : newCertificate.partyIdSupplier,
                PartyIdContractor: typeof newCertificate.partyIdContractor === "object" ? newCertificate.partyIdContractor.fromPartyId : newCertificate.partyIdContractor,
                EstimatedStartDate: newCertificate.estimatedStartDate ? new Date(newCertificate.estimatedStartDate).toISOString() : null,
                EstimatedCompletionDate: newCertificate.estimatedCompletionDate ? new Date(newCertificate.estimatedCompletionDate).toISOString() : null,
                FacilityId: newCertificate.facilityId,
                CertificateItems: items,
            };
            try {
                const updatedCertificate = await updateProjectCertificate(certificateData).unwrap();
                dispatch(
                    setSelectedCertificate({
                        workEffortId: updatedCertificate.workEffortId,
                        certificateNumber: updatedCertificate.certificateNumber,
                        projectId: newCertificate.projectId,
                        projectName: newCertificate.projectName || updatedCertificate.projectName || "",
                        partyIdSupplier: newCertificate.partyIdSupplier
                            ? {
                                fromPartyId: typeof newCertificate.partyIdSupplier === "object" ? newCertificate.partyIdSupplier.fromPartyId : newCertificate.partyIdSupplier,
                                partyName: newCertificate.partyIdSupplier?.partyName || updatedCertificate.partyNameSupplier || "",
                            }
                            : undefined,
                        partyIdContractor: newCertificate.partyIdContractor
                            ? {
                                fromPartyId: typeof newCertificate.partyIdContractor === "object" ? newCertificate.partyIdContractor.fromPartyId : newCertificate.partyIdContractor,
                                partyName: newCertificate.partyIdContractor?.partyName || updatedCertificate.partyNameContractor || "",
                            }
                            : undefined,
                        description: newCertificate.description,
                        estimatedStartDate: newCertificate.estimatedStartDate ? new Date(newCertificate.estimatedStartDate).toISOString() : null,
                        estimatedCompletionDate: newCertificate.estimatedCompletionDate ? new Date(newCertificate.estimatedCompletionDate).toISOString() : null,
                        statusDescription: updatedCertificate.statusDescription,
                        statusDescriptionArabic: updatedCertificate.statusDescriptionArabic || "",
                        currentStatusId: updatedCertificate.currentStatusId || CertificateStatus.CREATED,
                        relatedOrderId: updatedCertificate.relatedOrderId || "",
                        facilityId: updatedCertificate.facilityId,
                        facilityName: updatedCertificate.facilityName || "",
                        certificateCategory: updatedCertificate.certificateCategory,
                    })
                );
                dispatch(setProcessedCertificateItems(updatedCertificate.certificateItems || []));
                dispatch(setCertificateFormEditMode(2));
                formRef2.current = !formRef2.current;
                toast.success("Certificate and items updated successfully");
                return {workEffortId: updatedCertificate.workEffortId};
            } catch (error: any) {
                // console.error("Failed to update certificate:", error);
                toast.error(error?.data?.message || "Failed to update certificate and items");
                throw error;
            }
        },
        [updateProjectCertificate, dispatch, formRef2, certificateItemsFlat, nonDeletedCertificateItems, currentCertificateType]
    );


    const handleCreate = useCallback(
        async (data: any) => {
            setIsLoading(true);
            try {
                const newCertificate: Certificate = {
                    workEffortId: editMode === 2 ? selectedCertificate?.workEffortId : undefined,
                    workEffortTypeId: data.values.workEffortTypeId,
                    projectId: data.values.projectId?.projectId || data.values.projectId,
                    projectName: data.values.projectId?.projectName,
                    partyIdSupplier: data.values.partyIdSupplier,
                    partyIdContractor: data.values.partyIdContractor,
                    description: data.values.description,
                    estimatedStartDate: data.values.estimatedStartDate,
                    estimatedCompletionDate: data.values.estimatedCompletionDate,
                    facilityId: data.values.facilityId,
                    certificateItems: nonDeletedCertificateItems,
                };

                const deliverToSite: boolean = !!data.values.deliverToSite;


                if (nonDeletedCertificateItems.length === 0) {
                    toast.error("Certificate items cannot be empty");
                    return { success: false };
                }

                if (editMode === 2 && !newCertificate.workEffortId) {
                    toast.error("Work Effort ID is required for updating certificate");
                    return { success: false };
                }

                const action = data.selectedMenuItem || selectedMenuItem;
                if (action === "Create Certificate" || editMode === 1) {
                    await createCertificate(newCertificate);
                    return { success: true };
                } else if (action === "Update Certificate") {
                    await updateCertificate(newCertificate);
                    return { success: true };
                } else if (action === "Approve Certificate") {
                    if (!selectedCertificate?.workEffortId) {
                        toast.error("Work Effort ID is required for approving certificate");
                        return { success: false };
                    }

                    const hasPO = ["WORKMANSHIP_CONTRACTING_CERTIFICATE", "SUPPLY_PROCUREMENT_CERTIFICATE"].includes(currentCertificateType);

                    if (hasPO && selectedCertificate.relatedOrderId) {
                        await approvePOForCertificate(selectedCertificate.workEffortId).unwrap();
                        toast.success("Purchase order approved");
                    }

                    let result;
                    if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
                        result = await processWorkEffortCertificate({ workEffortId: selectedCertificate.workEffortId }).unwrap();
                    } else if (currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE") {
                        if (!selectedCertificate?.relatedOrderId || !selectedCertificate?.facilityId) {
                            toast.error("Missing orderId or facilityId for receiving inventory.");
                            return { success: false };
                        }

                        const receiveCommand = {
                            orderId: selectedCertificate.relatedOrderId,
                            facilityId: selectedCertificate.facilityId,
                            deliverToSite
                        };

                        result = await receiveInventoryFromPurchaseOrder(receiveCommand).unwrap();
                        refetch();

                        toast.success(
                            deliverToSite
                                ? "Inventory received directly at project site"
                                : "Inventory received at warehouse from purchase order"
                        );
                    } else if (currentCertificateType === "COMPANY_SUPPLY_SALE_CERTIFICATE") {
                        try {
                            result = await issueMaterialsForCertificate({
                                workEffortId: selectedCertificate.workEffortId,
                            }).unwrap();
                        } catch (error: any) {
                            // REFACTOR: Handle specific INSUFFICIENT_INVENTORY error from issueMaterialsForCertificate
                            // Purpose: Display the detailed error title from the API response in the Toast notification
                            // Improvement: Provides user-friendly feedback with specific inventory shortage details
                            // Context: Checks for errorCode to identify INSUFFICIENT_INVENTORY and uses the title field
                            if (error?.data?.errorCode === "INSUFFICIENT_INVENTORY" && error?.data?.title) {
                                toast.error(error.data.title);
                            } else {
                                toast.error(error?.data?.message || "Failed to issue materials for certificate");
                            }
                            setIsLoading(false); // Ensure loading state is reset
                            return { success: false };
                        }
                    }
                    else {
                        toast.error("Invalid certificate type for approval");
                        return { success: false };
                    }

                    // REFACTOR: Update selectedCertificate state with WEPR_APPROVED status after successful approval
                    // Purpose: Ensures the frontend reflects the new certificate status immediately without requiring a DB refresh
                    // Improvement: Prevents the "Approve" menu from remaining enabled after an approval action
                    // Context: Syncs the local Redux state with the backend's updated status (WEPR_APPROVED)
                    dispatch(
                        setSelectedCertificate({
                            ...selectedCertificate,
                            currentStatusId: CertificateStatus.APPROVED,
                            statusDescription: "Approved",
                            statusDescriptionArabic: "تمت الموافقة",
                            relatedOrderId: result?.orderId || selectedCertificate?.relatedOrderId || "",
                        })
                    );
                    dispatch(setCertificateFormEditMode(3)); // Set to edit mode for APPROVED status
                    formRef2.current = !formRef2.current; // Trigger form re-render
                    toast.success("Certificate approved successfully");
                    return { success: true };
                } else if (action === "MarkReadyForApproval" || action === "MarkRequiresEdit") {
                    const newStatus = action === "MarkReadyForApproval"
                        ? CertificateStatus.READY_FOR_APPROVAL
                        : CertificateStatus.REQUIRES_EDIT;

                    const result = await reviewCertificate({
                        workEffortId: selectedCertificate.workEffortId,
                        status: newStatus,
                        comments: data.values.comments, // ← now receives comments from dialog
                    }).unwrap();


                    dispatch(setSelectedCertificate({
                        ...selectedCertificate,
                        description: result.description || selectedCertificate?.description || "",
                        currentStatusId: newStatus,
                    }));

                    toast.success(`Certificate marked as ${newStatus}`);
                }else {
                    toast.error("Invalid action type");
                    return { success: false };
                }
            } catch (error: any) {
                toast.error(error?.data?.message || "Failed to process certificate action");
                return { success: false };
            } finally {
                setIsLoading(false);
            }
        },
        [
            createCertificate,
            updateCertificate,
            editMode,
            selectedCertificate,
            nonDeletedCertificateItems,
            selectedMenuItem,
            setIsLoading,
            updateProjectCertificate,
            receiveInventoryFromPurchaseOrder,
            processWorkEffortCertificate,
            issueMaterialsForCertificate,
            currentCertificateType,
            dispatch,
            refetch,
        ]
    );

    return {
        isAddCertificateLoading,
        isUpdateCertificateLoading,
        isProcessCertificateLoading,
        formEditMode,
        setFormEditMode,
        handleCreate,
        isReceiveLoading: isReceiveLoading
    };
};

export default useProjectCertificate;