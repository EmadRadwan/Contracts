import { useCallback, useEffect, useState } from "react";
import { MultiPaymentCertificate } from "../../../app/models/project/MultiPaymentCertificate";
import { MultiPaymentItem } from "../../../app/models/project/MultiPaymentItem";
import { toast } from "react-toastify";
import {
    useAddMultiPaymentCertificateMutation,
    useGetMultiPaymentItemsQuery
} from "../../../app/store/apis/multiPaymentCertificateApi";

interface UseMultiPaymentCertificateProps {
    selectedCertificate?: MultiPaymentCertificate;
    editMode: number; // 1: add, 2: edit
    setFormKey: (key: number) => void;
}

export default function useMultiPaymentCertificate({
                                                       selectedCertificate,
                                                       editMode,
                                                       setFormKey,
                                                   }: UseMultiPaymentCertificateProps) {
    const [certificate, setCertificate] = useState<MultiPaymentCertificate | undefined>(selectedCertificate);
    const [items, setItems] = useState<MultiPaymentItem[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [addMultiPaymentCertificate, { isLoading: addLoading }] = useAddMultiPaymentCertificateMutation();

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
        // Set certificate and items based on selectedCertificate or editMode
        if (selectedCertificate) {
            setCertificate(selectedCertificate);
        } else {
            setCertificate(undefined);
        }

        // Cleanup function to reset state when component unmounts or props change
        return () => {
            // REFACTOR: Clear certificate and items when the component unmounts or editMode/selectedCertificate changes
            // This ensures a clean slate when the form is no longer visible or mode changes
            setCertificate(undefined);
            setItems([]);
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

    const handleCreate = useCallback(
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
                const response = await addMultiPaymentCertificate(payload).unwrap();
                setCertificate(response); 
                setItems(response.items || items); 
                toast.success("Certificate created successfully");
            } catch (error: any) {
                toast.error("Error creating certificate: " + (error?.data?.message || error.message));
            } finally {
                setIsLoading(false);
            }
        },
        [items, setFormKey, addMultiPaymentCertificate]
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
        [items, setFormKey]
    );
    
    console.log('items', items)

    return {
        certificate,
        setCertificate,
        items,
        addItem,
        updateItem,
        deleteItem,
        handleCreate,
        handleUpdate, setItems,
        isLoading: isLoading || itemsLoading,
    };
}