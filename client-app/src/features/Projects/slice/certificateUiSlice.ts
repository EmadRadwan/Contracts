import { createSlice, PayloadAction } from "@reduxjs/toolkit";

interface CertificateUiState {
  currentCertificateType: string;
  certificateFormEditMode: number;
  selectedCertificate?: any;
}

export const certificateUiInitialState: CertificateUiState = {
  currentCertificateType: "",
  certificateFormEditMode: 0,
  selectedCertificate: undefined,
};

export const certificateUiSlice = createSlice({
  name: "certificateUi",
  initialState: certificateUiInitialState,
  reducers: {
    setCurrentCertificateType: (state, action: PayloadAction<string>) => {
      state.currentCertificateType = action.payload;
    },
    
    setCertificateFormEditMode: (state, action: PayloadAction<number>) => {
      state.certificateFormEditMode = action.payload;
    },
    setSelectedCertificate: (state, action: PayloadAction<any>) => {
      state.selectedCertificate = action.payload;
    },
    resetCertificateUi: (state) => {
      state.currentCertificateType = "";
      state.certificateFormEditMode = 0;
      state.selectedCertificate = undefined;
    },
  },
});

export const {
  setCurrentCertificateType,
  setCertificateFormEditMode,
  setSelectedCertificate,
  resetCertificateUi,
} = certificateUiSlice.actions;