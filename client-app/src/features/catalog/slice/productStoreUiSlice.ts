import {createSlice, PayloadAction} from "@reduxjs/toolkit";
import {ProductStore} from "../../../app/models/product/productStore";

interface ProductStoreUiState {
    selectedProductStore: ProductStore | undefined;


}


export const initialState: ProductStoreUiState = {
    selectedProductStore: undefined,
}

export const productStoreUiSlice = createSlice({
    name: 'productStoreUi',
    initialState: initialState,
    reducers: {
        resetUi: (state, action) => {
            state.selectedProductStore = undefined;
        },

        setSelectedProductStore: (state, action: PayloadAction<ProductStore | undefined>) => {
            state.selectedProductStore = action.payload;
        },
    },
})

export const {
    resetUi, setSelectedProductStore
} = productStoreUiSlice.actions;








