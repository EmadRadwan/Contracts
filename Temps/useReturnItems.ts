import { useEffect, useState } from "react";
import { toast } from "react-toastify";
import {
    useFetchReturnItemsQuery,
    useFetchReturnAdjustmentsQuery,
    useFetchReturnTypesQuery,
    useFetchReturnReasonsQuery,
    useFetchPartyOrdersQuery,
    useFetchReturnableItemsQuery,
    useUpdateReturnItemsMutation,
    useRemoveReturnItemMutation,
    useRemoveReturnAdjustmentMutation,
    useUpdateReturnMutation,
    useCreateReturnItemsMutation,
} from "../../../app/store/apis";
import { ReturnItem, ReturnAdjustment } from "../../../app/models/order/return";
import { useAppDispatch } from "../../../app/store/configureStore";
import { setUiReturnItems, setUiReturnAdjustments } from "../slice/returnUiSlice";

interface UseReturnItemsProps {
    returnId: string;
    orderId?: string;
    partyId?: string;
    roleTypeId?: string;
}

const useReturnItems = ({ returnId, orderId, partyId, roleTypeId }: UseReturnItemsProps) => {
    const dispatch = useAppDispatch();

    // Refactored: Fetch return items and adjustments
    const { data: returnItems, isFetching: isFetchingReturnItems } = useFetchReturnItemsQuery(returnId);
    const { data: returnAdjustments, isFetching: isFetchingReturnAdjustments } = useFetchReturnAdjustmentsQuery({ returnId, returnItemSeqId: "_NA_" });
    const { data: returnTypes, isFetching: isFetchingReturnTypes } = useFetchReturnTypesQuery(undefined);
    const { data: returnReasons, isFetching: isFetchingReturnReasons } = useFetchReturnReasonsQuery(undefined);
    const { data: partyOrders, isFetching: isFetchingPartyOrders } = useFetchPartyOrdersQuery(
        { partyId, roleTypeId },
        { skip: !partyId || !roleTypeId }
    );
    const { data: returnableItems, isFetching: isFetchingReturnableItems } = useFetchReturnableItemsQuery(orderId, { skip: !orderId });
    const [updateReturnItems] = useUpdateReturnItemsMutation();
    const [removeReturnItem] = useRemoveReturnItemMutation();
    const [removeReturnAdjustment] = useRemoveReturnAdjustmentMutation();
    const [updateReturn] = useUpdateReturnMutation();
    const [createReturnItems] = useCreateReturnItemsMutation();

    const [items, setItems] = useState<ReturnItem[]>([]);
    const [adjustments, setAdjustments] = useState<ReturnAdjustment[]>([]);

    // Refactored: Sync items and adjustments with fetched data
    useEffect(() => {
        if (returnItems) {
            setItems(returnItems);
            dispatch(setUiReturnItems(returnItems));
        }
        if (returnAdjustments) {
            setAdjustments(returnAdjustments);
            dispatch(setUiReturnAdjustments(returnAdjustments));
        }
    }, [returnItems, returnAdjustments, dispatch]);

    // Refactored: Handle loading order items
    const handleLoadOrderItems = async (orderId: string) => {
        try {
            const newItems = await createReturnItems({ returnId, orderId }).unwrap();
            setItems(newItems);
            dispatch(setUiReturnItems(newItems));
            toast.success("Order items loaded successfully");
        } catch (e) {
            toast.error("Failed to load order items");
            console.error(e);
        }
    };

    // Refactored: Handle removing return item
    const handleRemoveItem = async (returnItemSeqId: string) => {
        try {
            await removeReturnItem({ returnId, returnItemSeqId }).unwrap();
            toast.success("Return item removed successfully");
        } catch (e) {
            toast.error("Failed to remove return item");
            console.error(e);
        }
    };

    // Refactored: Handle removing return adjustment
    const handleRemoveAdjustment = async (returnAdjustmentId: string) => {
        try {
            await removeReturnAdjustment({ returnId, returnAdjustmentId }).unwrap();
            toast.success("Return adjustment removed successfully");
        } catch (e) {
            toast.error("Failed to remove return adjustment");
            console.error(e);
        }
    };

    // Refactored: Handle accepting return
    const handleAcceptReturn = async () => {
        try {
            const statusId = items.some(item => item.returnHeaderTypeId?.startsWith("CUSTOMER_")) ? "RETURN_ACCEPTED" : "SUP_RETURN_ACCEPTED";
            await updateReturn({
                returnId,
                statusId,
                needsInventoryReceive: items[0]?.needsInventoryReceive || "N",
            }).unwrap();
            toast.success("Return accepted successfully");
        } catch (e) {
            toast.error("Failed to accept return");
            console.error(e);
        }
    };

    return {
        returnItems: items,
        returnAdjustments: adjustments,
        returnTypes,
        returnReasons,
        partyOrders,
        returnableItems,
        isFetching: isFetchingReturnItems || isFetchingReturnAdjustments || isFetchingReturnTypes || isFetchingReturnReasons || isFetchingPartyOrders || isFetchingReturnableItems,
        handleLoadOrderItems,
        handleRemoveItem,
        handleRemoveAdjustment,
        handleAcceptReturn,
    };
};

export default useReturnItems;