// src/features/party/report/PartyFinancialHistoryExcel.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

interface LedgerRow {
    date: string;
    description: string;
    invoiceNumber?: string;
    paymentNumber?: string;
    value: number;        // إجمالي الفاتورة (للعرض فقط)
    toPay: number;        // مدين عندنا (فاتورة) → دائن عند الطرف
    paid: number;         // دائن عندنا (دفعة) → مدين عند الطرف
    balance: number;      // رصيدنا معاه: سالب = نحن مدينون، موجب = هو مدين لنا
    notes?: string;
}

interface PartyFinancialHistoryExcelProps {
    party: { partyId: string; partyName: string; certificateNumber?: string };
    ledgerItems: LedgerRow[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
}

const sharedUtils = {
    safeString: (value: any): string => (value == null || typeof value === 'object') ? 'N/A' : String(value),
    rtlEmbed: (text: string): string => /\p{Script=Arabic}/u.test(text) ? `\u202B${text}` : text,
};

export const PartyFinancialHistoryExcel: React.FC<PartyFinancialHistoryExcelProps> = ({
                                                                                          party,
                                                                                          ledgerItems,
                                                                                          getTranslatedLabel,
                                                                                          isFetching = false,
                                                                                      }) => {

    // REFACTOR: Full rewrite to match new ledgerItems logic
    // - Now correctly reflects PARTY's perspective (كشف حساب رسمي)
    // - Debit (مدين عنده) = our payments + unapplied payments
    // - Credit (دائن عنده) = our purchase invoices
    // - Balance reversed at the end: negative = he owes us, positive = we owe him
    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'Golden Land System';

        if (!ledgerItems || ledgerItems.length === 0 || isFetching) {
            console.warn('No data for Excel');
            return null;
        }

        // Logo (optional)
        let logoImageId: number | null = null;
        try {
            const response = await fetch('/goldenlandlogo.jpg');
            if (response.ok) {
                const buffer = await (await response.blob()).arrayBuffer();
                logoImageId = workbook.addImage({ buffer, extension: 'jpeg' });
            }
        } catch (e) { /* ignore */ }

        const worksheet = workbook.addWorksheet(
            party.partyName?.length > 30 ? party.partyName.substring(0, 28) + '...' : party.partyName || 'كشف حساب',
            { views: [{ rightToLeft: true }], pageSetup: { orientation: 'landscape', fitToPage: true } }
        );

        // Logo
        if (logoImageId) {
            worksheet.addImage(logoImageId, { tl: { col: 0, row: 0 }, ext: { width: 110, height: 110 } });
            worksheet.getRow(1).height = 82;
            worksheet.addRow([]); // 2
            worksheet.addRow([]); // 3
        }

        const titleRowNum = logoImageId ? 4 : 1;
        worksheet.addRow([getTranslatedLabel('party.financial.history.excel.title', 'كشف حساب') + ': ' + sharedUtils.rtlEmbed(party.partyName)]);
        worksheet.mergeCells(`A${titleRowNum}:K${titleRowNum}`);
        worksheet.getRow(titleRowNum).font = { name: 'Amiri', size: 16, bold: true };
        worksheet.getRow(titleRowNum).alignment = { horizontal: 'center', vertical: 'middle' };
        worksheet.addRow([]);
        worksheet.addRow([]);

        // REFACTOR: Headers now 100% from PARTY's view
        const headers = [
            'التاريخ',
            'الوصف',
            'رقم الفاتورة',
            'رقم الدفعة',
            'إجمالي الفاتورة',
            'مدين',   // ← مدين عند الطرف (دفعنا له أو مقدم)
            'دائن',   // ← دائن عند الطرف (فاتورة شراء منه)
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

        worksheet.columns = [
            { width: 12 }, { width: 42 }, { width: 16 }, { width: 16 },
            { width: 15 }, { width: 13 }, { width: 13 }, { width: 15 }, { width: 25 }
        ];
        [5, 6, 7, 8].forEach(col => worksheet.getColumn(col).numFmt = '#,##0.00');

        // REFACTOR: Correct debit/credit mapping
        ledgerItems.forEach(item => {
            const row = worksheet.addRow([
                item.date,
                sharedUtils.rtlEmbed(item.description),
                item.invoiceNumber || 'N/A',
                item.paymentNumber || 'N/A',
                item.value || 0,
                item.paid,   // مدين عند الطرف ← دفعنا له (يظهر في مدين)
                item.toPay,  // دائن عند الطرف ← فاتورة شراء (يظهر في دائن)
                -item.balance, // عكس الرصيد → من وجهة نظر الطرف
                sharedUtils.rtlEmbed(item.notes || ''),
            ]);

            row.font = { name: 'Amiri', size: 10 };
            row.alignment = { horizontal: 'right', vertical: 'middle' };
            row.eachCell(cell => {
                cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
            });
        });

        // Final balance from PARTY's perspective
        const finalOurBalance = ledgerItems[ledgerItems.length - 1]?.balance || 0;
        const finalPartyBalance = -finalOurBalance; // من وجهة نظر المورد
        
        
        const finalRow = worksheet.addRow(['', 'ال(r)الرصيد النهائي', '', '', '', '', '', finalPartyBalance, '']);
        finalRow.font = { name: 'Amiri', size: 12, bold: true };
        finalRow.getCell(8).numFmt = '#,##0.00';
        if (finalPartyBalance > 0) {
            finalRow.getCell(8).font = { ...finalRow.getCell(8).font, color: { argb: 'FF006400' } }; // Green = لصالحه
        } else if (finalPartyBalance < 0) {
            finalRow.getCell(8).font = { ...finalRow.getCell(8).font, color: { argb: 'FF8B0000' } }; // Red = عليه
        }

        // Clarity line – very important for Egyptian accountants
        const clarityText = finalPartyBalance > 0
            ? '← لصالحك (لك عندنا رصيد دائن)'
            : finalPartyBalance < 0
                ? '← عليك لنا (أنت مدين لنا)'
                : 'لا يوجد رصيد';

        const clarityRow = worksheet.addRow(['', clarityText, '', '', '', '', '', Math.abs(finalPartyBalance), '']);
        clarityRow.font = { name: 'Amiri', size: 13, bold: true, color: { argb: 'FF000080' } };
        clarityRow.getCell(8).font = { ...clarityRow.getCell(8).font, color: finalPartyBalance > 0 ? { argb: 'FF006400' } : { argb: 'FF8B0000' } };
        clarityRow.getCell(8).numFmt = '#,##0.00';

        worksheet.addRow(['', 'هذا كشف حساب رسمي يوضح مركزك المالي معنا حتى تاريخ اليوم', '', '', '', '', '', '', '']);
        worksheet.lastRow!.font = { name: 'Amiri', size: 10, italic: true, color: { argb: 'FF666666' } };

        worksheet.addRow([]);
        const footerRow = worksheet.addRow([
            `تاريخ الطباعة: ${new Date().toLocaleDateString('ar-EG')}`,
            `الوقت: ${new Date().toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' })}`,
            '', '', '', '', '', '', ''
        ]);
        footerRow.font = { name: 'Amiri', size: 9, color: { argb: 'FF666666' } };

        return await workbook.xlsx.writeBuffer();
    }, [party, ledgerItems, getTranslatedLabel, isFetching]);

    const handleDownload = useCallback(async () => {
        const buffer = await generateExcel();
        if (buffer) {
            const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            const today = new Date().toISOString().slice(0, 10);
            const safeName = (party.partyName || party.partyId).replace(/[/\\?%*:|"<>]/g, '_');
            saveAs(blob, `كشف_حساب_${safeName}_${today}.xlsx`);
        }
    }, [generateExcel, party]);

    const disabled = isFetching || ledgerItems.length === 0;

    return (
        <Button
            color="primary"
            variant="outlined"
            disabled={disabled}
            onClick={handleDownload}
            style={{ marginRight: 10 }}
        >
            كشف الحساب Excel 
        </Button>
    );
};