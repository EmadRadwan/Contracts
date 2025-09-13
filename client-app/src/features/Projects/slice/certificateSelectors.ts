import { createSelector } from "@reduxjs/toolkit";
import { RootState } from "../../../app/store/configureStore";
import {certificateItemsEntities} from "./certificateItemsUiSlice";
import {CertificateItem} from "../../../app/models/project/certificateItem";

// Purpose: Filter out items marked as deleted
// Context: Mirrors nonDeletedOrderItemsSelector
export const nonDeletedCertificateItemsSelector = createSelector(
    certificateItemsEntities,
    (certificateItems) =>
        Object.values(certificateItems).filter(
            (certificateItem): certificateItem is CertificateItem => !certificateItem?.isDeleted
        )
);

// Purpose: Determine if the certificate has no valid items
// Context: No key dependency, unchanged
export const allItemsAreDeletedOrNone = createSelector(
    certificateItemsEntities,
    (certificateItems) => {
        const items = Object.values(certificateItems);
        return !items || items.length === 0 || items.every((item) => item.isDeleted === true);
    }
);

// Purpose: Ensure subtotal reflects the effective total after adjustments like discount, deductions, insurance, trans, grat.
// Improvement: Provides accurate aggregation for display, aligning with form calculations and grid columns.
export const certificateSubTotal = createSelector(
    nonDeletedCertificateItemsSelector,
    (state: RootState) => state.certificateUi.currentCertificateType,
    (certificateItems, currentCertificateType) => {
        if (!certificateItems) return 0;
        return certificateItems.reduce((sum, item) => {
            let amount = 0;
            const total = item.totalAmount || 0;
            if (["SUPPLY_PROCUREMENT_CERTIFICATE", "EXTERNAL_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType)) {
                amount = total - (item.discount || 0) + (item.transportationExpenses || 0) + (item.gratuities || 0);
            } else if (["COMPANY_SUPPLY_SALE_CERTIFICATE", "CONTRACTOR_PURCHASE_CERTIFICATE"].includes(currentCertificateType)) {
                amount = total + (item.transportationExpenses || 0) + (item.gratuities || 0);
            } else if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
                const deserved = total - (item.deductions || 0);
                amount = Math.max(0, deserved - (item.insurance || 0));
            }
            return sum + Math.round(amount * 100) / 100;
        }, 0);
    }
);

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

// Purpose: Replace deprecated type checks with specific includes, calculate displayTotal as gross for contracting and final for supply.
// Improvement: Ensures grid displays accurate values, fixes missing additives, and supports dynamic column adjustments.
export const displayCertificateItemsSelector = createSelector(
    nonDeletedCertificateItemsSelector,
    (state: RootState) => state.certificateUi.currentCertificateType,
    (certificateItems, currentCertificateType) =>
        certificateItems.map((item) => {
            const total = item.totalAmount || 0;
            let displayTotal = total;
            let net = displayTotal;
            if (["SUPPLY_PROCUREMENT_CERTIFICATE", "EXTERNAL_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType)) {
                displayTotal = total - (item.discount || 0) + (item.transportationExpenses || 0) + (item.gratuities || 0);
            } else if (["COMPANY_SUPPLY_SALE_CERTIFICATE", "CONTRACTOR_PURCHASE_CERTIFICATE"].includes(currentCertificateType)) {
                displayTotal = total + (item.transportationExpenses || 0) + (item.gratuities || 0);
            } else if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
                const deserved = Math.max(0, Math.round((total - (item.deductions || 0)) * 1000) / 1000);
                net = Math.max(0, Math.round((deserved - (item.insurance || 0)) * 1000) / 1000);
                displayTotal = total;
            }
            const formattedProcurementDate = item.procurementDate
                ? new Date(item.procurementDate).toLocaleDateString("en-US", {
                    month: "2-digit",
                    day: "2-digit",
                    year: "numeric",
                })
                : "";
            return {
                ...item,
                displayTotal: +displayTotal.toFixed(2),
                net: +net.toFixed(2),
                formattedProcurementDate,
            };
        })
);