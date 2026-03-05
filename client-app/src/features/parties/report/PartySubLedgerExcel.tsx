import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

interface SubLedgerExcelProps {
    party: { partyId: string; partyName: string };
    subLedgers: SubLedgerGroup[];
    getTranslatedLabel: (key: string, def: string) => string;
    isFetching: boolean;
    currency: string;
}

export const PartySubLedgerExcel: React.FC<SubLedgerExcelProps> = ({
                                                                       party,
                                                                       subLedgers,
                                                                       getTranslatedLabel,
                                                                       isFetching,
                                                                       currency,
                                                                   }) => {
    const generate = useCallback(async () => {
        const wb = new ExcelJS.Workbook();
        wb.creator = 'Golden Land System';

        subLedgers.forEach((group, idx) => {
            const sheetName = `${group.roleTypeId || 'Unknown'}_${group.glAccountId}`.slice(0, 31);
            const ws = wb.addWorksheet(sheetName, { views: [{ rightToLeft: true }] });

            // Title
            ws.addRow([`دفتر الأستاذ الفرعي - ${party.partyName} - حساب ${group.glAccountId} (${group.accountNameArabic || group.roleTypeId})`]);
            ws.mergeCells('A1:G1');
            ws.getRow(1).font = { size: 16, bold: true };

            // Headers
            const headers = ['التاريخ', 'البيان', 'رقم القيد', 'رقم الدفعة', 'مدين', 'دائن', 'الرصيد'];
            ws.addRow(headers);
            ws.getRow(2).font = { bold: true };
            ws.columns = [
                { width: 14 }, { width: 50 }, { width: 18 }, { width: 18 },
                { width: 16, style: { numFmt: '#,##0.00' } },
                { width: 16, style: { numFmt: '#,##0.00' } },
                { width: 18, style: { numFmt: '#,##0.00' } },
            ];

            let running = 0;
            group.entries.forEach(e => {
                const debit  = e.debitCreditFlag === 'D' ? e.amount : 0;
                const credit = e.debitCreditFlag === 'C' ? e.amount : 0;

                ws.addRow([
                    e.transactionDate?.split('T')[0] ?? '',
                    e.description,
                    e.transactionId,
                    e.paymentId || '',
                    debit,
                    credit,
                    e.runningBalance
                ]);
            });

            // Final row
            const lastRow = ws.addRow(['', 'الرصيد النهائي', '', '', '', '', group.finalBalance]);
            lastRow.font = { bold: true, size: 13 };
        });

        const buffer = await wb.xlsx.writeBuffer();
        const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        const today = new Date().toISOString().slice(0,10);
        saveAs(blob, `دفتر_أستاذ_فرعي_${party.partyName}_${today}.xlsx`);
    }, [subLedgers, party, currency]);

    return (
        <Button
            variant="contained"
            onClick={generate}
            disabled={isFetching || subLedgers.length === 0}
        >
            تصدير دفتر الأستاذ الفرعي Excel
        </Button>
    );
};

export interface SubLedgerEntry {
    transactionDate: string;         // parsed from string
    transactionId: string;
    description: string;
    debitCreditFlag: string;
    amount: number;
    currencyUomId: string;
    glAccountId: string;
    paymentId: string | null;
    glAccountTypeId: string | null;
    roleTypeId: string | null;
    runningBalance: number;
}

export interface SubLedgerGroup {
    roleTypeId: string | null;
    glAccountId: string;
    glAccountTypeId: string | null;
    accountNameArabic: string | null;
    entries: SubLedgerEntry[];
    finalBalance: number;
}

export interface PartySubLedgerResponse {
    partyId: string;
    currencyUomId: string;
    subLedgers: SubLedgerGroup[];
}