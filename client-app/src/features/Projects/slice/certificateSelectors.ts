import { createSelector } from "@reduxjs/toolkit";
import { RootState } from "../../../app/store/configureStore";
import { CertificateItem } from "../../../app/models/project/certificateItem";

export const nonDeletedCertificateItemsSelector = createSelector(
    (state: RootState) => state.certificateItemsUi.certificateItems,
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

        // Purpose: Enable serial-based code generation (e.g., 000018/1) universally
        // Improvement: Simplifies logic by removing type-specific grouping
        const productIdGroups: { [key: string]: CertificateItem[] } = certificateItems.reduce(
            (acc, item) => {
                const productId = item.productId || `UNKNOWN-${Date.now()}`; // Fallback for missing productId
                if (!acc[productId]) acc[productId] = [];
                acc[productId].push(item);
                return acc;
            },
            {} as { [key: string]: CertificateItem[] }
        );

        return certificateItems.map((item) => {
            // Purpose: Ensure total, deserved, and net are calculated correctly for all types
            // Improvement: Centralizes calculation logic, making it easier to extend for new types
            const isContractingType = currentCertificateType === 'WORKMANSHIP_CONTRACTING_CERTIFICATE';
            const total = isContractingType
                ? (item.quantity || 0) * ((item.materialPrice || 0) + (item.laborPrice || 0))
                : item.totalAmount || 0;
            const deserved = isContractingType
                ? Math.max(0, total - (item.deductions || 0))
                : item.deserved || 0;
            const net = isContractingType
                ? Math.max(0, deserved - (item.insurance || 0) - (item.additionalInsurance || 0))
                : item.net || 0;

            // Purpose: Ensure consistent date display (MM/DD/YYYY) across all types
            // Improvement: Handles missing or invalid dates gracefully
            const formattedProcurementDate = item.procurementDate
                ? new Date(item.procurementDate).toLocaleDateString('en-US', {
                    month: '2-digit',
                    day: '2-digit',
                    year: 'numeric',
                })
                : '';

            // Purpose: Provide consistent productId/serial format for grouping
            // Improvement: Ensures unique codes even for non-contracting types
            const productItems = productIdGroups[item.productId || `UNKNOWN-${Date.now()}`];
            const serial = productItems ? productItems.indexOf(item) + 1 : 1;
            const code = `${item.productId || 'UNKNOWN'}/${serial}`;

            // Purpose: Sum net values for items with the same productId
            // Improvement: Supports grid display for grouped totals across all types
            const productSubtotal = productItems.reduce((sum, i) => sum + (i.net || 0), 0);

            console.log('Item calculation:', { item, total, deserved, net, code, productSubtotal });

            // Purpose: Ensure consistent output for grid display (e.g., Total Amount: 28, Net: 23)
            // Improvement: Rounds numbers for display and includes all necessary fields
            return {
                ...item,
                displayTotal: +total.toFixed(2),
                net: +net.toFixed(2),
                formattedProcurementDate,
                code, // Now applied to all types
                productSubtotal: +productSubtotal.toFixed(2),
                deductionDescription: item.deductionDescription || "", // Include for grid display
            };
        });
    }
);

export const certificateReportSelector = createSelector(
    (state: RootState) => state.certificateUi.selectedCertificate,
    (state: RootState) => state.certificateUi.currentCertificateType,
    nonDeletedCertificateItemsSelector,
    (selectedCertificate, currentCertificateType, certificateItems) => {
        // Purpose: Ensure consistent data structure for certificate details
        // Improvement: Handles missing fields and formats dates
        const certificateData = {
            certificateNumber: selectedCertificate.certificateNumber || "N/A",
            projectName: selectedCertificate.projectName || "N/A",
            partyIdSupplier: selectedCertificate.partyIdSupplier?.partyName || "N/A",
            partyIdContractor: selectedCertificate.partyIdContractor?.partyName || "N/A",
            description: selectedCertificate.description || "N/A",
            estimatedStartDate: selectedCertificate.estimatedStartDate
                ? new Date(selectedCertificate.estimatedStartDate).toLocaleDateString("en-US", {
                    year: "numeric",
                    month: "numeric",
                    day: "numeric",
                })
                : "N/A",
            estimatedCompletionDate: selectedCertificate.estimatedCompletionDate
                ? new Date(selectedCertificate.estimatedCompletionDate).toLocaleDateString("en-US", {
                    year: "numeric",
                    month: "numeric",
                    day: "numeric",
                })
                : "N/A",
            facilityName: selectedCertificate.facilityName || "N/A",
            status: selectedCertificate.statusDescriptionArabic || selectedCertificate.statusDescription || "N/A",
        };

        // Purpose: Ensure consistent item data with type-specific fields
        // Improvement: Groups items and calculates subtotals, matching CertificateItemsListGrouped
        const productIdGroups: { [key: string]: CertificateItem[] } = certificateItems.reduce(
            (acc, item) => {
                const productId = item.productId || `UNKNOWN-${Date.now()}`;
                if (!acc[productId]) acc[productId] = [];
                acc[productId].push(item);
                return acc;
            },
            {} as { [key: string]: CertificateItem[] }
        );

        const formattedItems = certificateItems.map((item) => {
            const isContractingType = currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE";
            const total = isContractingType
                ? (item.quantity || 0) * ((item.materialPrice || 0) + (item.laborPrice || 0))
                : item.totalAmount || 0;
            const deserved = isContractingType ? Math.max(0, total - (item.deductions || 0)) : item.deserved || 0;
            const net = isContractingType
                ? Math.max(0, deserved - (item.insurance || 0) - (item.additionalInsurance || 0))
                : item.net || 0;
            const productItems = productIdGroups[item.productId || `UNKNOWN-${Date.now()}`];
            const serial = productItems ? productItems.indexOf(item) + 1 : 1;
            const code = `${item.productId || "UNKNOWN"}/${serial}`;
            const productSubtotal = productItems.reduce((sum, i) => sum + (i.net || 0), 0);

            return {
                ...item,
                code,
                displayTotal: +total.toFixed(2),
                net: +net.toFixed(2),
                deserved: +deserved.toFixed(2),
                formattedProcurementDate: item.procurementDate
                    ? new Date(item.procurementDate).toLocaleDateString("en-US", {
                        month: "2-digit",
                        day: "2-digit",
                        year: "numeric",
                    })
                    : "N/A",
                productSubtotal: +productSubtotal.toFixed(2),
                deductionDescription: item.deductionDescription || "", // Include for PDF report
            };
        });

        return { certificate: certificateData, items: formattedItems };
    }
);