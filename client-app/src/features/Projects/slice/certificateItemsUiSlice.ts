import { createEntityAdapter, createSlice, EntityState, PayloadAction } from "@reduxjs/toolkit";
import { RootState } from "../../../app/store/configureStore";
import { CertificateItem } from "../../../app/models/project/certificateItem";

// REFACTOR: Update adapter to use workEffortId
// Purpose: Align with workEffort table where workEffortId is the primary key
// Context: Replaces itemId with workEffortId for entity state
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
    // REFACTOR: Update action to handle workEffortId
    // Purpose: Upsert items using workEffortId as key
    // Context: Matches previous setUiCertificateItems
    setUiCertificateItems: (state, action: PayloadAction<CertificateItem[]>) => {
      certificateItemsAdapter.upsertMany(state.certificateItems, action.payload);
    },
    // REFACTOR: Update action to use workEffortId for deletion
    // Purpose: Remove deleted items or upsert non-deleted ones
    // Context: Updates setProcessedCertificateItems to use workEffortId
    setProcessedCertificateItems: (state, action: PayloadAction<CertificateItem[]>) => {
      action.payload.forEach((item) => {
        if (item.isDeleted) {
          certificateItemsAdapter.removeOne(state.certificateItems, item.workEffortId);
        } else {
          certificateItemsAdapter.upsertOne(state.certificateItems, item);
        }
      });
    },
    // REFACTOR: Update action to set all items
    // Purpose: Replace all items in state using workEffortId
    // Context: Matches setUiCertificateItemsFromApi
    setUiCertificateItemsFromApi: (state, action: PayloadAction<CertificateItem[]>) => {
      certificateItemsAdapter.setAll(state.certificateItems, action.payload);
    },
    // REFACTOR: Reset state
    // Purpose: Clear all items and selected item
    // Context: Unchanged, no dependency on key
    resetUiCertificateItems: (state) => {
      certificateItemsAdapter.removeAll(state.certificateItems);
      state.selectedCertificateItem = undefined;
    },
    // REFACTOR: Update selected item
    // Purpose: Store selected item, no key change needed
    // Context: Unchanged, works with CertificateItem object
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