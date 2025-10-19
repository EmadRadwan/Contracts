import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import {MultiPaymentItem} from "../../../app/models/project/MultiPaymentItem";

// REFACTOR: Create multiPaymentItemsUiSlice to manage items state, mirroring certificateItemsUiSlice for local storage.
interface MultiPaymentItemsUiState {
    items: MultiPaymentItem[];
}

const initialState: MultiPaymentItemsUiState = {
    items: [],
};

const multiPaymentItemsUiSlice = createSlice({
    name: "multiPaymentItemsUi",
    initialState,
    reducers: {
        addUiMultiPaymentItem: (state, action: PayloadAction<MultiPaymentItem>) => {
            state.items.push(action.payload);
        },
        updateUiMultiPaymentItem: (state, action: PayloadAction<MultiPaymentItem>) => {
            const index = state.items.findIndex((item) => item.itemId === action.payload.itemId);
            if (index !== -1) {
                state.items[index] = action.payload;
            }
        },
        resetUiMultiPaymentItems: (state) => {
            state.items = [];
        },
    },
});

export const { addUiMultiPaymentItem, updateUiMultiPaymentItem, resetUiMultiPaymentItems } = multiPaymentItemsUiSlice.actions;
export default multiPaymentItemsUiSlice.reducer;