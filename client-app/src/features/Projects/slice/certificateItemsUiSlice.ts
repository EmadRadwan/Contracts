import { createEntityAdapter, createSlice, EntityState, PayloadAction } from "@reduxjs/toolkit";
import { RootState } from "../../../app/store/configureStore";
import { CertificateItem } from "../../../app/models/project/certificateItem";

const certificateItemsAdapter = createEntityAdapter<CertificateItem>({
  selectId: (certificateItem) => certificateItem.workEffortId,
});

interface CertificateItemsState {
  certificateItems: EntityState<CertificateItem>;
  selectedCertificateItem: CertificateItem | undefined;
}

export const certificateItemsInitialState: CertificateItemsState = {
  certificateItems: certificateItemsAdapter.getInitialState(),
  selectedCertificateItem: undefined,
};

export const certificateItemsSlice = createSlice({
  name: "certificateItemsUi",
  initialState: certificateItemsInitialState,
  reducers: {
    setUiCertificateItems: (state, action: PayloadAction<CertificateItem[]>) => {
      certificateItemsAdapter.upsertMany(state.certificateItems, action.payload);
    },
    setProcessedCertificateItems: (state, action: PayloadAction<CertificateItem[]>) => {
      action.payload.forEach((item) => {
        if (item.isDeleted) {
          certificateItemsAdapter.removeOne(state.certificateItems, item.workEffortId);
        } else {
          certificateItemsAdapter.upsertOne(state.certificateItems, item);
        }
      });
    },
    setUiCertificateItemsFromApi: (state, action: PayloadAction<CertificateItem[]>) => {
      certificateItemsAdapter.setAll(state.certificateItems, action.payload);
    },
    resetUiCertificateItems: (state) => {
      certificateItemsAdapter.removeAll(state.certificateItems);
      state.selectedCertificateItem = undefined;
    },
    setSelectedCertificateItem: (state, action: PayloadAction<CertificateItem | undefined>) => {
      state.selectedCertificateItem = action.payload;
    },
  },
});

export const {
  setUiCertificateItems,
  setProcessedCertificateItems,
  setUiCertificateItemsFromApi,
  resetUiCertificateItems,
  setSelectedCertificateItem,
} = certificateItemsSlice.actions;

export const certificateItemsSelectors = certificateItemsAdapter.getSelectors(
  (state: RootState) => state.certificateItemsUi.certificateItems
);

export const { selectAll: certificateItemsEntities } = certificateItemsSelectors;

export default certificateItemsSlice.reducer;