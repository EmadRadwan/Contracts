// src/features/accounting/organizationGlSettings/report/GlAccountTransactionsPdf.tsx
import React, { useCallback, useState } from 'react';
import { Button } from '@mui/material';
import PdfPreviewDialog from '../../../../app/common/modals/PdfPreviewDialog';
import { useLazyFetchGlAccountTransactionsPdfQuery } from '../../../../app/store/apis/accounting/accountingReportsApi';

// PDF counterpart of GlAccountTransactionsExcel. Unlike the Excel button (built client-side with
// ExcelJS), the PDF is rendered server-side with Telerik Reporting — kendo-drawing cannot shape or
// reorder Arabic text, which is why the project report moved to the same server pipeline.
// The button re-queries the same endpoint parameters the modal is showing (incl. the pre-period
// toggle) rather than posting the grid rows back, so the file always reflects the backend's
// running-balance calculation. Opens in the shared view / print / download dialog first.
interface GlAccountTransactionsPdfProps {
    organizationPartyId: string;
    customTimePeriodId: string;
    glAccountId: string;
    includePrePeriodTransactions: boolean;
    accountCode: string;
    accountName: string;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
}

const localizationKey = 'accounting.orgGL.reports.trial-balance.transactions';

export const GlAccountTransactionsPdf: React.FC<GlAccountTransactionsPdfProps> = ({
    organizationPartyId,
    customTimePeriodId,
    glAccountId,
    includePrePeriodTransactions,
    accountCode,
    accountName,
    getTranslatedLabel,
    isFetching = false,
}) => {
    const [triggerPdf] = useLazyFetchGlAccountTransactionsPdfQuery();
    const [loading, setLoading] = useState(false);
    const [showViewer, setShowViewer] = useState(false);
    const [pdfBlob, setPdfBlob] = useState<Blob | null>(null);

    const handleOpen = useCallback(async () => {
        setLoading(true);
        setPdfBlob(null);
        setShowViewer(true);
        try {
            const buffer = await triggerPdf({
                organizationPartyId,
                customTimePeriodId,
                glAccountId,
                includePrePeriodTransactions,
            }).unwrap();
            setPdfBlob(new Blob([buffer], { type: 'application/pdf' }));
        } catch (e) {
            console.error('GlAccountTransactionsPdf: render failed', e);
        } finally {
            setLoading(false);
        }
    }, [triggerPdf, organizationPartyId, customTimePeriodId, glAccountId, includePrePeriodTransactions]);

    const handleClose = () => {
        setShowViewer(false);
        setPdfBlob(null);
    };

    return (
        <>
            <Button
                variant="outlined"
                color="primary"
                disabled={isFetching || loading}
                onClick={handleOpen}
                sx={{ ml: 1 }}
            >
                {getTranslatedLabel(`${localizationKey}.pdf`, 'Preview PDF')}
            </Button>

            <PdfPreviewDialog
                open={showViewer}
                onClose={handleClose}
                title={`${getTranslatedLabel(`${localizationKey}.pdfTitle`, 'Transaction Details')} - ${accountName} (${accountCode})`}
                blob={pdfBlob}
                loading={loading}
                fileName={`GL_Transactions_${accountCode || glAccountId}.pdf`}
                loadingMessage={getTranslatedLabel(`${localizationKey}.pdfLoading`, 'Generating PDF...')}
                errorMessage={getTranslatedLabel(`${localizationKey}.pdfFailed`, 'Failed to generate PDF. Please try again.')}
            />
        </>
    );
};
