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

interface GlAccountTransactionsDateRangeExcelProps {
    accountCode: string;
    accountName: string;
    openingBalance: number; // Opening balance of the whole period
    rows: TransactionRow[]; // All transactions of the whole period
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
};

export const GlAccountTransactionsDateRangeExcel: React.FC<GlAccountTransactionsDateRangeExcelProps> = ({
                                                                                                            accountCode,
                                                                                                            accountName,
                                                                                                            openingBalance: originalOpeningBalance,
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
        
        const currentOpeningBalance = originalOpeningBalance + preDebit - preCredit;
        
        const postedDebits = filteredRows.reduce((sum, r) => sum + (r.debitCreditFlag === 'D' ? r.amount : 0), 0);
        const postedCredits = filteredRows.reduce((sum, r) => sum + (r.debitCreditFlag === 'C' ? r.amount : 0), 0);
        
        const currentEndingBalance = currentOpeningBalance + postedDebits - postedCredits;

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        let logoId: number | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) {
                const buf = await (await resp.blob()).arrayBuffer();
                logoId = workbook.addImage({ buffer: buf, extension: 'jpeg' });
            }
        } catch (e) { console.warn('Logo failed', e); }

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
            
            // Note: date range running balance calculation logic
            // We assume the same perspective as currentOpeningBalance
            // which in the existing code was originalOpeningBalance + preDebit - preCredit
            runningBalance += (debit - credit);

            const row = ws.addRow([
                utils.safeString(r.acctgTransId),
                r.transactionDate,
                utils.safeString(r.acctgTransTypeId),
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
    }, [accountCode, accountName, originalOpeningBalance, allRows, getTranslatedLabel, startDate, endDate]);

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
