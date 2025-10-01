import React, {useCallback, useState} from "react";
import {toast} from "react-toastify";
import {Certificate, CertificateStatus} from "../../../app/models/project/certificate";
import {CertificateItem} from "../../../app/models/project/certificateItem";
import {
    useAddProjectCertificateMutation,
    useFetchProjectCertificatesQuery, useIssueMaterialsForCertificateMutation, useProcessWorkEffortCertificateMutation,
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
        }));
    }, [nonDeletedCertificateItems]);


    const validateCertificate = useCallback(
        (cert: Certificate, items: CertificateItem[]) => {
            let isHeaderValid = true;
            if (currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE") {
                isHeaderValid = !!cert.partyIdSupplier && (editMode !== 2 || cert.workEffortId);
                if (!cert.partyIdSupplier) toast.error("Supplier ID is required");
            } else if (["WORKMANSHIP_CONTRACTING_CERTIFICATE", "COMPANY_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType)) {
                isHeaderValid = !!cert.partyIdContractor && (editMode !== 2 || cert.workEffortId);
                if (!cert.partyIdContractor) toast.error("Contractor ID is required");
            }
            // Purpose: Align validation with new requirement for contractor field visibility
            // Improvement: Ensures contractor field is mandatory where displayed, maintaining data integrity
            if (!isHeaderValid && editMode === 2 && !cert.workEffortId) {
                toast.error("Work Effort ID is required");
            }
            const isItemsValid = items.every((item) => {
                const isValid =
                    item.productId &&
                    item.description &&
                    item.quantity > 0 &&
                    item.unitPrice >= 0 &&
                    (currentCertificateType !== "WORKMANSHIP_CONTRACTING_CERTIFICATE" ||
                        (item.achievementPercentage && item.achievementPercentage >= 1 && item.achievementPercentage <= 100));
                if (!isValid) {
                    toast.error("Invalid certificate item: ensure product, quantity, price, and completion percentage (for contracting) are valid.");
                }
                return isValid;
            });
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
                // Purpose: Preserve object structure for FormComboBox components
                // Context: Flatten only for API calls, not for Redux state
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

                if (nonDeletedCertificateItems.length === 0) {
                    toast.error("Certificate items cannot be empty");
                    return;
                }

                if (editMode === 2 && !newCertificate.workEffortId) {
                    toast.error("Work Effort ID is required for updating certificate");
                    return;
                }

                const action = data.selectedMenuItem || selectedMenuItem;

                if (action === "Create Certificate" || editMode === 1) {
                    return await createCertificate(newCertificate);
                } else if (action === "Update Certificate") {
                    return await updateCertificate(newCertificate);
                } else if (action === "Approve Certificate") {
                    if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
                        if (!selectedCertificate?.workEffortId) {
                            toast.error("Work Effort ID is required for approving certificate");
                            return;
                        }
                        const result = await processWorkEffortCertificate({workEffortId: selectedCertificate.workEffortId}).unwrap();
                        dispatch(
                            setSelectedCertificate({
                                ...selectedCertificate,
                                currentStatusId: CertificateStatus.APPROVED,
                                statusDescription: "Approved",
                                statusDescriptionArabic: "تمت الموافقة",
                                relatedOrderId: result.orderId,
                            })
                        );
                        dispatch(setCertificateFormEditMode(3));
                        formRef2.current = !formRef2.current;
                        toast.success("Certificate approved successfully");
                        return;
                    }
                    if (action === "Approve Certificate" && ["SUPPLY_PROCUREMENT_CERTIFICATE"].includes(currentCertificateType)) {
                        if (selectedCertificate?.relatedOrderId && selectedCertificate?.facilityId) {
                            await receiveInventoryFromPurchaseOrder({
                                orderId: selectedCertificate.relatedOrderId,
                                facilityId: selectedCertificate.facilityId,
                            }).unwrap();
                            toast.success("Inventory received successfully after approval.");
                        } else {
                            toast.error("Missing orderId or facilityId for receiving inventory.");
                        }
                    }

                    if (action === "Approve Certificate" && currentCertificateType === "COMPANY_SUPPLY_SALE_CERTIFICATE") {
                        if (!selectedCertificate?.workEffortId) {
                            toast.error("Work Effort ID is required for issuing materials");
                            return;
                        }
                        try {
                            await issueMaterialsForCertificate({ workEffortId: selectedCertificate.workEffortId }).unwrap();
                            toast.success("Materials issued successfully for certificate.");
                        } catch (error: any) {
                            toast.error(error?.data?.message || "Failed to issue materials for certificate");
                            throw error; // Rethrow to maintain error handling
                        }
                    }
                    
                    formRef2.current = !formRef2.current;
                    dispatch(setCertificateFormEditMode(action === "Approve Certificate" ? 3 : 4));
                    return;
                } else {
                    toast.error("Invalid action type");
                    return;
                }
            } finally {
                setIsLoading(false);
            }
        },
        [createCertificate, updateCertificate, editMode, selectedCertificate, nonDeletedCertificateItems, selectedMenuItem, setIsLoading, updateProjectCertificate, receiveInventoryFromPurchaseOrder, currentCertificateType, dispatch, refetch]
    );

    return {
        isAddCertificateLoading,
        isUpdateCertificateLoading,
        isProcessCertificateLoading,
        formEditMode,
        setFormEditMode,
        handleCreate,
        isReceiveLoading
    };
};

export default useProjectCertificate;