// quoteSelectors.ts

import {createSelector} from "@reduxjs/toolkit";
import {quoteItemsEntities} from "./quoteItemsUiSlice";
import {quoteAdjustmentsEntities} from "./quoteAdjustmentsUiSlice";
import {QuoteItem} from "../../../app/models/order/quoteItem";


// returns non deleted quoteItems
export const nonDeletedQuoteItemsSelector = createSelector(quoteItemsEntities, (quoteItems) => {
    console.count("nonDeletedQuoteItemsSelector");

    return Object.values(quoteItems)
        .filter((quoteItem) => {
            if (!quoteItem!.isProductDeleted) return quoteItem;
        });
});

export const allItemsAreDeletedOrNone = createSelector(quoteItemsEntities, (quoteItems) => {

    const allItemsAreDeleted = quoteItems.every((item: any) => item.isProductDeleted === true);
    return !quoteItems || quoteItems.length === 0 || allItemsAreDeleted;
});


export const quoteItemAdjustments = createSelector(
    quoteAdjustmentsEntities,
    (state: { quoteItemsUi: { selectedQuoteItem: any; }; }) => state.quoteItemsUi.selectedQuoteItem,
    (quoteAdjustmentsEntities, selectedQuoteItem) => {
        console.count("quoteItemAdjustments");

        return (
            selectedQuoteItem &&
            Object.values(quoteAdjustmentsEntities).filter((quoteAdjustment: any) => {
                if (
                    quoteAdjustment!.quoteId === selectedQuoteItem.quoteId &&
                    !quoteAdjustment!.isAdjustmentDeleted &&
                    quoteAdjustment.quoteItemSeqId === selectedQuoteItem.quoteItemSeqId
                )
                    return quoteAdjustment;
            })
        );
    },
);

export const selectAdjustedQuoteItems = createSelector(
    quoteItemsEntities,
    (quoteItems) => {
        console.count("selectAdjustedQuoteItems");
        return Object.values(quoteItems).filter((quoteItem: any) => {
            if (!quoteItem!.isProductDeleted) return quoteItem;
        });
    },
);

export const selectAdjustedQuoteItemsWithMarkedForDeletionItems =
    createSelector(
        quoteItemsEntities,
        (quoteItems) => {
            console.count("selectAdjustedQuoteItemsWithMarkedForDeletionItems");

            return Object.values(quoteItems);
        },
    );


export const quoteSubTotal = createSelector(
    selectAdjustedQuoteItems,
    (filteredQuoteItems) => {
        console.count("quoteSubTotal");

        if (!filteredQuoteItems) return 0; // Return 0 for falsy values

        const subtotal = filteredQuoteItems.reduce((sum: number, record: any) => {
            // Ensure that record.subTotal is a number before adding it to the sum
            const subTotalValue = typeof record.subTotal === 'number' ? record.subTotal : 0;
            return sum + subTotalValue;
        }, 0);

        console.log("quoteSubTotal", Math.round(subtotal * 100) / 100);


        // Round the subtotal to two decimal places
        return Math.round(subtotal * 100) / 100;
    }
);


// (state: ...) indicates a function parameter named state.
// { quoteItemsUi: { quoteItemsEntities: any; }; } 
// is an inline type declaration for the state parameter.
// It states that state is expected to be an object with a property 
// named quoteItemsUi.quoteItemsUi is expected to be an object with a 
// property named quoteItemsEntities of type any.
export const quoteItemSubTotal = createSelector(
    (state: { quoteItemsUi: { quoteItemsEntities: any; selectedQuoteItem: any }; }) => state.quoteItemsUi.quoteItemsEntities,
    (state: { quoteItemsUi: { selectedQuoteItem: any; }; }) => state.quoteItemsUi.selectedQuoteItem,
    (quoteItemsEntities, selectedQuoteItem) => {
        console.count("quoteItemSubTotal");

        const filteredQuoteItems = quoteItemsEntities
            ? quoteItemsEntities.filter(
                (item: QuoteItem) =>
                    !item?.isProductDeleted &&
                    item?.quoteItemSeqId === selectedQuoteItem?.quoteItemSeqId
            )
            : [];

        return (
            filteredQuoteItems &&
            filteredQuoteItems.reduce((sum: any, record: { subTotal: any; }) => sum + (record?.subTotal || 0), 0)
        );
    }
);

export const quoteLevelAdjustmentsTotal = createSelector(
    quoteAdjustmentsEntities,
    (quoteAdjustmentsEntities) => {

        const adjustments = quoteAdjustmentsEntities
            .filter(
                (item) => item!.quoteItemSeqId === "_NA_" && !item!.isAdjustmentDeleted,
            );

        const total = adjustments.reduce((sum: number, record: any) => {
            // Ensure that record.amount is a number before adding it to the sum
            const amountValue = typeof record.amount === 'number' ? record.amount : 0;
            return sum + amountValue;
        }, 0);

        console.log("quoteLevelAdjustmentsTotal", Math.round(total * 100) / 100);
        // Round the total to two decimal places
        return Math.round(total * 100) / 100;
    }
);


export const quoteLevelTaxTotal = createSelector(quoteAdjustmentsEntities, (quoteAdjustmentsEntities) => {

    // Calculating the total tax amount with filtering included
    console.count("quoteLevelTaxTotal");

    const totalTaxAmount = quoteAdjustmentsEntities
        .map((adjustment) => {
            // Filtering tax adjustments based on conditions
            if (
                !adjustment?.isAdjustmentDeleted &&
                (adjustment?.quoteAdjustmentTypeId === "SALES_TAX" || adjustment?.quoteAdjustmentTypeId === "VAT_TAX")
            ) {
                // Include this adjustment in the calculation
                return adjustment?.amount ?? 0;
            }
            // Exclude adjustments that don't meet the conditions
            return 0;
        })
        .reduce((sum, amount) => sum + amount, 0);

    console.log("quoteLevelTaxTotal", totalTaxAmount);

    return totalTaxAmount;
});


export const quoteAdjustmentsSelector = createSelector(
    quoteAdjustmentsEntities,
    (quoteAdjustmentsEntities) => {
        console.count("quoteAdjustmentsSelector");


        return quoteAdjustmentsEntities.filter(
            (quoteAdjustment) => {
                if (!quoteAdjustment!.isAdjustmentDeleted) return quoteAdjustment;
            },
        );
    },
);

export const quoteLevelAdjustments = createSelector(
    quoteAdjustmentsEntities,
    (quoteAdjustmentsEntities) => {
        console.count("quoteLevelAdjustments");

        return quoteAdjustmentsEntities.filter(
            (quoteAdjustment) => {
                if (
                    !quoteAdjustment!.isAdjustmentDeleted &&
                    quoteAdjustment!.quoteItemSeqId === "_NA_" &&
                    (quoteAdjustment!.quoteAdjustmentTypeId === "PROMOTION_ADJUSTMENT" ||
                        quoteAdjustment!.quoteAdjustmentTypeId === "DISCOUNT_ADJUSTMENT")
                )
                    return quoteAdjustment;
            },
        );
    },
);

// Types of required quote selectors:

// 1. Selectors that return records
// a. quoteLevelAdjustments --> nonn deleted + quoteItemSeqId === "_NA_" + quoteAdjustmentTypeId === "PROMOTION_ADJUSTMENT" || "DISCOUNT_ADJUSTMENT"
// b. quoteAdjustmentsSelector --> non deleted 
// c. quoteItemAdjustments --> non deleted + quoteItemSeqId === quoteItem.quoteItemSeqId + quoteAdjustmentTypeId === "PROMOTION_ADJUSTMENT" || "DISCOUNT_ADJUSTMENT"
// d. quoteAdjustmentsSelector --> non deleted
// e. quoteItemsSelector --> non deleted


// 2. Selectors that return values
// a. quoteSubTotal --> sum of quoteItemsSelector.subTotal
// b. quoteItemSubTotal --> sum of quoteLevelAdjustments
// c. quoteLevelAdjustmentsTotal --> sum of all quote adjustment non deleted withoutt filter
    
