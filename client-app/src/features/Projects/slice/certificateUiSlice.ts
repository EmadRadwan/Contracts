import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { RootState } from "../../../app/store/configureStore";

// Purpose: Support FormComboBox components by storing projectId, partyIdSupplier, and partyIdContractor as objects
// Context: Ensures form re-renders with correct values after submission
interface SelectedCertificate {
  workEffortId: string;
  certificateNumber: string;
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
  facilityId?: string ;
  facilityName?: string;

}
interface CertificateUiState {
  currentCertificateType: string;
  certificateFormEditMode: number;
  selectedCertificate: SelectedCertificate | { [key: string]: any };
  // Deliberately NOT nested inside selectedCertificate: this tracks the facility currently
  // picked in the open form (changes on every project selection while editing), whereas
  // selectedCertificate is treated as the pristine server-data baseline that
  // ProjectCertificateForm's initialFormValues/resetForm effect resets the form to. Nesting
  // this here previously meant every project pick replaced the selectedCertificate object
  // reference too, which re-triggered that reset effect and snapped the just-picked project
  // straight back to the old one. See certificate 110-0009.
  currentFacilityId?: string;
}

export const certificateUiInitialState: CertificateUiState = {
  currentCertificateType: "",
  certificateFormEditMode: 0,
  selectedCertificate: {
    workEffortId: "",
    certificateNumber: "",
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
    facilityId: undefined,
    facilityName: "",
  },
  currentFacilityId: undefined,
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
      state.selectedCertificate = {
        ...action.payload,
        estimatedStartDate: action.payload.estimatedStartDate instanceof Date
            ? action.payload.estimatedStartDate.toISOString()
            : action.payload.estimatedStartDate,
        estimatedCompletionDate: action.payload.estimatedCompletionDate instanceof Date
            ? action.payload.estimatedCompletionDate.toISOString()
            : action.payload.estimatedCompletionDate,
        facilityId: action.payload.facilityId || "",
        facilityName: action.payload.facilityName || "",
      };
    },
    resetCertificateUi: (state) => {
      state.currentCertificateType = "";
      state.certificateFormEditMode = 0;
      state.selectedCertificate = {
        workEffortId: "",
        certificateNumber: "",
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
        facilityId: undefined,
        facilityName: "",
      };
      state.currentFacilityId = undefined;
    },
    setCurrentFacilityId: (state, action: PayloadAction<string | undefined>) => {
      state.currentFacilityId = action.payload;
    },
  },
});

export const {
  setCurrentCertificateType,
  setCertificateFormEditMode,
  setSelectedCertificate,
  resetCertificateUi, setCurrentFacilityId
} = certificateUiSlice.actions;

export const certificateUiSelectors = {
  selectCertificateUi: (state: RootState) => state.certificateUi,
  selectCurrentCertificateType: (state: RootState) => state.certificateUi.currentCertificateType,
  selectCertificateFormEditMode: (state: RootState) => state.certificateUi.certificateFormEditMode,
  selectSelectedCertificate: (state: RootState) => state.certificateUi.selectedCertificate,
};

export default certificateUiSlice.reducer;