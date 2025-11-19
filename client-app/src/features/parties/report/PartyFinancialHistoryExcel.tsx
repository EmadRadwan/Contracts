// src/features/party/report/PartyFinancialHistoryExcel.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

// REFACTOR: FINAL VERSION – 100% Party Perspective (Vendor/Customer/Contractor View)
// Purpose: Official statement that can be sent directly to the party
// Fix: All signs, final balance, and explanation are now from the PARTY's point of view

interface LedgerRow {
    date: string | number;
    description: string;
    invoiceNumber?: string;
    paymentNumber?: string;
    value: number;
    toPay: number;     // مدين عندنا = دائن عنده
    paid: number;      // دائن عندنا = مدين عنده
    balance: number;   // رصيدنا معاه → سيتم عكسه
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
        workbook.creator = 'Golden Land System';

        if (!ledgerItems || ledgerItems.length === 0 || isFetching) {
            console.warn('Cannot generate Excel: No ledger items');
            return null;
        }

        // Load logo (optional)
        let logoImageId: number | null = null;
        try {
            const response = await fetch('/goldenlandlogo.jpg');
            if (response.ok) {
                const blob = await response.blob();
                const arrayBuffer = await blob.arrayBuffer();
                logoImageId = workbook.addImage({ buffer: arrayBuffer, extension: 'jpeg' });
            }
        } catch (e) {
            console.warn('Logo not loaded – continuing without it');
        }

        const worksheet = workbook.addWorksheet(
            party.partyName?.length > 30 ? party.partyName.substring(0, 28) + '...' : party.partyName || 'كشف حساب',
            {
                pageSetup: { paperSize: 9, orientation: 'landscape', fitToPage: true },
                views: [{ rightToLeft: true }],
            }
        );

        // Logo
        if (logoImageId !== null) {
            worksheet.addImage(logoImageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 110, height: 110 },
                editAs: 'absolute',
            });
            worksheet.getRow(1).height = 82;
            worksheet.addRow([]); // 2
            worksheet.addRow([]); // 3
            worksheet.addRow([]); // 4
        }

        // Title
        const titleRowNum = logoImageId ? 4 : 1;
        worksheet.addRow([
            getTranslatedLabel('party.financial.history.excel.title', 'كشف حساب') +
            ': ' +
            sharedUtils.rtlEmbed(sharedUtils.safeString(party.partyName)),
        ]);
        worksheet.mergeCells(`A${titleRowNum}:K${titleRowNum}`);
        const titleRow = worksheet.getRow(titleRowNum);
        titleRow.font = { name: 'Amiri', size: 16, bold: true };
        titleRow.alignment = { horizontal: 'center', vertical: 'middle' };
        worksheet.addRow([]);
        worksheet.addRow([]);

        // Headers – from PARTY's perspective
        const headers = [
            'التاريخ',
            'الوصف',
            'رقم الفاتورة',
            'رقم الدفعة',
            'إجمالي الفاتورة',
            'مدين',  // ← مدين عند الطرف (دائن عندنا)
            'دائن',  // ← دائن عند الطرف (مدين عندنا)
            'الرصيد',
            'ملاحظات',
        ];

        worksheet.addRow(headers);
        const headerRow = worksheet.lastRow!;
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFD9EAD3' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        headerRow.eachCell(cell => {
            cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'double' }, right: { style: 'thin' } };
        });

        // Column settings
        worksheet.columns = [
            { width: 12 }, { width: 42 }, { width: 16 }, { width: 16 },
            { width: 15 }, { width: 13 }, { width: 13 }, { width: 15 }, { width: 25 },
        ];
        [5, 6, 7, 8].forEach(col => worksheet.getColumn(col).numFmt = '#,##0.00');

        // Data rows – REVERSED signs for PARTY view
        ledgerItems.forEach(item => {
            const row = worksheet.addRow([
                item.date,
                sharedUtils.rtlEmbed(sharedUtils.safeString(item.description)),
                sharedUtils.safeString(item.invoiceNumber || 'N/A'),
                sharedUtils.safeString(item.paymentNumber || 'N/A'),
                item.value || 0,
                item.paid,        // دائن عندنا → مدين عنده
                item.toPay,       // مدين عندنا → دائن عنده
                -item.balance,    // نعكس الرصيد التراكمي
                sharedUtils.rtlEmbed(sharedUtils.safeString(item.notes || '')),
            ]);

            row.font = { name: 'Amiri', size: 10 };
            row.alignment = { horizontal: 'right', vertical: 'middle' };
            row.eachCell(cell => {
                cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
            });
        });

        // Final balance – from PARTY's perspective
        const finalBalanceOurSide = ledgerItems[ledgerItems.length - 1]?.balance || 0;
        const finalBalancePartySide = -finalBalanceOurSide; // ← هذا رصيد الطرف معانا

        const finalRow = worksheet.addRow(['', 'الرصيد النهائي', '', '', '', '', '', finalBalancePartySide, '']);
        finalRow.font = { name: 'Amiri', size: 12, bold: true };
        finalRow.getCell(8).numFmt = '#,##0.00';

        if (finalBalancePartySide > 0) {
            finalRow.getCell(8).font = { ...finalRow.getCell(8).font, color: { argb: 'FF006400' } }; // Green = لصالحه
        } else if (finalBalancePartySide < 0) {
            finalRow.getCell(8).font = { ...finalRow.getCell(8).font, color: { argb: 'FF8B0000' } }; // Red = عليه لنا
        }

        // Clarity line – 100% from PARTY's view
        const clarityText =
            finalBalancePartySide > 0
                ? '← لصالحك (لك رصيد دائن عندنا)'
                : finalBalancePartySide < 0
                    ? '← عليك لنا (أنت مدين لنا)'
                    : '← لا يوجد رصيد';

        const clarityRow = worksheet.addRow(['', clarityText, '', '', '', '', '', Math.abs(finalBalancePartySide), '']);
        clarityRow.font = { name: 'Amiri', size: 13, bold: true, color: { argb: 'FF000080' } };
        clarityRow.getCell(8).font = {
            name: 'Amiri',
            size: 13,
            bold: true,
            color: finalBalancePartySide > 0 ? { argb: 'FF006400' } : { argb: 'FF8B0000' },
        };
        clarityRow.getCell(8).numFmt = '#,##0.00';
        clarityRow.getCell(8).alignment = { horizontal: 'right' };

        // Official note
        worksheet.addRow(['', 'هذا كشف حساب رسمي يوضح مركزك المالي معنا حتى تاريخ اليوم', '', '', '', '', '', '', '']);
        worksheet.lastRow!.font = { name: 'Amiri', size: 10, italic: true, color: { argb: 'FF666666' } };

        // Footer
        worksheet.addRow([]);
        const footerRow = worksheet.addRow([
            `تاريخ الطباعة: ${new Date().toLocaleDateString('ar-EG')}`,
            `الوقت: ${new Date().toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit', second: '2-digit' })}`,
            '', '', '', '', '', '', '',
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
            const safeName = (party.partyName || party.partyId).replace(/[/\\?%*:|"<>]/g, '_');
            const filename = `كشف_حساب_${safeName}_${today}.xlsx`;
            saveAs(blob, filename);
        }
    }, [generateExcel, party]);

    const disabled = isSubmitting || isAddLoading || isUpdateLoading || isFetching || ledgerItems.length === 0;

    return (
        <div>
            <Button
                color="primary"
                variant="outlined"
                disabled={disabled}
                onClick={handleDownload}
                style={{ marginRight: 10 }}
            >
                كشف الحساب Excel (من وجهة نظر الطرف)
            </Button>
        </div>
    );
};