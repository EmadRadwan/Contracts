import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

// Excel export of the Transaction Totals report's three tabs (posted / unposted / all) as three
// sections of one sheet, so the whole report prints as a single document. Same RTL / Amiri /
// logo house style as the other accounting exports.
//
// The logo is tracked by its fetched buffer, not by the id workbook.addImage returns — that id is
// 0 for the first image, which is falsy. See the ExcelJS gotcha in client-app/CLAUDE.md.

export interface TransactionTotalsRow {
    glAccountId: string;
    accountCode: string;
    accountName: string;
    openingD: number;
    openingC: number;
    d: number;
    c: number;
    balance: number;
}

interface Props {
    companyName: string;
    postedRows: TransactionTotalsRow[];
    unpostedRows: TransactionTotalsRow[];
    allRows: TransactionTotalsRow[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
    fromDate?: string;
    thruDate?: string;
}

const localizationKey = 'accounting.orgGL.reports.transaction-totals.list';

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
            console.warn(`TransactionTotalsExcel: logo fetch returned ${resp.status} ${resp.statusText} (attempt ${attempt})`);
        } catch (e) {
            console.warn(`TransactionTotalsExcel: logo fetch threw (attempt ${attempt})`, e);
        }
    }
    return null;
}

export const TransactionTotalsExcel: React.FC<Props> = ({
    companyName,
    postedRows,
    unpostedRows,
    allRows,
    getTranslatedLabel,
    isFetching = false,
    fromDate,
    thruDate,
}) => {
    const generateExcel = useCallback(async () => {
        if (isFetching) {
            console.warn('TransactionTotalsExcel: data is still fetching');
            return null;
        }

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'Golden Land System';

        const ws = workbook.addWorksheet('Transaction Totals', {
            views: [{ rightToLeft: true }],
            pageSetup: { paperSize: 9, orientation: 'landscape', fitToPage: true, fitToWidth: 1, fitToHeight: 0 },
        });

        ws.columns = [
            { width: 18 }, // code
            { width: 44 }, // name
            { width: 18 }, // opening D
            { width: 18 }, // opening C
            { width: 18 }, // debit
            { width: 18 }, // credit
            { width: 20 }, // balance
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
            `${getTranslatedLabel(`${localizationKey}.title`, 'Transaction Totals For: ')}${utils.rtlEmbed(utils.safeString(companyName))}`;
        ws.mergeCells(`A${titleRow}:G${titleRow}`);
        ws.getRow(titleRow).font = { name: 'Amiri', size: 16, bold: true };
        ws.getRow(titleRow).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        ws.getRow(titleRow).height = 45;

        const dateRow = currentRow++;
        const range = fromDate || thruDate
            ? `(${utils.formatDate(fromDate)} - ${utils.formatDate(thruDate)}) - `
            : '';
        ws.getCell(`A${dateRow}`).value =
            `${range}${getTranslatedLabel(`${localizationKey}.generated-at`, 'Generated At')}: ${utils.formatDate(new Date())}`;
        ws.mergeCells(`A${dateRow}:G${dateRow}`);
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

        const renderSection = (titleKey: string, defaultTitle: string, rows: TransactionTotalsRow[]) => {
            const secTitleRow = currentRow++;
            ws.getCell(`A${secTitleRow}`).value = getTranslatedLabel(titleKey, defaultTitle);
            ws.mergeCells(`A${secTitleRow}:G${secTitleRow}`);
            ws.getRow(secTitleRow).font = { name: 'Amiri', size: 13, bold: true };
            ws.getRow(secTitleRow).alignment = { horizontal: 'center', vertical: 'middle' };

            const headerRow = ws.getRow(currentRow++);
            headerRow.getCell(1).value = getTranslatedLabel(`${localizationKey}.code`, 'Account Code');
            headerRow.getCell(2).value = getTranslatedLabel(`${localizationKey}.name`, 'Account Name');
            headerRow.getCell(3).value = getTranslatedLabel(`${localizationKey}.openingD`, 'Opening D');
            headerRow.getCell(4).value = getTranslatedLabel(`${localizationKey}.openingC`, 'Opening C');
            headerRow.getCell(5).value = getTranslatedLabel(`${localizationKey}.debit`, 'DR');
            headerRow.getCell(6).value = getTranslatedLabel(`${localizationKey}.credit`, 'CR');
            headerRow.getCell(7).value = getTranslatedLabel(`${localizationKey}.balance`, 'Balance');
            headerRow.font = { name: 'Amiri', size: 11, bold: true };
            headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
            headerRow.alignment = { horizontal: 'center', vertical: 'middle' };
            bordered(headerRow);

            rows.forEach((r) => {
                const row = ws.getRow(currentRow++);
                row.getCell(1).value = utils.safeString(r.accountCode);
                row.getCell(2).value = utils.rtlEmbed(utils.safeString(r.accountName));
                row.getCell(3).value = r.openingD;
                row.getCell(4).value = r.openingC;
                row.getCell(5).value = r.d;
                row.getCell(6).value = r.c;
                row.getCell(7).value = r.balance;
                row.font = { name: 'Amiri', size: 10 };
                row.alignment = { horizontal: 'right', vertical: 'middle' };
                for (let col = 3; col <= 7; col++) row.getCell(col).numFmt = '#,##0.00';
                bordered(row);
            });

            const totalRow = ws.getRow(currentRow++);
            totalRow.getCell(1).value = getTranslatedLabel(`${localizationKey}.sectionTotal`, 'Total');
            totalRow.getCell(5).value = rows.reduce((sum, r) => sum + (r.d ?? 0), 0);
            totalRow.getCell(6).value = rows.reduce((sum, r) => sum + (r.c ?? 0), 0);
            totalRow.font = { name: 'Amiri', size: 11, bold: true };
            totalRow.alignment = { horizontal: 'right', vertical: 'middle' };
            [5, 6].forEach((col) => { totalRow.getCell(col).numFmt = '#,##0.00'; });
            bordered(totalRow);

            currentRow += 1;
        };

        renderSection(`${localizationKey}.posted`, 'Posted Totals', postedRows);
        renderSection(`${localizationKey}.unposted`, 'Unposted Totals', unpostedRows);
        renderSection(`${localizationKey}.all`, 'All Totals', allRows);

        return await workbook.xlsx.writeBuffer();
    }, [companyName, postedRows, unpostedRows, allRows, getTranslatedLabel, isFetching, fromDate, thruDate]);

    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            saveAs(blob, `TransactionTotals_${companyName.replace(/[^a-z0-9]/gi, '_')}_${new Date().toISOString().slice(0, 10)}.xlsx`);
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
            {getTranslatedLabel(`${localizationKey}.excel`, 'Export Transaction Totals')}
        </Button>
    );
};
