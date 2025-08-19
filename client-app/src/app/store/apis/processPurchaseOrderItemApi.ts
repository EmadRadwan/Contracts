import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../configureStore";
import { OrderItem } from "../../models/order/orderItem";
import { OrderAdjustment } from "../../models/order/orderAdjustment";
import { setProcessedOrderItems } from "../../../features/orders/slice/orderItemsUiSlice";
import { setUiOrderAdjustments } from "../../../features/orders/slice/orderAdjustmentsUiSlice";
import {getDiscountAdjustments} from "../../../features/orders/utility functions/productPromotions";
import {CalculateItem} from "../../../features/orders/utility functions/calculateAdjustmentsForOrder";
import {getExistingTaxAdjustments} from "../../../features/orders/utility functions/orderTax";

interface ProcessPurchaseOrderItemResult {
    status: "Success" | "Failed";
}

const processPurchaseOrderItemApi = createApi({
    reducerPath: "processPurchaseOrderItem",
    baseQuery: fetchBaseQuery({
        baseUrl: import.meta.env.VITE_API_URL,
        prepareHeaders: (headers) => {
            const token = store.getState().account.user?.token;
            if (token) {
                headers.set("authorization", `Bearer ${token}`);
            }
            return headers;
        },
    }),
    endpoints: (build) => ({
        processPurchaseOrderItem: build.mutation<
            ProcessPurchaseOrderItemResult,
            OrderItem | undefined
            >({
            async queryFn(orderItem, _queryApi, _extraOptions, fetchWithBQ) {
                let tempOrderItems: OrderItem[] = [];
                let tempOrderItemAdjustments: OrderAdjustment[] = [];
                let existingTaxAdjustments: OrderAdjustment[] = [];

                let processOrderItemResult: ProcessPurchaseOrderItemResult = {
                    status: "Success",
                };
                let customError: string | undefined = undefined;

                // Get all available order items from state
                const allAvailableOrderItems: OrderItem[] = Object.values(
                    _queryApi.getState().orderItemsUi.orderItems.entities || {}
                );
                const allAvailableAdjustments: OrderAdjustment[] = Object.values(
                    _queryApi.getState().orderAdjustmentsUi.orderAdjustments.entities
                );
                console.log("All Available Order Items:", allAvailableOrderItems);

                if (orderItem) {
                    tempOrderItems.push(orderItem);
                } else {
                    customError = "No order item provided";
                    processOrderItemResult.status = "Failed";
                    return { data: processOrderItemResult, error: customError };
                }

                // Add discount adjustments from state
                if (allAvailableAdjustments.length > 0) {
                    const discountAdjustments = getDiscountAdjustments(allAvailableAdjustments, orderItem.orderItemSeqId);
                    if (discountAdjustments.length > 0) {
                        tempOrderItemAdjustments.push(...discountAdjustments);
                    }
                }

                // Calculate adjusted order items
                const adjustedOrderItems = CalculateItem(tempOrderItems, tempOrderItemAdjustments);

                // Include existing tax adjustments
                if (allAvailableAdjustments.length > 0) {
                    existingTaxAdjustments = getExistingTaxAdjustments(tempOrderItems, allAvailableAdjustments);
                }

                // Add existing adjustments and items
                if (existingTaxAdjustments.length > 0) {
                    tempOrderItemAdjustments.push(...existingTaxAdjustments);
                }
                
                // Update state
                _queryApi.dispatch(setProcessedOrderItems(adjustedOrderItems));
                _queryApi.dispatch(setUiOrderAdjustments(tempOrderItemAdjustments));

                console.log("Final Order Items:", tempOrderItems);
                console.log("Final Order Adjustments:", tempOrderItemAdjustments);

                return { data: processOrderItemResult, error: customError };
            },
        }),
    }),
});

export const { useProcessPurchaseOrderItemMutation } = processPurchaseOrderItemApi;
export { processPurchaseOrderItemApi };