import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';
import { GlAccount } from '../../../../app/models/accounting/globalGlSettings';

// Purpose: Ensures consistent string and RTL handling across Excel exports
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

interface AdjustPowerBIPropsExcelProps {
    accounts: GlAccount[];
    companyId?: string;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

export const AdjustPowerBIPropsExcel: React.FC<AdjustPowerBIPropsExcelProps> = ({
                                                                              accounts,
                                                                              companyId,
                                                                              getTranslatedLabel,
                                                                          }) => {
    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';
        const worksheet = workbook.addWorksheet('Power BI Props', {
            pageSetup: { paperSize: 9, orientation: 'landscape' },
            views: [{ rightToLeft: true }],
        });
        worksheet.getColumn(1).font = { name: 'Amiri', size: 10 };

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
            worksheet.getRow(1).height = 75; 
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
            getTranslatedLabel('accounting.glAccount.bulkEdit.powerBiProps', 'Adjust Power BI Props') +
            (companyId ? `: Company ${companyId}` : ''),
        ]);
        worksheet.mergeCells(`A${logoImageId !== null ? 4 : 2}:G${logoImageId !== null ? 4 : 2}`);
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
            getTranslatedLabel('accounting.glAccount.list.accountId', 'Account ID'),
            getTranslatedLabel('accounting.glAccount.list.accountName', 'Account Name'),
            getTranslatedLabel('accounting.glAccount.list.report', 'Report'),
            getTranslatedLabel('accounting.glAccount.list.classCourse', 'Class Course'),
            getTranslatedLabel('accounting.glAccount.list.subClass', 'Sub Class'),
            getTranslatedLabel('accounting.glAccount.list.subClass2', 'Sub Class 2'),
            getTranslatedLabel('accounting.glAccount.list.courseLabel', 'Course Label'),
        ];
        const headerRow = worksheet.addRow(headers);
        headerRow.font = { name: 'Amiri', size: 10, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        headerRow.eachCell(cell => {
            cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
        });

        worksheet.columns = [
            { width: 16 }, // Account ID
            { width: 40 }, // Account Name
            { width: 25 }, // Report
            { width: 25 }, // Class Course
            { width: 25 }, // Sub Class
            { width: 25 }, // Sub Class 2
            { width: 25 }, // Course Label
        ];

        accounts.forEach(account => {
            const rowData = [
                sharedUtils.safeString(account.glAccountId),
                sharedUtils.rtlEmbed(sharedUtils.safeString(account.accountName)),
                sharedUtils.rtlEmbed(sharedUtils.safeString(account.glReportDescription || 'None')),
                sharedUtils.rtlEmbed(sharedUtils.safeString(account.glClassCourseDescription || 'None')),
                sharedUtils.rtlEmbed(sharedUtils.safeString(account.glSubClassDescription || 'None')),
                sharedUtils.rtlEmbed(sharedUtils.safeString(account.glSubClass2Description || 'None')),
                sharedUtils.rtlEmbed(sharedUtils.safeString(account.glAccountCourseLabelDescription || 'None')),
            ];
            const row = worksheet.addRow(rowData);
            row.font = { name: 'Amiri', size: 9 };
            row.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
            row.eachCell(cell => {
                cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
            });
        });

        const buffer = await workbook.xlsx.writeBuffer();
        return buffer;
    }, [accounts, companyId, getTranslatedLabel]);

    const handleDownload = useCallback(async () => {
        const buffer = await generateExcel();
        if (buffer) {
            const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            saveAs(blob, `AdjustPowerBIProps_${companyId || 'All'}.xlsx`);
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
