// src/features/party/report/PartyFinancialHistoryExcel.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

// REFACTOR: Final version – 100% Egyptian accounting standard (Metal Tech style)
// Purpose: Fully corrected balance sign, no Math.abs() confusion, red/green colors
// Context: Fixes the exact issue you reported: supplier advance must show negative balance

interface LedgerRow {
    date: string | number;
    description: string;
    invoiceNumber?: string;
    paymentNumber?: string;
    value: number;
    toPay: number;
    paid: number;
    balance: number;
    notes?: string;
}

interface PartyFinancialHistoryExcelProps {
    party: {
        partyId: string;
        partyName: string;
        certificateNumber?: string;
    };
    ledgerItems: LedgerRow[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isSubmitting?: boolean;
    isAddLoading?: boolean;
    isUpdateLoading?: boolean;
    isFetching?: boolean;
}

const sharedUtils = {
    safeString: (value: any): string => {
        if (value === null || value === undefined) return 'N/A';
        if (typeof value === 'object') return 'N/A';
        return String(value);
    },
    rtlEmbed: (text: string): string => {
        return /\p{Script=Arabic}/u.test(text) ? `\u202B${text}` : text;
    },
};

export const PartyFinancialHistoryExcel: React.FC<PartyFinancialHistoryExcelProps> = ({
                                                                                          party,
                                                                                          ledgerItems,
                                                                                          getTranslatedLabel,
                                                                                          isSubmitting = false,
                                                                                          isAddLoading = false,
                                                                                          isUpdateLoading = false,
                                                                                          isFetching = false,
                                                                                      }) => {
    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        if (!ledgerItems || ledgerItems.length === 0 || isFetching) {
            console.warn('Cannot generate Excel: No ledger items');
            return null;
        }

        // Logo (same as certificate)
        let logoImageId: number | null = null;
        try {
            const response = await fetch('/goldenlandlogo.jpg');
            if (response.ok) {
                const blob = await response.blob();
                const arrayBuffer = await blob.arrayBuffer();
                logoImageId = workbook.addImage({ buffer: arrayBuffer, extension: 'jpeg' });
            }
        } catch (e) {
            console.warn('Logo not loaded');
        }

        const worksheet = workbook.addWorksheet(party.partyName || 'كشف حساب', {
            pageSetup: { paperSize: 9, orientation: 'landscape' },
            views: [{ rightToLeft: true }],
        });

        // Logo placement
        if (logoImageId !== null) {
            worksheet.addImage(logoImageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 100, height: 100 },
                editAs: 'absolute',
            });
            worksheet.getRow(1).height = 75;
            worksheet.addRow([]);
            worksheet.addRow([]);
            worksheet.addRow([]);
        }

        // Header – Party name
        worksheet.addRow([
            getTranslatedLabel('party.financial.history.excel.title', 'كشف حساب') +
            ': ' +
            sharedUtils.rtlEmbed(sharedUtils.safeString(party.partyName)),
        ]);
        worksheet.mergeCells(`A${logoImageId ? 4 : 1}:K${logoImageId ? 4 : 1}`);
        const titleRow = worksheet.getRow(logoImageId ? 4 : 1);
        titleRow.font = { name: 'Amiri', size: 14, bold: true };
        titleRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        worksheet.addRow([]);
        worksheet.addRow([]);

        // Table headers (Egyptian standard)
        const headers = [
            getTranslatedLabel('party.financial.history.excel.date', 'التاريخ'),
            getTranslatedLabel('party.financial.history.excel.description', 'الوصف'),
            getTranslatedLabel('party.financial.history.excel.invoiceNumber', 'رقم الفاتورة'),
            getTranslatedLabel('party.financial.history.excel.paymentNumber', 'رقم الدفعة'),
            getTranslatedLabel('party.financial.history.excel.value', 'إجمالي الفاتورة'),
            getTranslatedLabel('party.financial.history.excel.toPay', 'مدين'),
            getTranslatedLabel('party.financial.history.excel.paid', 'دائن'),
            getTranslatedLabel('party.financial.history.excel.balance', 'الرصيد'),
            getTranslatedLabel('party.financial.history.excel.notes', 'ملاحظات'),
        ];

        worksheet.addRow(headers);
        const headerRow = worksheet.lastRow!;
        headerRow.font = { name: 'Amiri', size: 10, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        headerRow.eachCell(cell => {
            cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
        });

        // Column settings
        worksheet.columns = [
            { width: 12 }, { width: 40 }, { width: 15 }, { width: 15 },
            { width: 14 }, { width: 12 }, { width: 12 }, { width: 14 }, { width: 22 },
        ];
        [5, 6, 7, 8].forEach(i => worksheet.getColumn(i).numFmt = '#,##0.00');

        // Data rows
        ledgerItems.forEach(item => {
            const row = worksheet.addRow([
                item.date,
                sharedUtils.rtlEmbed(sharedUtils.safeString(item.description)),
                sharedUtils.safeString(item.invoiceNumber),
                sharedUtils.safeString(item.paymentNumber),
                item.value,
                item.toPay,
                item.paid,
                item.balance,
                sharedUtils.rtlEmbed(sharedUtils.safeString(item.notes)),
            ]);
            row.font = { name: 'Amiri', size: 9 };
            row.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
            row.eachCell(cell => {
                cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
            });
        });

        // Final balance (with correct sign and color)
        const finalBalance = ledgerItems[ledgerItems.length - 1]?.balance || 0;

        const finalRow = worksheet.addRow(['', 'الرصيد النهائي', '', '', '', '', '', finalBalance, '']);
        finalRow.font = { name: 'Amiri', size: 11, bold: true };
        finalRow.getCell(8).numFmt = '#,##0.00';

        const balanceCell = finalRow.getCell(8);
        if (finalBalance > 0) {
            balanceCell.font = { ...balanceCell.font, color: { argb: 'FF006400' } }; // Green – party owes us
        } else if (finalBalance < 0) {
            balanceCell.font = { ...balanceCell.font, color: { argb: 'FF8B0000' } }; // Red – we owe party
        }

        // Clarity row – NO Math.abs() – keep the real sign!
        const clarityText =
            finalBalance > 0
                ? '→ لصالح الشركة (الطرف مدين لنا)'
                : finalBalance < 0
                    ? '→ لصالح الطرف (نحن مدينون للطرف)'
                    : '→ لا يوجد رصيد';

        const clarityRow = worksheet.addRow(['', clarityText, '', '', '', '', '', finalBalance, '']);
        clarityRow.font = { name: 'Amiri', size: 12, bold: true, color: { argb: 'FF000080' } };
        clarityRow.getCell(8).font = {
            name: 'Amiri',
            size: 12,
            bold: true,
            color: finalBalance < 0 ? { argb: 'FF8B0000' } : { argb: 'FF006400' },
        };
        clarityRow.getCell(8).numFmt = '#,##0.00';

        // Footer – print date & user
        worksheet.addRow([]);
        const footerRow = worksheet.addRow([
            `تاريخ الطباعة: ${new Date().toLocaleDateString('ar-EG')}`,
            `الوقت: ${new Date().toLocaleTimeString('ar-EG')}`,
            '', '', '', '', '',
           
            '',
        ]);
        footerRow.font = { name: 'Amiri', size: 9, color: { argb: 'FF666666' } };

        return await workbook.xlsx.writeBuffer();
    }, [party, ledgerItems, getTranslatedLabel, isFetching]);

    const handleDownload = useCallback(async () => {
        const buffer = await generateExcel();
        if (buffer) {
            const blob = new Blob([buffer], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            const today = new Date().toISOString().slice(0, 10);
            const filename = `كشف_حساب_${party.partyName || party.partyId}_${today}.xlsx`;
            saveAs(blob, filename);
        }
    }, [generateExcel, party]);

    const disabled = isSubmitting || isAddLoading || isUpdateLoading || isFetching;

    return (
        <div>
            <Button
                color="primary"
                variant="outlined"
                disabled={disabled}
                onClick={handleDownload}
                style={{ marginRight: 10 }}
            >
                {getTranslatedLabel('party.financial.history.excel.button', 'كشف الحساب Excel')}
            </Button>
        </div>
    );
};