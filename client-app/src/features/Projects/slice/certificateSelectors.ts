import { createSelector } from '@reduxjs/toolkit';
import { RootState } from '../../../app/store/configureStore';
import { CertificateItem } from '../../../app/models/project/certificateItem';

/* -------------------------------------------------------------------------- */
/*  Helper – Workmanship totals (used in UI & reports)                       */
/* -------------------------------------------------------------------------- */
const calcWorkmanshipTotals = (item: CertificateItem): {
    total: number;
    deserved: number;
    net: number;
} => {
    // REFACTOR: Use API-provided fields only – backend already calculated them
    // Purpose: UI never drifts from server; zero client-side math
    return {
        total: item.totalAmount ?? 0,
        deserved: item.deserved ?? 0,
        net: item.net ?? 0,
    };
};

/* -------------------------------------------------------------------------- */
/*  Base selector – non-deleted items                                         */
/* -------------------------------------------------------------------------- */
export const nonDeletedCertificateItemsSelector = createSelector(
    (state: RootState) => state.certificateItemsUi.certificateItems,
    (certificateItems) =>
        certificateItems?.entities
            ? Object.values(certificateItems.entities).filter(
                (i): i is CertificateItem => !i?.isDeleted
            )
            : []
);

/* -------------------------------------------------------------------------- */
/*  Sub-total (used in the form footer)                                      */
/* -------------------------------------------------------------------------- */
export const certificateSubTotal = createSelector(
    nonDeletedCertificateItemsSelector,
    (state: RootState) => state.certificateUi.currentCertificateType,
    (items, certType) => {
        if (!items?.length) return 0;
        const isContracting = certType === 'WORKMANSHIP_CONTRACTING_CERTIFICATE';
        const sum = items.reduce((acc, i) => acc + (isContracting ? i.net ?? 0 : i.totalAmount ?? 0), 0);
        return +sum.toFixed(2);
    }
);

/* -------------------------------------------------------------------------- */
/*  UI grid selector – adds serial numbers, codes, formatting                */
/* -------------------------------------------------------------------------- */
/* UI grid selector – adds serial numbers, codes, formatting */
export const displayCertificateItemsSelector = createSelector(
    nonDeletedCertificateItemsSelector,
    (state: RootState) => state.certificateUi.currentCertificateType,
    (items, certType) => {
        if (!items?.length) return [];

        const isContracting = certType === 'WORKMANSHIP_CONTRACTING_CERTIFICATE';

        // ---- group by productId for serial numbering ----
        const groups: Record<string, CertificateItem[]> = items.reduce((acc, i) => {
            const key = i.productId ?? `UNKNOWN-${Date.now()}`;
            (acc[key] ??= []).push(i);
            return acc;
        }, {} as Record<string, CertificateItem[]>);

        return items.map((item) => {
            const group = groups[item.productId ?? `UNKNOWN-${Date.now()}`];
            const serial = group ? group.indexOf(item) + 1 : 1;
            const code = `${item.productId ?? 'UNKNOWN'}/${serial}`;

            const { total, deserved, net } = isContracting
                ? calcWorkmanshipTotals(item)
                : { total: item.totalAmount ?? 0, deserved: 0, net: item.totalAmount ?? 0 };

            const productSubtotal = group.reduce((s, i) => s + (i.net ?? 0), 0);

            const formattedProcurementDate = item.procurementDate
                ? new Date(item.procurementDate).toLocaleDateString('en-US', {
                    month: '2-digit',
                    day: '2-digit',
                    year: 'numeric',
                })
                : 'N/A';

            // REFACTOR: Add isLastInGroup so UI can show subtotal in first column
            // Purpose: Match legacy behavior – subtotal only on last row of group
            const isLastInGroup = group && group.indexOf(item) === group.length - 1;

            return {
                ...item,
                id: item.id ?? `item-${item.workEffortId ?? 'unknown'}-${Date.now()}`,
                displayTotal: +total.toFixed(2),
                deserved: +deserved.toFixed(2),
                net: +net.toFixed(2),
                formattedProcurementDate,
                code,
                productSubtotal: +productSubtotal.toFixed(2),
                deductionDescription: item.deductionDescription ?? 'N/A',
                isLastInGroup, // ← THIS WAS MISSING
            };
        });
    }
);

/* -------------------------------------------------------------------------- */
/*  Generic report selector (used by both supply & workmanship)               */
/* -------------------------------------------------------------------------- */
export const certificateReportSelector = createSelector(
    (state: RootState) => state.certificateUi.selectedCertificate,
    (state: RootState) => state.certificateUi.currentCertificateType,
    nonDeletedCertificateItemsSelector,
    (cert, certType, items) => {
        const certificateData = {
            certificateNumber: cert?.certificateNumber ?? 'N/A',
            projectName: cert?.projectName ?? 'N/A',
            partyIdSupplier: cert?.partyIdSupplier?.partyName ?? 'N/A',
            partyIdContractor: cert?.partyIdContractor?.partyName ?? 'N/A',
            description: cert?.description ?? 'N/A',
            estimatedStartDate: cert?.estimatedStartDate
                ? new Date(cert.estimatedStartDate).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: 'numeric',
                    day: 'numeric',
                })
                : 'N/A',
            estimatedCompletionDate: cert?.estimatedCompletionDate
                ? new Date(cert.estimatedCompletionDate).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: 'numeric',
                    day: 'numeric',
                })
                : 'N/A',
            facilityName: cert?.facilityName ?? 'N/A',
            status: cert?.statusDescriptionArabic ?? cert?.statusDescription ?? 'N/A',
        };

        if (!items?.length) return { certificate: certificateData, items: [] };

        const groups: Record<string, CertificateItem[]> = items.reduce((acc, i) => {
            const key = i.productId ?? `UNKNOWN-${Date.now()}`;
            (acc[key] ??= []).push(i);
            return acc;
        }, {} as Record<string, CertificateItem[]>);

        const formatted = items.map((item) => {
            const group = groups[item.productId ?? `UNKNOWN-${Date.now()}`];
            const serial = group ? group.indexOf(item) + 1 : 1;
            const code = `${item.productId ?? 'UNKNOWN'}/${serial}`;
            const productSubtotal = group.reduce((s, i) => s + (i.net ?? 0), 0);
            const formattedProcurementDate = item.procurementDate
                ? new Date(item.procurementDate).toLocaleDateString('en-US', {
                    month: '2-digit',
                    day: '2-digit',
                    year: 'numeric',
                })
                : 'N/A';

            const { total, deserved, net } = calcWorkmanshipTotals(item);

            return {
                ...item,
                id: item.id ?? `item-${item.workEffortId ?? 'unknown'}-${Date.now()}`,
                code,
                displayTotal: +total.toFixed(2),
                deserved: +deserved.toFixed(2),
                net: +net.toFixed(2),
                formattedProcurementDate,
                productSubtotal: +productSubtotal.toFixed(2),
                deductionDescription: item.deductionDescription ?? 'N/A',
            };
        });

        return { certificate: certificateData, items: formatted };
    }
);

/* -------------------------------------------------------------------------- */
/*  Supply-only report (keeps original shape)                                 */
/* -------------------------------------------------------------------------- */
export const supplyCertificateReportSelector = createSelector(
    (state: RootState) => state.certificateUi.selectedCertificate,
    nonDeletedCertificateItemsSelector,
    (state: RootState) => state.certificateUi.currentCertificateType,
    (cert, items) => {
        const certificateData = {
            certificateNumber: cert?.certificateNumber ?? 'N/A',
            description: cert?.description ?? 'N/A',
            partyIdSupplier: cert?.partyIdSupplier?.partyName ?? 'N/A',
            facilityName: cert?.facilityName ?? 'N/A',
        };

        if (!items?.length) return { certificate: certificateData, items: [] };

        const groups: Record<string, CertificateItem[]> = items.reduce((acc, i) => {
            const key = i.productId ?? `UNKNOWN-${Date.now()}`;
            (acc[key] ??= []).push(i);
            return acc;
        }, {} as Record<string, CertificateItem[]>);

        const formatted = items.map((item) => {
            const group = groups[item.productId ?? `UNKNOWN-${Date.now()}`];
            const serial = group ? group.indexOf(item) + 1 : 1;
            const code = `${item.productId ?? 'UNKNOWN'}/${serial}`;
            const productSubtotal = group.reduce((s, i) => s + (i.net ?? 0), 0);

            const formattedProcurementDate = item.procurementDate
                ? new Date(item.procurementDate).toLocaleDateString('en-US', {
                    month: '2-digit',
                    day: '2-digit',
                    year: 'numeric',
                })
                : 'N/A';

            return {
                id: item.id ?? `item-${item.workEffortId ?? 'unknown'}-${Date.now()}`,
                productName: item.productName ?? 'N/A',
                code,
                description: item.description ?? 'N/A',
                quantity: item.quantity ?? 0,
                uomName: item.uomName ?? 'N/A',
                unitPrice: item.unitPrice ?? 0,
                displayTotal: item.totalAmount ? +item.totalAmount.toFixed(2) : 0,
                discount: item.discount ?? 0,
                formattedProcurementDate,
                transportationExpenses: item.transportationExpenses ?? 0,
                gratuities: item.gratuities ?? 0,
                isLastInGroup: item.isLastInGroup ?? false,
                productSubtotal: +productSubtotal.toFixed(2),
                mainItemDescription: item.mainItemDescription ?? '',
                discountNote: item.discountNote ?? '',
            };
        });

        return { certificate: certificateData, items: formatted };
    }
);

/* -------------------------------------------------------------------------- */
/*  Workmanship-only report (uses helper)                                    */
/* -------------------------------------------------------------------------- */
export const workmanshipCertificateReportSelector = createSelector(
    (state: RootState) => state.certificateUi.selectedCertificate,
    nonDeletedCertificateItemsSelector,
    (state: RootState) => state.certificateUi.currentCertificateType,
    (cert, items) => {
        const certificateData = {
            certificateNumber: cert?.certificateNumber ?? 'N/A',
            description: cert?.description ?? 'N/A',
            partyIdContractor: cert?.partyIdContractor?.partyName ?? 'N/A',
        };

        if (!items?.length) return { certificate: certificateData, items: [] };

        const groups: Record<string, CertificateItem[]> = items.reduce((acc, i) => {
            const key = i.productId ?? `UNKNOWN-${Date.now()}`;
            (acc[key] ??= []).push(i);
            return acc;
        }, {} as Record<string, CertificateItem[]>);

        const formatted = items.map((item) => {
            const { total, deserved, net } = calcWorkmanshipTotals(item);

            const group = groups[item.productId ?? `UNKNOWN-${Date.now()}`];
            const serial = group ? group.indexOf(item) + 1 : 1;
            const code = `${item.productId ?? 'UNKNOWN'}/${serial}`;
            const productSubtotal = group.reduce((s, i) => s + (i.net ?? 0), 0);

            return {
                id: item.id ?? `item-${item.workEffortId ?? 'unknown'}-${Date.now()}`,
                productName: item.productName ?? 'N/A',
                code,
                description: item.description ?? 'N/A',
                quantity: item.quantity ?? 0,
                uomName: item.uomName ?? 'N/A',
                materialPrice: item.materialPrice ?? 0,
                laborPrice: item.laborPrice ?? 0,
                displayTotal: +total.toFixed(2),
                deductions: item.deductions ?? 0,
                deductionDescription: item.deductionDescription ?? 'N/A',
                deserved: +deserved.toFixed(2),
                insurance: item.insurance ?? 0,
                additionalInsurance: item.additionalInsurance ?? 0,
                net: +net.toFixed(2),
                achievementPercentage: typeof item.achievementPercentage === 'string'
                    ? parseFloat(item.achievementPercentage.replace('%', '')) || 0
                    : item.achievementPercentage ?? 0,
                isLastInGroup: item.isLastInGroup ?? false,
                productSubtotal: +productSubtotal.toFixed(2),
                mainItemDescription: item.mainItemDescription ?? '',
                discountNote: item.discountNote ?? '',
            };
        });

        return { certificate: certificateData, items: formatted };
    }
);