import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';
import { GlAccount } from '../../../../app/models/accounting/globalGlSettings';

// REFACTOR: Reuse sharedUtils from SupplyCertificateExcel
// Purpose: Ensures consistent string and RTL handling across Excel exports
// Improvement: Avoids code duplication; supports Arabic text with \u202B embedding
// Context: Matches SupplyCertificateExcel for safeString and rtlEmbed
const sharedUtils = {
    safeString: (value: any): string => {
        if (value === null || value === undefined) return 'N/A';
        if (typeof value === 'object') {
            console.warn('safeString received object:', value);
            return 'N/A';
        }
        if (typeof value === 'number') return value.toString();
        return String(value);
    },
    rtlEmbed: (text: string): string => {
        return /\p{Script=Arabic}/u.test(text) ? `\u202B${text}` : text;
    },
};

interface ChartOfAccountsExcelProps {
    accounts: GlAccount[];
    companyId?: string;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

export const ChartOfAccountsExcel: React.FC<ChartOfAccountsExcelProps> = ({
                                                                              accounts,
                                                                              companyId,
                                                                              getTranslatedLabel,
                                                                          }) => {
    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';
        const worksheet = workbook.addWorksheet('Chart of Accounts', {
            pageSetup: { paperSize: 9, orientation: 'landscape' },
            views: [{ rightToLeft: true }],
        });
        worksheet.getColumn(1).font = { name: 'Amiri', size: 10 };

        // REFACTOR: Add logo to worksheet
        // Purpose: Places logo at top-left (A1:B3) for branding consistency
        // Improvement: Matches SupplyCertificateExcel's logo placement; fallback if logo fails
        // Context: Uses browser-native fetch for /goldenlandlogo.jpg
        let logoImageId: number | null = null;
        try {
            const response = await fetch('/goldenlandlogo.jpg');
            if (!response.ok) throw new Error('Failed to fetch logo');
            const blob = await response.blob();
            const arrayBuffer = await blob.arrayBuffer();
            logoImageId = workbook.addImage({
                buffer: arrayBuffer,
                extension: 'jpeg',
            });
            worksheet.addImage(logoImageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 100, height: 100 },
                editAs: 'absolute',
            });
            worksheet.getRow(1).height = 75; // Approx 100px at 96 DPI
            worksheet.getRow(2).height = 20;
            worksheet.getRow(3).height = 20;
            worksheet.addRow([]);
            worksheet.addRow([]);
            worksheet.addRow([]);
        } catch (error) {
            console.warn('Logo fetch failed:', error);
            worksheet.addRow(['Logo Unavailable']);
            worksheet.getRow(1).font = { name: 'Amiri', size: 10, color: { argb: 'FF0000' } };
            worksheet.getRow(1).alignment = { horizontal: 'center', vertical: 'middle' };
        }

        // Header Section
        worksheet.addRow([
            getTranslatedLabel('accounting.chartOfAccounts.report.title', 'Chart of Accounts') +
            (companyId ? `: Company ${companyId}` : ''),
        ]);
        worksheet.mergeCells(`A${logoImageId !== null ? 4 : 2}:D${logoImageId !== null ? 4 : 2}`);
        worksheet.getRow(logoImageId !== null ? 4 : 2).font = { name: 'Amiri', size: 14, bold: true };
        worksheet.getRow(logoImageId !== null ? 4 : 2).alignment = {
            horizontal: 'center',
            vertical: 'middle',
            wrapText: true,
        };
        worksheet.addRow([
            getTranslatedLabel('accounting.chartOfAccounts.date', 'Date') +
            ': ' +
            new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'numeric', day: 'numeric' }),
        ]);
        worksheet.getRow(logoImageId !== null ? 5 : 3).font = { name: 'Amiri', size: 10 };
        worksheet.getRow(logoImageId !== null ? 5 : 3).alignment = { horizontal: 'right', wrapText: true };
        worksheet.addRow([]);

        // Table Headers
        const headers = [
            getTranslatedLabel('accounting.chartOfAccounts.accountNumber', 'Account Number'),
            getTranslatedLabel('accounting.chartOfAccounts.accountName', 'Account Name'),
            getTranslatedLabel('accounting.chartOfAccounts.parentAccountName', 'Parent Account Name'),
        ];
        const headerRow = worksheet.addRow(headers);
        headerRow.font = { name: 'Amiri', size: 10, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        headerRow.eachCell(cell => {
            cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
        });

        // REFACTOR: Set column widths to match KendoGrid
        // Purpose: Aligns column widths with UI display for consistent layout
        // Improvement: Wider Account Name column to accommodate indentation and Arabic text
        // Context: Approximates KendoGrid widths (120px ~ 16 Excel units, 400px ~ 50 units)
        worksheet.columns = [
            { width: 16 }, // Account Number
            { width: 50 }, // Account Name
            { width: 50 }, // Parent Account Name
        ];

        // REFACTOR: Recursively add accounts to worksheet
        // Purpose: Processes hierarchical accounts, indenting Account Name by depth
        // Improvement: Preserves tree structure in Excel using 2-space indentation per level
        // Context: Matches KendoGrid's DetailComponent tree display
        const addAccounts = (accounts: GlAccount[], level: number = 0) => {
            accounts.forEach(account => {
                const rowData = [
                    sharedUtils.safeString(account.glAccountId),
                    sharedUtils.rtlEmbed(sharedUtils.safeString(account.text)).padStart(
                        sharedUtils.safeString(account.text).length + level * 2,
                        '  '
                    ),
                    sharedUtils.rtlEmbed(sharedUtils.safeString(account.parentAccountName || 'None')),
                ];
                const row = worksheet.addRow(rowData);
                row.font = { name: 'Amiri', size: 9 };
                row.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
                row.eachCell(cell => {
                    cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
                });

                if (account.items && account.items.length > 0) {
                    addAccounts(account.items, level + 1);
                }
            });
        };

        addAccounts(accounts);

        const buffer = await workbook.xlsx.writeBuffer();
        return buffer;
    }, [accounts, companyId, getTranslatedLabel]);

    const handleDownload = useCallback(async () => {
        const buffer = await generateExcel();
        if (buffer) {
            const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            saveAs(blob, `ChartOfAccounts_${companyId || 'All'}.xlsx`);
        }
    }, [generateExcel, companyId]);

    return (
        <Button
            color="primary"
            variant="outlined"
            onClick={handleDownload}
            style={{ marginLeft: 10 }}
        >
            {getTranslatedLabel('accounting.chartOfAccounts.excel', 'Export to Excel')}
        </Button>
    );
};