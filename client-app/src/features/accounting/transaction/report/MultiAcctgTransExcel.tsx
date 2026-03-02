// src/features/accounting/transaction/report/MultiAcctgTransExcel.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';

interface TransEntry {
    id: string;
    debitGlAccountId?: string;
    creditGlAccountId?: string;
    amount: number;
    description?: string;
    debitCreditFlag: "D" | "C";
}

interface MultiAcctgTransExcelProps {
    companyName: string;
    transactionId: string;
    headerValues: {
        transactionDate: Date;
        headerDescription: string;
        party: { fromPartyId: string; fromPartyName: string } | null;
        acctgTransTypeId: string | null;
    };
    entries: TransEntry[];
    acctgTransTypes: any[];
    accountMap: Map<string, any>;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    language: string;
}

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? 'N/A' : v.toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

export const MultiAcctgTransExcel: React.FC<MultiAcctgTransExcelProps> = ({
                                                                             companyName,
                                                                             transactionId,
                                                                             headerValues,
                                                                             entries,
                                                                             acctgTransTypes,
                                                                             accountMap,
                                                                             getTranslatedLabel,
                                                                             language,
                                                                         }) => {
    const localizationKey = "accounting.orgGL.accounting.trans.multi";

    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        // --- FETCH LOGO ---
        let logoBuffer: ArrayBuffer | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) {
                logoBuffer = await (await resp.blob()).arrayBuffer();
            }
        } catch (e) {
            console.warn('Logo fetch error:', e);
        }

        const safeSheet = companyName
            ? companyName.replace(/[*\?\\:\[\]\/]/g, '_').trim().slice(0, 31)
            : 'Transaction';
        const ws = workbook.addWorksheet(safeSheet, { views: [{ rightToLeft: language === 'ar' }] });
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.getColumn(1).font = { name: 'Amiri', size: 10 };

        // === ADD LOGO ===
        if (logoBuffer) {
            const imageId = workbook.addImage({
                buffer: logoBuffer,
                extension: 'jpeg',
            });
            const logoRow = ws.getRow(1);
            logoRow.height = 75;
            ws.addImage(imageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 100, height: 100 },
            });
            ws.getRow(2).height = 20;
            ws.getRow(3).height = 20;
            ws.addRow([]); ws.addRow([]); ws.addRow([]);
        } else {
            const fallbackRow = ws.getRow(1);
            fallbackRow.getCell(1).value = 'Logo Unavailable';
            fallbackRow.font = { name: 'Amiri', size: 10, color: { argb: 'FFFF0000' } };
            fallbackRow.alignment = { horizontal: 'center', vertical: 'middle' };
        }

        const startRow = logoBuffer ? 5 : 2;

        // === TITLE ===
        const transType = acctgTransTypes?.find(t => t.acctgTransTypeId === headerValues.acctgTransTypeId);
        const title = utils.rtlEmbed(getTranslatedLabel(`${localizationKey}.title`, 'Accounting Transaction')) + ' - ' + transactionId;
        ws.addRow([title]);
        ws.mergeCells(`A${startRow}:F${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 16, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.addRow([]); // spacer

        // === HEADER DATA ===
        const headerStart = ws.lastRow!.number + 1;
        ws.addRow([
            getTranslatedLabel(`${localizationKey}.transactionDate`, 'Transaction Date'),
            getTranslatedLabel(`${localizationKey}.acctgTransType`, 'Transaction Type'),
            getTranslatedLabel(`${localizationKey}.headerDescription`, 'Header Description'),
            getTranslatedLabel(`${localizationKey}.party`, 'Employee/Party'),
        ]);
        const headerLabelRow = ws.getRow(headerStart);
        headerLabelRow.font = { name: 'Amiri', size: 10, bold: true };
        headerLabelRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        headerLabelRow.alignment = { horizontal: 'center' };

        ws.addRow([
            utils.formatDate(headerValues.transactionDate),
            utils.rtlEmbed(transType?.description || headerValues.acctgTransTypeId || ''),
            utils.rtlEmbed(headerValues.headerDescription || ''),
            utils.rtlEmbed(headerValues.party?.fromPartyName || ''),
        ]);
        ws.getRow(ws.lastRow!.number).font = { name: 'Amiri', size: 10 };
        ws.getRow(ws.lastRow!.number).alignment = { horizontal: 'center' };
        ws.addRow([]); // spacer

        // === ENTRIES ===
        const entriesStart = ws.lastRow!.number + 1;
        ws.addRow([getTranslatedLabel(`${localizationKey}.entries`, 'Transaction Entries')]);
        ws.mergeCells(`A${entriesStart}:F${entriesStart}`);
        ws.getRow(entriesStart).font = { name: 'Amiri', size: 12, bold: true };
        ws.getRow(entriesStart).alignment = { horizontal: 'center' };

        ws.addRow([
            getTranslatedLabel(`${localizationKey}.glAccount`, 'GL Account'),
            getTranslatedLabel(`${localizationKey}.amount`, 'Amount'),
            getTranslatedLabel(`${localizationKey}.debitCredit`, 'Debit/Credit'),
            getTranslatedLabel(`${localizationKey}.description`, 'Description'),
        ]);
        const entriesHeaderRow = ws.getRow(ws.lastRow!.number);
        entriesHeaderRow.font = { name: 'Amiri', size: 10, bold: true };
        entriesHeaderRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'E6F3FF' } };
        entriesHeaderRow.alignment = { horizontal: 'center' };
        entriesHeaderRow.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

        entries.forEach(entry => {
            const glAccountId = entry.debitGlAccountId || entry.creditGlAccountId;
            const glAccount = accountMap.get(glAccountId || '');
            const glAccountText = glAccount?.text || glAccount?.accountName || glAccountId || '-';

            ws.addRow([
                utils.rtlEmbed(glAccountText),
                entry.amount || 0,
                entry.debitCreditFlag === 'D' ? getTranslatedLabel('accounting.orgGL.accounting.initialBalance.debit', 'Debit') : getTranslatedLabel('accounting.orgGL.accounting.initialBalance.credit', 'Credit'),
                utils.rtlEmbed(entry.description || ''),
            ]);
        });
        ws.getRow(ws.lastRow!.number).eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

        // === TOTALS ===
        const totalDebit = entries.filter(e => e.debitCreditFlag === 'D').reduce((sum, e) => sum + (e.amount || 0), 0);
        const totalCredit = entries.filter(e => e.debitCreditFlag === 'C').reduce((sum, e) => sum + (e.amount || 0), 0);

        ws.addRow([]);
        const totalsRow = ws.addRow([
            getTranslatedLabel(`${localizationKey}.totalDebit`, 'Total Debit') + ': ' + totalDebit.toFixed(2),
            getTranslatedLabel(`${localizationKey}.totalCredit`, 'Total Credit') + ': ' + totalCredit.toFixed(2),
        ]);
        totalsRow.font = { name: 'Amiri', size: 10, bold: true, color: { argb: '1565C0' } };

        // === COLUMN WIDTHS ===
        ws.columns = [
            { width: 40 }, // GL Account
            { width: 15 }, // Amount
            { width: 15 }, // Debit/Credit
            { width: 50 }, // Description
            { width: 20 },
            { width: 20 },
        ];
        ws.getColumn(2).numFmt = '#,##0.00';

        return await workbook.xlsx.writeBuffer();
    }, [companyName, transactionId, headerValues, entries, acctgTransTypes, accountMap, getTranslatedLabel, language]);

    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            saveAs(blob, `AcctgTrans_${transactionId}.xlsx`);
        }
    }, [generateExcel, transactionId]);

    return (
        <Button
            variant="contained"
            color="success"
            onClick={handleDownload}
            sx={{ mr: 1 }}
        >
            {getTranslatedLabel(
                'projects.multiPaymentCertificate.report.excel',
                'Export Excel'
            )}
        </Button>
    );
};
