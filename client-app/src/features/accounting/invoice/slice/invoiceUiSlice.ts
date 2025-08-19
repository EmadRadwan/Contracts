import {createSlice, PayloadAction} from "@reduxjs/toolkit"

interface InvoiceState {
    invoiceFormEditMode: number;
    selectedInvoiceId: string | undefined;

}


export const initialState: InvoiceState = {
    invoiceFormEditMode: 0,
    selectedInvoiceId: undefined,
};


export const invoiceSlice = createSlice({
    name: 'invoiceUi',
    initialState: initialState,
    reducers: {
        setInvoiceFormEditMode(state, action: PayloadAction<number>) {
            state.invoiceFormEditMode = action.payload;
        }
    },
});


export const {
    setInvoiceFormEditMode
} = invoiceSlice.actions;