import {createSlice, PayloadAction} from "@reduxjs/toolkit";


interface PaymentsState {
    paymentFormEditMode: number;
    paymentType: number;
}

export const paymentsInitialState: PaymentsState = {
    paymentFormEditMode: 0,
    paymentType: 0,
};


export const paymentsSlice = createSlice({
    name: "paymentsUi",
    initialState: paymentsInitialState,
    reducers: {
        setPaymentFormEditMode(state, action: PayloadAction<number>) {
            state.paymentFormEditMode = action.payload;
        },
        setPaymentType(state, action: PayloadAction<number>) {
            state.paymentType = action.payload;
        },
    },
});

export const {
    setPaymentFormEditMode, setPaymentType
} = paymentsSlice.actions;

