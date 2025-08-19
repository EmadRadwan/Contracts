import { useCallback } from "react";
import { useProcessOrderItemMutation } from "../../../app/store/apis";
import { useAppDispatch } from "../../../app/store/configureStore";
import { OrderItem } from "../../../app/models/order/orderItem";
import { toast } from "react-toastify";
import { useSelector } from "react-redux";
import { nonDeletedOrderItemsSelector } from "../slice/orderSelectors";
import { useProductPromotionsData } from "./useProductPromotionsData";
import { setProcessedOrderItems } from "../slice/orderItemsUiSlice";
import {setNeedsTaxRecalculation} from "../slice/sharedOrderUiSlice";

type UseSalesOrderItemProps = {
    orderItem: any;
    editMode: number;
    productPrice: any;
};

export default function useSalesOrderItem({ orderItem, editMode, productPrice }: UseSalesOrderItemProps) {
    const productPromotions = useProductPromotionsData().productPromotions;
    const dispatch = useAppDispatch();
    const orderItemsFromUi: OrderItem[] = useSelector(nonDeletedOrderItemsSelector);
    const addTax = useSelector((state: any) => state.sharedOrderUi.addTax);
    const [processOrderItem, { data: processOrderItemData, error: processOrderItemError, isLoading: processOrderItemLoading, isFetching: processOrderItemFetching }] = useProcessOrderItemMutation();

    const emptyPromotion = { productPromoId: "", promoText: "" };
    const productPromotionsWithEmpty = productPromotions ? [emptyPromotion, ...productPromotions] : [emptyPromotion];

    // REFACTOR: Enhanced error handling with specific messages
    // Purpose: Provide clearer feedback for different failure scenarios
    // Why: Improves debugging and user experience
    const handleError = useCallback((error: any, defaultMessage: string) => {
        const message = error?.data?.message || error?.message || defaultMessage;
        console.error("Error:", JSON.stringify(error, null, 2));
        toast.error(message);
    }, []);

    async function handleSubmitData(data: any) {
        console.log('data', data);
        try {
            const newOrderItem = await createOrUpdateOrderItem(data);
            const processResult = await processOrderItem(newOrderItem).unwrap();
            if (processResult.status !== "Success") {
                throw new Error(processOrderItemError?.data?.message || "Failed to process order item");
            }
            const updatedOrderItems =
                editMode === 2
                    ? orderItemsFromUi.map((item: OrderItem) =>
                        item.orderItemSeqId === newOrderItem.orderItemSeqId ? newOrderItem : item
                    )
                    : [...orderItemsFromUi, newOrderItem];

            dispatch(setProcessedOrderItems(updatedOrderItems));
            if (addTax) {
                dispatch(setNeedsTaxRecalculation(true));
            }
        } catch (error) {
            handleError(error, editMode === 2 ? "Failed to update order item" : "Failed to create order item");
        }
    }

    async function createOrUpdateOrderItem(data: any): Promise<OrderItem> {
        let newOrderItem: OrderItem;
        if (editMode === 2) {
            newOrderItem = {
                ...data,
                quantity: data.quantity,
                productId: data.productId.productId,
                productFeatureId: data.productFeatureId, // REFACTOR: Added productFeatureId to order item
                subTotal: +productPrice.price.toFixed(2) * data.quantity * +productPrice.piecesIncluded,
                collectTax: addTax, 
            };
        } else {
            const orderItemSeqId = orderItemsFromUi?.length ? orderItemsFromUi?.length + 1 : 1;
            newOrderItem = {
              inventoryItemId: data.productId.inventoryItem,
              productTypeId: data.productId.productTypeId,
              isProductDeleted: false,
              orderItemSeqId: orderItemSeqId.toString().padStart(2, "0"),
              productId: data.productId.productId,
              productLov: data.productId,
              productName: data.productId.productName + ' ' + data.productId.colorDescription,
              quantity: data.quantity,
              unitPrice: +productPrice.price.toFixed(2),
              unitListPrice: +productPrice.price.toFixed(2),
              productPromoId: data.productPromoId,
                productFeatureId: data.productFeatureId, // REFACTOR: Added productFeatureId to new order item
                subTotal: +productPrice.price.toFixed(2) * data.quantity * +productPrice.piecesIncluded,
              collectTax: addTax,
            };
            if (orderItem) {
                newOrderItem.orderId = orderItem.orderId;
            } else {
                newOrderItem.orderId = "ORDER-DUMMY";
            }
        }
        console.log("DEBUG: New Order Item:", JSON.stringify(newOrderItem, null, 2));
        return newOrderItem;
    }

    return {
        productPromotionsWithEmpty,
        handleSubmitData,
        productPromotions,
        processOrderItemData,
        processOrderItemError,
        processOrderItemLoading,
        processOrderItemFetching,
    };
}