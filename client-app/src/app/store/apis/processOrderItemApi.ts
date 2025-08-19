import { createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import { store } from "../configureStore";
import { ProcessOrderItemResult } from "../../models/order/ProcessOrderItemResult";
import { OrderItem } from "../../models/order/orderItem";
import { OrderAdjustment } from "../../models/order/orderAdjustment";
import { OrderPromoResult } from "../../models/order/OrderPromoResult";
import {
    generateNewPromoOrderItem,
    generateNewPromoOrderItemAdjustment,
    getDiscountAdjustments,
    getExistingPromoOrderAdjustments,
    getExistingPromoOrderItems
} from "../../../features/orders/utility functions/productPromotions";
import { setProcessedOrderItems } from "../../../features/orders/slice/orderItemsUiSlice";
import { setUiOrderAdjustments } from "../../../features/orders/slice/orderAdjustmentsUiSlice";
import { CalculateItem } from "../../../features/orders/utility functions/calculateAdjustmentsForOrder";
import { getExistingTaxAdjustments } from "../../../features/orders/utility functions/orderTax";

interface ApplyOrderItemPromoResult {
    data: OrderPromoResult;
    error: any;
}


// External function to apply promotions
async function applyPromotions(orderItem: OrderItem, fetchWithBQ: any): Promise<ApplyOrderItemPromoResult> {
    return await fetchWithBQ({
        url: '/products/applyOrderItemPromo',
        method: 'POST',
        body: { ...orderItem },
    });
}

// External function to handle promo results
function handlePromoResults(
    promoResult: ApplyOrderItemPromoResult,
    tempOrderItems: OrderItem[],
    tempOrderItemAdjustments: OrderAdjustment[]
) {
    if (promoResult.data && promoResult.data.orderItems) {
        let orderItemSeqId = parseInt(tempOrderItems[tempOrderItems.length - 1].orderItemSeqId) + 1;
        promoResult.data.orderItems.forEach((oi: any) => {
            tempOrderItems.push(generateNewPromoOrderItem(oi, orderItemSeqId, tempOrderItems[tempOrderItems.length - 1]));
            promoResult.data.orderItemAdjustments?.forEach((oia: any) => {
                tempOrderItemAdjustments.push(generateNewPromoOrderItemAdjustment(oia, orderItemSeqId));
            });
            orderItemSeqId++;
        });
    }
}

const processOrderItemApi = createApi({
    reducerPath: "processOrderItem",
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
        processOrderItem: build.mutation<ProcessOrderItemResult, OrderItem>({
            async queryFn(orderItem, _queryApi, _extraOptions, fetchWithBQ) {
                let tempOrderItems: OrderItem[] = [];
                let existingPromoOrderItems: OrderItem[] = [];
                let tempOrderItemAdjustments: OrderAdjustment[] = [];
                let existingTaxAdjustments: OrderAdjustment[] = [];
                let existingPromoOrderItemAdjustments: OrderAdjustment[] = [];
                let promoResult: ApplyOrderItemPromoResult;
                let processOrderItemResult: ProcessOrderItemResult = { status: "Success" };
                let customError: string | undefined = undefined;

                const allAvailableAdjustments: OrderAdjustment[] = Object.values(
                    _queryApi.getState().orderAdjustmentsUi.orderAdjustments.entities
                );
                const allAvailableOrderItems: OrderItem[] = Object.values(
                    _queryApi.getState().orderItemsUi.orderItems.entities
                );

                if (orderItem) {
                    tempOrderItems.push(orderItem);
                } else {
                    customError = "No order item provided";
                    processOrderItemResult.status = "Failed";
                    return { data: processOrderItemResult, error: customError };
                }

                // Apply promotions if productPromoId is present
                if (orderItem.productPromoId?.trim()) {
                    promoResult = await applyPromotions(orderItem, fetchWithBQ);
                    if (promoResult.data.resultMessage !== "Success") {
                        customError = promoResult.data.resultMessage;
                        processOrderItemResult.status = "Failed";
                        return { data: processOrderItemResult, error: customError };
                    } else {
                        handlePromoResults(promoResult, tempOrderItems, tempOrderItemAdjustments);
                        try {
                            if (allAvailableOrderItems.length > 0) {
                                existingPromoOrderItems = getExistingPromoOrderItems(allAvailableOrderItems, orderItem);
                            }
                        } catch (error) {
                            console.error("Error getting existing promo order items:", error);
                        }
                        try {
                            if (allAvailableAdjustments.length > 0) {
                                existingPromoOrderItemAdjustments = getExistingPromoOrderAdjustments(
                                    allAvailableAdjustments,
                                    orderItem
                                );
                            }
                        } catch (error) {
                            console.error("Error getting existing promo adjustments:", error);
                        }
                    }
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
                if (existingPromoOrderItems.length > 0) {
                    adjustedOrderItems.push(...existingPromoOrderItems);
                }
                if (existingPromoOrderItemAdjustments.length > 0) {
                    tempOrderItemAdjustments.push(...existingPromoOrderItemAdjustments);
                }

                // Update state
                _queryApi.dispatch(setProcessedOrderItems(adjustedOrderItems));
                _queryApi.dispatch(setUiOrderAdjustments(tempOrderItemAdjustments));

                console.log("Final Adjusted Order Items:", adjustedOrderItems);
                console.log("Final Order Adjustments:", tempOrderItemAdjustments);

                return { data: processOrderItemResult, error: customError };
            },
        }),
    }),
});

export const { useProcessOrderItemMutation } = processOrderItemApi;
export { processOrderItemApi };