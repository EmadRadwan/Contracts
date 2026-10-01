import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

// Excel export of the Cash Flow Statement's three sections (opening balances, period movements,
// closing balances) plus the totals block. Same RTL / Amiri / logo house style as the other
// accounting exports.
//
// Note the logo is tracked by its fetched buffer, not by the id workbook.addImage returns: that
// id is 0 for the first image, which is falsy, and `if (logoId)` would take the "no logo" branch
// on a perfectly successful fetch. See the ExcelJS gotcha in client-app/CLAUDE.md.

export interface CashFlowRow {
    glAccountId: string;
    accountCode: string;
    accountName: string;
    debit: number;
    credit: number;
    balance: number;
}

interface CashFlowStatementExcelProps {
    companyName: string;
    openingRows: CashFlowRow[];
    periodRows: CashFlowRow[];
    closingRows: CashFlowRow[];
    totals: {
        openingCashBalanceTotal?: number;
        periodCashBalanceTotal?: number;
        closingCashBalanceTotal?: number;
        endingCashBalanceTotal?: number;
    };
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
    fromDate?: string;
    thruDate?: string;
}

const localizationKey = 'accounting.orgGL.reports.cash-flow.list';

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
            console.warn(`CashFlowStatementExcel: logo fetch returned ${resp.status} ${resp.statusText} (attempt ${attempt})`);
        } catch (e) {
            console.warn(`CashFlowStatementExcel: logo fetch threw (attempt ${attempt})`, e);
        }
    }
    return null;
}

export const CashFlowStatementExcel: React.FC<CashFlowStatementExcelProps> = ({
    companyName,
    openingRows,
    periodRows,
    closingRows,
    totals,
    getTranslatedLabel,
    isFetching = false,
    fromDate,
    thruDate,
}) => {
    const generateExcel = useCallback(async () => {
        if (isFetching) {
            console.warn('CashFlowStatementExcel: data is still fetching');
            return null;
        }

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'Golden Land System';

        const ws = workbook.addWorksheet('Cash Flow', {
            views: [{ rightToLeft: true }],
            pageSetup: { paperSize: 9, orientation: 'portrait', fitToPage: true, fitToWidth: 1, fitToHeight: 0 },
        });

        ws.columns = [
            { width: 18 }, // Account code
            { width: 48 }, // Account name
            { width: 20 }, // Debit
            { width: 20 }, // Credit
            { width: 22 }, // Balance
        ];

        // ==================== LOGO ====================
        const logoBuffer = await fetchLogoBuffer();
        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: 'jpeg' });
            ws.getRow(1).height = 90;
            ws.addImage(imageId, { tl: { col: 0, row: 0 }, ext: { width: 160, height: 115 } });
        } else {
            ws.getRow(1).height = 35;
        }

        let currentRow = logoBuffer ? 6 : 3;

        // ==================== TITLE ====================
        const titleRow = currentRow++;
        ws.getCell(`A${titleRow}`).value =
            `${getTranslatedLabel(`${localizationKey}.title`, 'Cash Flow Statement For: ')}${utils.rtlEmbed(utils.safeString(companyName))}`;
        ws.mergeCells(`A${titleRow}:E${titleRow}`);
        ws.getRow(titleRow).font = { name: 'Amiri', size: 16, bold: true };
        ws.getRow(titleRow).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        ws.getRow(titleRow).height = 45;

        // ==================== DATE LINE ====================
        const dateRow = currentRow++;
        const range = fromDate || thruDate
            ? `(${utils.formatDate(fromDate)} - ${utils.formatDate(thruDate)}) - `
            : '';
        ws.getCell(`A${dateRow}`).value =
            `${range}${getTranslatedLabel(`${localizationKey}.generated-at`, 'Generated At')}: ${utils.formatDate(new Date())}`;
        ws.mergeCells(`A${dateRow}:E${dateRow}`);
        ws.getRow(dateRow).font = { name: 'Amiri', size: 10, italic: true };
        ws.getRow(dateRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(dateRow).height = 28;

        currentRow += 2;

        const bordered = (row: ExcelJS.Row) => {
            row.eachCell((cell) => {
                cell.border = {
                    top: { style: 'thin' }, left: { style: 'thin' },
                    bottom: { style: 'thin' }, right: { style: 'thin' },
                };
            });
        };

        const renderSection = (titleKey: string, defaultTitle: string, rows: CashFlowRow[], total?: number) => {
            const secTitleRow = currentRow++;
            ws.getCell(`A${secTitleRow}`).value = getTranslatedLabel(titleKey, defaultTitle);
            ws.mergeCells(`A${secTitleRow}:E${secTitleRow}`);
            ws.getRow(secTitleRow).font = { name: 'Amiri', size: 13, bold: true };
            ws.getRow(secTitleRow).alignment = { horizontal: 'center', vertical: 'middle' };

            const headerRow = ws.getRow(currentRow++);
            headerRow.getCell(1).value = getTranslatedLabel(`${localizationKey}.code`, 'Account Code');
            headerRow.getCell(2).value = getTranslatedLabel(`${localizationKey}.name`, 'Account Name');
            headerRow.getCell(3).value = getTranslatedLabel(`${localizationKey}.debit`, 'Debit');
            headerRow.getCell(4).value = getTranslatedLabel(`${localizationKey}.credit`, 'Credit');
            headerRow.getCell(5).value = getTranslatedLabel(`${localizationKey}.balance`, 'Balance');
            headerRow.font = { name: 'Amiri', size: 11, bold: true };
            headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
            headerRow.alignment = { horizontal: 'center', vertical: 'middle' };
            bordered(headerRow);

            rows.forEach((item) => {
                const row = ws.getRow(currentRow++);
                row.getCell(1).value = utils.safeString(item.accountCode);
                row.getCell(2).value = utils.rtlEmbed(utils.safeString(item.accountName));
                row.getCell(3).value = item.debit;
                row.getCell(4).value = item.credit;
                row.getCell(5).value = item.balance;
                row.font = { name: 'Amiri', size: 10 };
                row.alignment = { horizontal: 'right', vertical: 'middle' };
                [3, 4, 5].forEach((c) => { row.getCell(c).numFmt = '#,##0.00'; });
                bordered(row);
            });

            if (total !== undefined) {
                const totalRow = ws.getRow(currentRow++);
                totalRow.getCell(1).value = getTranslatedLabel(`${localizationKey}.section-total`, 'Section Total');
                totalRow.getCell(5).value = total;
                totalRow.getCell(5).numFmt = '#,##0.00';
                totalRow.font = { name: 'Amiri', size: 11, bold: true };
                totalRow.alignment = { horizontal: 'right', vertical: 'middle' };
                bordered(totalRow);
            }

            currentRow += 1;
        };

        renderSection(`${localizationKey}.opening`, 'Opening Cash Balances', openingRows, totals.openingCashBalanceTotal);
        renderSection(`${localizationKey}.period`, 'Cash Movements For The Period', periodRows, totals.periodCashBalanceTotal);
        renderSection(`${localizationKey}.closing`, 'Closing Cash Balances', closingRows, totals.closingCashBalanceTotal);

        // ==================== TOTALS ====================
        const totalsTitleRow = currentRow++;
        ws.getCell(`A${totalsTitleRow}`).value = getTranslatedLabel(`${localizationKey}.totals`, 'Totals');
        ws.mergeCells(`A${totalsTitleRow}:E${totalsTitleRow}`);
        ws.getRow(totalsTitleRow).font = { name: 'Amiri', size: 13, bold: true };
        ws.getRow(totalsTitleRow).alignment = { horizontal: 'center', vertical: 'middle' };

        const addTotalRow = (labelKey: string, defaultLabel: string, value?: number) => {
            const row = ws.getRow(currentRow++);
            row.getCell(1).value = getTranslatedLabel(labelKey, defaultLabel);
            row.getCell(5).value = value ?? 0;
            row.getCell(5).numFmt = '#,##0.00';
            row.font = { name: 'Amiri', size: 11, bold: true };
            row.alignment = { horizontal: 'right', vertical: 'middle' };
            bordered(row);
        };

        addTotalRow(`${localizationKey}.opening-total`, 'Opening Cash Balance', totals.openingCashBalanceTotal);
        addTotalRow(`${localizationKey}.period-total`, 'Net Cash Movement For The Period', totals.periodCashBalanceTotal);
        addTotalRow(`${localizationKey}.closing-total`, 'Closing Cash Balance', totals.closingCashBalanceTotal);
        addTotalRow(`${localizationKey}.ending-total`, 'Ending Cash Balance', totals.endingCashBalanceTotal);

        return await workbook.xlsx.writeBuffer();
    }, [companyName, openingRows, periodRows, closingRows, totals, getTranslatedLabel, isFetching, fromDate, thruDate]);

    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            saveAs(blob, `CashFlow_${companyName.replace(/[^a-z0-9]/gi, '_')}_${new Date().toISOString().slice(0, 10)}.xlsx`);
        }
    }, [generateExcel, companyName]);

    return (
        <Button
            variant="outlined"
            color="primary"
            disabled={isFetching}
            onClick={handleDownload}
            sx={{ ml: 1 }}
        >
            {getTranslatedLabel(`${localizationKey}.excel`, 'Export Cash Flow')}
        </Button>
    );
};
