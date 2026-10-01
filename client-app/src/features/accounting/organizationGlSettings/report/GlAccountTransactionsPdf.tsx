// src/features/accounting/organizationGlSettings/report/GlAccountTransactionsPdf.tsx
import React, { useCallback, useState } from 'react';
import { Button } from '@mui/material';
import PdfPreviewDialog from '../../../../app/common/modals/PdfPreviewDialog';

// PDF counterpart of GlAccountTransactionsExcel. Unlike the Excel button (built client-side with
// ExcelJS), the PDF is rendered server-side with Telerik Reporting — kendo-drawing cannot shape or
// reorder Arabic text, which is why the project report moved to the same server pipeline.
// The button re-queries the same endpoint parameters the modal is showing (incl. the pre-period
// toggle) rather than posting the grid rows back, so the file always reflects the backend's
// running-balance calculation. Opens in the shared view / print / download dialog first.
interface GlAccountTransactionsPdfProps {
    /** Runs the report's own PDF endpoint and resolves the rendered bytes. Each drill-down modal
     *  supplies its own (trial balance by time period, balance sheet by thru date, income
     *  statement by date range/month) — the button itself is report-agnostic. */
    fetchPdf: () => Promise<ArrayBuffer>;
    glAccountId: string;
    accountCode: string;
    accountName: string;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
}

// The button's own four labels (Preview PDF / title / generating / failed) are generic and are
// deliberately read from the trial balance subtree for every report, so the balance sheet and
// income statement get the existing Arabic translations instead of falling back to English under
// their own prefixes.
const localizationKey = 'accounting.orgGL.reports.trial-balance.transactions';

export const GlAccountTransactionsPdf: React.FC<GlAccountTransactionsPdfProps> = ({
    fetchPdf,
    glAccountId,
    accountCode,
    accountName,
    getTranslatedLabel,
    isFetching = false,
}) => {
    const [loading, setLoading] = useState(false);
    const [showViewer, setShowViewer] = useState(false);
    const [pdfBlob, setPdfBlob] = useState<Blob | null>(null);

    const handleOpen = useCallback(async () => {
        setLoading(true);
        setPdfBlob(null);
        setShowViewer(true);
        try {
            const buffer = await fetchPdf();
            setPdfBlob(new Blob([buffer], { type: 'application/pdf' }));
        } catch (e) {
            console.error('GlAccountTransactionsPdf: render failed', e);
        } finally {
            setLoading(false);
        }
    }, [fetchPdf]);

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
