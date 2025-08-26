import { createSelector } from "@reduxjs/toolkit";
import { RootState } from "../../../app/store/configureStore";
import {certificateItemsEntities} from "./certificateItemsUiSlice";
import {CertificateItem} from "../../../app/models/project/certificateItem";

// REFACTOR: Select non-deleted certificate items
// Purpose: Filter out items marked as deleted
// Context: Mirrors nonDeletedOrderItemsSelector
export const nonDeletedCertificateItemsSelector = createSelector(
    certificateItemsEntities,
    (certificateItems) =>
        Object.values(certificateItems).filter(
            (certificateItem): certificateItem is CertificateItem => !certificateItem?.isDeleted
        )
);

// REFACTOR: Check if all items are deleted or none exist
// Purpose: Determine if the certificate has no valid items
// Context: No key dependency, unchanged
export const allItemsAreDeletedOrNone = createSelector(
    certificateItemsEntities,
    (certificateItems) => {
        const items = Object.values(certificateItems);
        return !items || items.length === 0 || items.every((item) => item.isDeleted === true);
    }
);

// REFACTOR: Calculate certificate subtotal
// Purpose: Sum totalAmount of non-deleted items
// Context: No key dependency, unchanged
export const certificateSubTotal = createSelector(
    nonDeletedCertificateItemsSelector,
    (certificateItems) => {
        if (!certificateItems) return 0;
        const subtotal = certificateItems.reduce((sum, item) => {
            const totalAmount = typeof item.totalAmount === "number" ? item.totalAmount : 0;
            return sum + totalAmount;
        }, 0);
        return Math.round(subtotal * 100) / 100;
    }
);

// REFACTOR: Calculate selected item subtotal
// Purpose: Get totalAmount for the selected item using workEffortId
// Context: Updated to use workEffortId
export const certificateItemSubTotal = createSelector(
    certificateItemsEntities,
    (state: RootState) => state.certificateItemsUi.selectedCertificateItem,
    (certificateItemsEntities, selectedCertificateItem) => {
        const filteredItems =
            certificateItemsEntities?.filter(
                (item: CertificateItem) => !item?.isDeleted && item?.workEffortId === selectedCertificateItem?.workEffortId
            ) || [];
        return filteredItems.reduce((sum, item) => sum + (item?.totalAmount || 0), 0);
    }
);