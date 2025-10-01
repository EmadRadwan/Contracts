import React from 'react';
import { Button } from '@mui/material';
import { WorkmanshipCertificateExcel } from './WorkmanshipCertificateExcel';
import { SupplyCertificateExcel } from './SupplyCertificateExcel';

// REFACTOR: Removed PDF component calls and added SupplyCertificateExcel
// Purpose: Eliminates WorkmanshipCertificatePDF and SupplyCertificatePDF; uses Excel components only
// Improvement: Simplifies logic by removing showPDF state and PDF button; adds SupplyCertificateExcel for supply certificates
// Context: User wants only Excel reports, aligning with WorkmanshipCertificateExcel usage
const renderCertificateReport = () => {
    const commonProps = {
        getTranslatedLabel,
        subtotal,
        isSubmitting,
        isAddCertificateLoading,
        isUpdateCertificateLoading,
        isReceiveLoading,
        isFetching,
    };

    // REFACTOR: Removed PDF button and showPDF logic
    // Purpose: Eliminates PDF export button as only Excel is required
    // Improvement: Reduces UI clutter and simplifies rendering logic
    // Context: PDF components are no longer needed
    if (selectedCertificate?.currentStatusId !== CertificateStatus.CREATED) {
        return (
            <WorkmanshipCertificateExcel
                certificate={workmanshipReportData.certificate}
                items={workmanshipReportData.items}
                {...commonProps}
            />
        );
    }

    const isValidItems = items && items.length > 0 && validateItems(
        currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE"
            ? workmanshipReportData.items
            : supplyReportData.items,
        currentCertificateType
    ).isValid;

    if (!isValidItems) {
        return (
            <WorkmanshipCertificateExcel
                certificate={workmanshipReportData.certificate}
                items={workmanshipReportData.items}
                {...commonProps}
            />
        );
    }

    // REFACTOR: Added SupplyCertificateExcel for supply certificate types
    // Purpose: Handles SUPPLY_PROCUREMENT_CERTIFICATE and COMPANY_SUPPLY_SALE_CERTIFICATE with SupplyCertificateExcel
    // Improvement: Matches certificate type to appropriate Excel component; uses supplyReportData for supply types
    // Context: Aligns with WorkmanshipCertificateExcel for consistent Excel output
    if (currentCertificateType === "WORKMANSHIP_CONTRACTING_CERTIFICATE") {
        if (workmanshipReportData.items?.length > 0 && workmanshipReportData.items[0].materialPrice !== undefined) {
            return (
                <WorkmanshipCertificateExcel
                    certificate={workmanshipReportData.certificate}
                    items={workmanshipReportData.items}
                    {...commonProps}
                    key={`${selectedCertificate.workEffortId}-workmanship`}
                />
            );
        }
    } else if (["SUPPLY_PROCUREMENT_CERTIFICATE", "COMPANY_SUPPLY_SALE_CERTIFICATE"].includes(currentCertificateType)) {
        if (supplyReportData.items?.length > 0 && supplyReportData.items[0].unitPrice !== undefined) {
            return (
                <SupplyCertificateExcel
                    certificate={supplyReportData.certificate}
                    items={supplyReportData.items}
                    {...commonProps}
                    key={`${selectedCertificate.workEffortId}-supply`}
                />
            );
        }
    }

    // REFACTOR: Default to WorkmanshipCertificateExcel for invalid cases
    // Purpose: Provides fallback Excel component when no valid items or type
    // Improvement: Ensures a component is rendered even for edge cases
    // Context: Maintains consistency with original fallback behavior
    return (
        <WorkmanshipCertificateExcel
            certificate={workmanshipReportData.certificate}
            items={workmanshipReportData.items}
            {...commonProps}
        />
    );
};