// src/features/accounting/payment/report/PaymentExcelParty.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

/* ------------------------------------------------------------------ */
/* PAYMENT ROW – customer-facing only */
/* ------------------------------------------------------------------ */
interface PaymentRow {
    paymentId: string;
    paymentType: string;
    fromParty: string;
    toParty: string;
    amount: number;
    currency: string;
    effectiveDate: string;
    status: string;
    paymentMethod: string;
    chequeNumber?: string;
    chequeDate?: string;
    comments?: string;
}

/* ------------------------------------------------------------------ */
/* PROPS */
/* ------------------------------------------------------------------ */
interface PaymentExcelCustomerProps {
    companyName: string;
    payment: PaymentRow;
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

export const PaymentExcelParty: React.FC<PaymentExcelCustomerProps> = ({
                                                                           companyName,
                                                                           payment,
                                                                           getTranslatedLabel,
                                                                           isFetching = false,
                                                                       }) => {
    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        if (isFetching) return null;

        // --- FETCH LOGO ---
        let logoBuffer: ArrayBuffer | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) {
                logoBuffer = await (await resp.blob()).arrayBuffer();
                console.log('PaymentExcelParty: Logo fetched, size:', logoBuffer.byteLength);
            }
        } catch (e) {
            console.warn('Logo fetch error:', e);
        }

        const safeSheet = companyName
            ? companyName.replace(/[*\?\\:\[\]\/]/g, '_').trim().slice(0, 31)
            : 'Payment';
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
        ws.addRow([utils.rtlEmbed(getTranslatedLabel(
            'accounting.payments.report.customer.title',
            'بيان دفعة'
        ))]);
        ws.mergeCells(`A${startRow}:I${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 16, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.addRow([]); // spacer

        // === PAYMENT ID + STATUS ===
        const hdrRow = ws.lastRow!.number + 1;
        ws.addRow([utils.rtlEmbed(payment.paymentId)]);
        ws.mergeCells(`A${hdrRow}:D${hdrRow}`);
        ws.getCell(`A${hdrRow}`).font = { name: 'Amiri', size: 14, bold: true };
        ws.getCell(`A${hdrRow}`).alignment = { horizontal: 'right' };

        ws.mergeCells(`E${hdrRow}:I${hdrRow}`);
        const statusCell = ws.getCell(`E${hdrRow}`);
        statusCell.value = utils.rtlEmbed(payment.status);
        statusCell.font = { name: 'Amiri', size: 13, bold: true, color: { argb: 'FF000000' } };
        statusCell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFFFFF00' } };
        statusCell.alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(hdrRow).height = 30;
        ws.addRow([]); // spacer

        // === PARTIES ===
        const partyRow = ws.lastRow!.number + 1;
        ws.addRow([
            getTranslatedLabel('accounting.payments.form.from', 'من *'),
            utils.rtlEmbed(payment.fromParty),
            '',
            getTranslatedLabel('accounting.payments.form.to', 'إلى *'),
            utils.rtlEmbed(payment.toParty),
        ]);
        ws.mergeCells(`B${partyRow}:C${partyRow}`);
        ws.mergeCells(`E${partyRow}:F${partyRow}`);
        ws.getRow(partyRow).font = { name: 'Amiri', size: 11, bold: true };
        ws.getRow(partyRow).alignment = { horizontal: 'right' };
        ws.addRow([]); // spacer

        // === TYPE & METHOD ===
        const typeRow = ws.lastRow!.number + 1;
        ws.addRow([
            getTranslatedLabel('accounting.payments.form.paymentType', 'نوع الدفعة *'),
            utils.rtlEmbed(payment.paymentType),
            '',
            getTranslatedLabel('accounting.payments.form.paymentMethod', 'طريقة الدفع *'),
            utils.rtlEmbed(payment.paymentMethod),
        ]);
        ws.mergeCells(`B${typeRow}:C${typeRow}`);
        ws.mergeCells(`E${typeRow}:F${typeRow}`);
        ws.getRow(typeRow).font = { name: 'Amiri', size: 11, bold: true };
        ws.getRow(typeRow).alignment = { horizontal: 'right' };
        ws.addRow([]); // spacer

        // === CHEQUE (if applicable) ===
        if (payment.paymentMethod !== 'CASH' && payment.chequeNumber) {
            const chqRow = ws.lastRow!.number + 1;
            ws.addRow([
                getTranslatedLabel('accounting.payments.form.chequeNumber', 'رقم الشيك'),
                utils.rtlEmbed(payment.chequeNumber),
                '',
                getTranslatedLabel('accounting.payments.form.chequeDate', 'تاريخ الشيك'),
                payment.chequeDate ? utils.formatDate(payment.chequeDate) : '',
            ]);
            ws.mergeCells(`B${chqRow}:C${chqRow}`);
            ws.mergeCells(`E${chqRow}:F${chqRow}`);
            ws.getRow(chqRow).font = { name: 'Amiri', size: 11, bold: true };
            ws.getRow(chqRow).alignment = { horizontal: 'right' };
            ws.addRow([]); // spacer
        }

        // === AMOUNT & CURRENCY ===
        const amtRow = ws.lastRow!.number + 1;
        ws.addRow([
            getTranslatedLabel('accounting.payments.form.amount', 'المبلغ *'),
            utils.formatNumber(payment.amount),
            '',
            getTranslatedLabel('accounting.payments.form.currency', 'العملة'),
            payment.currency,
        ]);
        ws.mergeCells(`B${amtRow}:C${amtRow}`);
        ws.mergeCells(`E${amtRow}:F${amtRow}`);
        ws.getRow(amtRow).font = { name: 'Amiri', size: 11, bold: true };
        ws.getRow(amtRow).alignment = { horizontal: 'right' };
        ws.addRow([]); // spacer

        // === EFFECTIVE DATE ===
        const effRow = ws.lastRow!.number + 1;
        ws.addRow([
            getTranslatedLabel('accounting.payments.form.effectiveDate', 'تاريخ السريان *'),
            utils.formatDate(payment.effectiveDate),
        ]);
        ws.mergeCells(`B${effRow}:C${effRow}`);
        ws.getRow(effRow).font = { name: 'Amiri', size: 11, bold: true };
        ws.getRow(effRow).alignment = { horizontal: 'right' };
        ws.addRow([]); // spacer

        // === COMMENTS (if present) ===
        if (payment.comments) {
            const comRow = ws.lastRow!.number + 1;
            ws.addRow([getTranslatedLabel('accounting.payments.form.comments', 'البيان')]);
            ws.mergeCells(`A${comRow}:I${comRow}`);
            ws.getRow(comRow).font = { name: 'Amiri', size: 11, bold: true };
            ws.getRow(comRow).alignment = { horizontal: 'right' };

            ws.addRow([utils.rtlEmbed(payment.comments)]);
            ws.mergeCells(`A${ws.lastRow!.number}:I${ws.lastRow!.number}`);
            ws.getRow(ws.lastRow!.number).alignment = { horizontal: 'right', wrapText: true };
            ws.getRow(ws.lastRow!.number).height = 45;
        }

        // === COLUMN WIDTHS ===
        ws.columns = [
            { width: 22 }, // A
            { width: 28 }, // B
            { width: 5 },  // C
            { width: 22 }, // D
            { width: 28 }, // E
            { width: 5 },  // F
            { width: 5 },  // G
            { width: 5 },  // H
            { width: 5 },  // I
        ];
        ws.getColumn(2).numFmt = '#,##0.00';

        return await workbook.xlsx.writeBuffer();
    }, [companyName, payment, getTranslatedLabel, isFetching]);

    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            saveAs(blob, `${utils.rtlEmbed(payment.paymentId)}_Customer.xlsx`);
        }
    }, [generateExcel, payment.paymentId]);

    return (
        <Button
            variant="contained"
            color="success"
            disabled={isFetching}
            onClick={handleDownload}
            sx={{ mt: 2, mr: 1 }}
        >
            {getTranslatedLabel(
                'accounting.payments.report.customer.excel',
                'تصدير بيان الدفعة'
            )}
        </Button>
    );
};