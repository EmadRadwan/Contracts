import React, { useCallback, useState } from "react";
import { toast } from "react-toastify";
import { Certificate } from "../../../app/models/project/certificate";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import { useAddProjectCertificateMutation, useUpdateProjectCertificateMutation } from "../../../app/store/apis/projectsApi";
import { certificateItemsEntities, setProcessedCertificateItems } from "../slice/certificateItemsUiSlice";
import { setCertificateFormEditMode } from "../slice/certificateUiSlice";
import { useAppDispatch, useAppSelector } from "../../../app/store/configureStore";
import { useSelector } from "react-redux";
import { nonDeletedCertificateItemsSelector } from "../slice/certificateSelectors";

type UseProjectCertificateProps = {
    selectedMenuItem: string;
    formRef2: React.MutableRefObject<boolean>;
    editMode: number; // 1: create, 2: edit
    selectedCertificate?: Certificate;
    setIsLoading: React.Dispatch<React.SetStateAction<boolean>>;
};

const useProjectCertificate = ({
                                   selectedMenuItem,
                                   formRef2,
                                   editMode,
                                   selectedCertificate,
                                   setIsLoading,
                               }: UseProjectCertificateProps) => {
    const dispatch = useAppDispatch();
    const [addProjectCertificate, { isLoading: isAddCertificateLoading }] = useAddProjectCertificateMutation();
    const [updateProjectCertificate, { isLoading: isUpdateCertificateLoading }] = useUpdateProjectCertificateMutation();
    const [certificate, setCertificate] = useState<Certificate | undefined>(selectedCertificate);
    const nonDeletedCertificateItems: CertificateItem[] = useSelector(nonDeletedCertificateItemsSelector);
    const { currentCertificateType } = useAppSelector((state) => state.certificateUi);

    const certificateItemsFlat = useCallback(() => {
        return nonDeletedCertificateItems.map((item: CertificateItem) => ({
            ...item,
            productId: typeof item.productId === "object" ? item.productId.productId : item.productId,
            uomId: typeof item.uomId === "object" ? item.uomId.UomId : item.uomId,
            procurementDate: item.procurementDate ? new Date(item.procurementDate).toISOString() : undefined,
        }));
    }, [nonDeletedCertificateItems]);

    // REFACTOR: Updated validateCertificate to check partyIdSupplier and partyIdContractor
    // Purpose: Validate required supplier and contractor fields based on certificate type
    // Context: Replaces partyId with explicit fields to match ProjectCertificateForm
    const validateCertificate = useCallback(
        (cert: Certificate, items: CertificateItem[]) => {
            let isHeaderValid = true;
            if (currentCertificateType === "SUPPLY_PROCUREMENT_CERTIFICATE") {
                isHeaderValid = !!cert.partyIdSupplier && (editMode !== 2 || cert.workEffortId);
                if (!cert.partyIdSupplier) toast.error("Supplier ID is required");
            } else if (
                currentCertificateType === "COMPANY_SUPPLY_SALE_CERTIFICATE" ||
                currentCertificateType === "CONTRACTOR_PURCHASE_CERTIFICATE" ||
                currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE"
            ) {
                isHeaderValid = !!cert.partyIdContractor && (editMode !== 2 || cert.workEffortId);
                if (!cert.partyIdContractor) toast.error("Contractor ID is required");
            } else if (currentCertificateType === "EXTERNAL_SUPPLY_SALE_CERTIFICATE") {
                isHeaderValid = !!cert.partyIdSupplier && !!cert.partyIdContractor && (editMode !== 2 || cert.workEffortId);
                if (!cert.partyIdSupplier) toast.error("Supplier ID is required");
                if (!cert.partyIdContractor) toast.error("Contractor ID is required");
            }
            if (!isHeaderValid && editMode === 2 && !cert.workEffortId) {
                toast.error("Work Effort ID is required");
            }

            const isItemsValid = items.every((item) => {
                const isValid =
                    item.productId &&
                    item.quantity > 0 &&
                    item.unitPrice >= 0 &&
                    (currentCertificateType !== "WORKMANSHIP_CONTRACTING_CERTIFICATE" ||
                        (item.completionPercentage && item.completionPercentage >= 1 && item.completionPercentage <= 100));
                if (!isValid) {
                    toast.error(
                        "Invalid certificate item: ensure product, quantity, price, and completion percentage (for contracting) are valid."
                    );
                }
                return isValid;
            });

            return isHeaderValid && isItemsValid && items.length > 0;
        },
        [editMode, currentCertificateType]
    );

    // REFACTOR: Updated createCertificate to use partyIdSupplier and partyIdContractor
    // Purpose: Send correct supplier and contractor fields to the backend
    // Context: Aligns with ProjectCertificateForm's data structure
    const createCertificate = useCallback(
        async (newCertificate: Certificate) => {
            const items = certificateItemsFlat();
            if (items.length === 0) {
                toast.error("Certificate must have at least one item.");
                return;
            }
            if (!validateCertificate(newCertificate, nonDeletedCertificateItems)) {
                return;
            }

            const certificateData = {
                WorkEffortId: newCertificate.workEffortId,
                CertificateCategory: currentCertificateType,
                Description: newCertificate.description,
                ProjectId: newCertificate.projectId,
                PartyIdSupplier: newCertificate.partyIdSupplier,
                PartyIdContractor: newCertificate.partyIdContractor,
                EstimatedStartDate: newCertificate.estimatedStartDate,
                EstimatedCompletionDate: newCertificate.estimatedCompletionDate,
                CertificateItems: items,
            };

            try {
                const createdCertificate = await addProjectCertificate(certificateData).unwrap();
                // REFACTOR: Updated setCertificate to use partyIdSupplier and partyIdContractor
                // Purpose: Reflect backend data accurately in the frontend state
                // Context: Matches updated Certificate interface
                setCertificate({
                    workEffortId: createdCertificate.WorkEffortId,
                    workEffortTypeId: createdCertificate.WorkEffortTypeId,
                    certificateNumber: createdCertificate.CertificateNumber,
                    projectId: createdCertificate.ProjectId,
                    partyIdSupplier: createdCertificate.PartyIdSupplier,
                    partyIdContractor: createdCertificate.PartyIdContractor,
                    description: createdCertificate.Description,
                    estimatedStartDate: createdCertificate.EstimatedStartDate,
                    estimatedCompletionDate: createdCertificate.EstimatedCompletionDate,
                    statusDescription: createdCertificate.StatusDescription,
                    certificateItems: createdCertificate.CertificateItems,
                });
                dispatch(setProcessedCertificateItems(createdCertificate.CertificateItems || []));
                dispatch(setCertificateFormEditMode(2)); // Move to CREATED state
                formRef2.current = !formRef2.current;
                toast.success("Certificate and items created successfully");
                return { workEffortId: createdCertificate.WorkEffortId };
            } catch (error: any) {
                console.error("Failed to create certificate:", error);
                toast.error(error?.data?.message || "Failed to create certificate and items");
                throw error;
            }
        },
        [addProjectCertificate, dispatch, formRef2, certificateItemsFlat, nonDeletedCertificateItems, currentCertificateType]
    );

    // REFACTOR: Updated updateCertificate to use partyIdSupplier and partyIdContractor
    // Purpose: Send correct supplier and contractor fields to the backend
    // Context: Aligns with ProjectCertificateForm's data structure
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

            const certificateData = {
                WorkEffortId: newCertificate.workEffortId,
                WorkEffortTypeId: newCertificate.workEffortTypeId,
                Description: newCertificate.description,
                ProjectId: newCertificate.projectId,
                PartyIdSupplier: newCertificate.partyIdSupplier,
                PartyIdContractor: newCertificate.partyIdContractor,
                EstimatedStartDate: newCertificate.estimatedStartDate,
                EstimatedCompletionDate: newCertificate.estimatedCompletionDate,
                CertificateItems: items,
            };

            try {
                const updatedCertificate = await updateProjectCertificate(certificateData).unwrap();
                // REFACTOR: Updated setCertificate to use partyIdSupplier and partyIdContractor
                // Purpose: Reflect backend data accurately in the frontend state
                // Context: Matches updated Certificate interface
                setCertificate({
                    workEffortId: updatedCertificate.WorkEffortId,
                    workEffortTypeId: updatedCertificate.WorkEffortTypeId,
                    certificateNumber: updatedCertificate.CertificateNumber,
                    projectId: updatedCertificate.ProjectId,
                    partyIdSupplier: updatedCertificate.PartyIdSupplier,
                    partyIdContractor: updatedCertificate.PartyIdContractor,
                    description: updatedCertificate.Description,
                    estimatedStartDate: updatedCertificate.EstimatedStartDate,
                    estimatedCompletionDate: updatedCertificate.EstimatedCompletionDate,
                    statusDescription: updatedCertificate.StatusDescription,
                    certificateItems: updatedCertificate.CertificateItems,
                });
                dispatch(setProcessedCertificateItems(updatedCertificate.CertificateItems || []));
                dispatch(setCertificateFormEditMode(2)); // Stay in CREATED state
                formRef2.current = !formRef2.current;
                toast.success("Certificate and items updated successfully");
                return { workEffortId: updatedCertificate.WorkEffortId };
            } catch (error: any) {
                console.error("Failed to update certificate:", error);
                toast.error(error?.data?.message || "Failed to update certificate and items");
                throw error;
            }
        },
        [updateProjectCertificate, dispatch, formRef2, certificateItemsFlat, nonDeletedCertificateItems, currentCertificateType]
    );

    // REFACTOR: Updated handleCreate to use partyIdSupplier and partyIdContractor
    // Purpose: Construct newCertificate with correct supplier and contractor fields
    // Context: Aligns with ProjectCertificateForm's form values
    const handleCreate = useCallback(
        async (data: any) => {
            setIsLoading(true);
            try {
                const newCertificate: Certificate = {
                    workEffortId: editMode > 1 ? certificate?.workEffortId : undefined,
                    workEffortTypeId: data.values.workEffortTypeId,
                    projectId: data.values.projectId?.projectId || data.values.projectId,
                    partyIdSupplier: data.values.partyIdSupplier?.fromPartyId || data.values.partyIdSupplier,
                    partyIdContractor: data.values.partyIdContractor?.fromPartyId || data.values.partyIdContractor,
                    description: data.values.description,
                    estimatedStartDate: data.values.estimatedStartDate,
                    estimatedCompletionDate: data.values.estimatedCompletionDate,
                    certificateItems: nonDeletedCertificateItems,
                };

                if (nonDeletedCertificateItems.length === 0) {
                    toast.error("Certificate items cannot be empty");
                    return;
                }

                if (selectedMenuItem === "Create Certificate" || editMode === 1) {
                    return await createCertificate(newCertificate);
                } else if (selectedMenuItem === "Update Certificate" || editMode === 2) {
                    return await updateCertificate(newCertificate);
                } else if (selectedMenuItem === "Approve Certificate") {
                    // Purpose: Provide a stub for future implementation
                    // Context: Currently not supported by backend
                    toast.error("Approve action not yet implemented");
                    return;
                } else {
                    toast.error("Invalid action type");
                    return;
                }
            } finally {
                setIsLoading(false);
            }
        },
        [createCertificate, updateCertificate, editMode, certificate, nonDeletedCertificateItems, selectedMenuItem, setIsLoading]
    );

    return {
        isAddCertificateLoading,
        isUpdateCertificateLoading,
        certificate,
        setCertificate,
        handleCreate,
    };
};

export default useProjectCertificate;