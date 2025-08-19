import {createSlice, PayloadAction} from "@reduxjs/toolkit";
import {Product} from "../../../app/models/product/product";

interface ProductUiState {
    selectedProduct: Product | undefined;


}


export const initialState: ProductUiState = {
    selectedProduct: undefined,
}

export const productUiSlice = createSlice({
    name: 'productUi',
    initialState: initialState,
    reducers: {
        resetUi: (state, action) => {
            state.selectedProduct = undefined;
        },

        setSelectedProduct: (state, action: PayloadAction<Product | undefined>) => {
            state.selectedProduct = action.payload;
        },
    },
})

export const {
    resetUi, setSelectedProduct
} = productUiSlice.actions;








