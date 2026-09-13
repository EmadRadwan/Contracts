// src/features/accounting/organizationGlSettings/report/TrialBalanceByLevelExcel.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

// REFACTOR: Mirror TrialBalanceExcel pattern (same RTL/Amiri/logo/number-format flow), extended
// with a Level column and bold styling for roll-up (non-leaf) rows.
interface TrialBalanceByLevelRow {
    accountCode: string;
    accountName: string;
    level: number;
    isLeaf: boolean;
    openingBalance: number;
    postedDebits: number;
    postedCredits: number;
    endingBalance: number;
}

interface TrialBalanceByLevelExcelProps {
    companyName: string;
    rows: TrialBalanceByLevelRow[];
    totals: { postedDebitsTotal: number; postedCreditsTotal: number };
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
}

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `‫${t}` : t,
};

export const TrialBalanceByLevelExcel: React.FC<TrialBalanceByLevelExcelProps> = ({
                                                                        companyName,
                                                                        rows,
                                                                        totals,
                                                                        getTranslatedLabel,
                                                                        isFetching = false,
                                                                    }) => {
    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        if (!rows.length || isFetching) {
            console.warn('TrialBalanceByLevelExcel: no data or fetching');
            return null;
        }

        let logoId: number | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) {
                const buf = await (await resp.blob()).arrayBuffer();
                logoId = workbook.addImage({ buffer: buf, extension: 'jpeg' });
            }
        } catch (e) { console.warn('Logo failed', e); }

        const ws = workbook.addWorksheet(companyName.slice(0, 31) || 'Trial Balance By Level');
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.views = [{ rightToLeft: true }];
        ws.getColumn(1).font = { name: 'Amiri', size: 10 };

        if (logoId) {
            ws.addImage(logoId, { tl: { col: 0, row: 0 }, ext: { width: 120, height: 100 } });
            ws.getRow(1).height = 75; ws.getRow(2).height = 20; ws.getRow(3).height = 20;
            ws.addRow([]); ws.addRow([]); ws.addRow([]);
        } else {
            ws.addRow(['Logo Unavailable']).getCell(1).font = { color: { argb: 'FF0000' } };
        }

        const titleRow = logoId ? 4 : 2;
        ws.addRow([getTranslatedLabel('accounting.orgGL.reports.trial-balance-by-level.title', 'Trial Balance By Level') + ': ' + utils.rtlEmbed(utils.safeString(companyName))]);
        ws.mergeCells(`A${titleRow}:H${titleRow}`);
        ws.getRow(titleRow).font = { name: 'Amiri', size: 14, bold: true };
        ws.getRow(titleRow).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        ws.addRow([]); ws.addRow([]);

        const headers = [
            getTranslatedLabel('accounting.orgGL.reports.trial-balance-by-level.accountCode', 'Account Code'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance-by-level.accountName', 'Account Name'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance-by-level.level', 'Level'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance-by-level.openingBalance', 'Opening Balance'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance-by-level.postedDebits', 'Debit'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance-by-level.postedCredits', 'Credit'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance-by-level.endingBalance', 'Ending Balance'),
        ];
        ws.addRow(headers);
        const hRow = ws.getRow(ws.lastRow!.number);
        hRow.font = { name: 'Amiri', size: 10, bold: true };
        hRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        hRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        hRow.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

        ws.columns = [
            { width: 15 }, // code
            { width: 40 }, // name
            { width: 10 }, // level
            { width: 15 }, // opening
            { width: 15 }, // debit
            { width: 15 }, // credit
            { width: 15 }, // ending
        ];
        [4, 5, 6, 7].forEach(i => ws.getColumn(i).numFmt = '#,##0.00');

        rows.forEach(r => {
            const row = ws.addRow([
                utils.safeString(r.accountCode),
                utils.rtlEmbed(utils.safeString(r.accountName)),
                r.level,
                r.openingBalance,
                r.postedDebits,
                r.postedCredits,
                r.endingBalance,
            ]);
            row.font = { name: 'Amiri', size: 9, bold: !r.isLeaf };
            if (!r.isLeaf) {
                row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F5F5F5' } };
            }
            row.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
            row.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });
        });

        const totRow = ws.addRow([
            '', getTranslatedLabel('accounting.orgGL.reports.trial-balance-by-level.totals', 'Totals'),
            '', '', totals.postedDebitsTotal, totals.postedCreditsTotal, '',
        ]);
        totRow.font = { name: 'Amiri', size: 10, bold: true };
        totRow.getCell(5).numFmt = '#,##0.00';
        totRow.getCell(6).numFmt = '#,##0.00';

        return await workbook.xlsx.writeBuffer();
    }, [companyName, rows, totals, getTranslatedLabel, isFetching]);

    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            saveAs(blob, `TrialBalanceByLevel_${companyName || 'Report'}.xlsx`);
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
            {getTranslatedLabel('accounting.orgGL.reports.trial-balance-by-level.excel', 'Export Excel')}
        </Button>
    );
};
