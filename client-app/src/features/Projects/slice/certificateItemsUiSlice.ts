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
      state.certificateItems = action.payload.reduce((acc, item) => {
        acc[item.workEffortId] = item;
        return acc;
      }, {} as { [key: string]: CertificateItem });
    },
    updateCertificateItem: (
        state,
        action: PayloadAction<{ certificateItem: CertificateItem; editMode: number }>
    ) => {
      const { certificateItem, editMode } = action.payload;
      if (editMode === 1) {
        // Add new item
        state.certificateItems[certificateItem.workEffortId] = certificateItem;
      } else {
        // Update existing item
        if (state.certificateItems[certificateItem.workEffortId]) {
          state.certificateItems[certificateItem.workEffortId] = certificateItem;
        }
      }
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
  resetUiCertificateItems, updateCertificateItem,
  setSelectedCertificateItem,
} = certificateItemsSlice.actions;

export const certificateItemsSelectors = certificateItemsAdapter.getSelectors(
  (state: RootState) => state.certificateItemsUi.certificateItems
);

export const { selectAll: certificateItemsEntities } = certificateItemsSelectors;

export default certificateItemsSlice.reducer;