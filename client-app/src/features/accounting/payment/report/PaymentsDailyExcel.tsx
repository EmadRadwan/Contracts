// src/features/accounting/payment/report/PaymentsDailyExcel.tsx
import React, { useCallback, useState } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';
import {useLazyFetchDailyPaymentsLazyQuery} from "../../../../app/store/apis";

/* ------------------------------------------------------------------ */
/* PROPS */
/* ------------------------------------------------------------------ */
interface PaymentsDailyExcelProps {
    companyName: string;
    paymentType: 'incoming' | 'outgoing';
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

/* ------------------------------------------------------------------ */
/* UTILS – RTL, formatting, safe values */
/* ------------------------------------------------------------------ */
const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? 'N/A' : v.toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

/* ------------------------------------------------------------------ */
/* MAIN COMPONENT */
/* ------------------------------------------------------------------ */
export const PaymentsDailyExcel: React.FC<PaymentsDailyExcelProps> = ({
                                                                          companyName,
                                                                          paymentType,
                                                                          getTranslatedLabel,
                                                                      }) => {
    const [trigger, { isFetching }] = useLazyFetchDailyPaymentsLazyQuery();
    const [isGenerating, setIsGenerating] = useState(false);

    // REFACTOR: Generate Excel only when we have confirmed data
    // Purpose: Avoid race condition with previous timed-wait approach
    // Improvement: Uses lazy query + await → 100% reliable
    const generateExcel = useCallback(async (data: { data: any[]; total: number }) => {
        if (!data || data.data.length === 0) return null;

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'Golden Land System';

        // --- FETCH LOGO ---
        let logoBuffer: ArrayBuffer | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) logoBuffer = await resp.blob().then(b => b.arrayBuffer());
        } catch (e) {
            console.warn('Logo not found:', e);
        }

        const safeSheetName = `${paymentType} Payments - ${new Date().toISOString().split('T')[0]}`.replace(/[*\?\\:\[\]\/]/g, '_').slice(0, 31);
        const ws = workbook.addWorksheet(safeSheetName);
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.views = [{ rightToLeft: true }];

        // Initialize rows early to avoid ExcelJS bugs
        ws.getRow(1).height = logoBuffer ? 75 : 30;

        // === ADD LOGO ===
        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: 'jpeg' });
            ws.addImage(imageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 120, height: 100 },
            });
        } else {
            ws.getCell('A1').value = 'Golden Land';
            ws.getCell('A1').font = { name: 'Amiri', size: 16, bold: true };
        }

        const startRow = logoBuffer ? 5 : 3;

        // === TITLE ===
        const title = utils.rtlEmbed(
            getTranslatedLabel(
                'accounting.payments.report.daily.title',
                `${paymentType === 'incoming' ? 'Incoming' : 'Outgoing'} Payments - Today`
            )
        );
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:K${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(startRow).height = 40;

        // === HEADER ===
        const headerRowNum = startRow + 2;
        const headers = [
            getTranslatedLabel('accounting.payments.list.paymentId', 'Payment Number'),
            getTranslatedLabel('accounting.payments.list.paymentType', 'Payment Type'),
            getTranslatedLabel('accounting.payments.list.orderId', 'Order ID'),
            getTranslatedLabel('accounting.payments.list.certificateNumber', 'Certificate Number'),
            getTranslatedLabel('accounting.payments.list.from', 'From Party'),
            getTranslatedLabel('accounting.payments.list.to', 'To Party'),
            getTranslatedLabel('accounting.payments.list.date', 'Payment Date'),
            getTranslatedLabel('accounting.payments.list.status', 'Status'),
            getTranslatedLabel('accounting.payments.list.amount', 'Amount'),
            getTranslatedLabel('accounting.payments.list.comments', 'Comments'),
        ];

        ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

        // === DATA ROWS ===
        const dataStartRow = headerRowNum + 2;
        data.data.forEach((payment, idx) => {
            const row = ws.addRow([
                utils.rtlEmbed(utils.safeString(payment.paymentId)),
                utils.rtlEmbed(utils.safeString(payment.paymentTypeDescription)),
                utils.rtlEmbed(utils.safeString(payment.orderId ?? '')),
                utils.rtlEmbed(utils.safeString(payment.certificateNumber ?? '')),
                utils.rtlEmbed(utils.safeString(payment.partyIdFromName)),
                utils.rtlEmbed(utils.safeString(payment.partyIdToName)),
                utils.formatDate(payment.effectiveDate),
                utils.rtlEmbed(utils.safeString(payment.statusDescription)),
                utils.formatNumber(payment.amount),
                utils.rtlEmbed(utils.safeString(payment.comments ?? '')),
            ]);
            row.font = { name: 'Amiri', size: 10 };
            row.alignment = { horizontal: 'right', wrapText: true };
            row.height = 22;
        });

        // === TOTAL ROW ===
        if (data.data.length > 0) {
            const totalAmount = data.data.reduce((sum, p) => sum + (p.amount || 0), 0);
            const totalRowNum = dataStartRow + data.data.length;
            ws.addRow([
                '', '', '', '', '',
                utils.rtlEmbed(getTranslatedLabel('common.total', 'Total')),
                '', '', utils.formatNumber(totalAmount), ''
            ]);
            ws.mergeCells(`F${totalRowNum}:H${totalRowNum}`);
            ws.getRow(totalRowNum).font = { name: 'Amiri', size: 12, bold: true };
            ws.getCell(`I${totalRowNum}`).font = { bold: true };
        }

        // === COLUMN WIDTHS ===
        ws.columns = [
            { width: 15 }, { width: 22 }, { width: 15 }, { width: 20 },
            { width: 28 }, { width: 28 }, { width: 15 }, { width: 15 },
            { width: 16 }, { width: 35 }
        ];
        ws.getColumn(9).numFmt = '#,##0.00';

        return await workbook.xlsx.writeBuffer();
    }, [companyName, paymentType, getTranslatedLabel]);

    // REFACTOR: Full async flow with proper loading state
    // Purpose: Wait for API → then generate → then download
    // Improvement: No race conditions, clear UX, works every time
    const handleDownload = useCallback(async () => {
        setIsGenerating(true);
        try {
            const result = await trigger({ paymentType }).unwrap(); // ← waits for real data
            const buffer = await generateExcel(result);
            if (buffer) {
                const today = new Date().toISOString().split('T')[0];
                const blob = new Blob([buffer], {
                    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                });
                saveAs(blob, `${paymentType}_Daily_Payments_${today}.xlsx`);
            }
        } catch (err) {
            console.error('Excel generation failed:', err);
            alert('Failed to generate report. Please try again.');
        } finally {
            setIsGenerating(false);
        }
    }, [trigger, generateExcel, paymentType]);

    const isLoading = isFetching || isGenerating;

    return (
        <Button
            variant="contained"
            color="secondary"
            disabled={isLoading}
            onClick={handleDownload}
            sx={{ ml: 1 }}
        >
            {isLoading
                ? getTranslatedLabel('common.generating', 'Generating...')
                : getTranslatedLabel('accounting.payments.report.daily.excel', "Export Today's Payments")
            }
        </Button>
    );
};