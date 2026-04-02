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
import {useLazyFetchAcctTransEntriesByDateRangeQuery} from "../../../../app/store/apis";

interface AccountingTransactionEntriesDateRangeExcelProps {
    companyId: string;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? 'N/A' : v.toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

export const AccountingTransactionEntriesDateRangeExcel: React.FC<AccountingTransactionEntriesDateRangeExcelProps> = ({
                                                                                  companyId,
                                                                                  getTranslatedLabel,
                                                                              }) => {
    const [open, setOpen] = useState(false);
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf('month'));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [trigger, { isFetching }] = useLazyFetchAcctTransEntriesByDateRangeQuery();
    const [isGenerating, setIsGenerating] = useState(false);

    const handleOpen = () => setOpen(true);
    const handleClose = () => {
        setOpen(false);
    };

    const generateExcel = useCallback(async (data: { data: any[]; total: number }) => {
        if (!data || data.data.length === 0) return null;

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'Golden Land System';

        let logoBuffer: ArrayBuffer | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) logoBuffer = await resp.blob().then(b => b.arrayBuffer());
        } catch (e) {
            console.warn('Logo not found:', e);
        }

        const period = `${startDate?.format('YYYY-MM-DD') || 'start'}_to_${endDate?.format('YYYY-MM-DD') || 'end'}`;
        const safeSheetName = `Acct Trans Entries ${period}`.replace(/[*\?\\:\[\]\/]/g, '_').slice(0, 31);
        const ws = workbook.addWorksheet(safeSheetName);
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.views = [{ rightToLeft: true }];

        ws.getRow(1).height = logoBuffer ? 75 : 30;

        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: 'jpeg' });
            ws.addImage(imageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 120, height: 100 },
            });
        } else {
            ws.getCell('A1').value = 'Golden Land';
            ws.getCell('A1').font = { name: 'Amiri', size: 16, bold: true };
        }

        const startRow = logoBuffer ? 5 : 3;

        const title = utils.rtlEmbed(
            getTranslatedLabel(
                'accounting.orgGL.accounting.summary.report.daterange.title',
                `Accounting Transaction Entries (${startDate?.format('DD/MM/YYYY') || ''} - ${endDate?.format('DD/MM/YYYY') || ''})`
            )
        );
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:P${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(startRow).height = 40;

        const headerRowNum = startRow + 2;
        const headers = [
            getTranslatedLabel('accounting.orgGL.accounting.summary.txns.acctgTransId', 'Acctg Trans Id'),
            getTranslatedLabel('accounting.payments.list.amount', 'Amount'),
            getTranslatedLabel('accounting.orgGL.accounting.summary.txnEntries.acctgTransEntrySeqId', 'SEQ Id'),
            'D/C',
            getTranslatedLabel('accounting.orgGL.accounting.summary.txns.transactionDate', 'Transaction Date'),
            getTranslatedLabel('accounting.orgGL.accounting.summary.txnEntries.glAccountId', 'Gl Account Id'),
            getTranslatedLabel('accounting.orgGL.accounting.summary.txnEntries.glAccountName', 'Gl Account Name'),
            getTranslatedLabel('accounting.orgGL.accounting.summary.txnEntries.partyName', 'Party Name'),
            getTranslatedLabel('accounting.orgGL.accounting.summary.txns.description', 'Description'),
            getTranslatedLabel('accounting.orgGL.accounting.summary.txns.salesRequestId', 'Sales Request Id'),
            getTranslatedLabel('accounting.invoices.list.invoiceId', 'Invoice Id'),
            getTranslatedLabel('accounting.payments.list.paymentId', 'Payment Id'),
            getTranslatedLabel('accounting.orgGL.accounting.summary.txnEntries.workEffortId', 'WorkEffort Id'),
            getTranslatedLabel('accounting.orgGL.accounting.summary.txns.productName', 'Product Name'),
            getTranslatedLabel('accounting.orgGL.accounting.summary.txns.isPosted', 'Is Posted'),
            getTranslatedLabel('accounting.orgGL.accounting.summary.txns.acctgTransType', 'Acctg Trans Type'),
        ];

        ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

        const dataStartRow = headerRowNum + 1;
        data.data.forEach((entry: any) => {
            const row = ws.addRow([
                utils.rtlEmbed(utils.safeString(entry.acctgTransId)),
                utils.formatNumber(entry.amount),
                utils.rtlEmbed(utils.safeString(entry.acctgTransEntrySeqId)),
                utils.rtlEmbed(utils.safeString(entry.debitCreditFlag)),
                utils.formatDate(entry.transactionDate),
                utils.rtlEmbed(utils.safeString(entry.glAccountId)),
                utils.rtlEmbed(utils.safeString(entry.glAccountName)),
                utils.rtlEmbed(utils.safeString(entry.partyName ?? '')),
                utils.rtlEmbed(utils.safeString(entry.description ?? '')),
                utils.rtlEmbed(utils.safeString(entry.salesRequestId ?? '')),
                utils.rtlEmbed(utils.safeString(entry.invoiceId ?? '')),
                utils.rtlEmbed(utils.safeString(entry.paymentId ?? '')),
                utils.rtlEmbed(utils.safeString(entry.workEffortId ?? '')),
                utils.rtlEmbed(utils.safeString(entry.productName ?? '')),
                utils.rtlEmbed(utils.safeString(entry.isPosted ?? '')),
                utils.rtlEmbed(utils.safeString(entry.acctgTransactionTypeDescription ?? '')),
            ]);
            row.font = { name: 'Amiri', size: 10 };
            row.alignment = { horizontal: 'right', wrapText: true };
            row.height = 22;
        });

        ws.columns = [
            { width: 15 }, // A Acctg Trans Id
            { width: 15 }, // B Amount
            { width: 10 }, // C SEQ Id
            { width: 8 },  // D D/C
            { width: 15 }, // E Transaction Date
            { width: 15 }, // F Gl Account Id
            { width: 30 }, // G Gl Account Name
            { width: 25 }, // H Party Name
            { width: 35 }, // I Description
            { width: 15 }, // J Sales Request Id
            { width: 15 }, // K Invoice Id
            { width: 15 }, // L Payment Id
            { width: 15 }, // M WorkEffort Id
            { width: 20 }, // N Product Name
            { width: 10 }, // O Is Posted
            { width: 20 }, // P Acctg Trans Type
        ];
        ws.getColumn(2).numFmt = '#,##0.00'; // Column B

        return await workbook.xlsx.writeBuffer();
    }, [getTranslatedLabel, startDate, endDate]);

    const handleDownload = useCallback(async () => {
        if (!startDate || !endDate) {
            alert('Please select both dates.');
            return;
        }
        setIsGenerating(true);
        try {
            const result = await trigger({
                companyId,
                fromDate: startDate.format('YYYY-MM-DD'),
                toDate: endDate.format('YYYY-MM-DD'),
            }).unwrap();

            const buffer = await generateExcel(result);
            if (buffer) {
                const fileName = `AcctTransEntries_${startDate.format('YYYYMMDD')}_to_${endDate.format('YYYYMMDD')}.xlsx`;
                const blob = new Blob([buffer], {
                    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                });
                saveAs(blob, fileName);
            }
            handleClose();
        } catch (err) {
            console.error('Excel generation failed:', err);
            alert('Failed to generate report. Please try again.');
        } finally {
            setIsGenerating(false);
        }
    }, [trigger, generateExcel, companyId, startDate, endDate]);

    const isLoading = isFetching || isGenerating;

    return (
        <>
            <Button variant="contained" color="success" onClick={handleOpen} sx={{ ml: 1 }}>
                {getTranslatedLabel('accounting.payments.report.daterange.excel', 'Export by Date Range')}
            </Button>

            <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel('accounting.payments.report.daterange.title', 'Select Date Range for Export')}
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
                        disabled={isLoading || !startDate || !endDate}
                        startIcon={isLoading ? <CircularProgress size={20} /> : null}
                    >
                        {isLoading ? getTranslatedLabel('common.generating', 'Generating...') : getTranslatedLabel('common.download', 'Download')}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
};
