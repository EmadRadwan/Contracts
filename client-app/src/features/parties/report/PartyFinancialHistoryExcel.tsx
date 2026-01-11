import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

interface LedgerRow {
    date: string;
    description: string;
    invoiceNumber?: string;
    paymentNumber?: string;
    value: number;
    toPay: number;   // دائن
    paid: number;    // مدين
    balance: number;
    notes?: string;
}

interface Props {
    party: { partyId: string; partyName: string };
    ledgerItems: LedgerRow[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    isFetching?: boolean;
    perspective: string;
}

const sharedUtils = {
    safeString: (value: any): string => (value == null || typeof value === 'object') ? 'N/A' : String(value),
    rtlEmbed: (text: string): string => /\p{Script=Arabic}/u.test(text) ? `\u202B${text}` : text,
};

export const PartyFinancialHistoryExcel: React.FC<Props> = ({
                                                                party,
                                                                ledgerItems,
                                                                getTranslatedLabel,
                                                                isFetching = false,
                                                                perspective,
                                                            }) => {
    const isExternalView = perspective.startsWith('External');

    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.creator = 'Golden Land System';
        workbook.created = new Date();

        if (!ledgerItems?.length || isFetching) return null;

        // Optional logo
        let logoImageId: number | null = null;
        try {
            const response = await fetch('/goldenlandlogo.jpg');
            if (response.ok) {
                const buffer = await (await response.blob()).arrayBuffer();
                logoImageId = workbook.addImage({ buffer, extension: 'jpeg' });
            }
        } catch {}

        const ws = workbook.addWorksheet(
            party.partyName?.length > 28 ? party.partyName.substring(0, 28) + '...' : party.partyName || 'كشف حساب',
            { views: [{ rightToLeft: true }], pageSetup: { orientation: 'landscape', fitToPage: true } }
        );

        // Logo
        if (logoImageId) {
            ws.addImage(logoImageId, { tl: { col: 0, row: 0 }, ext: { width: 110, height: 110 } });
            ws.getRow(1).height = 82;
            ws.addRow([]); ws.addRow([]);
        }

        const titleRow = logoImageId ? 4 : 1;
        ws.addRow([`${getTranslatedLabel('party.financial.history.excel.title', 'كشف حساب')} : ${sharedUtils.rtlEmbed(party.partyName)}`]);
        ws.mergeCells(`A${titleRow}:I${titleRow}`);
        ws.getRow(titleRow).font = { name: 'Amiri', size: 18, bold: true };
        ws.getRow(titleRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.addRow([]); ws.addRow([]);

        // Headers
        const headers = [
            'التاريخ', 'البيان', 'رقم الفاتورة', 'رقم الدفعة',
            'إجمالي الفاتورة', 'مدين', 'دائن', 'الرصيد', 'ملاحظات'
        ];
        const headerRow = ws.addRow(headers);
        headerRow.font = { name: 'Amiri', size: 12, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE3F2FD' } };
        headerRow.alignment = { horizontal: 'center' };

        ws.columns = [
            { width: 12 }, { width: 45 }, { width: 16 }, { width: 16 },
            { width: 16 }, { width: 14 }, { width: 14 }, { width: 16 }, { width: 28 }
        ];

        [5,6,7,8].forEach(col => ws.getColumn(col).numFmt = '#,##0.00 "ج.م"');

        // Data
        ledgerItems.forEach(item => {
            const row = ws.addRow([
                item.date,
                sharedUtils.rtlEmbed(item.description),
                item.invoiceNumber || '',
                item.paymentNumber || '',
                item.value || 0,
                item.paid,      // مدين
                item.toPay,     // دائن
                item.balance,
                sharedUtils.rtlEmbed(item.notes || ''),
            ]);

            row.alignment = { horizontal: 'right', vertical: 'middle' };
            row.font = { name: 'Amiri', size: 10 };

            const balanceCell = row.getCell(8);
            if (item.balance !== 0) {
                balanceCell.font = {
                    color: { argb: item.balance > 0 ? 'FF006400' : 'FF8B0000' },
                    bold: true
                };
            }
        });

        // Final summary
        const finalBalance = ledgerItems[ledgerItems.length - 1]?.balance || 0;

        ws.addRow([]);
        const finalRow = ws.addRow(['', 'الرصيد النهائي', '', '', '', '', '', finalBalance, '']);
        finalRow.font = { name: 'Amiri', size: 13, bold: true };
        finalRow.getCell(8).font = {
            color: { argb: finalBalance > 0 ? 'FF006400' : finalBalance < 0 ? 'FF8B0000' : 'FF000000' }
        };

        const clarityText = isExternalView
            ? finalBalance > 0
                ? '← لنا عليهم (دائنون لنا)'
                : finalBalance < 0
                    ? '← علينا للطرف (مدينون له)'
                    : 'لا يوجد رصيد متبقي'
            : finalBalance > 0
                ? '← للطرف عندنا (دائنون لنا)'
                : finalBalance < 0
                    ? '← علينا للطرف (مدينون له)'
                    : 'لا يوجد رصيد متبقي';

        const clarityRow = ws.addRow(['', clarityText, '', '', '', '', '', Math.abs(finalBalance), '']);
        clarityRow.font = { name: 'Amiri', size: 14, bold: true, color: { argb: 'FF000080' } };

        ws.addRow([]);
        ws.addRow([
            `تاريخ الطباعة: ${new Date().toLocaleDateString('ar-EG')}`,
            `الوقت: ${new Date().toLocaleTimeString('ar-EG', { hour: '2-digit', minute: '2-digit' })}`
        ]);

        const buffer = await workbook.xlsx.writeBuffer();
        const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        const safeName = (party.partyName || party.partyId).replace(/[/\\?%*:|"<>]/g, '_');
        saveAs(blob, `كشف_حساب_${safeName}_${new Date().toISOString().slice(0,10)}.xlsx`);
    }, [ledgerItems, party, getTranslatedLabel, isFetching, perspective]);

    const handleDownload = useCallback(async () => {
        await generateExcel();
    }, [generateExcel]);

    return (
        <Button
            variant="contained"
            color="success"
            disabled={isFetching || !ledgerItems.length}
            onClick={handleDownload}
            sx={{ fontWeight: 'bold' }}
        >
            تصدير كشف الحساب Excel
        </Button>
    );
};