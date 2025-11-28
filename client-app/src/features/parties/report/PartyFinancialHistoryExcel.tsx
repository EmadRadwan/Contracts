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
    value: number;      // invoice total
    toPay: number;      // debit to us = purchase invoice
    paid: number;       // credit to us = payment made
    balance: number;    // running balance: + = we owe him, - = he owes us
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
    // REFACTOR: Final version – fully aligned with new ledgerItems and Egyptian accounting standards
    // - "مدين" = payments we made (دفعنا له)
    // - "دائن" = invoices we received (نشتري منه → علينا له)
    // - Balance: positive = we owe him → red, negative = he owes us → green
    // - No reversal of balance – this is the correct way

    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'Golden Land System';

        if (!ledgerItems || ledgerItems.length === 0 || isFetching) {
            console.warn('No data for Excel');
            return null;
        }

        // Optional logo
        let logoImageId: number | null = null;
        try {
            const response = await fetch('/goldenlandlogo.jpg');
            if (response.ok) {
                const buffer = await (await response.blob()).arrayBuffer();
                logoImageId = workbook.addImage({ buffer, extension: 'jpeg' });
            }
        } catch (e) { /* ignore */ }

        const worksheet = workbook.addWorksheet(
            party.partyName?.length > 28 ? party.partyName.substring(0, 28) + '...' : party.partyName || 'كشف حساب',
            { views: [{ rightToLeft: true }], pageSetup: { orientation: 'landscape', fitToPage: true } }
        );

        // Logo
        if (logoImageId) {
            worksheet.addImage(logoImageId, { tl: { col: 0, row: 0 }, ext: { width: 110, height: 110 } });
            worksheet.getRow(1).height = 82;
            worksheet.addRow([]);
            worksheet.addRow([]);
        }

        const titleRowNum = logoImageId ? 4 : 1;
        worksheet.addRow([getTranslatedLabel('party.financial.history.excel.title', 'كشف حساب عميل / مورد') + `: ${sharedUtils.rtlEmbed(party.partyName)}`]);
        worksheet.mergeCells(`A${titleRowNum}:I${titleRowNum}`);
        worksheet.getRow(titleRowNum).font = { name: 'Amiri', size: 18, bold: true };
        worksheet.getRow(titleRowNum).alignment = { horizontal: 'center', vertical: 'middle' };

        worksheet.addRow([]);
        worksheet.addRow([]);

        // REFACTOR: Correct column headers – standard Egyptian ledger format
        const headers = [
            'التاريخ',
            'البيان',
            'رقم الفاتورة',
            'رقم الدفعة',
            'إجمالي الفاتورة',
            'مدين',     // ← دفعنا له (Payment)
            'دائن',     // ← فاتورة شراء (Invoice)
            'الرصيد',
            'ملاحظات',
        ];

        worksheet.addRow(headers);
        const headerRow = worksheet.lastRow!;
        headerRow.font = { name: 'Amiri', size: 12, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE3F2FD' } };
        headerRow.eachCell(cell => {
            cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'double' }, right: { style: 'thin' } };
        });

        worksheet.columns = [
            { width: 12 },
            { width: 45 },
            { width: 16 },
            { width: 16 },
            { width: 16 },
            { width: 14 },
            { width: 14 },
            { width: 16 },
            { width: 28 },
        ];

        // Format currency columns
        [5, 6, 7, 8].forEach(col => worksheet.getColumn(col).numFmt = '#,##0.00');

        // REFACTOR: Map ledgerItems correctly – no balance reversal!
        ledgerItems.forEach(item => {
            const row = worksheet.addRow([
                item.date,
                sharedUtils.rtlEmbed(item.description),
                item.invoiceNumber || '',
                item.paymentNumber || '',
                item.value || 0,
                item.paid,    // مدين = payment (we paid → increases his debit)
                item.toPay,   // دائن = invoice (we owe → increases his credit)
                item.balance, // ← CORRECT: keep as-is from ledger
                sharedUtils.rtlEmbed(item.notes || ''),
            ]);

            row.font = { name: 'Amiri', size: 10 };
            row.alignment = { horizontal: 'right', vertical: 'middle' };

            // Color balance cell
            const balanceCell = row.getCell(8);
            if (item.balance > 0) {
                balanceCell.font = { ...balanceCell.font, color: { argb: 'FF8B0000' }, bold: true }; // Red = we owe
            } else if (item.balance < 0) {
                balanceCell.font = { ...balanceCell.font, color: { argb: 'FF006400' }, bold: true }; // Green = he owes
            }
        });

        // Final Balance Row
        const finalBalance = ledgerItems[ledgerItems.length - 1]?.balance || 0;

        const finalRow = worksheet.addRow([
            '', 'الرصيد النهائي', '', '', '', '', '', finalBalance, ''
        ]);
        finalRow.font = { name: 'Amiri', size: 13, bold: true };
        finalRow.getCell(8).numFmt = '#,##0.00';

        if (finalBalance > 0) {
            finalRow.getCell(8).font = { color: { argb: 'FF8B0000' }, bold: true };
        } else if (finalBalance < 0) {
            finalRow.getCell(8).font = { color: { argb: 'FF006400' }, bold: true };
        }

        // Clarity line – critical for accountants
        const clarityText = finalBalance > 0
            ? '← علينا للطرف (مدينون له)'
            : finalBalance < 0
                ? '← للطرف عندنا (دائنون لنا)'
                : 'لا يوجد رصيد متبقي';

        worksheet.addRow(['', clarityText, '', '', '', '', '', Math.abs(finalBalance), '']);
        worksheet.lastRow!.font = { name: 'Amiri', size: 14, bold: true, color: { argb: 'FF000080' } };

        worksheet.addRow([]);
        worksheet.addRow([`تاريخ الطباعة: ${new Date().toLocaleDateString('ar-EG')}`, `الوقت: ${new Date().toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' })}`]);
        worksheet.lastRow!.font = { name: 'Amiri', size: 9, color: { argb: 'FF666666' } };

        return await workbook.xlsx.writeBuffer();
    }, [party, ledgerItems, getTranslatedLabel, isFetching]);

    const handleDownload = useCallback(async () => {
        const buffer = await generateExcel();
        if (buffer) {
            const blob = new Blob([buffer], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
            });
            const today = new Date().toISOString().slice(0,10);
            const safeName = (party.partyName || party.partyId).replace(/[/\\?%*:|"<>]/g, '_');
            saveAs(blob, `كشف_حساب_${safeName}_${today}.xlsx`);
        }
    }, [generateExcel, party]);

    const disabled = isFetching || ledgerItems.length === 0;

    return (
        <Button
            variant="contained"
            color="success"
            disabled={disabled}
            onClick={handleDownload}
            sx={{ fontWeight: 'bold' }}
        >
            تصدير كشف الحساب Excel
        </Button>
    );
};