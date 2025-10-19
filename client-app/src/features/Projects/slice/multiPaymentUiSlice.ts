import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import {MultiPaymentCertificate} from "../../../app/models/project/MultiPaymentCertificate";

// REFACTOR: Update multiPaymentUiSlice to include formEditMode and selectedCertificate, mirroring certificateUiSlice structure.
interface MultiPaymentUiState {
    certificate: MultiPaymentCertificate | null;
    formEditMode: number; // 0: view, 1: create, 2: CREATED, 3: APPROVED, 4: COMPLETED
}

const initialState: MultiPaymentUiState = {
    certificate: null,
    formEditMode: 0,
};

const multiPaymentUiSlice = createSlice({
    name: "multiPaymentUi",
    initialState,
    reducers: {
        setUiMultiPaymentCertificate: (state, action: PayloadAction<MultiPaymentCertificate>) => {
            state.certificate = action.payload;
        },
        setSelectedMultiPaymentCertificate: (state, action: PayloadAction<MultiPaymentCertificate>) => {
            state.certificate = action.payload;
        },
        setMultiPaymentFormEditMode: (state, action: PayloadAction<number>) => {
            state.formEditMode = action.payload;
        },
        resetMultiPaymentUi: (state) => {
            state.certificate = null;
            state.formEditMode = 0;
        },
    },
});

export const {
    setUiMultiPaymentCertificate,
    setSelectedMultiPaymentCertificate,
    setMultiPaymentFormEditMode,
    resetMultiPaymentUi,
} = multiPaymentUiSlice.actions;

export const multiPaymentUiSelectors = {
    selectMultiPaymentUi: (state: any) => state.multiPaymentUi as MultiPaymentUiState,
};

export default multiPaymentUiSlice.reducer;