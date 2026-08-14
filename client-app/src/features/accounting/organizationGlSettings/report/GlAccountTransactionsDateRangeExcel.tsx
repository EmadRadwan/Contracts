import React, { useCallback, useState } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import {
    Button,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Box,
    CircularProgress,
} from '@mui/material';
import { DesktopDatePicker } from '@mui/x-date-pickers/DesktopDatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';

interface TransactionRow {
    acctgTransId: string;
    transactionDate: string;          // ISO string
    acctgTransTypeId: string;
    acctgTransTypeDescription?: string;
    invoiceId?: string;
    paymentId?: string;
    workEffortId?: string;
    partyName?: string;
    productName?: string;
    isPosted: boolean;
    debitCreditFlag: 'D' | 'C';
    amount: number;
    runningBalance: number;
    description?: string;
    projectName?: string;
    costCenterDescription?: string;
    paymentRefNum?: string;
}

// REFACTOR (2026-08-14): two fixes bundled into this component.
//
// 1) Credit-account balance direction. Every GL account has a "natural side": debit-natured
//    accounts (assets, expenses \u2014 e.g. a bank account) grow when debited and shrink when
//    credited; credit-natured accounts (liabilities, equity, revenue \u2014 e.g. accounts payable)
//    do the OPPOSITE \u2014 they grow when credited and shrink when debited. The balance math below
//    used to always compute "debit minus credit", which is only correct for debit-natured
//    accounts. On a credit-natured account it silently moved the balance the wrong direction:
//    e.g. an AP account with a 100,000 opening balance and a 20,000 payment made (a debit,
//    which should REDUCE what's owed to 80,000) would have been reported as 120,000 \u2014 the
//    payment added instead of subtracted. Bank account 110100 happens to be debit-natured, so
//    this bug was invisible there; it would only show up on liability/equity/revenue accounts.
//    Fix: every place that combines a debit and a credit amount now checks `isDebit` (passed in
//    from the backend, which already knows the account's side) and flips the subtraction order
//    for credit-natured accounts.
//
// 2) Rounding. Cell values were raw, unrounded JS floats \u2014 summing many 2-decimal amounts drifts
//    into visible tails like 12602390.450000003. The #,##0.00 Excel number format hid it on
//    screen, but the underlying cell value (and anything reading the file programmatically)
//    still carried the tail. `round2` below is applied everywhere a balance is written into a
//    cell so the stored value is clean, not just its display.
interface GlAccountTransactionsDateRangeExcelProps {
    accountCode: string;
    accountName: string;
    openingBalance: number; // Opening balance of the whole period, signed from the account's natural side
    isDebit: boolean;       // Account's natural side \u2014 balances grow with debits when true, credits when false
    rows: TransactionRow[]; // All transactions of the whole period
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    // Rounds to 2 decimal places (whole cents). Use this on every balance figure written to a
    // cell \u2014 see the file-level comment above for why raw sums aren't safe to write directly.
    round2: (n: number) => Math.round(n * 100) / 100,
};

// See GlAccountTransactionsExcel.tsx for why this exists: a non-OK fetch response used to fail
// completely silently (only thrown errors were logged). One retry covers a transient hiccup, and
// every failure path now logs enough to diagnose it.
async function fetchLogoBuffer(label: string): Promise<ArrayBuffer | null> {
    for (let attempt = 1; attempt <= 2; attempt++) {
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) {
                return await (await resp.blob()).arrayBuffer();
            }
            console.warn(`${label}: logo fetch returned ${resp.status} ${resp.statusText} (attempt ${attempt})`);
        } catch (e) {
            console.warn(`${label}: logo fetch threw (attempt ${attempt})`, e);
        }
    }
    return null;
}

export const GlAccountTransactionsDateRangeExcel: React.FC<GlAccountTransactionsDateRangeExcelProps> = ({
                                                                                                            accountCode,
                                                                                                            accountName,
                                                                                                            openingBalance: originalOpeningBalance,
                                                                                                            isDebit,
                                                                                                            rows: allRows,
                                                                                                            getTranslatedLabel,
                                                                                                        }) => {
    const [open, setOpen] = useState(false);
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf('month'));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [isGenerating, setIsGenerating] = useState(false);

    const handleOpen = () => setOpen(true);
    const handleClose = () => setOpen(false);

    const generateExcel = useCallback(async () => {
        if (!startDate || !endDate) return null;

        const filteredRows = allRows.filter(r => {
            const d = dayjs(r.transactionDate);
            return (d.isSame(startDate, 'day') || d.isAfter(startDate, 'day')) &&
                   (d.isSame(endDate, 'day') || d.isBefore(endDate, 'day'));
        });

        const rowsBefore = allRows.filter(r => dayjs(r.transactionDate).isBefore(startDate, 'day'));

        const preDebit = rowsBefore.reduce((sum, r) => sum + (r.debitCreditFlag === 'D' ? r.amount : 0), 0);
        const preCredit = rowsBefore.reduce((sum, r) => sum + (r.debitCreditFlag === 'C' ? r.amount : 0), 0);

        // Roll pre-range activity into the opening balance from the account's own natural side —
        // see the "Credit-account balance direction" note at the top of this file. originalOpeningBalance
        // itself is already signed that way by the backend (isDebit ? D−C : C−D), so this has to match.
        const currentOpeningBalance = utils.round2(
            originalOpeningBalance + (isDebit ? preDebit - preCredit : preCredit - preDebit));

        const postedDebits = utils.round2(
            filteredRows.reduce((sum, r) => sum + (r.debitCreditFlag === 'D' ? r.amount : 0), 0));
        const postedCredits = utils.round2(
            filteredRows.reduce((sum, r) => sum + (r.debitCreditFlag === 'C' ? r.amount : 0), 0));

        // Same natural-side rule applied to the ending balance.
        const currentEndingBalance = utils.round2(currentOpeningBalance + (isDebit
            ? postedDebits - postedCredits
            : postedCredits - postedDebits));

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        let logoId: number | null = null;
        const logoBuf = await fetchLogoBuffer('GlAccountTransactionsDateRangeExcel');
        if (logoBuf) {
            logoId = workbook.addImage({ buffer: logoBuf, extension: 'jpeg' });
        }

        const ws = workbook.addWorksheet(accountCode.slice(0, 31) || 'Transactions');
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.views = [{ rightToLeft: true }];
        ws.getColumn(1).font = { name: 'Amiri', size: 10 };

        if (logoId) {
            ws.addImage(logoId, { tl: { col: 0, row: 0 }, ext: { width: 120, height: 100 } });
            ws.getRow(1).height = 75; ws.getRow(2).height = 20; ws.getRow(3).height = 20;
            ws.addRow([]); ws.addRow([]); ws.addRow([]);
        } else {
            ws.addRow(['Logo Unavailable']).getCell(1).font = { color: { argb: 'FF0000' } };
        }

        const titleRow = logoId ? 4 : 2;
        const periodTitle = ` (${startDate.format('DD/MM/YYYY')} - ${endDate.format('DD/MM/YYYY')})`;
        ws.addRow([`${getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.title', 'Transaction Details')} – ${utils.rtlEmbed(utils.safeString(accountName))} (${accountCode})${periodTitle}`]);
        ws.mergeCells(`A${titleRow}:P${titleRow}`);
        ws.getRow(titleRow).font = { name: 'Amiri', size: 14, bold: true };
        ws.getRow(titleRow).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        ws.addRow([]); ws.addRow([]);

        const summaryStart = ws.lastRow!.number + 1;
        ws.addRow([getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.openingBalance', 'Opening Balance'), currentOpeningBalance]);
        ws.addRow([getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.postedDebits', 'Posted Debits'), postedDebits]);
        ws.addRow([getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.postedCredits', 'Posted Credits'), postedCredits]);
        ws.addRow([getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.endingBalance', 'Ending Balance'), currentEndingBalance]);
        ws.getCell(`A${summaryStart}`).font = { bold: true };
        ws.getCell(`A${summaryStart + 1}`).font = { bold: true };
        ws.getCell(`A${summaryStart + 2}`).font = { bold: true };
        ws.getCell(`A${summaryStart + 3}`).font = { bold: true };
        ws.getColumn(2).numFmt = '#,##0.00';
        ws.addRow([]);

        const headers = [
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.transId', 'Acctg Trans ID'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.transDate', 'Transaction Date'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.transType', 'Acctg Trans Type'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.invoiceId', 'Invoice ID'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.paymentId', 'Payment ID'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.paymentRefNum', 'Payment Ref Num'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.workEffortId', 'Work Effort ID'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.projectName', 'Project Name'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.costCenter', 'Cost Center'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.partyId', 'Party Name'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.productId', 'Product Name'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.isPosted', 'Is Posted'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.debit', 'Debit'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.credit', 'Credit'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.balance', 'Balance'),
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.description', 'Description'),
        ];
        ws.addRow(headers);
        const hRow = ws.getRow(ws.lastRow!.number);
        hRow.font = { name: 'Amiri', size: 10, bold: true };
        hRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        hRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        hRow.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

        ws.columns = [
            { width: 14 }, // transId
            { width: 18 }, // date
            { width: 16 }, // trans type
            { width: 12 }, // invoice
            { width: 12 }, // payment
            { width: 15 }, // paymentRefNum
            { width: 12 }, // work effort
            { width: 28 }, // project name
            { width: 28 }, // cost center
            { width: 18 }, // party
            { width: 18 }, // product
            { width: 10 }, // posted
            { width: 14, style: { numFmt: '#,##0.00' } }, // debit
            { width: 14, style: { numFmt: '#,##0.00' } }, // credit
            { width: 18, style: { numFmt: '#,##0.00' } }, // balance
            { width: 30 }, // description
        ];

        let runningBalance = currentOpeningBalance;
        filteredRows.forEach(r => {
            const debit = r.debitCreditFlag === 'D' ? r.amount : 0;
            const credit = r.debitCreditFlag === 'C' ? r.amount : 0;

            // Third and last spot with the same natural-side rule (see top-of-file comment): the
            // per-row "Balance" column must step in the same direction as the opening/ending totals.
            runningBalance = utils.round2(runningBalance + (isDebit ? debit - credit : credit - debit));

            const row = ws.addRow([
                utils.safeString(r.acctgTransId),
                r.transactionDate,
                utils.rtlEmbed(utils.safeString(r.acctgTransTypeDescription || r.acctgTransTypeId)),
                utils.safeString(r.invoiceId),
                utils.safeString(r.paymentId),
                utils.safeString(r.paymentRefNum),
                utils.safeString(r.workEffortId),
                utils.rtlEmbed(utils.safeString(r.projectName)),
                utils.rtlEmbed(utils.safeString(r.costCenterDescription)),
                utils.rtlEmbed(utils.safeString(r.partyName)),
                utils.rtlEmbed(utils.safeString(r.productName)),
                r.isPosted ? 'Yes' : 'No',
                debit,
                credit,
                runningBalance,
                utils.rtlEmbed(utils.safeString(r.description)),
            ]);
            row.font = { name: 'Amiri', size: 9 };
            row.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
            row.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });
        });

        const totRow = ws.addRow([
            '', '', '', '', '', '', '', '', '', '', '',
            getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.totals', 'Totals'),
            postedDebits,
            postedCredits,
            '', ''
        ]);
        totRow.font = { name: 'Amiri', size: 10, bold: true };
        totRow.getCell(13).numFmt = '#,##0.00';
        totRow.getCell(14).numFmt = '#,##0.00';

        return await workbook.xlsx.writeBuffer();
    }, [accountCode, accountName, originalOpeningBalance, isDebit, allRows, getTranslatedLabel, startDate, endDate]);

    const handleDownload = useCallback(async () => {
        setIsGenerating(true);
        try {
            const buf = await generateExcel();
            if (buf) {
                const blob = new Blob([buf], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                const fileName = `Transactions_${accountCode}_${startDate?.format('YYYYMMDD')}_to_${endDate?.format('YYYYMMDD')}.xlsx`;
                saveAs(blob, fileName);
            }
            handleClose();
        } catch (e) {
            console.error('Excel generation failed:', e);
        } finally {
            setIsGenerating(false);
        }
    }, [generateExcel, accountCode, startDate, endDate]);

    return (
        <>
            <Button
                variant="outlined"
                color="success"
                onClick={handleOpen}
                sx={{ ml: 1 }}
            >
                {getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.daterange.excel', 'تصدير حسب نطاق التاريخ')}
            </Button>

            <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel('accounting.orgGL.reports.trial-balance.transactions.daterange.title', 'اختر نطاق التاريخ للتصدير')}
                </DialogTitle>
                <DialogContent>
                    <LocalizationProvider dateAdapter={AdapterDayjs}>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, mt: 2 }}>
                            <DesktopDatePicker
                                label={getTranslatedLabel('common.fromDate', 'From Date')}
                                value={startDate}
                                onChange={(newValue) => setStartDate(newValue)}
                                slotProps={{ textField: { fullWidth: true } }}
                            />
                            <DesktopDatePicker
                                label={getTranslatedLabel('common.toDate', 'To Date')}
                                value={endDate}
                                minDate={startDate ?? undefined}
                                onChange={(newValue) => setEndDate(newValue)}
                                slotProps={{ textField: { fullWidth: true } }}
                            />
                        </Box>
                    </LocalizationProvider>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose}>{getTranslatedLabel('common.cancel', 'Cancel')}</Button>
                    <Button
                        onClick={handleDownload}
                        variant="contained"
                        disabled={isGenerating || !startDate || !endDate}
                        startIcon={isGenerating ? <CircularProgress size={20} /> : null}
                    >
                        {isGenerating ? getTranslatedLabel('common.generating', 'Generating...') : getTranslatedLabel('common.download', 'Download')}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
};
