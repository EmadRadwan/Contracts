// src/features/accounting/payment/report/PaymentExcel.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';


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
    costCenter?: string;
    project?: string;
}

interface PaymentApplicationRow {
    invoiceId?: string;
    toPaymentId?: string;
    billingAccountId?: string;
    taxAuthGeoId?: string;
    amountApplied: number;
}

interface AcctgTransEntryRow {
    acctgTransId: string;
    acctgTransEntrySeqId: string;
    glAccountId: string;
    glAccountName: string;
    debitCreditFlag: 'D' | 'C';
    origAmount: number;
    currency: string;
    transactionDate: string;
}

interface PaymentExcelProps {
    companyName: string;
    payment: PaymentRow;
    applications: PaymentApplicationRow[];
    transactions: AcctgTransEntryRow[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
}

// REFACTOR: Centralised safe/RTL helpers
const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? 'N/A' : v.toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) =>
        d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

export const PaymentExcelTechnical: React.FC<PaymentExcelProps> = ({
                                                              companyName,
                                                              payment,
                                                              applications,
                                                              transactions,
                                                              getTranslatedLabel,
                                                              isFetching = false,
                                                          }) => {
    // REFACTOR: Memoise generation – stable reference
    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        // ---- early exit -------------------------------------------------
        if (isFetching) {
            console.warn('PaymentExcel: fetching in progress');
            return null;
        }

        // ---- logo -------------------------------------------------------
        let logoId: number | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) {
                const buf = await (await resp.blob()).arrayBuffer();
                logoId = workbook.addImage({ buffer: buf, extension: 'jpeg' });
            }
        } catch (e) { console.warn('Logo failed', e); }

        const safeSheetName = companyName
            ? companyName.replace(/[*\?\\:\[\]\/]/g, '_').trim().slice(0, 31)
            : 'Payment Report';
        const ws = workbook.addWorksheet(safeSheetName);
        
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.views = [{ rightToLeft: true }];
        ws.getColumn(1).font = { name: 'Amiri', size: 10 };

        // ---- logo rows --------------------------------------------------
        const startRow = logoId ? 4 : 2;
        if (logoId) {
            ws.addImage(logoId, { tl: { col: 0, row: 0 }, ext: { width: 100, height: 100 }, editAs: 'absolute' });
            ws.getRow(1).height = 75; ws.getRow(2).height = 20; ws.getRow(3).height = 20;
            ws.addRow([]); ws.addRow([]); ws.addRow([]);
        } else {
            ws.addRow(['Logo Unavailable']).getCell(1).font = { color: { argb: 'FF0000' } };
        }

        // ---- title ------------------------------------------------------
        ws.addRow([getTranslatedLabel('accounting.payments.report.title', 'Payment Report') + ': ' + utils.rtlEmbed(utils.safeString(companyName))]);
        ws.mergeCells(`A${startRow}:K${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 14, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        ws.addRow([]); ws.addRow([]);

        // ---- Payment Summary ------------------------------------------------
        const summaryStart = ws.lastRow!.number + 1;
        ws.addRow([
            getTranslatedLabel('accounting.payments.report.paymentId', 'Payment ID'),
            getTranslatedLabel('accounting.payments.report.paymentType', 'Type'),
            getTranslatedLabel('accounting.payments.report.from', 'From'),
            getTranslatedLabel('accounting.payments.report.to', 'To'),
            getTranslatedLabel('accounting.payments.report.amount', 'Amount'),
            getTranslatedLabel('accounting.payments.report.currency', 'Currency'),
            getTranslatedLabel('accounting.payments.report.date', 'Date'),
            getTranslatedLabel('accounting.payments.report.status', 'Status'),
            getTranslatedLabel('accounting.payments.report.method', 'Method'),
            getTranslatedLabel('accounting.payments.report.cheque', 'Cheque #'),
            getTranslatedLabel('accounting.payments.report.chequeDate', 'Cheque Date'),
            getTranslatedLabel('accounting.payments.form.costCenter', 'Cost Center'),
            getTranslatedLabel('projects.certificate.form.project', 'Project'),
        ]);
        const headerRow = ws.getRow(ws.lastRow!.number);
        headerRow.font = { name: 'Amiri', size: 10, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        headerRow.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

        ws.addRow([
            payment.paymentId,
            payment.paymentType,
            utils.rtlEmbed(payment.fromParty),
            utils.rtlEmbed(payment.toParty),
            payment.amount,
            payment.currency,
            utils.formatDate(payment.effectiveDate),
            payment.status,
            payment.paymentMethod,
            payment.chequeNumber || '',
            payment.chequeDate ? utils.formatDate(payment.chequeDate) : '',
            utils.rtlEmbed(payment.costCenter || 'غير محدد'),
            utils.rtlEmbed(payment.project || 'غير محدد'),
        ]);
        ws.getRow(ws.lastRow!.number).font = { name: 'Amiri', size: 9 };
        ws.getRow(ws.lastRow!.number).alignment = { horizontal: 'right', vertical: 'middle' };

        ws.addRow([]); // spacer

        // ---- Applications ---------------------------------------------------
        if (applications.length > 0) {
            const appStart = ws.lastRow!.number + 1;
            ws.addRow([getTranslatedLabel('accounting.payments.report.applications', 'Payment Applications')]);
            ws.mergeCells(`A${appStart}:F${appStart}`);
            ws.getRow(appStart).font = { name: 'Amiri', size: 12, bold: true };
            ws.getRow(appStart).alignment = { horizontal: 'center' };

            ws.addRow([
                getTranslatedLabel('accounting.payments.report.invoice', 'Invoice'),
                getTranslatedLabel('accounting.payments.report.payment', 'Payment'),
                getTranslatedLabel('accounting.payments.report.billing', 'Billing'),
                getTranslatedLabel('accounting.payments.report.tax', 'Tax'),
                getTranslatedLabel('accounting.payments.report.amountApplied', 'Amount Applied'),
            ]);
            const appHeader = ws.getRow(ws.lastRow!.number);
            appHeader.font = { name: 'Amiri', size: 10, bold: true };
            appHeader.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'E6F3FF' } };
            appHeader.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

            applications.forEach(app => {
                ws.addRow([
                    app.invoiceId || '',
                    app.toPaymentId || '',
                    app.billingAccountId || '',
                    app.taxAuthGeoId || '',
                    app.amountApplied,
                ]);
            });
            ws.getRow(ws.lastRow!.number).eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });
            ws.addRow([]);
        }

        // ---- Transactions ---------------------------------------------------
        if (transactions.length > 0) {
            const transStart = ws.lastRow!.number + 1;
            ws.addRow([getTranslatedLabel('accounting.payments.report.transactions', 'Accounting Transactions')]);
            ws.mergeCells(`A${transStart}:H${transStart}`);
            ws.getRow(transStart).font = { name: 'Amiri', size: 12, bold: true };
            ws.getRow(transStart).alignment = { horizontal: 'center' };

            ws.addRow([
                getTranslatedLabel('accounting.payments.report.transId', 'Trans ID'),
                getTranslatedLabel('accounting.payments.report.seq', 'Seq'),
                getTranslatedLabel('accounting.payments.report.account', 'GL Account'),
                getTranslatedLabel('accounting.payments.report.name', 'Account Name'),
                getTranslatedLabel('accounting.payments.report.dc', 'D/C'),
                getTranslatedLabel('accounting.payments.report.amount', 'Amount'),
                getTranslatedLabel('accounting.payments.report.currency', 'Currency'),
                getTranslatedLabel('accounting.payments.report.date', 'Date'),
            ]);
            const transHeader = ws.getRow(ws.lastRow!.number);
            transHeader.font = { name: 'Amiri', size: 10, bold: true };
            transHeader.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFF2CC' } };
            transHeader.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

            transactions.forEach(t => {
                ws.addRow([
                    t.acctgTransId,
                    t.acctgTransEntrySeqId,
                    t.glAccountId,
                    utils.rtlEmbed(t.glAccountName),
                    t.debitCreditFlag,
                    t.origAmount,
                    t.currency,
                    utils.formatDate(t.transactionDate),
                ]);
            });
            ws.getRow(ws.lastRow!.number).eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });
        }

        // ---- Column config -----------------------------------------------
        ws.columns = [
            { width: 15 }, // ID
            { width: 18 }, // Type / Seq
            { width: 25 }, // Party / Account ID
            { width: 30 }, // Name
            { width: 12 }, // Amount / D/C
            { width: 12 }, // Currency
            { width: 15 }, // Date
            { width: 15 }, // Status / Method
            { width: 15 }, // Cheque
            { width: 15 }, // Cheque Date
            { width: 20 }, // Cost Center (NEW)
            { width: 25 }, // Project (NEW)
        ];
        [5].forEach(i => ws.getColumn(i).numFmt = '#,##0.00');

        return await workbook.xlsx.writeBuffer();
    }, [companyName, payment, applications, transactions, getTranslatedLabel, isFetching]);

    // ---- download ----------------------------------------------------
    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            saveAs(blob, `Payment_${payment.paymentId || 'Report'}.xlsx`);
        }
    }, [generateExcel, payment.paymentId]);

    return (
        <Button
            variant="contained"     // REFACTOR: Match form's primary action buttons (contained)
            color="success"         // REFACTOR: Use success color to indicate "export/download" action
            disabled={isFetching}
            onClick={handleDownload}
            sx={{ mt: 2, mr: 1 }}   // REFACTOR: Match margin from Update/Cancel buttons
        >
            {getTranslatedLabel('accounting.payments.report.excel', 'Export Payment Report')}
        </Button>
    );
};