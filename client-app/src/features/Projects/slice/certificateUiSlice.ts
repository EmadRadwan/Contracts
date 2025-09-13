import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { RootState } from "../../../app/store/configureStore";

// Purpose: Support FormComboBox components by storing projectId, partyIdSupplier, and partyIdContractor as objects
// Context: Ensures form re-renders with correct values after submission
interface SelectedCertificate {
  workEffortId: string;
  projectNum: string;
  projectId: string;
  projectName: string;
  currentStatusId: string;
  partyIdSupplier?: { fromPartyId: string; partyName: string };
  partyIdContractor?: { fromPartyId: string; partyName: string };
  description: string;
  estimatedStartDate: string | null;
  estimatedCompletionDate: string | null;
  statusDescription: string;
  statusDescriptionArabic?: string; // Added for localization
  relatedOrderId: string;
}
interface CertificateUiState {
  currentCertificateType: string;
  certificateFormEditMode: number;
  selectedCertificate: SelectedCertificate | { [key: string]: any };
}

export const certificateUiInitialState: CertificateUiState = {
  currentCertificateType: "",
  certificateFormEditMode: 0,
  selectedCertificate: {
    workEffortId: "",
    projectNum: "",
    projectId: "",
    projectName: "",
    partyIdSupplier: undefined,
    partyIdContractor: undefined,
    description: "",
    estimatedStartDate: null,
    estimatedCompletionDate: null,
    statusDescription: "",
    statusDescriptionArabic: "",
    relatedOrderId: "",
  },
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
    setSelectedCertificate: (state, action: PayloadAction<SelectedCertificate>) => {
      console.log("Dispatching setSelectedCertificate with payload:", action.payload);
      state.selectedCertificate = {
        ...action.payload,
        estimatedStartDate: action.payload.estimatedStartDate instanceof Date
            ? action.payload.estimatedStartDate.toISOString()
            : action.payload.estimatedStartDate,
        estimatedCompletionDate: action.payload.estimatedCompletionDate instanceof Date
            ? action.payload.estimatedCompletionDate.toISOString()
            : action.payload.estimatedCompletionDate,
      };
    },
    resetCertificateUi: (state) => {
      state.currentCertificateType = "";
      state.certificateFormEditMode = 0;
      state.selectedCertificate = {
        workEffortId: "",
        projectNum: "",
        projectId: "",
        projectName: "",
        partyIdSupplier: undefined,
        partyIdContractor: undefined,
        description: "",
        estimatedStartDate: null,
        estimatedCompletionDate: null,
        statusDescription: "",
        statusDescriptionArabic: "",
        relatedOrderId: "",
      };
    },
    debugCertificateUiState: (state) => {
      console.log("Current certificateUi state:", state);
    },
  },
});

export const {
  setCurrentCertificateType,
  setCertificateFormEditMode,
  setSelectedCertificate,
  resetCertificateUi,
  debugCertificateUiState,
} = certificateUiSlice.actions;

export const certificateUiSelectors = {
  selectCertificateUi: (state: RootState) => state.certificateUi,
  selectCurrentCertificateType: (state: RootState) => state.certificateUi.currentCertificateType,
  selectCertificateFormEditMode: (state: RootState) => state.certificateUi.certificateFormEditMode,
  selectSelectedCertificate: (state: RootState) => state.certificateUi.selectedCertificate,
};

export default certificateUiSlice.reducer;