import {createSlice, PayloadAction} from "@reduxjs/toolkit";


interface OrdersState {
    orderFormEditMode: number;
    selectedOrderId: string | undefined
}

export const ordersInitialState: OrdersState = {
    orderFormEditMode: 0,
    selectedOrderId: undefined
};


export const ordersSlice = createSlice({
    name: "ordersUi",
    initialState: ordersInitialState,
    reducers: {
        setOrderFormEditMode(state, action: PayloadAction<number>) {
            state.orderFormEditMode = action.payload;
        },
        setSelectedOrderId(state, {payload}: PayloadAction<string>) {
            state.selectedOrderId = payload
        }
    },
});

export const {
    setOrderFormEditMode,
    setSelectedOrderId
} = ordersSlice.actions;

