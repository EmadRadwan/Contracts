import {createEntityAdapter, createSlice, EntityState, PayloadAction} from "@reduxjs/toolkit";
import {RootState} from "../../../app/store/configureStore";
import {QuoteAdjustment} from "../../../app/models/order/quoteAdjustment";

const quoteAdjustmentsAdapter = createEntityAdapter<QuoteAdjustment>({
    selectId: (quoteAdjustment) => quoteAdjustment.quoteAdjustmentId,
});

interface QuoteAdjustmentsState {
    quoteAdjustments: EntityState<QuoteAdjustment>;
}

export const quoteAdjustmentsInitialState: QuoteAdjustmentsState = {
    quoteAdjustments: quoteAdjustmentsAdapter.getInitialState(),
};

export const quoteAdjustmentsSlice = createSlice({
    name: "quoteAdjustmentsUi",
    initialState: quoteAdjustmentsInitialState,
    reducers: {
        setUiQuoteAdjustments: (state, action: PayloadAction<QuoteAdjustment[]>) => {
            quoteAdjustmentsAdapter.upsertMany(state.quoteAdjustments, action.payload);
        },
        setUiQuoteAdjustmentsFromApi: (state, action: PayloadAction<QuoteAdjustment[]>) => {
            quoteAdjustmentsAdapter.setAll(state.quoteAdjustments, action.payload);
        },
        resetUiQuoteAdjustments: (state) => {
            quoteAdjustmentsAdapter.removeAll(state.quoteAdjustments);
        },
        deletePromoQuoteAdjustments: (state, action) => {
            if (action.payload.length > 0) {
                quoteAdjustmentsAdapter.upsertMany(
                    state.quoteAdjustments,
                    action.payload,
                );
            }
        },
        deleteTaxAdjustments: (state, action) => {
            if (action.payload.length > 0) {
                quoteAdjustmentsAdapter.upsertMany(
                    state.quoteAdjustments,
                    action.payload,
                );
            }
        },
    },
});

export const {
    setUiQuoteAdjustments,
    resetUiQuoteAdjustments,
    deletePromoQuoteAdjustments,
    deleteTaxAdjustments,
    setUiQuoteAdjustmentsFromApi
} = quoteAdjustmentsSlice.actions;

export const quoteAdjustmentsSelectors = quoteAdjustmentsAdapter.getSelectors(
    (state: RootState) => state.quoteAdjustmentsUi.quoteAdjustments
);

export const {selectAll: quoteAdjustmentsEntities} = quoteAdjustmentsSelectors;
