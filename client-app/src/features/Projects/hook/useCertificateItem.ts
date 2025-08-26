import { useCallback } from "react";
import { useSelector } from "react-redux";
import { toast } from "react-toastify";
import {
    useAddCertificateItemsMutation,
    useUpdateCertificateItemsMutation
} from "../../../app/store/apis/certificateItemsApi";
import {useAppDispatch} from "../../../app/store/configureStore";
import {nonDeletedCertificateItemsSelector} from "../slice/certificateSelectors";
import {CertificateItem} from "../../../app/models/project/certificateItem";

// REFACTOR: Define props
// Purpose: Type safety for hook props
// Context: Adapted from usePurchaseOrderItem, tailored for CertificateItem
interface UseCertificateItemProps {
    certificateItem?: CertificateItem;
    editMode: number; // 1: add, 2: edit
    setFormKey: (key: number) => void;
    setInitValue: (value: CertificateItem | undefined) => void;
    updateCertificateItems: (certificateItem: CertificateItem, editMode: number) => void;
}

// REFACTOR: Main hook
// Purpose: Handle certificate item submission with API and Redux integration
// Context: Modeled after usePurchaseOrderItem, uses workEffortId
export default function useCertificateItem({ certificateItem, editMode, setFormKey, setInitValue, updateCertificateItems }: UseCertificateItemProps) {
    const dispatch = useAppDispatch();
    const certificateItemsFromUi: CertificateItem[] = useSelector(nonDeletedCertificateItemsSelector);
    const [addCertificateItems, { isLoading: addCertificateItemLoading, error: addCertificateItemError }] = useAddCertificateItemsMutation();
    const [updateCertificateItemsMutation, { isLoading: updateCertificateItemLoading, error: updateCertificateItemError }] = useUpdateCertificateItemsMutation();

    // REFACTOR: Handle errors
    // Purpose: Log and display error messages
    // Context: Matches usePurchaseOrderItem's handleError
    const handleError = useCallback((error: any, defaultMessage: string) => {
        const message = error?.data?.message || error?.message || defaultMessage;
        console.error("Error:", JSON.stringify(error, null, 2));
        toast.error(message);
    }, []);

    // REFACTOR: Create or update certificate item
    // Purpose: Prepare CertificateItem for API submission
    // Context: Simplified from createOrUpdateOrderItem, uses workEffortId
    async function createOrUpdateCertificateItem(data: CertificateItem): Promise<CertificateItem> {
        let newCertificateItem: CertificateItem;
        if (editMode === 2) {
            // Update existing item
            newCertificateItem = {
                ...data,
                workEffortId: certificateItem?.workEffortId || "",
                workEffortParentId: certificateItem?.workEffortParentId || "",
                quantity: data.quantity,
                unitPrice: +data.unitPrice.toFixed(2),
                totalAmount: +data.totalAmount.toFixed(2),
                completionPercentage: data.completionPercentage,
                isDeleted: false,
            };
        } else {
            // Add new item
            const itemSeqId = certificateItemsFromUi?.length ? certificateItemsFromUi.length + 1 : 1;
            newCertificateItem = {
                ...data,
                workEffortId: undefined, // Backend assigns
                workEffortParentId: certificateItem?.workEffortParentId || "", // From parent certificate
                quantity: data.quantity,
                unitPrice: +data.unitPrice.toFixed(2),
                totalAmount: +data.totalAmount.toFixed(2),
                completionPercentage: data.completionPercentage,
                isDeleted: false,
            };
        }
        return newCertificateItem;
    }

    // REFACTOR: Handle form submission
    // Purpose: Process form data, call API, and update Redux
    // Context: Adapted from usePurchaseOrderItem's handleSubmitData
    async function handleSubmitData(data: CertificateItem) {
        try {
            const newCertificateItem = await createOrUpdateCertificateItem(data);
            let result;
            if (editMode === 1) {
                // Add new item
                result = await addCertificateItems(newCertificateItem).unwrap();
                updateCertificateItems({ ...newCertificateItem, workEffortId: result.workEffortId }, editMode);
                toast.success("Certificate item added successfully");
            } else if (editMode === 2) {
                // Update existing item
                result = await updateCertificateItemsMutation(newCertificateItem).unwrap();
                updateCertificateItems(newCertificateItem, editMode);
                toast.success("Certificate item updated successfully");
            } else {
                throw new Error("Invalid edit mode");
            }

            // Reset form
            setFormKey(Math.random());
            setInitValue(undefined);
        } catch (error: any) {
            handleError(error, `Failed to ${editMode === 1 ? "add" : "update"} certificate item`);
        }
    }

    return {
        handleSubmitData,
        addCertificateItemLoading,
        updateCertificateItemLoading,
        addCertificateItemError,
        updateCertificateItemError,
    };
}