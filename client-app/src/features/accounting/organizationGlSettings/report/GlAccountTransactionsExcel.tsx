// src/features/accounting/report/GlAccountTransactionsExcel.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

// REFACTOR: Mirror PartyFinancialHistoryExcel / TrialBalanceExcel pattern
// Purpose: Consistent RTL, Amiri, logo, number formatting, final totals
// Improvement: Re‑use proven 9‑step flow, no duplication
// Context: Client wants identical “ميتال تك” style for all reports
interface TransactionRow {
    acctgTransId: string;
    acctgTransEntrySeqId: string;
    transactionDate: string;          // ISO string – will be formatted in Excel
    acctgTransTypeId: string;
    glFiscalTypeId: string;
    invoiceId?: string;
    paymentId?: string;
    workEffortId?: string;
    partyName?: string;
    productName?: string;
    isPosted: boolean;
    postedDate?: string;
    debitCreditFlag: 'D' | 'C';
    amount: number;
    description?: string;
    projectName?: string;
}

interface GlAccountTransactionsExcelProps {
    accountCode: string;
    accountName: string;
    openingBalance: number;
    postedDebits: number;
    postedCredits: number;
    endingBalance: number;
    rows: TransactionRow[];
    totalDebit: number;
    totalCredit: number;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
}

// REFACTOR: Centralised safe/RTL helpers (same as previous reports)
const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
};

export const GlAccountTransactionsExcel: React.FC<GlAccountTransactionsExcelProps> = ({
                                                                                          accountCode,
                                                                                          accountName,
                                                                                          openingBalance,
                                                                                          postedDebits,
                                                                                          postedCredits,
                                                                                          endingBalance,
                                                                                          rows,
                                                                                          totalDebit,
                                                                                          totalCredit,
                                                                                          getTranslatedLabel,
                                                                                          isFetching = false,
                                                                                      }) => {
    // REFACTOR: Memoise generation – stable reference
    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        // ---- early exit -------------------------------------------------
        if (!rows.length || isFetching) {
            console.warn('GlAccountTransactionsExcel: no data or fetching');
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

        const ws = workbook.addWorksheet(accountCode.slice(0, 31) || 'Transactions');
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.views = [{ rightToLeft: true }];
        ws.getColumn(1).font = { name: 'Amiri', size: 10 };

        // ---- logo rows --------------------------------------------------
        if (logoId) {
            ws.addImage(logoId, { tl: { col: 0, row: 0 }, ext: { width: 100, height: 100 }, editAs: 'absolute' });
            ws.getRow(1).height = 75; ws.getRow(2).height = 20; ws.getRow(3).height = 20;
            ws.addRow([]); ws.addRow([]); ws.addRow([]);
        } else {
            ws.addRow(['Logo Unavailable']).getCell(1).font = { color: { argb: 'FF0000' } };
        }

        // ---- title ------------------------------------------------------
        const titleRow = logoId ? 4 : 2;
        ws.addRow([`${getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.title', 'Transaction Details')} – ${utils.rtlEmbed(utils.safeString(accountName))} (${accountCode})`]);
        ws.mergeCells(`A${titleRow}:O${titleRow}`);
        ws.getRow(titleRow).font = { name: 'Amiri', size: 14, bold: true };
        ws.getRow(titleRow).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        ws.addRow([]); ws.addRow([]);

        // ---- account summary --------------------------------------------
        const summaryStart = ws.lastRow!.number + 1;
        ws.addRow([getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.openingBalance', 'Opening Balance'), openingBalance]);
        ws.addRow([getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.postedDebits', 'Posted Debits'), postedDebits]);
        ws.addRow([getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.postedCredits', 'Posted Credits'), postedCredits]);
        ws.addRow([getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.endingBalance', 'Ending Balance'), endingBalance]);
        ws.getCell(`A${summaryStart}`).font = { bold: true };
        ws.getCell(`A${summaryStart + 1}`).font = { bold: true };
        ws.getCell(`A${summaryStart + 2}`).font = { bold: true };
        ws.getCell(`A${summaryStart + 3}`).font = { bold: true };
        ws.getColumn(2).numFmt = '#,##0.00';
        ws.addRow([]); // empty line

        // ---- table headers -----------------------------------------------
        const headers = [
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.transId', 'Acctg Trans ID'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.transEntrySeqId', 'Entry ID'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.transDate', 'Transaction Date'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.transType', 'Acctg Trans Type'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.fiscalType', 'Fiscal GL Type'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.invoiceId', 'Invoice ID'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.paymentId', 'Payment ID'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.workEffortId', 'Work Effort ID'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.projectName', 'Project Name'),   // ← new
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.partyId', 'Party Name'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.productId', 'Product Name'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.isPosted', 'Is Posted'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.postedDate', 'Posted Date'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.debitCredit', 'Debit/Credit'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.amount', 'Amount'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.description', 'Description'),
        ];
        ws.addRow(headers);
        const hRow = ws.getRow(ws.lastRow!.number);
        hRow.font = { name: 'Amiri', size: 10, bold: true };
        hRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        hRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        hRow.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

        // ---- column config -----------------------------------------------
        ws.columns = [
            { width: 14 }, // transId
            { width: 12 }, // entryId
            { width: 18 }, // date
            { width: 16 }, // trans type
            { width: 14 }, // fiscal
            { width: 12 }, // invoice
            { width: 12 }, // payment
            { width: 12 }, // work effort
            { width: 28 },
            { width: 18 }, // party
            { width: 18 }, // product
            { width: 10 }, // posted
            { width: 18 }, // posted date
            { width: 10 }, // D/C
            { width: 14 }, // amount
            { width: 30 }, // description
        ];
        ws.getColumn(14).numFmt = '#,##0.00';

        // ---- data rows --------------------------------------------------
        rows.forEach(r => {
            const row = ws.addRow([
                utils.safeString(r.acctgTransId),
                utils.safeString(r.acctgTransEntrySeqId),
                r.transactionDate,
                utils.safeString(r.acctgTransTypeId),
                utils.safeString(r.glFiscalTypeId),
                utils.safeString(r.invoiceId),
                utils.safeString(r.paymentId),
                utils.safeString(r.workEffortId),
                utils.rtlEmbed(utils.safeString(r.projectName)), 
                utils.rtlEmbed(utils.safeString(r.partyName)),
                utils.rtlEmbed(utils.safeString(r.productName)),
                r.isPosted ? 'Yes' : 'No',
                r.postedDate ?? '',
                r.debitCreditFlag,
                r.amount,
                utils.rtlEmbed(utils.safeString(r.description)),
            ]);
            row.font = { name: 'Amiri', size: 9 };
            row.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
            row.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });
        });

        // ---- totals row -------------------------------------------------
        const totRow = ws.addRow([
            '', '', '', '', '', '', '', '', '',
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.totalDebit', 'Total Debit'),
            totalDebit,
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.totalCredit', 'Total Credit'),
            totalCredit,
            '', ''
        ]);
        totRow.font = { name: 'Amiri', size: 10, bold: true };
        totRow.getCell(11).numFmt = '#,##0.00';
        totRow.getCell(13).numFmt = '#,##0.00';

        return await workbook.xlsx.writeBuffer();
    }, [
        accountCode, accountName, openingBalance, postedDebits, postedCredits, endingBalance,
        rows, totalDebit, totalCredit, getTranslatedLabel, isFetching,
    ]);

    // ---- download ----------------------------------------------------
    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            const fileName = `Transactions_${accountCode}_${accountName.replace(/[^a-z0-9]/gi, '_')}.xlsx`;
            saveAs(blob, fileName);
        }
    }, [generateExcel, accountCode, accountName]);

    return (
        <Button
            variant="outlined"
            color="primary"
            disabled={isFetching}
            onClick={handleDownload}
            sx={{ ml: 1 }}
        >
            {getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.excel', 'Export Transactions')}
        </Button>
    );
};