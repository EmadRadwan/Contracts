import {createEntityAdapter, createSlice, EntityState, PayloadAction} from "@reduxjs/toolkit";
import {RootState} from "../../../app/store/configureStore";
import {QuoteItem} from "../../../app/models/order/quoteItem";

const quoteItemsAdapter = createEntityAdapter<QuoteItem>({
    selectId: (quoteItem) => quoteItem.quoteId!.concat(quoteItem.quoteItemSeqId),
});

interface QuoteItemsState {
    quoteItems: EntityState<QuoteItem>;
    selectedQuoteItem: QuoteItem | undefined;
    relatedRecords: QuoteItem[] | undefined
    isSelectedServicePriceZero: boolean;
    isNewServiceRateAndSpecificationAdded: boolean;

}

export const quoteItemsInitialState: QuoteItemsState = {
    quoteItems: quoteItemsAdapter.getInitialState(),
    selectedQuoteItem: undefined,
    relatedRecords: undefined,
    isSelectedServicePriceZero: false,
    isNewServiceRateAndSpecificationAdded: false,
};

export const quoteItemsSlice = createSlice({
    name: "quoteItemsUi",
    initialState: quoteItemsInitialState,
    reducers: {
        setUiQuoteItems: (state, action: PayloadAction<QuoteItem[]>) => {
            quoteItemsAdapter.upsertMany(state.quoteItems, action.payload);
        },
        setUiQuoteItemsFromApi: (state, action: PayloadAction<QuoteItem[]>) => {
            quoteItemsAdapter.setAll(state.quoteItems, action.payload);
        },
        resetUiQuoteItems: (state) => {
            quoteItemsAdapter.removeAll(state.quoteItems);
            state.selectedQuoteItem = undefined;
            state.relatedRecords = undefined;
        },
        deletePromoQuoteItem: (state, action) => {
            if (action.payload.length > 0) {
                quoteItemsAdapter.upsertMany(state.quoteItems, action.payload);
            }
        },
        setRelatedRecords: (state, action) => {
            state.relatedRecords = action.payload;
        },
        setSelectedQuoteItem(state, action: PayloadAction<QuoteItem | undefined>) {
            state.selectedQuoteItem = action.payload
        },
        setProcessedQuoteItems: (state, action: PayloadAction<QuoteItem[]>) => {
            // loop thru payload, if isProductDeleted, then remove from state, else upsert
            action.payload.forEach((item) => {
                item.isProductDeleted ? quoteItemsAdapter.removeOne(state.quoteItems, item) :
                    quoteItemsAdapter.upsertOne(state.quoteItems, item);
            })
        },
        setIsSelectedPriceZero(state, action: PayloadAction<boolean>) {
            state.isSelectedServicePriceZero = action.payload;
        },
        setIsNewServiceRateAndSpecificationAdded(
            state,
            action: PayloadAction<boolean>,
        ) {
            state.isNewServiceRateAndSpecificationAdded = action.payload;
        },
    },
});

export const {
    setUiQuoteItems,
    resetUiQuoteItems,
    deletePromoQuoteItem,
    setRelatedRecords,
    setSelectedQuoteItem,
    setUiQuoteItemsFromApi,
    setProcessedQuoteItems, setIsSelectedPriceZero, setIsNewServiceRateAndSpecificationAdded
} = quoteItemsSlice.actions;

export const quoteItemsSelectors = quoteItemsAdapter.getSelectors(
    (state: RootState) => state.quoteItemsUi.quoteItems
);

export const {selectAll: quoteItemsEntities} = quoteItemsSelectors;

