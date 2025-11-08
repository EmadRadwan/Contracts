// src/features/accounting/report/TrialBalanceExcel.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

// REFACTOR: Mirror PartyFinancialHistoryExcel pattern
// Purpose: Consistent RTL, Amiri, logo, number formatting, final totals
// Improvement: Re-use proven 9-step flow, no duplication
// Context: Client wants identical “ميتال تك” style for all reports
interface TrialBalanceRow {
    accountCode: string;
    accountName: string;
    openingBalance: number;
    postedDebits: number;
    postedCredits: number;
    endingBalance: number;
}

interface TrialBalanceExcelProps {
    companyName: string;
    rows: TrialBalanceRow[];
    totals: { postedDebitsTotal: number; postedCreditsTotal: number };
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
}

// REFACTOR: Centralised safe/RTL helpers (same as PartyFinancialHistoryExcel)
const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? 'N/A' : v.toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }),
};

export const TrialBalanceExcel: React.FC<TrialBalanceExcelProps> = ({
                                                                        companyName,
                                                                        rows,
                                                                        totals,
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
            console.warn('TrialBalanceExcel: no data or fetching');
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

        const ws = workbook.addWorksheet(companyName.slice(0, 31) || 'Trial Balance');
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
        ws.addRow([getTranslatedLabel('accounting.orgGL.reports.trial-balance.title', 'Trial Balance') + ': ' + utils.rtlEmbed(utils.safeString(companyName))]);
        ws.mergeCells(`A${titleRow}:G${titleRow}`);
        ws.getRow(titleRow).font = { name: 'Amiri', size: 14, bold: true };
        ws.getRow(titleRow).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        ws.addRow([]); ws.addRow([]);

        // ---- headers ----------------------------------------------------
        const headers = [
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.accountCode', 'Account Code'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.accountName', 'Account Name'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.openingBalance', 'Opening Balance'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.postedDebits', 'Debit'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.postedCredits', 'Credit'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.endingBalance', 'Ending Balance'),
        ];
        ws.addRow(headers);
        const hRow = ws.getRow(ws.lastRow!.number);
        hRow.font = { name: 'Amiri', size: 10, bold: true };
        hRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        hRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        hRow.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

        // ---- column config -----------------------------------------------
        ws.columns = [
            { width: 15 }, // code
            { width: 35 }, // name
            { width: 15 }, // opening
            { width: 15 }, // debit
            { width: 15 }, // credit
            { width: 15 }, // ending
        ];
        [3, 4, 5, 6].forEach(i => ws.getColumn(i).numFmt = '#,##0.00');

        // ---- data rows --------------------------------------------------
        rows.forEach(r => {
            const row = ws.addRow([
                utils.safeString(r.accountCode),
                utils.rtlEmbed(utils.safeString(r.accountName)),
                r.openingBalance,
                r.postedDebits,
                r.postedCredits,
                r.endingBalance,
            ]);
            row.font = { name: 'Amiri', size: 9 };
            row.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
            row.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });
        });

        // ---- totals row -------------------------------------------------
        const totRow = ws.addRow([
            '', getTranslatedLabel('accounting.orgGL.reports.trial-balance.totals', 'Totals'),
            '', totals.postedDebitsTotal, totals.postedCreditsTotal, '',
        ]);
        totRow.font = { name: 'Amiri', size: 10, bold: true };
        totRow.getCell(4).numFmt = '#,##0.00';
        totRow.getCell(5).numFmt = '#,##0.00';

        return await workbook.xlsx.writeBuffer();
    }, [companyName, rows, totals, getTranslatedLabel, isFetching]);

    // ---- download ----------------------------------------------------
    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            saveAs(blob, `TrialBalance_${companyName || 'Report'}.xlsx`);
        }
    }, [generateExcel, companyName]);

    return (
        <Button
            variant="outlined"
            color="primary"
            disabled={isFetching}
            onClick={handleDownload}
            sx={{ mr: 1 }}
        >
            {getTranslatedLabel('accounting.orgGL.reports.trial-balance.excel', 'Export Trial Balance')}
        </Button>
    );
};