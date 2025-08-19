import {createSelector} from "@reduxjs/toolkit";
import {invoiceItemsEntities} from "./invoiceItemsUiSlice";


// returns non deleted invoiceItems
export const nonDeletedInvoiceItemsSelector = createSelector(invoiceItemsEntities, (invoiceItems) => {
    return Object.values(invoiceItems)
        .filter((invoiceItem) => {
            if (!invoiceItem!.isInvoiceItemDeleted) return invoiceItem;
        });
});

export const invoiceTotal = createSelector(
    invoiceItemsEntities,
    (invoiceItemsEntities) => {

        const items = invoiceItemsEntities
            .filter(
                (item) => !item!.isInvoiceItemDeleted,
            );

        const total = items.reduce((sum: number, record: any) => {
            const amountValue = typeof record.amount === 'number' ? record.amount * record.quantity : 0;
            return sum + amountValue;
        }, 0);

        return Math.round(total * 100) / 100;
    }
);
