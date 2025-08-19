import { useEffect } from 'react';
import { InvoiceItem } from '../../../../app/models/accounting/invoiceItem';
import { toast } from 'react-toastify';
import {
    useAddInvoiceItemMutation,
    useUpdateInvoiceItemsMutation
} from "../../../../app/store/apis/invoice/invoiceItemsApi";

interface Props {
    editMode: number; // 1 for create, 2 for edit
    invoiceItem?: InvoiceItem;
    invoiceId: string; // REFACTOR: Added invoiceId to Props
    setIsLoading?: React.Dispatch<React.SetStateAction<boolean>>;
}

const useInvoiceItem = ({ editMode, invoiceItem, invoiceId, setIsLoading }: Props) => {
    const [createInvoiceItem, { isLoading: isCreateItemLoading, error: isCreateItemError }] = useAddInvoiceItemMutation();
    const [updateInvoiceItem, { isLoading: isUpdateItemLoading, error: isUpdateItemError }] = useUpdateInvoiceItemsMutation();

    // REFACTOR: Removed useAppSelector for selectedInvoiceId
    // Purpose: Eliminates dependency on Redux store for invoiceId
    // Improvement: Uses invoiceId prop directly, improving decoupling and reliability
    useEffect(() => {
        // Placeholder for any future initialization logic
    }, [invoiceItem]);

    // REFACTOR: Updated handleCreate to use invoiceId prop
    // Purpose: Replaces selectedInvoiceId with invoiceId from Props
    // Improvement: Ensures consistency with EditInvoiceItem's invoiceId prop
    const handleCreate = async (data: any) => {
        try {
            const formattedData: InvoiceItem = {
                ...data,
                invoiceId, // REFACTOR: Use invoiceId from Props instead of selectedInvoiceId
                // REFACTOR: Set taxableFlag to 'N' if undefined, aligning with form
                // Purpose: Ensures consistent data submission
                // Improvement: Prevents undefined values in API payload
                taxableFlag: data.taxableFlag ?? 'N',
                // REFACTOR: Handle productId as object or string
                // Purpose: Supports FormMultiColumnComboBoxVirtualFacilityProduct output
                // Improvement: Ensures compatibility with conditional product field
                productId: typeof data.productId === 'object' && data.productId ? data.productId.productId : data.productId || null,
                // REFACTOR: Explicitly include uomId to match form's mapped data
                // Purpose: Ensures uomId (mapped from quantityUomId) is sent
                // Improvement: Maintains consistency with backend expectations
                uomId: data.uomId || null
            };

            let result;
            if (editMode === 1) {
                result = await createInvoiceItem(formattedData).unwrap();
                toast.success('Invoice item created successfully');
            } else {
                result = await updateInvoiceItem(formattedData).unwrap();
                toast.success('Invoice item updated successfully');
            }

            return result;
        } catch (e) {
            // REFACTOR: Improved error handling with toast notification
            // Purpose: Provides user feedback on failure
            // Improvement: Enhances UX by communicating errors
            toast.error('Failed to save invoice item');
            throw e;
        } finally {
            // REFACTOR: Ensured setIsLoading is called only if provided
            // Purpose: Prevents errors if setIsLoading is undefined
            // Improvement: Makes hook more robust for varying component usage
            setIsLoading?.(false);
        }
    };

    return {
        handleCreate,
        isCreateItemError,
        isCreateItemLoading,
        isUpdateItemError,
        isUpdateItemLoading
    };
};

export default useInvoiceItem;