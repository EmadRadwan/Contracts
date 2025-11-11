// src/features/accounting/payment/report/PaymentsDailyExcel.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

/* ------------------------------------------------------------------ */
/* PROPS */
/* ------------------------------------------------------------------ */
interface PaymentsDailyExcelProps {
    paymentsData: { data: any[]; total: number }; // Assumes PaymentRow[] in data
    companyName: string;
    paymentType: 'incoming' | 'outgoing';
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
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

export const PaymentsDailyExcel: React.FC<PaymentsDailyExcelProps> = ({
                                                                          paymentsData,
                                                                          companyName,
                                                                          paymentType,
                                                                          getTranslatedLabel,
                                                                          isFetching = false,
                                                                      }) => {
    const localizationKey = "accounting.payments.list";
    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        if (isFetching || !paymentsData.data.length) return null;

        // --- FETCH LOGO ---
        let logoBuffer: ArrayBuffer | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) {
                logoBuffer = await (await resp.blob()).arrayBuffer();
                console.log('PaymentsDailyExcel: Logo fetched, size:', logoBuffer.byteLength);
            }
        } catch (e) {
            console.warn('Logo fetch error:', e);
        }

        const safeSheet = `${paymentType} Payments - ${new Date().toISOString().split('T')[0]}`.replace(/[*\?\\:\[\]\/]/g, '_').trim().slice(0, 31);
        const ws = workbook.addWorksheet(safeSheet);
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.views = [{ rightToLeft: true }];
        ws.getColumn(1).font = { name: 'Amiri', size: 10 };

        // === ADD LOGO ===
        if (logoBuffer) {
            const imageId = workbook.addImage({
                buffer: logoBuffer,
                extension: 'jpeg',
            });

            // REFACTOR: Initialize row 1 BEFORE addImage to avoid 'tl' undefined error
            // Purpose: ExcelJS requires the row to be accessed (getRow) before absolute image positioning
            // Improvement: Prevents crash by ensuring worksheet.model.rows[0] exists
            const logoRow = ws.getRow(1);
            logoRow.height = 75;

            ws.addImage(imageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 100, height: 100 },
            });

            // REFACTOR: Pre-initialize subsequent rows for consistent layout
            // Purpose: Avoids lazy row creation issues when setting height later
            // Improvement: Guarantees row model is ready before content
            ws.getRow(2).height = 20;
            ws.getRow(3).height = 20;

            // Add spacing rows (rows 2, 3, 4)
            ws.addRow([]); // row 2
            ws.addRow([]); // row 3
            ws.addRow([]); // row 4 → content starts at row 5
        } else {
            // REFACTOR: Use getRow(1) in fallback for consistency
            // Purpose: Maintains same row initialization pattern
            // Improvement: Prevents future bugs if logic expands
            const fallbackRow = ws.getRow(1);
            fallbackRow.getCell(1).value = 'Logo Unavailable';
            fallbackRow.font = { name: 'Amiri', size: 10, color: { argb: 'FFFF0000' } };
            fallbackRow.alignment = { horizontal: 'center', vertical: 'middle' };
        }

        const startRow = logoBuffer ? 5 : 2;

        // === TITLE ===
        const title = utils.rtlEmbed(getTranslatedLabel(
            'accounting.payments.report.daily.title',
            `${paymentType === 'incoming' ? 'Incoming' : 'Outgoing'} Payments - Today`
        ));
        ws.addRow([title]);
        ws.mergeCells(`A${startRow}:L${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 16, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.addRow([]); // spacer

        // === HEADER ROW ===
        const headerRow = ws.lastRow!.number + 1;
        const headers = [
            getTranslatedLabel(`${localizationKey}.paymentId`, "Payment Number"),
            getTranslatedLabel(`${localizationKey}.paymentType`, "Payment Type"),
            getTranslatedLabel(`${localizationKey}.orderId`, "Order ID"),
            getTranslatedLabel(`${localizationKey}.certificateNumber`, "Certificate Number"),
            getTranslatedLabel(`${localizationKey}.from`, "From Party"),
            getTranslatedLabel(`${localizationKey}.to`, "To Party"),
            getTranslatedLabel(`${localizationKey}.date`, "Payment Date"),
            getTranslatedLabel(`${localizationKey}.status`, "Status"),
            getTranslatedLabel(`${localizationKey}.amount`, "Amount"),
            getTranslatedLabel(`${localizationKey}.comments`, "Comments"),
        ];
        ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        ws.getRow(headerRow).font = { name: 'Amiri', size: 11, bold: true };
        ws.getRow(headerRow).fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        ws.getRow(headerRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.addRow([]); // spacer

        // === DATA ROWS ===
        const dataStartRow = ws.lastRow!.number + 1;
        paymentsData.data.forEach((payment, index) => {
            const rowNum = dataStartRow + index;
            ws.addRow([
                utils.rtlEmbed(utils.safeString(payment.paymentId)),
                utils.rtlEmbed(utils.safeString(payment.paymentTypeDescription)),
                utils.rtlEmbed(utils.safeString(payment.orderId)),
                utils.rtlEmbed(utils.safeString(payment.certificateNumber)),
                utils.rtlEmbed(utils.safeString(payment.partyIdFromName)),
                utils.rtlEmbed(utils.safeString(payment.partyIdToName)),
                utils.formatDate(payment.effectiveDate),
                utils.rtlEmbed(utils.safeString(payment.statusDescription)),
                utils.formatNumber(payment.amount),
                utils.rtlEmbed(utils.safeString(payment.comments)),
            ]);
            ws.getRow(rowNum).font = { name: 'Amiri', size: 10 };
            ws.getRow(rowNum).alignment = { horizontal: 'right', wrapText: true };
            ws.getRow(rowNum).height = 20;
        });

        // === TOTALS ROW (if data exists) ===
        if (paymentsData.data.length > 0) {
            const totalRow = dataStartRow + paymentsData.data.length + 1;
            const totalAmount = paymentsData.data.reduce((sum, p) => sum + (p.amount || 0), 0);
            ws.addRow([
                '',
                '',
                '',
                '',
                '',
                '',
                utils.rtlEmbed(getTranslatedLabel('common.total', 'Total')),
                '',
                utils.formatNumber(totalAmount),
                '',
            ]);
            ws.mergeCells(`G${totalRow}:H${totalRow}`);
            ws.getRow(totalRow).font = { name: 'Amiri', size: 11, bold: true };
            ws.getRow(totalRow).alignment = { horizontal: 'right' };
            ws.getCell(`I${totalRow}`).font = { bold: true };
        }

        // === COLUMN WIDTHS ===
        ws.columns = [
            { width: 15 }, // Payment ID
            { width: 20 }, // Type
            { width: 15 }, // Order ID
            { width: 20 }, // Certificate
            { width: 25 }, // From
            { width: 25 }, // To
            { width: 15 }, // Date
            { width: 15 }, // Status
            { width: 15 }, // Amount
            { width: 30 }, // Comments
        ];
        ws.getColumn(9).numFmt = '#,##0.00'; // Amount column

        return await workbook.xlsx.writeBuffer();
    }, [paymentsData, companyName, paymentType, getTranslatedLabel, isFetching]);

    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const today = new Date().toISOString().split('T')[0];
            const blob = new Blob([buf], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            saveAs(blob, `${paymentType}_Daily_Payments_${today}.xlsx`);
        }
    }, [generateExcel, paymentType]);

    return (
        <Button
            variant="contained"
            color="secondary"
            disabled={isFetching || !paymentsData.data.length}
            onClick={handleDownload}
            sx={{ ml: 1 }}
        >
            {getTranslatedLabel(
                'accounting.payments.report.daily.excel',
                'Export Today\'s Payments'
            )}
        </Button>
    );
};