import { createSelector } from "@reduxjs/toolkit";
import { RootState } from "../../../app/store/configureStore";
import { CertificateItem } from "../../../app/models/project/certificateItem";

export const nonDeletedCertificateItemsSelector = createSelector(
    (state: RootState) => state.certificateItemsUi.certificateItems,
    // REFACTOR: Handle entity adapter structure
    // Purpose: Ensure selector works with createEntityAdapter
    // Improvement: Safely handles empty or uninitialized state
    (certificateItems) =>
        certificateItems?.entities
            ? Object.values(certificateItems.entities).filter(
                (certificateItem): certificateItem is CertificateItem => !certificateItem?.isDeleted
            )
            : []
);

export const certificateSubTotal = createSelector(
    nonDeletedCertificateItemsSelector,
    (state: RootState) => state.certificateUi.currentCertificateType,
    (certificateItems, currentCertificateType) => {
        console.log('certificateSubTotal input:', certificateItems);
        if (!certificateItems) return 0;

        // REFACTOR: Use net for WORKMANSHIP_CONTRACTING_CERTIFICATE, totalAmount for others
        // Purpose: Align toolbar Total with net (e.g., 23.00) for WORKMANSHIP_CONTRACTING_CERTIFICATE
        // Improvement: Fixes incorrect Total (0 or totalAmount) in CertificateItemsListGrouped toolbar
        const isContractingType = currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE";
        const total = certificateItems.reduce((sum, item) => {
            const amount = isContractingType ? (item.net || 0) : (item.totalAmount || 0);
            console.log('certificateSubTotal item:', { item, amount });
            return sum + amount;
        }, 0);
        console.log('certificateSubTotal result:', total);
        return +total.toFixed(2);
    }
);

export const displayCertificateItemsSelector = createSelector(
    nonDeletedCertificateItemsSelector,
    (state: RootState) => state.certificateUi.currentCertificateType,
    (certificateItems, currentCertificateType) => {
        console.log('displayCertificateItemsSelector input:', certificateItems);
        const isContractingType = currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE";

        // REFACTOR: Robust code generation for WORKMANSHIP_CONTRACTING_CERTIFICATE
        // Purpose: Ensure valid productId and serial for code (e.g., 000018/1)
        // Improvement: Preserves correct behavior for CertificateItemsListGrouped
        let productIdGroups: { [key: string]: CertificateItem[] } = {};
        if (isContractingType) {
            productIdGroups = certificateItems.reduce(
                (acc, item) => {
                    const productId = item.productId || `UNKNOWN-${Date.now()}`;
                    if (!acc[productId]) acc[productId] = [];
                    acc[productId].push(item);
                    return acc;
                },
                {} as { [key: string]: CertificateItem[] }
            );
        }

        return certificateItems.map((item) => {
            // REFACTOR: Use totalAmount for displayTotal, net for contracting types
            // Purpose: Maintain correct grid display (e.g., Total Amount: 28, Net: 23)
            // Improvement: Ensures consistency with database and form calculations
            const total = isContractingType
                ? (item.quantity || 0) * ((item.materialPrice || 0) + (item.laborPrice || 0))
                : item.totalAmount || 0;
            const deserved = isContractingType
                ? Math.max(0, total - (item.deductions || 0))
                : item.deserved || 0;
            const net = isContractingType
                ? Math.max(0, deserved - (item.insurance || 0) - (item.additionalInsurance || 0))
                : item.net || 0;
            const formattedProcurementDate = item.procurementDate
                ? new Date(item.procurementDate).toLocaleDateString("en-US", {
                    month: "2-digit",
                    day: "2-digit",
                    year: "numeric",
                })
                : "";

            let code = "";
            let productSubtotal = 0;
            if (isContractingType) {
                const productItems = productIdGroups[item.productId || `UNKNOWN-${Date.now()}`];
                const serial = productItems ? productItems.indexOf(item) + 1 : 1;
                code = `${item.productId || 'UNKNOWN'}/${serial}`;
                productSubtotal = productItems.reduce((sum, i) => sum + (i.net || 0), 0);
            }

            console.log('Item calculation:', { item, total, deserved, net, code, productSubtotal });
            return {
                ...item,
                displayTotal: +total.toFixed(2),
                net: +net.toFixed(2),
                formattedProcurementDate,
                code: isContractingType ? code : "",
                productSubtotal: isContractingType ? +productSubtotal.toFixed(2) : 0,
            };
        });
    }
);