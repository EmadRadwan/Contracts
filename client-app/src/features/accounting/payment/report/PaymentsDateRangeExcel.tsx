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
import {useLazyFetchPaymentsByDateRangeQuery} from "../../../../app/store/apis";

interface PaymentsDateRangeExcelProps {
    companyName: string;
    paymentType: 'incoming' | 'outgoing';
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? 'N/A' : v.toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

export const PaymentsDateRangeExcel: React.FC<PaymentsDateRangeExcelProps> = ({
                                                                                  companyName,
                                                                                  paymentType,
                                                                                  getTranslatedLabel,
                                                                              }) => {
    const [open, setOpen] = useState(false);
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf('month'));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [trigger, { isFetching }] = useLazyFetchPaymentsByDateRangeQuery();
    const [isGenerating, setIsGenerating] = useState(false);

    const handleOpen = () => setOpen(true);
    const handleClose = () => {
        setOpen(false);
        // Reset dates if needed, but optional
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
        const safeSheetName = `${paymentType} Payments ${period}`.replace(/[*\?\\:\[\]\/]/g, '_').slice(0, 31);
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
                'accounting.payments.report.daterange.title',
                `${paymentType === 'incoming' ? 'Incoming' : 'Outgoing'} Payments (${startDate?.format('DD/MM/YYYY') || ''} - ${endDate?.format('DD/MM/YYYY') || ''})`
            )
        );
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:O${startRow}`); // Adjusted to 15 columns
        ws.getRow(startRow).font = { name: 'Amiri', size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(startRow).height = 40;

        const headerRowNum = startRow + 2;
        const headers = [
            getTranslatedLabel('accounting.payments.list.paymentId', 'Payment Number'),
            getTranslatedLabel('accounting.payments.list.paymentRefNum', 'Reference Number'),
            getTranslatedLabel('accounting.payments.list.paymentType', 'Payment Type'),
            getTranslatedLabel('accounting.payments.list.orderId', 'Order ID'),
            getTranslatedLabel('accounting.payments.list.productId', 'Product ID'),
            getTranslatedLabel('accounting.payments.list.buildingNumber', 'Building Number'),
            getTranslatedLabel('accounting.payments.list.certificateNumber', 'Certificate Number'),
            getTranslatedLabel('projects.certificate.form.project', 'Project'),
            getTranslatedLabel('accounting.payments.form.costCenter', 'Cost Center'),
            getTranslatedLabel('accounting.payments.list.from', 'From Party'),
            getTranslatedLabel('accounting.payments.list.to', 'To Party'),
            getTranslatedLabel('accounting.payments.list.date', 'Payment Date'),
            getTranslatedLabel('accounting.payments.list.status', 'Status'),
            getTranslatedLabel('accounting.payments.list.amount', 'Amount'),
            getTranslatedLabel('accounting.payments.list.comments', 'Comments'),
        ];

        ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

        const dataStartRow = headerRowNum + 2;
        data.data.forEach((payment: any) => {
            const row = ws.addRow([
                utils.rtlEmbed(utils.safeString(payment.paymentId)),
                utils.rtlEmbed(utils.safeString(payment.paymentRefNum ?? '')),
                utils.rtlEmbed(utils.safeString(payment.paymentTypeDescription)),
                utils.rtlEmbed(utils.safeString(payment.orderId ?? '')),
                utils.rtlEmbed(utils.safeString(payment.productId ?? '')),
                utils.rtlEmbed(utils.safeString(payment.buildingNumber ?? '')),
                utils.rtlEmbed(utils.safeString(payment.certificateNumber ?? '')),
                utils.rtlEmbed(utils.safeString(payment.projectName ?? '')),
                utils.rtlEmbed(utils.safeString(payment.costCenterDescription ?? '')),
                utils.rtlEmbed(utils.safeString(payment.partyIdFromName)),
                utils.rtlEmbed(utils.safeString(payment.partyIdToName)),
                utils.formatDate(payment.effectiveDate),
                utils.rtlEmbed(utils.safeString(payment.statusDescription)),
                utils.formatNumber(payment.amount),
                utils.rtlEmbed(utils.safeString(payment.comments ?? '')),
            ]);
            row.font = { name: 'Amiri', size: 10 };
            row.alignment = { horizontal: 'right', wrapText: true };
            row.height = 22;
        });

        if (data.data.length > 0) {
            const totalAmount = data.data.reduce((sum: number, p: any) => sum + (p.amount || 0), 0);
            const totalRowNum = dataStartRow + data.data.length;
            ws.addRow([
                '', '', '', '', '', '', '',
                utils.rtlEmbed(getTranslatedLabel('common.total', 'Total')),
                '', '', '', '', '', utils.formatNumber(totalAmount), ''
            ]);
            ws.mergeCells(`H${totalRowNum}:M${totalRowNum}`);
            ws.getRow(totalRowNum).font = { name: 'Amiri', size: 12, bold: true };
            ws.getCell(`N${totalRowNum}`).font = { bold: true };
        }

        ws.columns = [
            { width: 15 }, // A Payment ID
            { width: 20 }, // B Ref Num
            { width: 22 }, // C Type
            { width: 15 }, // D Order ID
            { width: 15 }, // E Product ID
            { width: 18 }, // F Building Number
            { width: 20 }, // G Cert
            { width: 30 }, // H Project
            { width: 28 }, // I Cost Center
            { width: 28 }, // J From
            { width: 28 }, // K To
            { width: 15 }, // L Date
            { width: 15 }, // M Status
            { width: 16 }, // N Amount
            { width: 35 }  // O Comments
        ];
        ws.getColumn(14).numFmt = '#,##0.00'; // Column N (index 14)

        return await workbook.xlsx.writeBuffer();
    }, [getTranslatedLabel, paymentType, startDate, endDate]);

    const handleDownload = useCallback(async () => {
        if (!startDate || !endDate) {
            alert('Please select both dates.');
            return;
        }
        setIsGenerating(true);
        try {
            const result = await trigger({
                paymentType,
                fromDate: startDate.format('YYYY-MM-DD'),
                toDate: endDate.format('YYYY-MM-DD'),
            }).unwrap();

            const buffer = await generateExcel(result);
            if (buffer) {
                const fileName = `${paymentType}_Payments_${startDate.format('YYYYMMDD')}_to_${endDate.format('YYYYMMDD')}.xlsx`;
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
    }, [trigger, generateExcel, paymentType, startDate, endDate]);

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