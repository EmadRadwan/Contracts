import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

// Excel export of the per-account, month-by-month trial balance. Same RTL / Amiri / logo house
// style as the other accounting exports.
//
// The logo is tracked by its fetched buffer, not by the id workbook.addImage returns — that id is
// 0 for the first image, which is falsy. See the ExcelJS gotcha in client-app/CLAUDE.md.

export interface GlAccountTrialBalanceMonthRow {
    periodLabel: string;
    debitTotal: number;
    creditTotal: number;
    debitCreditDifference: number;
    totalOfYearToDateDebit: number;
    totalOfYearToDateCredit: number;
    balance: number;
    balanceOfTheAcctgForYear: number;
    transactionCount: number;
}

interface Props {
    companyName: string;
    accountCode: string;
    accountName: string;
    periodLabel: string;
    openingBalance: number;
    rows: GlAccountTrialBalanceMonthRow[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
}

const localizationKey = 'accounting.orgGL.reports.gl-trial-balance';

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `‫${t}` : t,
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

async function fetchLogoBuffer(): Promise<ArrayBuffer | null> {
    for (let attempt = 1; attempt <= 2; attempt++) {
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) return await (await resp.blob()).arrayBuffer();
            console.warn(`GlAccountTrialBalanceExcel: logo fetch returned ${resp.status} ${resp.statusText} (attempt ${attempt})`);
        } catch (e) {
            console.warn(`GlAccountTrialBalanceExcel: logo fetch threw (attempt ${attempt})`, e);
        }
    }
    return null;
}

export const GlAccountTrialBalanceExcel: React.FC<Props> = ({
    companyName,
    accountCode,
    accountName,
    periodLabel,
    openingBalance,
    rows,
    getTranslatedLabel,
    isFetching = false,
}) => {
    const generateExcel = useCallback(async () => {
        if (isFetching) {
            console.warn('GlAccountTrialBalanceExcel: data is still fetching');
            return null;
        }

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'Golden Land System';

        const ws = workbook.addWorksheet('GL Account Trial Balance', {
            views: [{ rightToLeft: true }],
            pageSetup: { paperSize: 9, orientation: 'landscape', fitToPage: true, fitToWidth: 1, fitToHeight: 0 },
        });

        ws.columns = [
            { width: 22 }, // Month
            { width: 18 }, // Debit
            { width: 18 }, // Credit
            { width: 18 }, // Difference
            { width: 18 }, // YTD debit
            { width: 18 }, // YTD credit
            { width: 18 }, // Month balance
            { width: 22 }, // Running balance
        ];

        const logoBuffer = await fetchLogoBuffer();
        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: 'jpeg' });
            ws.getRow(1).height = 90;
            ws.addImage(imageId, { tl: { col: 0, row: 0 }, ext: { width: 160, height: 115 } });
        } else {
            ws.getRow(1).height = 35;
        }

        let currentRow = logoBuffer ? 6 : 3;

        const titleRow = currentRow++;
        ws.getCell(`A${titleRow}`).value =
            `${getTranslatedLabel(`${localizationKey}.title`, 'Gl Account Trial Balance For: ')}${utils.rtlEmbed(utils.safeString(companyName))}`;
        ws.mergeCells(`A${titleRow}:H${titleRow}`);
        ws.getRow(titleRow).font = { name: 'Amiri', size: 16, bold: true };
        ws.getRow(titleRow).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        ws.getRow(titleRow).height = 45;

        const subRow = currentRow++;
        ws.getCell(`A${subRow}`).value =
            `${utils.safeString(accountCode)} - ${utils.rtlEmbed(utils.safeString(accountName))}` +
            (periodLabel ? `  |  ${utils.rtlEmbed(periodLabel)}` : '') +
            `  |  ${getTranslatedLabel(`${localizationKey}.generated-at`, 'Generated At')}: ${utils.formatDate(new Date())}`;
        ws.mergeCells(`A${subRow}:H${subRow}`);
        ws.getRow(subRow).font = { name: 'Amiri', size: 10, italic: true };
        ws.getRow(subRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(subRow).height = 28;

        currentRow += 1;

        const bordered = (row: ExcelJS.Row) => {
            row.eachCell((cell) => {
                cell.border = {
                    top: { style: 'thin' }, left: { style: 'thin' },
                    bottom: { style: 'thin' }, right: { style: 'thin' },
                };
            });
        };

        // Opening balance sits on its own line above the months — it is the base the running
        // balance column builds on, not a month of its own.
        const openingRow = ws.getRow(currentRow++);
        openingRow.getCell(1).value = getTranslatedLabel(`${localizationKey}.openingBalance`, 'Opening Balance');
        openingRow.getCell(8).value = openingBalance;
        openingRow.getCell(8).numFmt = '#,##0.00';
        openingRow.font = { name: 'Amiri', size: 11, bold: true };
        openingRow.alignment = { horizontal: 'right', vertical: 'middle' };
        bordered(openingRow);

        currentRow += 1;

        const headerRow = ws.getRow(currentRow++);
        headerRow.getCell(1).value = getTranslatedLabel(`${localizationKey}.month`, 'Month');
        headerRow.getCell(2).value = getTranslatedLabel(`${localizationKey}.debit`, 'Debit');
        headerRow.getCell(3).value = getTranslatedLabel(`${localizationKey}.credit`, 'Credit');
        headerRow.getCell(4).value = getTranslatedLabel(`${localizationKey}.difference`, 'Difference');
        headerRow.getCell(5).value = getTranslatedLabel(`${localizationKey}.ytdDebit`, 'YTD Debit');
        headerRow.getCell(6).value = getTranslatedLabel(`${localizationKey}.ytdCredit`, 'YTD Credit');
        headerRow.getCell(7).value = getTranslatedLabel(`${localizationKey}.monthBalance`, 'Month Balance');
        headerRow.getCell(8).value = getTranslatedLabel(`${localizationKey}.runningBalance`, 'Running Balance');
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };
        bordered(headerRow);

        rows.forEach((r) => {
            const row = ws.getRow(currentRow++);
            row.getCell(1).value = r.periodLabel;
            row.getCell(2).value = r.debitTotal;
            row.getCell(3).value = r.creditTotal;
            row.getCell(4).value = r.debitCreditDifference;
            row.getCell(5).value = r.totalOfYearToDateDebit;
            row.getCell(6).value = r.totalOfYearToDateCredit;
            row.getCell(7).value = r.balance;
            row.getCell(8).value = r.balanceOfTheAcctgForYear;
            row.font = { name: 'Amiri', size: 10 };
            row.alignment = { horizontal: 'right', vertical: 'middle' };
            for (let c = 2; c <= 8; c++) row.getCell(c).numFmt = '#,##0.00';
            bordered(row);
        });

        // Year totals. The debit/credit columns are summed; the closing balance is the last
        // month's running balance rather than a sum, since that column already accumulates.
        const last = rows[rows.length - 1];
        const totalRow = ws.getRow(currentRow++);
        totalRow.getCell(1).value = getTranslatedLabel(`${localizationKey}.yearTotal`, 'Year Total');
        totalRow.getCell(2).value = rows.reduce((sum, r) => sum + r.debitTotal, 0);
        totalRow.getCell(3).value = rows.reduce((sum, r) => sum + r.creditTotal, 0);
        totalRow.getCell(8).value = last ? last.balanceOfTheAcctgForYear : openingBalance;
        totalRow.font = { name: 'Amiri', size: 11, bold: true };
        totalRow.alignment = { horizontal: 'right', vertical: 'middle' };
        [2, 3, 8].forEach((c) => { totalRow.getCell(c).numFmt = '#,##0.00'; });
        bordered(totalRow);

        return await workbook.xlsx.writeBuffer();
    }, [companyName, accountCode, accountName, periodLabel, openingBalance, rows, getTranslatedLabel, isFetching]);

    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            saveAs(blob, `GlAccountTrialBalance_${(accountCode || 'account').replace(/[^a-z0-9]/gi, '_')}_${new Date().toISOString().slice(0, 10)}.xlsx`);
        }
    }, [generateExcel, accountCode]);

    return (
        <Button
            variant="outlined"
            color="primary"
            disabled={isFetching || rows.length === 0}
            onClick={handleDownload}
            sx={{ ml: 1 }}
        >
            {getTranslatedLabel(`${localizationKey}.excel`, 'Export Trial Balance')}
        </Button>
    );
};
