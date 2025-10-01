import { createSelector } from '@reduxjs/toolkit';
import { RootState } from '../../../app/store/configureStore';
import { CertificateItem } from '../../../app/models/project/certificateItem';

export const nonDeletedCertificateItemsSelector = createSelector(
    (state: RootState) => state.certificateItemsUi.certificateItems,
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
        console.log('certificateSubTotal input:', { certificateItems, currentCertificateType });
        if (!certificateItems || certificateItems.length === 0) return 0;
        const isContractingType = currentCertificateType === 'WORKMANSHIP_CONTRACTING_CERTIFICATE';
        const total = certificateItems.reduce((sum, item) => {
            const amount = isContractingType ? item.net || 0 : item.totalAmount || 0;
            return sum + amount;
        }, 0);
        return +total.toFixed(2);
    }
);

export const displayCertificateItemsSelector = createSelector(
    nonDeletedCertificateItemsSelector,
    (state: RootState) => state.certificateUi.currentCertificateType,
    (certificateItems, currentCertificateType) => {
        console.log('displayCertificateItemsSelector input:', { certificateItems, currentCertificateType });
        if (!certificateItems || certificateItems.length === 0) return [];
        const productIdGroups: { [key: string]: CertificateItem[] } = certificateItems.reduce(
            (acc, item) => {
                const productId = item.productId || `UNKNOWN-${Date.now()}`;
                if (!acc[productId]) acc[productId] = [];
                acc[productId].push(item);
                return acc;
            },
            {} as { [key: string]: CertificateItem[] }
        );
        return certificateItems.map((item) => {
            const isContractingType = currentCertificateType === 'WORKMANSHIP_CONTRACTING_CERTIFICATE';
            const total = isContractingType
                ? (item.quantity || 0) * ((item.materialPrice || 0) + (item.laborPrice || 0))
                : item.totalAmount || 0;
            const deserved = isContractingType ? Math.max(0, total - (item.deductions || 0)) : item.deserved || 0;
            const net = isContractingType
                ? Math.max(0, deserved - (item.insurance || 0) - (item.additionalInsurance || 0))
                : item.net || 0;
            const formattedProcurementDate = item.procurementDate
                ? new Date(item.procurementDate).toLocaleDateString('en-US', { month: '2-digit', day: '2-digit', year: 'numeric' })
                : 'N/A';
            const productItems = productIdGroups[item.productId || `UNKNOWN-${Date.now()}`];
            const serial = productItems ? productItems.indexOf(item) + 1 : 1;
            const code = `${item.productId || 'UNKNOWN'}/${serial}`;
            const productSubtotal = productItems.reduce((sum, i) => sum + (i.net || 0), 0);
            return {
                ...item,
                id: item.id || `item-${item.workEffortId || 'unknown'}-${Date.now()}`, // REFACTOR: Add fallback id with workEffortId
                // Purpose: Ensure unique id for rendering
                displayTotal: +total.toFixed(2),
                net: +net.toFixed(2),
                formattedProcurementDate,
                code,
                productSubtotal: +productSubtotal.toFixed(2),
                deductionDescription: item.deductionDescription || 'N/A',
            };
        });
    }
);

export const certificateReportSelector = createSelector(
    (state: RootState) => state.certificateUi.selectedCertificate,
    (state: RootState) => state.certificateUi.currentCertificateType,
    nonDeletedCertificateItemsSelector,
    (selectedCertificate, currentCertificateType, certificateItems) => {
        const certificateData = {
            certificateNumber: selectedCertificate?.certificateNumber || 'N/A',
            projectName: selectedCertificate?.projectName || 'N/A',
            partyIdSupplier: selectedCertificate?.partyIdSupplier?.partyName || 'N/A',
            partyIdContractor: selectedCertificate?.partyIdContractor?.partyName || 'N/A',
            description: selectedCertificate?.description || 'N/A',
            estimatedStartDate: selectedCertificate?.estimatedStartDate
                ? new Date(selectedCertificate.estimatedStartDate).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: 'numeric',
                    day: 'numeric',
                })
                : 'N/A',
            estimatedCompletionDate: selectedCertificate?.estimatedCompletionDate
                ? new Date(selectedCertificate.estimatedCompletionDate).toLocaleDateString('en-US', {
                    year: 'numeric',
                    month: 'numeric',
                    day: 'numeric',
                })
                : 'N/A',
            facilityName: selectedCertificate?.facilityName || 'N/A',
            status: selectedCertificate?.statusDescriptionArabic || selectedCertificate?.statusDescription || 'N/A',
        };
        if (!certificateItems || certificateItems.length === 0) return { certificate: certificateData, items: [] };
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
            const isContractingType = currentCertificateType === 'WORKMANSHIP_CONTRACTING_CERTIFICATE';
            const total = isContractingType
                ? (item.quantity || 0) * ((item.materialPrice || 0) + (item.laborPrice || 0))
                : item.totalAmount || 0;
            const deserved = isContractingType ? Math.max(0, total - (item.deductions || 0)) : item.deserved || 0;
            const net = isContractingType
                ? Math.max(0, deserved - (item.insurance || 0) - (item.additionalInsurance || 0))
                : item.net || 0;
            const formattedProcurementDate = item.procurementDate
                ? new Date(item.procurementDate).toLocaleDateString('en-US', {
                    month: '2-digit',
                    day: '2-digit',
                    year: 'numeric',
                })
                : 'N/A';
            const productItems = productIdGroups[item.productId || `UNKNOWN-${Date.now()}`];
            const serial = productItems ? productItems.indexOf(item) + 1 : 1;
            const code = `${item.productId || 'UNKNOWN'}/${serial}`;
            const productSubtotal = productItems.reduce((sum, i) => sum + (i.net || 0), 0);
            return {
                ...item,
                id: item.id || `item-${item.workEffortId || 'unknown'}-${Date.now()}`, // REFACTOR: Add fallback id
                // Purpose: Ensure unique id for rendering
                code,
                displayTotal: +total.toFixed(2),
                net: +net.toFixed(2),
                deserved: +deserved.toFixed(2),
                formattedProcurementDate,
                productSubtotal: +productSubtotal.toFixed(2),
                deductionDescription: item.deductionDescription || 'N/A',
            };
        });
        return { certificate: certificateData, items: formattedItems };
    }
);

export const supplyCertificateReportSelector = createSelector(
    (state: RootState) => state.certificateUi.selectedCertificate,
    nonDeletedCertificateItemsSelector,
    (state: RootState) => state.certificateUi.currentCertificateType,
    (selectedCertificate, certificateItems, currentCertificateType) => {
        console.log('supplyCertificateReportSelector:', { selectedCertificate, certificateItems, currentCertificateType });
        const certificateData = {
            certificateNumber: selectedCertificate?.certificateNumber || 'N/A',
            description: selectedCertificate?.description || 'N/A',
            partyIdSupplier: selectedCertificate?.partyIdSupplier?.partyName || 'N/A',
            facilityName: selectedCertificate?.facilityName || 'N/A',
        };
        if (!certificateItems || certificateItems.length === 0) return { certificate: certificateData, items: [] };
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
            const productItems = productIdGroups[item.productId || `UNKNOWN-${Date.now()}`];
            const serial = productItems ? productItems.indexOf(item) + 1 : 1;
            const code = `${item.productId || 'UNKNOWN'}/${serial}`;
            const productSubtotal = productItems.reduce((sum, i) => sum + (i.net || 0), 0);
            return {
                id: item.id || `item-${item.workEffortId || 'unknown'}-${Date.now()}`, // REFACTOR: Add fallback id
                // Purpose: Ensure unique id for rendering
                productName: item.productName || 'N/A',
                code,
                description: item.description || 'N/A',
                quantity: item.quantity || 0,
                uomName: item.uomName || 'N/A',
                unitPrice: item.unitPrice || 0,
                displayTotal: item.totalAmount ? +item.totalAmount.toFixed(2) : 0,
                discount: item.discount || 0,
                formattedProcurementDate: item.procurementDate
                    ? new Date(item.procurementDate).toLocaleDateString('en-US', { month: '2-digit', day: '2-digit', year: 'numeric' })
                    : 'N/A',
                transportationExpenses: item.transportationExpenses || 0,
                gratuities: item.gratuities || 0,
                isLastInGroup: item.isLastInGroup || false,
                productSubtotal: +productSubtotal.toFixed(2),
                mainItemDescription: item.mainItemDescription || '',
                discountNote: item.discountNote || '',
            };
        });
        return { certificate: certificateData, items: formattedItems };
    }
);

export const workmanshipCertificateReportSelector = createSelector(
    (state: RootState) => state.certificateUi.selectedCertificate,
    nonDeletedCertificateItemsSelector,
    (state: RootState) => state.certificateUi.currentCertificateType,
    (selectedCertificate, certificateItems, currentCertificateType) => {
        console.log('workmanshipCertificateReportSelector:', { selectedCertificate, certificateItems, currentCertificateType });
        const certificateData = {
            certificateNumber: selectedCertificate?.certificateNumber || 'N/A',
            description: selectedCertificate?.description || 'N/A',
            partyIdContractor: selectedCertificate?.partyIdContractor?.partyName || 'N/A',
        };
        if (!certificateItems || certificateItems.length === 0) return { certificate: certificateData, items: [] };
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
            const isContractingType = currentCertificateType === 'WORKMANSHIP_CONTRACTING_CERTIFICATE';
            const total = isContractingType
                ? (item.quantity || 0) * ((item.materialPrice || 0) + (item.laborPrice || 0))
                : item.totalAmount || 0;
            const deserved = isContractingType ? Math.max(0, total - (item.deductions || 0)) : item.deserved || 0;
            const net = isContractingType
                ? Math.max(0, deserved - (item.insurance || 0) - (item.additionalInsurance || 0))
                : item.net || 0;
            const productItems = productIdGroups[item.productId || `UNKNOWN-${Date.now()}`];
            const serial = productItems ? productItems.indexOf(item) + 1 : 1;
            const code = `${item.productId || 'UNKNOWN'}/${serial}`;
            const productSubtotal = productItems.reduce((sum, i) => sum + (i.net || 0), 0);
            return {
                id: item.id || `item-${item.workEffortId || 'unknown'}-${Date.now()}`, // REFACTOR: Add fallback id
                // Purpose: Ensure unique id for rendering
                productName: item.productName || 'N/A',
                code,
                description: item.description || 'N/A',
                quantity: item.quantity || 0,
                uomName: item.uomName || 'N/A',
                materialPrice: item.materialPrice || 0,
                laborPrice: item.laborPrice || 0,
                displayTotal: +total.toFixed(2),
                deductions: item.deductions || 0,
                deductionDescription: item.deductionDescription || 'N/A',
                deserved: +deserved.toFixed(2),
                insurance: item.insurance || 0,
                additionalInsurance: item.additionalInsurance || 0,
                net: +net.toFixed(2),
                achievementPercentage: item.achievementPercentage || 'N/A',
                isLastInGroup: item.isLastInGroup || false,
                productSubtotal: +productSubtotal.toFixed(2),
                mainItemDescription: item.mainItemDescription || '',
                discountNote: item.discountNote || '',
            };
        });
        return { certificate: certificateData, items: formattedItems };
    }
);