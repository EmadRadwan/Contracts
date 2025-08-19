import { useSelector } from "react-redux";
import { useAppDispatch } from "../../../app/store/configureStore";
import { OrderItem } from "../../../app/models/order/orderItem";
import { nonDeletedOrderItemsSelector } from "../slice/orderSelectors";
import { toast } from "react-toastify";
import { useProcessPurchaseOrderItemMutation } from "../../../app/store/apis";
import {useCallback} from "react";

type UsePurchaseOrderItem = {
    orderItem: any;
    editMode: number;
    setFormKey: (key: number) => void;
    setInitValue: (value: OrderItem | undefined) => void;
};

export default function usePurchaseOrderItem({
                                                 orderItem,
                                                 editMode,
                                                 setFormKey,
                                                 setInitValue,
                                             }: UsePurchaseOrderItem) {
    const orderItemsFromUi: any = useSelector(nonDeletedOrderItemsSelector);
    const addTax = useSelector((state: any) => state.sharedOrderUi.addTax);
    const dispatch = useAppDispatch();
    const [trigger, { data: processOrderItemData, error: processOrderItemError, isLoading: processOrderItemLoading }] =
        useProcessPurchaseOrderItemMutation();

    async function handleSubmitData(data: any) {
        try {
            const newOrderItem = await createOrUpdateOrderItem(data);
            const result = await trigger(newOrderItem).unwrap();

            if (result.status === "Success") {
                setFormKey(Math.random());
                setInitValue(undefined);
                //toast.success("Order item processed successfully");
            } else {
                throw new Error("Failed to process order item");
            }
        } catch (error: any) {
            //toast.error(error.message || "Error processing order item");
            handleError(error);
        }
    }

    async function createOrUpdateOrderItem(data: any): Promise<OrderItem> {
        let newOrderItem: OrderItem;
        if (editMode === 2) {
            newOrderItem = {
                ...data,
                quantity: data.quantity,
                unitPrice: +data.unitPrice.toFixed(2),
                subTotal: +data.unitPrice.toFixed(2) * data.quantity,
                collectTax: addTax, // Optional: Remove if not needed
            };
        } else {
            const orderItemSeqId = orderItemsFromUi?.length
                ? orderItemsFromUi.length + 1
                : 1;

            newOrderItem = {
                inventoryItemId: data.productId.inventoryItem,
                productTypeId: data.productId.productTypeId,
                isProductDeleted: false,
                orderItemSeqId: orderItemSeqId.toString().padStart(2, "0"),
                productId: data.productId.productId,
                productFeatureId: data.productId.productFeatureId,
                productLov: data.productId,
                productName: data.productId.productName,
                quantity: data.quantity,
                unitPrice: +data.unitPrice.toFixed(2),
                subTotal: +data.unitPrice.toFixed(2) * data.quantity,
                collectTax: addTax, // Optional: Remove if not needed
            };
            if (orderItem) {
                newOrderItem.orderId = orderItem.orderId;
            } else {
                newOrderItem.orderId = "ORDER-DUMMY";
            }
        }
        return newOrderItem;
    }

    const handleError = useCallback((error: any, defaultMessage: string) => {
        const message = error?.data?.message || error?.message || defaultMessage;
        console.error("Error:", JSON.stringify(error, null, 2));
        toast.error(message);
    }, []);

    return {
        handleSubmitData,
        processOrderItemLoading,
        processOrderItemError,
        processOrderItemData,
    };
}