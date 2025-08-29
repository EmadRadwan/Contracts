import { useCallback } from "react";
import { useSelector } from "react-redux";
import { toast } from "react-toastify";
import { useAppDispatch } from "../../../app/store/configureStore";
import { nonDeletedCertificateItemsSelector } from "../slice/certificateSelectors";
import { CertificateItem } from "../../../app/models/project/certificateItem";
import {setProcessedCertificateItems} from "../slice/certificateItemsUiSlice";

interface UseCertificateItemProps {
    certificateItem?: CertificateItem;
    editMode: number; // 1: add, 2: edit
    setFormKey: (key: number) => void;
    setInitValue: (value: CertificateItem | undefined) => void;
    updateCertificateItems: (certificateItem: CertificateItem, editMode: number) => void;
}

export default function useCertificateItem({
                                               certificateItem,
                                               editMode,
                                               setFormKey,
                                               setInitValue,
                                               updateCertificateItems,
                                           }: UseCertificateItemProps) {
    const dispatch = useAppDispatch();
    const certificateItemsFromUi: CertificateItem[] = useSelector(nonDeletedCertificateItemsSelector);

    // REFACTOR: Simplified error handling to log and display errors via toast
    const handleError = useCallback((error: any, defaultMessage: string) => {
        const message = error?.data?.message || error?.message || defaultMessage;
        console.error("Error:", JSON.stringify(error, null, 2));
        toast.error(message);
    }, []);

    // REFACTOR: Simplified createOrUpdateCertificateItem to prepare CertificateItem for direct dispatch
    const createOrUpdateCertificateItem = useCallback(
        (data: CertificateItem): CertificateItem => {
            let newCertificateItem: CertificateItem;
            if (editMode === 2) {
                // Update existing item
                newCertificateItem = {
                    ...data,
                    workEffortId: certificateItem?.workEffortId || "",
                    workEffortParentId: certificateItem?.workEffortParentId || "",
                    quantity: data.quantity,
                    unitPrice: +data.unitPrice.toFixed(2),
                    total: +data.total.toFixed(2), // Use total instead of totalAmount
                    net: +data.net.toFixed(2),
                    completionPercentage: data.completionPercentage,
                    isDeleted: false,
                };
            } else {
                // Add new item
                const itemSeqId = certificateItemsFromUi?.length ? certificateItemsFromUi.length + 1 : 1;
                newCertificateItem = {
                    ...data,
                    workEffortId: `TEMP-${itemSeqId}`, // Temporary ID since no backend save
                    workEffortParentId: certificateItem?.workEffortParentId || "",
                    quantity: data.quantity,
                    unitPrice: +data.unitPrice.toFixed(2),
                    total: +data.total.toFixed(2), // Use total instead of totalAmount
                    net: +data.net.toFixed(2),
                    completionPercentage: data.completionPercentage,
                    isDeleted: false,
                };
            }
            return newCertificateItem;
        },
        [certificateItem, editMode, certificateItemsFromUi]
    );

    // REFACTOR: Updated handleSubmitData to dispatch directly to Redux without API call
    const handleSubmitData = useCallback(
        async (data: CertificateItem) => {
            try {
                const newCertificateItem = createOrUpdateCertificateItem(data);
                // Dispatch to Redux store using setProcessedCertificateItems
                dispatch(setProcessedCertificateItems([newCertificateItem]));
                // Call updateCertificateItems to maintain parent component state
                updateCertificateItems(newCertificateItem, editMode);
                toast.success(`Certificate item ${editMode === 1 ? "added" : "updated"} successfully`);
                // Reset form
                setFormKey(Math.random());
                setInitValue(undefined);
            } catch (error: any) {
                handleError(error, `Failed to ${editMode === 1 ? "add" : "update"} certificate item`);
            }
        },
        [dispatch, createOrUpdateCertificateItem, editMode, updateCertificateItems, setFormKey, setInitValue, handleError]
    );

    return {
        handleSubmitData,
    };
}