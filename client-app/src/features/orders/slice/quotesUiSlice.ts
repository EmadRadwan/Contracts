import {createSlice, PayloadAction} from "@reduxjs/toolkit";


interface QuotesState {
    quoteFormEditMode: number;
}

export const quotesInitialState: QuotesState = {
    quoteFormEditMode: 0,
};


export const quotesSlice = createSlice({
    name: "quotesUi",
    initialState: quotesInitialState,
    reducers: {
        setQuoteFormEditMode(state, action: PayloadAction<number>) {
            state.quoteFormEditMode = action.payload;
        },
    },
});

export const {
    setQuoteFormEditMode,
} = quotesSlice.actions;

