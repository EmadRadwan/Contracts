const IncomeStatementForm = ({ onSubmit }: IncomeStatementFormProps) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const localizationKey = "accounting.orgGL.reports.income-statement.form";

    const now = new Date();
    const firstDayOfYear = new Date(now.getFullYear(), 0, 1);

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

    interface IncomeStatementExcelProps {
        companyName: string;
        revenueAccountBalances: AccountBalanceRow[];
        expenseAccountBalances: AccountBalanceRow[];
        incomeAccountBalances: AccountBalanceRow[];
        totals: {
            contraRevenueBalanceTotal?: number;
            cogsExpenseBalanceTotal?: number;
            netSales?: number;
            grossMargin?: number;
            depreciationBalanceTotal?: number;
            sgaExpenseBalanceTotal?: number;           // ← Added (important)
            incomeFromOperations?: number;
            incomeBalanceTotal?: number;               // ← Other Income
            netIncome?: number;
        };
        getTranslatedLabel: (key: string, defaultValue: string) => string;
        isFetching?: boolean;
        fromDate?: string;
        thruDate?: string;
    }

    const utils = {
        safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
        rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
        formatDate: (d: string | Date | undefined) =>
            d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
    };

    export const IncomeStatementExcel: React.FC<IncomeStatementExcelProps> = ({
                                                                                  companyName,
                                                                                  revenueAccountBalances,
                                                                                  expenseAccountBalances,
                                                                                  incomeAccountBalances,
                                                                                  totals,
                                                                                  getTranslatedLabel,
                                                                                  isFetching = false,
                                                                                  fromDate,
                                                                                  thruDate,
                                                                              }) => {
        const generateExcel = useCallback(async () => {
            if (isFetching) return null;

            const workbook = new ExcelJS.Workbook();
            workbook.creator = 'Golden Land System';
            const ws = workbook.addWorksheet('Income Statement', {
                views: [{ rightToLeft: true }],
                pageSetup: {
                    paperSize: 9,
                    orientation: 'portrait',
                    fitToPage: true,
                    fitToWidth: 1,
                },
            });

            // ==================== LOGO & TITLE (unchanged) ====================
            let logoId: number | null = null;
            try {
                const resp = await fetch('/goldenlandlogo.jpg');
                if (resp.ok) {
                    const buf = await (await resp.blob()).arrayBuffer();
                    logoId = workbook.addImage({ buffer: buf, extension: 'jpeg' });
                    ws.getRow(1).height = 90;
                    ws.addImage(logoId, { tl: { col: 0, row: 0 }, ext: { width: 160, height: 115 } });
                }
            } catch (e) {
                console.warn('Logo failed to load:', e);
            }

            let currentRow = logoId ? 6 : 3;

            // Title
            const titleRow = currentRow++;
            const reportTitle = `${getTranslatedLabel(
                'accounting.orgGL.reports.income-statement.list.title',
                'Income Statement For'
            )}: ${utils.rtlEmbed(utils.safeString(companyName))}`;
            ws.getCell(`A${titleRow}`).value = reportTitle;
            ws.mergeCells(`A${titleRow}:C${titleRow}`);
            ws.getRow(titleRow).font = { name: 'Amiri', size: 16, bold: true };
            ws.getRow(titleRow).alignment = { horizontal: 'center', vertical: 'middle' };
            ws.getRow(titleRow).height = 45;

            // Date line
            const dateRow = currentRow++;
            const fromStr = fromDate
                ? `${getTranslatedLabel('accounting.orgGL.reports.income-statement.form.fromDate', 'From Date')}: ${utils.formatDate(fromDate)}`
                : '';
            const thruStr = thruDate
                ? `${getTranslatedLabel('accounting.orgGL.reports.income-statement.form.thruDate', 'Thru Date')}: ${utils.formatDate(thruDate)}`
                : '';
            const dateSubTitle = (fromStr || thruStr) ? `(${fromStr} ${thruStr})` : '';
            const genDate = ` - ${getTranslatedLabel(
                'accounting.orgGL.reports.income-statement.list.generated-at',
                'Generated At'
            )}: ${utils.formatDate(new Date())}`;

            ws.getCell(`A${dateRow}`).value = `${dateSubTitle}${genDate}`;
            ws.mergeCells(`A${dateRow}:C${dateRow}`);
            ws.getRow(dateRow).font = { name: 'Amiri', size: 10, italic: true };
            ws.getRow(dateRow).alignment = { horizontal: 'center', vertical: 'middle' };
            ws.getRow(dateRow).height = 28;

            currentRow += 2;

            // Column settings
            ws.columns = [
                { width: 18 }, // Code
                { width: 48 }, // Name
                { width: 25 }, // Balance
            ];
            ws.getColumn(3).numFmt = '#,##0.00';

            // ==================== SECTIONS ====================
            const renderSection = (titleKey: string, defaultTitle: string, data: AccountBalanceRow[]) => {
                const secTitleRow = currentRow++;
                ws.getCell(`A${secTitleRow}`).value = getTranslatedLabel(titleKey, defaultTitle);
                ws.mergeCells(`A${secTitleRow}:C${secTitleRow}`);
                ws.getRow(secTitleRow).font = { name: 'Amiri', size: 13, bold: true };
                ws.getRow(secTitleRow).alignment = { horizontal: 'center', vertical: 'middle' };

                const headerRow = currentRow++;
                const header = ws.getRow(headerRow);
                header.getCell(1).value = getTranslatedLabel('accounting.orgGL.reports.income-statement.list.code', 'Account Code');
                header.getCell(2).value = getTranslatedLabel('accounting.orgGL.reports.income-statement.list.name', 'Account Name');
                header.getCell(3).value = getTranslatedLabel('accounting.orgGL.reports.income-statement.list.balance', 'Balance');
                header.font = { name: 'Amiri', size: 11, bold: true };
                header.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
                header.alignment = { horizontal: 'center', vertical: 'middle' };

                header.eachCell((cell) => {
                    cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
                });

                data.forEach((item) => {
                    const r = currentRow++;
                    const row = ws.getRow(r);
                    row.getCell(1).value = utils.safeString(item.accountCode);
                    row.getCell(2).value = utils.rtlEmbed(utils.safeString(item.accountName));
                    row.getCell(3).value = item.balance || 0;
                    row.font = { name: 'Amiri', size: 10 };
                    row.alignment = { horizontal: 'right', vertical: 'middle' };
                    row.eachCell((cell) => {
                        cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
                    });
                });

                currentRow += 1;
            };

            renderSection('accounting.orgGL.reports.income-statement.list.revenues', 'Revenues', revenueAccountBalances);

            if (totals.cogsExpenseBalanceTotal && totals.cogsExpenseBalanceTotal !== 0) {
                renderSection('accounting.orgGL.reports.income-statement.list.cost-of-goods-sold', 'Cost of Goods Sold', []);
            }

            renderSection('accounting.orgGL.reports.income-statement.list.expenses', 'Expenses', expenseAccountBalances);

            if (incomeAccountBalances.length > 0) {
                renderSection('accounting.orgGL.reports.income-statement.list.income', 'Other Income', incomeAccountBalances);
            }

            // ==================== TOTALS SECTION (Fixed & Arabized) ====================
            const totalsTitleRow = currentRow++;
            ws.getCell(`A${totalsTitleRow}`).value = getTranslatedLabel(
                'accounting.orgGL.reports.income-statement.list.totals',
                'Totals'
            );
            ws.mergeCells(`A${totalsTitleRow}:C${totalsTitleRow}`);
            ws.getRow(totalsTitleRow).font = { name: 'Amiri', size: 13, bold: true };
            ws.getRow(totalsTitleRow).alignment = { horizontal: 'center', vertical: 'middle' };

            const addTotalRow = (labelKey: string, defaultLabel: string, value: number | undefined) => {
                const rowNum = currentRow++;
                const row = ws.getRow(rowNum);
                row.getCell(1).value = getTranslatedLabel(labelKey, defaultLabel);
                row.getCell(2).value = '';
                row.getCell(3).value = value ?? 0;
                row.getCell(3).numFmt = '#,##0.00';
                row.font = { name: 'Amiri', size: 11, bold: true };
                row.alignment = { horizontal: 'right', vertical: 'middle' };

                row.eachCell((cell) => {
                    cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
                });
            };

            // Logical order matching standard Income Statement + your UI
            addTotalRow('accounting.orgGL.reports.income-statement.list.net-sales', 'Net Sales', totals.netSales);
            addTotalRow('accounting.orgGL.reports.income-statement.list.gross-margin', 'Gross Margin', totals.grossMargin);

            addTotalRow(
                'accounting.orgGL.reports.income-statement.list.cost-of-goods-sold',
                'Cost of Goods Sold',
                totals.cogsExpenseBalanceTotal
            );

            addTotalRow(
                'accounting.orgGL.reports.income-statement.list.operating-expenses',
                'Operating Expenses',
                (totals.sgaExpenseBalanceTotal || 0) + (totals.depreciationBalanceTotal || 0)
            );

            addTotalRow(
                'accounting.orgGL.reports.income-statement.list.income-from-operations',
                'Income From Operations',
                totals.incomeFromOperations
            );

            addTotalRow(
                'accounting.orgGL.reports.income-statement.list.income',
                'Other Income',
                totals.incomeBalanceTotal
            );

            addTotalRow(
                'accounting.orgGL.reports.income-statement.list.net-income',
                'Net Income',
                totals.netIncome
            );

            const buffer = await workbook.xlsx.writeBuffer();
            return buffer;
        }, [companyName, revenueAccountBalances, expenseAccountBalances, incomeAccountBalances, totals, getTranslatedLabel, isFetching, fromDate, thruDate]);

        const handleDownload = useCallback(async () => {
            const buf = await generateExcel();
            if (buf) {
                const blob = new Blob([buf], {
                    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                });
                saveAs(
                    blob,
                    `IncomeStatement_${companyName.replace(/[^a-z0-9]/gi, '_')}_${new Date().toISOString().slice(0, 10)}.xlsx`
                );
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
                {getTranslatedLabel('accounting.orgGL.reports.income-statement.list.excel', 'Export Income Statement')}
            </Button>
        );
    };