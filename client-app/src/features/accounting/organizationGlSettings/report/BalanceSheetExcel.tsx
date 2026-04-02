import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

interface AccountBalanceRow {
    glAccountId: string;
    accountCode: string;
    accountName: string;
    balance: number;
}

interface BalanceSheetExcelProps {
    companyName: string;
    assetAccountBalanceList: AccountBalanceRow[];
    liabilityAccountBalanceList: AccountBalanceRow[];
    equityAccountBalanceList: AccountBalanceRow[];
    totals: {
        currentAssetBalanceTotal?: number;
        longtermAssetBalanceTotal?: number;
        accumDepreciationBalanceTotal?: number;
        assetBalanceTotal?: number;
        currentLiabilityBalanceTotal?: number;
        equityBalanceTotal?: number;
        liabilityEquityBalanceTotal?: number;
    };
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
    thruDate?: string;
}

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

export const BalanceSheetExcel: React.FC<BalanceSheetExcelProps> = ({
                                                                        companyName,
                                                                        assetAccountBalanceList,
                                                                        liabilityAccountBalanceList,
                                                                        equityAccountBalanceList,
                                                                        totals,
                                                                        getTranslatedLabel,
                                                                        isFetching = false,
                                                                        thruDate,
                                                                    }) => {
    const generateExcel = useCallback(async () => {
        if (isFetching) {
            console.warn('BalanceSheetExcel: data is still fetching');
            return null;
        }

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'Golden Land System';

        const ws = workbook.addWorksheet('Balance Sheet', {
            views: [{ rightToLeft: true }],
            pageSetup: {
                paperSize: 9,
                orientation: 'portrait',
                fitToPage: true,
                fitToWidth: 1,
                fitToHeight: 0
            }
        });

        // ==================== LOGO ====================
        let logoId: number | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) {
                const buf = await (await resp.blob()).arrayBuffer();
                logoId = workbook.addImage({
                    buffer: buf,
                    extension: 'jpeg'
                });

                // Place logo at top-left with proper height
                ws.getRow(1).height = 90;
                ws.addImage(logoId, {
                    tl: { col: 0, row: 0 },
                    ext: { width: 160, height: 115 },
                });
            }
        } catch (e) {
            console.warn('Logo failed to load:', e);
            ws.getRow(1).height = 35;
        }

        let currentRow = logoId ? 6 : 3;   // Start title after logo space

        // ==================== TITLE ====================
        const titleRow = currentRow++;
        const reportTitle = `${getTranslatedLabel(
            'accounting.orgGL.reports.balance-sheet.list.title-for',
            'Balance Sheet For'
        )}: ${utils.rtlEmbed(utils.safeString(companyName))}`;

        ws.getCell(`A${titleRow}`).value = reportTitle;
        ws.mergeCells(`A${titleRow}:C${titleRow}`);
        ws.getRow(titleRow).font = { name: 'Amiri', size: 16, bold: true };
        ws.getRow(titleRow).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        ws.getRow(titleRow).height = 45;

        // ==================== DATE LINE ====================
        const dateRow = currentRow++;
        const dateSubTitle = thruDate
            ? `(${getTranslatedLabel('accounting.orgGL.reports.balance-sheet.list.thru-date', 'Thru Date')}: ${utils.formatDate(thruDate)})`
            : '';
        const genDate = ` - ${getTranslatedLabel('accounting.orgGL.reports.balance-sheet.list.generated-at', 'Generated At')}: ${utils.formatDate(new Date())}`;

        ws.getCell(`A${dateRow}`).value = `${dateSubTitle}${genDate}`;
        ws.mergeCells(`A${dateRow}:C${dateRow}`);
        ws.getRow(dateRow).font = { name: 'Amiri', size: 10, italic: true };
        ws.getRow(dateRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(dateRow).height = 28;

        currentRow += 2; // Extra spacing

        // ==================== COLUMN WIDTHS ====================
        ws.columns = [
            { width: 18 }, // Code
            { width: 48 }, // Name
            { width: 25 }, // Balance
        ];
        ws.getColumn(3).numFmt = '#,##0.00';

        // ==================== HELPER: RENDER SECTION ====================
        const renderSection = (titleKey: string, defaultTitle: string, data: AccountBalanceRow[]) => {
            // Section Title
            const secTitleRow = currentRow++;
            ws.getCell(`A${secTitleRow}`).value = getTranslatedLabel(titleKey, defaultTitle);
            ws.mergeCells(`A${secTitleRow}:C${secTitleRow}`);
            ws.getRow(secTitleRow).font = { name: 'Amiri', size: 13, bold: true };
            ws.getRow(secTitleRow).alignment = { horizontal: 'center', vertical: 'middle' };

            // Header Row
            const headerRow = currentRow++;
            const headerCells = ws.getRow(headerRow);
            headerCells.getCell(1).value = getTranslatedLabel('accounting.orgGL.reports.balance-sheet.list.code', 'Account Code');
            headerCells.getCell(2).value = getTranslatedLabel('accounting.orgGL.reports.balance-sheet.list.name', 'Account Name');
            headerCells.getCell(3).value = getTranslatedLabel('accounting.orgGL.reports.balance-sheet.list.balance', 'Balance');

            headerCells.font = { name: 'Amiri', size: 11, bold: true };
            headerCells.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
            headerCells.alignment = { horizontal: 'center', vertical: 'middle' };

            headerCells.eachCell((cell) => {
                cell.border = {
                    top: { style: 'thin' },
                    left: { style: 'thin' },
                    bottom: { style: 'thin' },
                    right: { style: 'thin' },
                };
            });

            // Data Rows
            data.forEach((item) => {
                const rowNum = currentRow++;
                const row = ws.getRow(rowNum);
                row.getCell(1).value = utils.safeString(item.accountCode);
                row.getCell(2).value = utils.rtlEmbed(utils.safeString(item.accountName));
                row.getCell(3).value = item.balance;

                row.font = { name: 'Amiri', size: 10 };
                row.alignment = { horizontal: 'right', vertical: 'middle' };

                row.eachCell((cell) => {
                    cell.border = {
                        top: { style: 'thin' },
                        left: { style: 'thin' },
                        bottom: { style: 'thin' },
                        right: { style: 'thin' },
                    };
                });
            });

            currentRow += 1; // Space after section
        };

        // Render Sections
        renderSection('accounting.orgGL.reports.balance-sheet.list.assets', 'Assets', assetAccountBalanceList);
        renderSection('accounting.orgGL.reports.balance-sheet.list.liabilities', 'Liabilities', liabilityAccountBalanceList);
        renderSection('accounting.orgGL.reports.balance-sheet.list.equities', 'Equities', equityAccountBalanceList);

        // ==================== TOTALS ====================
        const totalsTitleRow = currentRow++;
        ws.getCell(`A${totalsTitleRow}`).value = getTranslatedLabel('accounting.orgGL.reports.balance-sheet.list.totals', 'Totals');
        ws.mergeCells(`A${totalsTitleRow}:C${totalsTitleRow}`);
        ws.getRow(totalsTitleRow).font = { name: 'Amiri', size: 13, bold: true };
        ws.getRow(totalsTitleRow).alignment = { horizontal: 'center', vertical: 'middle' };

        const addTotalRow = (labelKey: string, defaultLabel: string, value?: number) => {
            const rowNum = currentRow++;
            const row = ws.getRow(rowNum);

            row.getCell(1).value = getTranslatedLabel(labelKey, defaultLabel);
            row.getCell(2).value = '';
            row.getCell(3).value = value ?? 0;

            row.font = { name: 'Amiri', size: 11, bold: true };
            row.getCell(3).numFmt = '#,##0.00';
            row.alignment = { horizontal: 'right', vertical: 'middle' };

            row.eachCell((cell) => {
                cell.border = {
                    top: { style: 'thin' },
                    left: { style: 'thin' },
                    bottom: { style: 'thin' },
                    right: { style: 'thin' },
                };
            });
        };

        addTotalRow('accounting.orgGL.reports.balance-sheet.list.current-assets', 'Current Assets', totals.currentAssetBalanceTotal);
        addTotalRow('accounting.orgGL.reports.balance-sheet.list.long-term-assets', 'Long Term Assets', totals.longtermAssetBalanceTotal);
        addTotalRow('accounting.orgGL.reports.balance-sheet.list.accumulated-depreciation', 'Total Accumulated Depreciation', totals.accumDepreciationBalanceTotal);
        addTotalRow('accounting.orgGL.reports.balance-sheet.list.total-assets', 'Total Assets', totals.assetBalanceTotal);
        addTotalRow('accounting.orgGL.reports.balance-sheet.list.current-liabilities', 'Current Liabilities', totals.currentLiabilityBalanceTotal);
        addTotalRow('accounting.orgGL.reports.balance-sheet.list.total-equities', 'Equities', totals.equityBalanceTotal);
        addTotalRow('accounting.orgGL.reports.balance-sheet.list.total-liabilities-equities', 'Total Liabilities and Equities', totals.liabilityEquityBalanceTotal);

        return await workbook.xlsx.writeBuffer();
    }, [companyName, assetAccountBalanceList, liabilityAccountBalanceList, equityAccountBalanceList, totals, getTranslatedLabel, isFetching, thruDate]);

    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            saveAs(blob, `BalanceSheet_${companyName.replace(/[^a-z0-9]/gi, '_')}_${new Date().toISOString().slice(0,10)}.xlsx`);
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
            {getTranslatedLabel('accounting.orgGL.reports.balance-sheet.list.excel', 'Export Balance Sheet')}
        </Button>
    );
};