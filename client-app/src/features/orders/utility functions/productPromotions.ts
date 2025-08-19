import {OrderItem} from "../../../app/models/order/orderItem";
import {Order} from "../../../app/models/order/order";
import {OrderAdjustment} from "../../../app/models/order/orderAdjustment";


export function getDiscountAdjustments(orderAdjustments: any, orderItemSeqId: string) {
    return orderAdjustments.filter(
        (orderAdjustment: any) =>
            orderAdjustment.orderItemSeqId === orderItemSeqId &&
            orderAdjustment.orderAdjustmentTypeId === 'DISCOUNT_ADJUSTMENT'
    );
}

export function generateNewPromoOrderItem(
    oi: any,
    orderItemSeqId: number,
    newOrderItem: OrderItem
): Order {
    return {
        ...oi,
        orderItemSeqId: orderItemSeqId.toString().padStart(2, "0"),
        parentOrderItemSeqId: newOrderItem.orderItemSeqId,
    };
}

export function generateNewPromoOrderItemAdjustment(
    oia: any,
    orderItemSeqId: number
): OrderAdjustment {
    return {
        ...oia,
        orderItemSeqId: orderItemSeqId.toString().padStart(2, "0"),
    };
}

export const getExistingPromoOrderItems = (orderItems: any, newOrderItem: OrderItem) => {
    return orderItems
        .filter((oi: any) => oi.productPromoId === newOrderItem.productPromoId &&
            oi.orderId === newOrderItem.orderId &&
            oi.orderItemSeqId === newOrderItem.orderItemSeqId)
        .map((oi: any) => ({...oi, isProductDeleted: true}));
};

export const getExistingPromoOrderAdjustments = (orderAdjustments: any, newOrderItem: OrderItem) => {
    return orderAdjustments
        .filter(
            (uiOa: any) => uiOa.productPromoId === newOrderItem.productPromoId &&
                uiOa.orderItemSeqId === newOrderItem.orderItemSeqId &&
                uiOa.orderId === newOrderItem.orderId
        )
        .map((uiOa: any) => ({...uiOa, isAdjustmentDeleted: true}));
};

