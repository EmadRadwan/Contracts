import {createEntityAdapter, createSlice, EntityState, PayloadAction} from "@reduxjs/toolkit";
import {InvoiceItem} from "../../../../app/models/accounting/invoiceItem";
import {RootState} from "../../../../app/store/configureStore";


const invoiceItemsAdapter = createEntityAdapter<InvoiceItem>({
    selectId: (invoiceItem) => invoiceItem.invoiceId!.concat(invoiceItem.invoiceItemSeqId),
});

interface InvoiceItemsState {
    invoiceItems: EntityState<InvoiceItem>;
    selectedInvoiceItem: InvoiceItem | undefined;

}

export const invoiceItemsInitialState: InvoiceItemsState = {
    invoiceItems: invoiceItemsAdapter.getInitialState(),
    selectedInvoiceItem: undefined,
};

export const invoiceItemsSlice = createSlice({
    name: "invoiceItemsUi",
    initialState: invoiceItemsInitialState,
    reducers: {
        setUiInvoiceItems: (state, action: PayloadAction<InvoiceItem[]>) => {
            invoiceItemsAdapter.upsertMany(state.invoiceItems, action.payload);
        },
        setUiInvoiceItemsFromApi: (state, action: PayloadAction<InvoiceItem[]>) => {
            invoiceItemsAdapter.setAll(state.invoiceItems, action.payload);
        },
        resetUiInvoiceItems: (state) => {
            invoiceItemsAdapter.removeAll(state.invoiceItems);
            state.selectedInvoiceItem = undefined;
        },
    },
});

export const {
    setUiInvoiceItems,
    resetUiInvoiceItems,
    setUiInvoiceItemsFromApi
} = invoiceItemsSlice.actions;

export const invoiceItemsSelectors = invoiceItemsAdapter.getSelectors(
    (state: RootState) => state.invoiceItemsUi.invoiceItems
);

export const {selectAll: invoiceItemsEntities} = invoiceItemsSelectors;

