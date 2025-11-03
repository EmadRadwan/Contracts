import {createSlice, PayloadAction} from "@reduxjs/toolkit";


interface PaymentsState {
    formEditMode: number;
    paymentType: number;
}

export const paymentsInitialState: PaymentsState = {
    formEditMode: 0,
    paymentType: 0,
};


export const paymentsSlice = createSlice({
    name: "paymentsUi",
    initialState: paymentsInitialState,
    reducers: {
        setFormEditMode(state, action: PayloadAction<number>) {
            state.formEditMode = action.payload;
        },
        setPaymentType(state, action: PayloadAction<number>) {
            state.paymentType = action.payload;
        },
        resetForm: (state) => {
            state.formEditMode = 0;
        },
    },
});

export const {
    setFormEditMode, setPaymentType, resetForm
} = paymentsSlice.actions;

