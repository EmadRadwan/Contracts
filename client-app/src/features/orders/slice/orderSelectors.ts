// orderSelectors.ts

import {createSelector} from "@reduxjs/toolkit";
import {orderItemsEntities} from "./orderItemsUiSlice";
import {OrderItem} from "../../../app/models/order/orderItem";
import {orderAdjustmentsEntities} from "./orderAdjustmentsUiSlice";
import {orderPaymentsEntities} from "./orderPaymentsUiSlice";
import { RootState } from "../../../app/store/configureStore";
import { orderTermsEntities } from "./orderTermsUiSlice";


// returns non deleted orderItems
export const nonDeletedOrderItemsSelector = createSelector(orderItemsEntities, (orderItems) => {

    return Object.values(orderItems)
        .filter((orderItem) => {
            if (!orderItem!.isProductDeleted) return orderItem;
        });
});

export const allItemsAreDeletedOrNone = createSelector(orderItemsEntities, (orderItems) => {

    const allItemsAreDeleted = orderItems.every((item: any) => item.isProductDeleted === true);
    return !orderItems || orderItems.length === 0 || allItemsAreDeleted;
});


export const orderItemAdjustments = createSelector(
    orderAdjustmentsEntities,
    (state: { orderItemsUi: { selectedOrderItem: any; }; }) => state.orderItemsUi.selectedOrderItem,
    (orderAdjustmentsEntities, selectedOrderItem) => {

        return (
            selectedOrderItem &&
            Object.values(orderAdjustmentsEntities).filter((orderAdjustment: any) => {
                if (
                    orderAdjustment!.orderId === selectedOrderItem.orderId &&
                    !orderAdjustment!.isAdjustmentDeleted &&
                    orderAdjustment.orderItemSeqId === selectedOrderItem.orderItemSeqId
                )
                    return orderAdjustment;
            })
        );
    },
);

export const selectAdjustedOrderItems = createSelector(
    orderItemsEntities,
    (orderItems) => {
        return Object.values(orderItems).filter((orderItem: any) => {
            if (!orderItem!.isProductDeleted) return orderItem;
        });
    },
);

export const selectAdjustedOrderTerms = createSelector(
    orderTermsEntities,
    (orderTerms) => {
        return Object.values(orderTerms)
    },
);



export const selectAdjustedOrderItemsWithMarkedForDeletionItems =
    createSelector(
        orderItemsEntities,
        (orderItems) => {

            return Object.values(orderItems);
        },
    );


export const orderSubTotal = createSelector(
    selectAdjustedOrderItems,
    (filteredOrderItems) => {
        if (!filteredOrderItems) return 0; // Return 0 for falsy values

        const subtotal = filteredOrderItems.reduce((sum: number, record: any) => {
            let subTotalValue;
            if (record.isPromo!  && record.isPromo === "Y") {
                subTotalValue = record.subTotal + record.discountAndPromotionAdjustments
            } else {
                subTotalValue = typeof record.subTotal === 'number' ? record.subTotal : 0;
            }
            return sum + subTotalValue;
        }, 0);

        return Math.round(subtotal * 100) / 100;
    }
);



export const orderItemSubTotal = createSelector(
    (state: RootState) => orderItemsEntities(state),
    (state: RootState) => state.orderItemsUi.selectedOrderItem,
    (orderItemsEntities, selectedOrderItem) => {

        const filteredOrderItems = orderItemsEntities?.filter(
            (item: OrderItem) =>
                !item?.isProductDeleted &&
                item?.orderItemSeqId === selectedOrderItem?.orderItemSeqId
        ) || [];

        return filteredOrderItems.reduce(
            (sum, record) => sum + (record?.subTotal || 0),
            0
        );
    }
);

export const orderLevelAdjustmentsTotal = createSelector(
    orderAdjustmentsEntities,
    (orderAdjustmentsEntities) => {

        const adjustments = orderAdjustmentsEntities
            .filter(
                (item) => item!.orderItemSeqId === "_NA_" && !item!.isAdjustmentDeleted,
            );

        const total = adjustments.reduce((sum: number, record: any) => {
            // Ensure that record.amount is a number before adding it to the sum
            const amountValue = typeof record.amount === 'number' ? record.amount : 0;
            return sum + amountValue;
        }, 0);

        // Round the total to two decimal places
        return Math.round(total * 100) / 100;
    }
);


// create a selector to calculate the order payments total
export const orderPaymentsTotal = createSelector(orderPaymentsEntities, (orderPaymentsEntities) => {
    const total = orderPaymentsEntities.reduce(
        (sum: any, record: any) => {
            return sum + record.amount;
        },
        0,
    );

    return total;
});

export const orderLevelTaxTotal = createSelector(
    orderAdjustmentsEntities,
    (orderAdjustmentsEntities) => {
        //console.log('orderAdjustmentsEntities:', orderAdjustmentsEntities);

        const filteredAdjustments = orderAdjustmentsEntities.filter((adjustment) =>
            !adjustment?.isAdjustmentDeleted &&
            adjustment?.orderAdjustmentTypeId === "VAT_TAX"
        );
        //console.log('Filtered VAT_TAX adjustments:', filteredAdjustments);

        const totalTaxAmount = filteredAdjustments.reduce(
            (sum, adjustment) => {
                const amount = adjustment?.amount ?? 0;
                //console.log(`Adding adjustment amount for ${adjustment.orderAdjustmentId}:`, amount);
                return sum + amount;
            },
            0
        );
        //console.log('Total tax amount before rounding:', totalTaxAmount);

        const roundedTotal = Math.round(totalTaxAmount * 100) / 100;
        //console.log('Final rounded total VAT amount:', roundedTotal);

        return roundedTotal;
    }
);


export const  orderAdjustmentsSelector = createSelector(
    orderAdjustmentsEntities,
    (orderAdjustmentsEntities) => {


        return orderAdjustmentsEntities.filter(
            (orderAdjustment) => {
                if (!orderAdjustment!.isAdjustmentDeleted) return orderAdjustment;
            },
        );
    },
);

export const orderLevelAdjustments = createSelector(
    orderAdjustmentsEntities,
    (orderAdjustmentsEntities) => {

        return orderAdjustmentsEntities.filter(
            (orderAdjustment) => {
                if (
                    !orderAdjustment!.isAdjustmentDeleted &&
                    orderAdjustment!.orderItemSeqId === "_NA_"
                )
                    return orderAdjustment;
            },
        );
    },
);

// Types of required order selectors:

// 1. Selectors that return records
// a. orderLevelAdjustments --> nonn deleted + orderItemSeqId === "_NA_" + orderAdjustmentTypeId === "PROMOTION_ADJUSTMENT" || "DISCOUNT_ADJUSTMENT"
// b. orderAdjustmentsSelector --> non deleted 
// c. orderItemAdjustments --> non deleted + orderItemSeqId === orderItem.orderItemSeqId + orderAdjustmentTypeId === "PROMOTION_ADJUSTMENT" || "DISCOUNT_ADJUSTMENT"
// d. orderAdjustmentsSelector --> non deleted
// e. orderItemsSelector --> non deleted


// 2. Selectors that return values
// a. orderSubTotal --> sum of orderItemsSelector.subTotal
// b. orderItemSubTotal --> sum of orderLevelAdjustments
// c. orderLevelAdjustmentsTotal --> sum of all order adjustment non deleted withoutt filter
    
