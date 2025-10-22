import { useCallback, useEffect, useState } from "react";
import { MultiPaymentCertificate } from "../../../app/models/project/MultiPaymentCertificate";
import { MultiPaymentItem } from "../../../app/models/project/MultiPaymentItem";
import { toast } from "react-toastify";
import {
    useAddMultiPaymentCertificateMutation, useApproveMultiPaymentCertificateMutation,
    useGetMultiPaymentItemsQuery
} from "../../../app/store/apis/multiPaymentCertificateApi";
import {useAppSelector} from "../../../app/store/configureStore";

interface UseMultiPaymentCertificateProps {
    selectedCertificate?: MultiPaymentCertificate;
    editMode: number; // 1: add, 2: edit
    setFormKey: (key: number) => void;
    setEditMode?: (mode: number) => void;
    setParentCertificate?: (certificate: MultiPaymentCertificate | null) => void;
}

export default function useMultiPaymentCertificate({
                                                       selectedCertificate,
                                                       editMode,
                                                       setFormKey,
                                                       setEditMode,
                                                       setParentCertificate
                                                   }: UseMultiPaymentCertificateProps) {
    const [certificate, setCertificate] = useState<MultiPaymentCertificate | undefined>(selectedCertificate);
    const [items, setItems] = useState<MultiPaymentItem[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [addMultiPaymentCertificate, { isLoading: addLoading }] = useAddMultiPaymentCertificateMutation();
    const [approveMultiPaymentCertificate, { isLoading: approveLoading }] = useApproveMultiPaymentCertificateMutation();
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "";

    const { data: fetchedItems = [], isLoading: itemsLoading } = useGetMultiPaymentItemsQuery(
        certificate?.workEffortId || "",
        { skip: editMode !== 2 || !certificate?.workEffortId } // Skip query in add mode or without workEffortId
    );

    useEffect(() => {
        if (editMode === 2 && fetchedItems.length > 0) {
            setItems(fetchedItems);
        }
    }, [fetchedItems, editMode]);

    useEffect(() => {
        if (selectedCertificate) {
            setCertificate(selectedCertificate);
        }
        if (editMode === 1) {
            setCertificate(undefined);
            setItems([]);
        }
        return () => {
            if (editMode === 1) {
                setCertificate(undefined);
                setItems([]);
            }
        };
    }, [selectedCertificate, editMode]);

    const addItem = useCallback((item: MultiPaymentItem) => {
        setItems((prev) => [...prev, { ...item, itemId: item.itemId || uuidv4() }]);
    }, []);

    const updateItem = useCallback((updatedItem: MultiPaymentItem) => {
        setItems((prev) =>
            prev.map((item) =>
                item.itemId === updatedItem.itemId ? { ...updatedItem } : item
            )
        );
    }, []);

    const deleteItem = useCallback((itemId: string) => {
        setItems((prev) => prev.filter((item) => item.itemId !== itemId));
    }, []);

    const validateCertificate = useCallback((certificate: MultiPaymentCertificate, items: MultiPaymentItem[]) => {
        if (items.length === 0) {
            toast.error("Certificate must have at least one item.");
            return false;
        }
        const isItemsValid = items.every((item) => {
            const isValid = item.amount > 0 && !!item.description;
            if (!isValid) {
                if (item.amount <= 0) toast.error("Item amount must be greater than 0");
                if (!item.description) toast.error("Item description is required");
            }
            return isValid;
        });
        return isItemsValid;
    }, []);


    const handleCreate = useCallback(
        async ({ values, isValid }: { values: MultiPaymentCertificate; isValid: boolean }) => {
            if (!isValid) {
                toast.error("Form is invalid. Please check all fields.");
                return;
            }
            if (!validateCertificate(values, items)) {
                return;
            }
            setIsLoading(true);
            try {
                const payload = {
                    ...values,
                    items,
                    currentStatusId: "WEPR_CREATED", // REFACTOR: Set status to WEPR_CREATED
                    statusDescription: "Created",
                    statusDescriptionArabic: "تم الإنشاء",
                };
                const response = await addMultiPaymentCertificate(payload).unwrap();
                setCertificate(response); 
                setItems(response.items || items);
                setParentCertificate?.(response);
                toast.success("Certificate created successfully");
                setEditMode?.(2);
                return { success: true, certificate: response };
            } catch (error: any) {
                toast.error("Error creating certificate: " + (error?.data?.message || error.message));
            } finally {
                setIsLoading(false);
            }
        },
        [items, setFormKey, addMultiPaymentCertificate, setParentCertificate]
    );

    const handleUpdate = useCallback(
        async ({ values, isValid }: { values: MultiPaymentCertificate; isValid: boolean }) => {
            if (!isValid) {
                toast.error("Form is invalid. Please check all fields.");
                return;
            }
            setIsLoading(true);
            try {
                const payload = {
                    ...values,
                    items,
                };

                const response = await fetch(`/api/multi-payment-certificates/${values.workEffortId}`, {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(payload),
                });

                if (!response.ok) {
                    throw new Error("Failed to update certificate");
                }
                setParentCertificate?.(await response.json());

                toast.success("Certificate updated successfully");
                setCertificate(undefined);
                setItems([]);
                setFormKey((prev) => prev + 1);
            } catch (error: any) {
                toast.error("Error updating certificate: " + error.message);
            } finally {
                setIsLoading(false);
            }
        },
        [items, setFormKey, setParentCertificate]
    );


    const handleApprove = useCallback(
        async ({ workEffortId, isValid }: { workEffortId: string; isValid: boolean }) => {
            if (!isValid) {
                toast.error("Form is invalid. Please check all fields.");
                return;
            }
            if (!validateCertificate(certificate || { workEffortId }, items)) {
                return;
            }
            if (!workEffortId) {
                toast.error("Work Effort ID is required for approval");
                return;
            }
            setIsLoading(true);
            try {
                const response = await approveMultiPaymentCertificate({ workEffortId, companyId }).unwrap();
                const updatedCertificate = {
                    ...certificate,
                    currentStatusId: "WEPR_APPROVED",
                    statusDescription: "Approved",
                    statusDescriptionArabic: "تمت الموافقة",
                };
                setCertificate(updatedCertificate);
                setParentCertificate?.(updatedCertificate);
                setEditMode?.(3); // Keep post-approval mode as 3 per your preference
                toast.success("Certificate approved successfully");
                return { success: true, certificate: response };
            } catch (error: any) {
                toast.error("Error approving certificate: " + (error?.data?.message || error.message));
            } finally {
                setIsLoading(false);
            }
        },
        [certificate, items, approveMultiPaymentCertificate, validateCertificate]
    );

    return {
        certificate,
        setCertificate,
        items,
        addItem,
        updateItem,
        deleteItem,
        handleCreate,
        handleUpdate,
        handleApprove,
        setItems,
        isLoading: isLoading || itemsLoading || addLoading || approveLoading,
    };
}