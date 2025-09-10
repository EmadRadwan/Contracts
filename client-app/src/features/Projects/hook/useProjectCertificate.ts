import React, { useCallback, useState } from "react";
import { toast } from "react-toastify";
import { Certificate } from "../../../app/models/project/certificate";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import {
  useAddProjectCertificateMutation,
  useFetchProjectCertificatesQuery,
  useUpdateProjectCertificateMutation
} from "../../../app/store/apis/projectsApi";
import { certificateItemsEntities, setProcessedCertificateItems } from "../slice/certificateItemsUiSlice";
import { setCertificateFormEditMode, setSelectedCertificate } from "../slice/certificateUiSlice";
import { useAppDispatch, useAppSelector } from "../../../app/store/configureStore";
import { useSelector } from "react-redux";
import { nonDeletedCertificateItemsSelector } from "../slice/certificateSelectors";

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
  const [addProjectCertificate, { isLoading: isAddCertificateLoading }] = useAddProjectCertificateMutation();
  const [updateProjectCertificate, { isLoading: isUpdateCertificateLoading }] = useUpdateProjectCertificateMutation();
  const nonDeletedCertificateItems: CertificateItem[] = useSelector(nonDeletedCertificateItemsSelector);
  const { currentCertificateType, selectedCertificate, certificateFormEditMode } = useAppSelector((state) => state.certificateUi);
  const { refetch } = useFetchProjectCertificatesQuery({ skip: 0, take: 6 });

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
      procurementDate: item.procurementDate ? new Date(item.procurementDate).toISOString() : undefined,
    }));
  }, [nonDeletedCertificateItems]);


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
            (item.achievementPercentage && item.achievementPercentage >= 1 && item.achievementPercentage <= 100));
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
                certificateItems: items,
            };

            try {
                const createdCertificate = await addProjectCertificate(certificateData).unwrap();
                // REFACTOR: Update Redux selectedCertificate with full object structures
                // Purpose: Ensure form re-renders with complete objects for FormComboBox components
                // Context: API returns workEffortId and certificateNumber; other fields are from input
                dispatch(setSelectedCertificate({
                    workEffortId: createdCertificate.workEffortId,
                    projectNum: createdCertificate.certificateNumber,
                    projectId: newCertificate.projectId,
                    projectName: newCertificate.projectName || createdCertificate.projectName || "",
                    partyIdSupplier: newCertificate.partyIdSupplier ? {
                        fromPartyId: typeof newCertificate.partyIdSupplier === "object"
                            ? newCertificate.partyIdSupplier.fromPartyId
                            : newCertificate.partyIdSupplier,
                        partyName: newCertificate.partyIdSupplier?.partyName || createdCertificate.partyNameSupplier || "",
                    } : undefined,
                    partyIdContractor: newCertificate.partyIdContractor ? {
                        fromPartyId: typeof newCertificate.partyIdContractor === "object"
                            ? newCertificate.partyIdContractor.fromPartyId
                            : newCertificate.partyIdContractor,
                        partyName: newCertificate.partyIdContractor?.partyName || createdCertificate.partyNameContractor || "",
                    } : undefined,
                    description: newCertificate.description,
                    estimatedStartDate: newCertificate.estimatedStartDate
                        ? new Date(newCertificate.estimatedStartDate).toISOString()
                        : null,
                    estimatedCompletionDate: newCertificate.estimatedCompletionDate
                        ? new Date(newCertificate.estimatedCompletionDate).toISOString()
                        : null,
                    statusDescription: createdCertificate.statusDescription,
                }));
                dispatch(setCertificateFormEditMode(2));
                dispatch(setProcessedCertificateItems(createdCertificate.certificateItems || []));
                formRef2.current = !formRef2.current;
                toast.success("Certificate and items created successfully");
                refetch();
                return { workEffortId: createdCertificate.workEffortId };
            } catch (error: any) {
                console.error("Failed to create certificate:", error);
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

            const certificateData = {
                workEffortId: newCertificate.workEffortId,
                workEffortTypeId: newCertificate.workEffortTypeId,
                description: newCertificate.description,
                projectId: newCertificate.projectId,
                partyIdSupplier: typeof newCertificate.partyIdSupplier === "object"
                    ? newCertificate.partyIdSupplier.fromPartyId
                    : newCertificate.partyIdSupplier,
                partyIdContractor: typeof newCertificate.partyIdContractor === "object"
                    ? newCertificate.partyIdContractor.fromPartyId
                    : newCertificate.partyIdContractor,
                estimatedStartDate: newCertificate.estimatedStartDate,
                estimatedCompletionDate: newCertificate.estimatedCompletionDate,
                certificateItems: items,
            };

            try {
                const updatedCertificate = await updateProjectCertificate(certificateData).unwrap();
                // REFACTOR: Update Redux selectedCertificate with full object structures
                // Purpose: Ensure form re-renders with complete objects for FormComboBox components
                // Context: API returns updated workEffortId and certificateNumber; other fields are from input
                dispatch(setSelectedCertificate({
                    workEffortId: updatedCertificate.workEffortId,
                    projectNum: updatedCertificate.certificateNumber,
                    projectId: newCertificate.projectId,
                    projectName: newCertificate.projectName || updatedCertificate.projectName || "",
                    partyIdSupplier: newCertificate.partyIdSupplier ? {
                        fromPartyId: typeof newCertificate.partyIdSupplier === "object"
                            ? newCertificate.partyIdSupplier.fromPartyId
                            : newCertificate.partyIdSupplier,
                        partyName: newCertificate.partyIdSupplier?.partyName || updatedCertificate.partyNameSupplier || "",
                    } : undefined,
                    partyIdContractor: newCertificate.partyIdContractor ? {
                        fromPartyId: typeof newCertificate.partyIdContractor === "object"
                            ? newCertificate.partyIdContractor.fromPartyId
                            : newCertificate.partyIdContractor,
                        partyName: newCertificate.partyIdContractor?.partyName || updatedCertificate.partyNameContractor || "",
                    } : undefined,
                    description: newCertificate.description,
                    estimatedStartDate: newCertificate.estimatedStartDate
                        ? new Date(newCertificate.estimatedStartDate).toISOString()
                        : null,
                    estimatedCompletionDate: newCertificate.estimatedCompletionDate
                        ? new Date(newCertificate.estimatedCompletionDate).toISOString()
                        : null,
                    statusDescription: updatedCertificate.statusDescription,
                }));
                dispatch(setProcessedCertificateItems(updatedCertificate.certificateItems || []));
                dispatch(setCertificateFormEditMode(2));
                formRef2.current = !formRef2.current;
                toast.success("Certificate and items updated successfully");
                return { workEffortId: updatedCertificate.workEffortId };
            } catch (error: any) {
                console.error("Failed to update certificate:", error);
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
                // REFACTOR: Use full objects from form data for Redux
                // Purpose: Preserve object structure for FormComboBox components
                // Context: Flatten only for API calls, not for Redux state
                const newCertificate: Certificate = {
                    workEffortId: editMode > 1 ? selectedCertificate?.workEffortId : undefined,
                    workEffortTypeId: data.values.workEffortTypeId,
                    projectId: data.values.projectId?.projectId || data.values.projectId,
                    projectName: data.values.projectId?.projectName,
                    partyIdSupplier: data.values.partyIdSupplier,
                    partyIdContractor: data.values.partyIdContractor,
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
        [createCertificate, updateCertificate, editMode, selectedCertificate, nonDeletedCertificateItems, selectedMenuItem, setIsLoading]
    );

    return {
        isAddCertificateLoading,
        isUpdateCertificateLoading,
        formEditMode,
        setFormEditMode,
        handleCreate,
    };
};

export default useProjectCertificate;