// src/features/party/report/PartyFinancialHistoryExcel.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

// REFACTOR: Mirror WorkmanshipCertificateExcel structure
// Purpose: Reuse proven ExcelJS pattern with RTL, logo, Amiri font, numFmt
// Improvement: Ensures consistency, reduces bugs, leverages sharedUtils
// Context: Client wants exact match to "ميتال تك" sheet

// REFACTOR: Define LedgerRow to represent flattened invoice/payment rows
// Purpose: Normalize data for Excel row generation
// Improvement: Enables running balance calculation and clean mapping
// Context: Matches "فاتورة → دفعة → رصيد" flow from ميتال تك
interface LedgerRow {
    date: string | number; // Excel serial date or ISO string
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

// REFACTOR: Reuse shared utilities from certificate report
// Purpose: Centralize RTL, formatting, safety
// Improvement: Prevents duplication, ensures Arabic correctness
// Context: Critical for bidi text (e.g., "INV123" not reversed)
const sharedUtils = {
    safeString: (value: any): string => {
        if (value === null || value === undefined) return 'N/A';
        if (typeof value === 'object') return 'N/A';
        return String(value);
    },
    rtlEmbed: (text: string): string => {
        return /\p{Script=Arabic}/u.test(text) ? `\u202B${text}` : text;
    },
    formatNumber: (value: number | undefined, decimals: number = 2): string => {
        if (value === undefined || value === null) return 'N/A';
        return value.toLocaleString('en-US', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals,
        });
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
    // REFACTOR: Memoize generateExcel to prevent re-creation
    // Purpose: Optimize performance, avoid unnecessary fetches
    // Improvement: useCallback ensures stable reference
    // Context: Matches WorkmanshipCertificateExcel pattern
    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        // REFACTOR: Validate input before proceeding
        // Purpose: Prevent empty or invalid Excel
        // Improvement: Early exit with console warning
        // Context: Matches validateItems() in certificate
        if (!ledgerItems || ledgerItems.length === 0 || isFetching) {
            console.warn('Cannot generate Excel: No ledger items or fetching');
            return null;
        }

        // REFACTOR: Fetch logo (same as certificate)
        // Purpose: Brand consistency
        // Improvement: Graceful fallback
        // Context: Reuse from WorkmanshipCertificateExcel
        let logoImageId: number | null = null;
        try {
            const response = await fetch('/goldenlandlogo.jpg');
            if (!response.ok) throw new Error('Logo fetch failed');
            const blob = await response.blob();
            const arrayBuffer = await blob.arrayBuffer();
            logoImageId = workbook.addImage({
                buffer: arrayBuffer,
                extension: 'jpeg',
            });
        } catch (error) {
            console.warn('Logo fetch failed:', error);
        }

        const worksheet = workbook.addWorksheet(party.partyName || 'Financial History');
        worksheet.pageSetup = { paperSize: 9, orientation: 'landscape' };
        worksheet.views = [{ rightToLeft: true }];
        worksheet.getColumn(1).font = { name: 'Amiri', size: 10 };

        // REFACTOR: Add logo if available
        // Purpose: Visual branding
        // Improvement: Same layout as certificate
        if (logoImageId !== null) {
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
        } else {
            worksheet.addRow(['Logo Unavailable']);
            worksheet.getRow(1).font = { name: 'Amiri', size: 10, color: { argb: 'FF0000' } };
            worksheet.getRow(1).alignment = { horizontal: 'center', vertical: 'middle' };
        }

        // REFACTOR: Header - Party Name
        // Purpose: Match "حسابات ميتال تك احمد رسلان"
        // Improvement: RTL embedded, centered
        worksheet.addRow([getTranslatedLabel('party.financial.history.title', 'Financial History Accounts') + ': ' + sharedUtils.rtlEmbed(sharedUtils.safeString(party.partyName))]);
        worksheet.mergeCells(`A${logoImageId !== null ? 4 : 2}:K${logoImageId !== null ? 4 : 2}`);
        worksheet.getRow(logoImageId !== null ? 4 : 2).font = { name: 'Amiri', size: 14, bold: true };
        worksheet.getRow(logoImageId !== null ? 4 : 2).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };

        worksheet.addRow([]);
        worksheet.addRow([]);

        // REFACTOR: Table Headers - Exact match to ميتال تك
        // Purpose: Column-for-column parity
        // Improvement: Arabic labels, RTL
        const headers = [
            getTranslatedLabel('party.financial.date', 'Date'),
            getTranslatedLabel('party.financial.description', 'Description'),
            getTranslatedLabel('party.financial.invoiceNumber', 'Invoice Number'),
            getTranslatedLabel('party.financial.paymentNumber', 'Payment Number'),
            getTranslatedLabel('party.financial.value', 'Value'),
            getTranslatedLabel('party.financial.toPay', 'To Pay'),
            getTranslatedLabel('party.financial.paid', 'Paid'),
            getTranslatedLabel('party.financial.balance', 'Balance'),
            getTranslatedLabel('party.financial.notes', 'Notes'),
        ];
        worksheet.addRow(headers);
        const headerRow = worksheet.getRow(worksheet.lastRow!.number);
        headerRow.font = { name: 'Amiri', size: 10, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        headerRow.eachCell(cell => {
            cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
        });

        // REFACTOR: Column widths & number formats
        // Purpose: Match ميتال تك layout and Excel-native formatting
        // Improvement: numFmt enables commas in Excel
        worksheet.columns = [
            { width: 12 }, // Date
            { width: 40 }, // Description
            { width: 15 }, // Invoice Number
            { width: 15 }, // Payment Number
            { width: 12 }, // Value
            { width: 12 }, // To Pay
            { width: 12 }, // Paid
            { width: 12 }, // Balance
            { width: 20 }, // Notes
        ];
        worksheet.getColumn(5).numFmt = '#,##0.00';
        worksheet.getColumn(6).numFmt = '#,##0.00';
        worksheet.getColumn(7).numFmt = '#,##0.00';
        worksheet.getColumn(8).numFmt = '#,##0.00';

        // REFACTOR: Add ledger rows
        // Purpose: Generate invoice → payment → balance flow
        // Improvement: Running balance calculated in data layer
        ledgerItems.forEach((item) => {
            const rowData = [
                item.date,
                sharedUtils.rtlEmbed(sharedUtils.safeString(item.description)),
                sharedUtils.safeString(item.invoiceNumber),
                sharedUtils.safeString(item.paymentNumber),
                item.value,
                item.toPay,
                item.paid,
                item.balance,
                sharedUtils.rtlEmbed(sharedUtils.safeString(item.notes)),
            ];
            const row = worksheet.addRow(rowData);
            row.font = { name: 'Amiri', size: 9 };
            row.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
            row.eachCell(cell => {
                cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
            });
        });

        // REFACTOR: Final balance row (bold)
        // Purpose: Match last row in ميتال تك
        // Improvement: Visual emphasis
        const finalBalance = ledgerItems[ledgerItems.length - 1]?.balance || 0;
        const finalRow = worksheet.addRow([
            '',
            getTranslatedLabel('party.financial.finalBalance', 'Final Balance'),
            '', '', '', '', '',
            finalBalance,
            '',
        ]);
        finalRow.font = { name: 'Amiri', size: 10, bold: true };
        finalRow.getCell(8).font = { name: 'Amiri', size: 10, bold: true };
        finalRow.getCell(8).numFmt = '#,##0.00';

        const buffer = await workbook.xlsx.writeBuffer();
        return buffer;
    }, [party, ledgerItems, getTranslatedLabel, isFetching]);

    // REFACTOR: Handle download with saveAs
    // Purpose: Match WorkmanshipCertificateExcel
    // Improvement: Consistent UX
    const handleDownload = useCallback(async () => {
        const buffer = await generateExcel();
        if (buffer) {
            const blob = new Blob([buffer], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            const filename = `FinancialHistory_${party.partyName || party.partyId}.xlsx`;
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
                {getTranslatedLabel('party.financial.excel', 'Financial History Excel')}
            </Button>
        </div>
    );
};