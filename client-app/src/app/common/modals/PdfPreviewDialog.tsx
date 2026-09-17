import React, { useEffect, useRef, useState } from 'react';
import {
    Box,
    Button,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    IconButton,
    Typography,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { saveAs } from 'file-saver';
import { Worker, Viewer, SpecialZoomLevel } from '@react-pdf-viewer/core';
import { defaultLayoutPlugin } from '@react-pdf-viewer/default-layout';
// Worker served from our own bundle (Vite emits it as a hashed asset) so the viewer never
// depends on unpkg.com being reachable from the user's browser, and main thread + worker
// can never drift apart in version.
import pdfWorkerUrl from 'pdfjs-dist/build/pdf.worker.min.js?url';
import '@react-pdf-viewer/core/lib/styles/index.css';
import '@react-pdf-viewer/default-layout/lib/styles/index.css';
import { useTranslationHelper } from '../../hooks/useTranslationHelper';

/**
 * View / print / download dialog for a server-rendered PDF (Telerik Reporting on the API side).
 * The window itself is MUI, the pages are drawn by @react-pdf-viewer (pdf.js) — Telerik's job ends
 * once the API has returned the bytes. Same shape as the payment voucher preview in
 * EditPaymentForm; shared here so every Telerik report gets an identical preview.
 *
 * Usage: open the dialog immediately (so the user sees the spinner), fetch the bytes, then pass the
 * resulting Blob in `blob`. `loading` drives the spinner; `blob === null` with `loading === false`
 * shows the error state. The object URL is created and revoked in here — callers hand over a Blob only.
 */
interface PdfPreviewDialogProps {
    open: boolean;
    onClose: () => void;
    title: React.ReactNode;
    blob: Blob | null;
    loading: boolean;
    /** File name used by the Download button. */
    fileName: string;
    /** Shown in the body when `loading` is false and `blob` is null. */
    errorMessage?: string;
    loadingMessage?: string;
}

export const PdfPreviewDialog: React.FC<PdfPreviewDialogProps> = ({
    open,
    onClose,
    title,
    blob,
    loading,
    fileName,
    errorMessage,
    loadingMessage,
}) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const [blobUrl, setBlobUrl] = useState<string | null>(null);
    const printFrameRef = useRef<HTMLIFrameElement | null>(null);
    // Must be called plainly on every render — react-pdf-viewer plugins register React hooks
    // internally, so wrapping this in useMemo changes the hook count between renders and crashes.
    const defaultLayoutPluginInstance = defaultLayoutPlugin();

    // One object URL per blob, revoked when the blob changes or the dialog unmounts.
    useEffect(() => {
        if (!blob) {
            setBlobUrl(null);
            return;
        }
        const url = URL.createObjectURL(blob);
        setBlobUrl(url);
        return () => URL.revokeObjectURL(url);
    }, [blob]);

    const handleDownload = () => {
        if (blob) saveAs(blob, fileName);
    };

    // Print the PDF itself (via a hidden iframe holding the blob), not the page behind the dialog —
    // window.print() here would print the screen the dialog was opened from.
    const handlePrint = () => {
        const frame = printFrameRef.current;
        if (!frame || !blobUrl) return;
        try {
            frame.contentWindow?.focus();
            frame.contentWindow?.print();
        } catch (e) {
            console.error('PdfPreviewDialog: print failed', e);
        }
    };

    return (
        <Dialog
            open={open}
            onClose={onClose}
            maxWidth="lg"
            fullWidth
            fullScreen={window.innerWidth <= 900}
            PaperProps={{ sx: { height: '90vh', width: '90vw', m: 2 } }}
        >
            <DialogTitle sx={{ m: 0, p: 2, pr: 6 }}>
                {title}
                <IconButton aria-label="close" onClick={onClose} sx={{ position: 'absolute', right: 8, top: 8 }}>
                    <CloseIcon />
                </IconButton>
            </DialogTitle>

            <DialogContent dividers sx={{ p: 0, position: 'relative' }}>
                {loading ? (
                    <Box sx={{ height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                        <CircularProgress />
                        <Typography sx={{ ml: 2 }}>
                            {loadingMessage ?? getTranslatedLabel('common.pdfPreview.loading', 'Generating PDF...')}
                        </Typography>
                    </Box>
                ) : blobUrl ? (
                    <>
                        {/* pdfjs-dist is pinned to 3.11.x: @react-pdf-viewer/core 3.12 supports pdfjs ^2.16 || ^3 only —
                            pdfjs 5 removed renderTextLayer, which broke every page after the first
                            ("PdfJsApi.renderTextLayer is not a function"). */}
                        <Worker workerUrl={pdfWorkerUrl}>
                            <Viewer
                                fileUrl={blobUrl}
                                plugins={[defaultLayoutPluginInstance]}
                                defaultScale={SpecialZoomLevel.PageFit}
                            />
                        </Worker>
                        <iframe
                            ref={printFrameRef}
                            title="pdf-print-frame"
                            src={blobUrl}
                            style={{ position: 'absolute', width: 0, height: 0, border: 0, visibility: 'hidden' }}
                        />
                    </>
                ) : (
                    <Box sx={{ p: 4, textAlign: 'center' }}>
                        <Typography color="error">
                            {errorMessage ?? getTranslatedLabel('common.pdfPreview.failed', 'Failed to generate PDF. Please try again.')}
                        </Typography>
                    </Box>
                )}
            </DialogContent>

            <DialogActions sx={{ p: 2 }}>
                <Button onClick={handlePrint} variant="contained" color="primary" disabled={!blobUrl}>
                    {getTranslatedLabel('common.print', 'Print')}
                </Button>
                <Button onClick={handleDownload} variant="outlined" disabled={!blob}>
                    {getTranslatedLabel('common.download', 'Download')}
                </Button>
                <Button onClick={onClose} variant="outlined">
                    {getTranslatedLabel('common.close', 'Close')}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default PdfPreviewDialog;
