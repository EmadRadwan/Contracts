import React, { useCallback, useEffect, useState } from "react";
import { useAppDispatch } from "../../../app/store/configureStore";
import { useSelector } from "react-redux";
import { toast } from "react-toastify";
import {Certificate} from "../../../app/models/project/certificate";
import {useAddProjectCertificateMutation} from "../../../app/store/apis/projectsApi";
import {CertificateItem} from "../../../app/models/project/certificateItem";
import {certificateItemsEntities} from "../slice/certificateItemsUiSlice";
import {nonDeletedCertificateItemsSelector} from "../slice/certificateSelectors";




type UseProjectCertificateProps = {
  selectedMenuItem: string;
  formRef2: React.MutableRefObject<boolean>;
  editMode: number;
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
  const [addProjectCertificate, { data: certificateResults, error, isLoading: isAddCertificateLoading }] =
    useAddProjectCertificateMutation();
  //const [updateProjectCertificate, { isLoading: isUpdateCertificateLoading }] = useUpdateProjectCertificateMutation();
  const [formEditMode, setFormEditMode] = useState(editMode);
  const [certificate, setCertificate] = useState<Certificate | undefined>(selectedCertificate);
  const certificateItemsFromUi: CertificateItem[] = useSelector(certificateItemsEntities);
  const nonDeletedCertificateItems: CertificateItem[] = useSelector(nonDeletedCertificateItemsSelector);


  const certificateItemsFlat = certificateItemsFromUi.map((item: CertificateItem) => ({
    ...item,
    workEffortParentId: certificate?.workEffortId || item.workEffortParentId,
  }));

  
  const createCertificate = useCallback(
    async (newCertificate: Certificate) => {
      try {
        newCertificate.certificateItems = certificateItemsFlat;
        const createdCertificate = await addProjectCertificate(newCertificate).unwrap();
        setCertificate({
          workEffortId: createdCertificate.workEffortId,
          projectNum: createdCertificate.projectNum,
          projectName: createdCertificate.projectName,
          partyId: createdCertificate.partyId,
          description: createdCertificate.description,
          estimated_start_date: createdCertificate.estimated_start_date,
          estimated_completion_date: createdCertificate.estimated_completion_date,
          statusDescription: createdCertificate.statusDescription,
        });
        setFormEditMode(2); // Move to CREATED state
        //dispatch(setCertificateFormEditMode(2));
        formRef2.current = !formRef2.current;
        toast.success("Certificate Created Successfully");
        return { workEffortId: createdCertificate.workEffortId };
      } catch (error: any) {
        console.error("Failed to create certificate:", error);
        toast.error("Failed to create certificate");
        throw error;
      } finally {
        setIsLoading(false);
      }
    },
    [addProjectCertificate, dispatch, formRef2, setIsLoading]
  );

  
  // REFACTOR: Handle create/update/approve actions
  // Purpose: Centralize action logic
  // Context: Mirrors handleCreate in usePurchaseOrder
  const handleCreate = useCallback(
    async (data: any) => {
      setIsLoading(true);
      const actionType = data.selectedMenuItem || (formEditMode === 1 ? "Create Certificate" : "Update Certificate");
      const newCertificate: Certificate = {
        workEffortId: formEditMode > 1 ? certificate?.workEffortId : undefined,
        work_effort_type_id: data.values.work_effort_type_id,
        projectName: data.values.projectId,
        partyId: data.values.partyId,
        description: data.values.description,
        estimated_start_date: data.values.estimatedStartDate?.toISOString() || null,
        estimated_completion_date: data.values.estimatedCompletionDate?.toISOString() || null,
      };

      if (nonDeletedCertificateItems.length === 0) {
        toast.error("Certificate items cannot be empty");
        setIsLoading(false);
        return;
      }

      if (actionType === "Create Certificate") {
        return await createCertificate(newCertificate);
      } else if (actionType === "Update Certificate") {
        //return await updateOrApproveCertificate(newCertificate, "UPDATE");
      } else if (actionType === "Approve Certificate") {
        //return await updateOrApproveCertificate(newCertificate, "APPROVE");
      } else {
        toast.error("Invalid action type");
        setIsLoading(false);
        return;
      }
    },
    [createCertificate, formEditMode, certificate, nonDeletedCertificateItems]
  );

  return {
    certificateResults,
    error,
    isAddCertificateLoading,
    //isUpdateCertificateLoading,
    formEditMode,
    setFormEditMode,
    certificate,
    setCertificate,
    handleCreate,
  };
};

export default useProjectCertificate;