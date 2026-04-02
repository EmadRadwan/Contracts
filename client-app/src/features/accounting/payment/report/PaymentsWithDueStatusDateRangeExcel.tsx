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
import {useLazyFetchPaymentsWithDueStatusByDateRangeQuery} from "../../../../app/store/apis";

interface PaymentsWithDueStatusDateRangeExcelProps {
    companyName: string;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? '0.00' : v.toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

export const PaymentsWithDueStatusDateRangeExcel: React.FC<PaymentsWithDueStatusDateRangeExcelProps> = ({
                                                                                  companyName,
                                                                                  getTranslatedLabel,
                                                                              }) => {
    const [open, setOpen] = useState(false);
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf('month'));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [trigger, { isFetching }] = useLazyFetchPaymentsWithDueStatusByDateRangeQuery();
    const [isGenerating, setIsGenerating] = useState(false);

    const handleOpen = () => setOpen(true);
    const handleClose = () => setOpen(false);

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
        const ws = workbook.addWorksheet('Payments Due Status');
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.views = [{ rightToLeft: true }];

        ws.getRow(1).height = logoBuffer ? 75 : 30;

        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: 'jpeg' });
            ws.addImage(imageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 120, height: 100 },
            });
        }

        const startRow = logoBuffer ? 5 : 3;

        const title = utils.rtlEmbed(
            getTranslatedLabel(
                'accounting.payments.report.dueStatus.daterange.title',
                `Payments Due Status Report (${startDate?.format('DD/MM/YYYY') || ''} - ${endDate?.format('DD/MM/YYYY') || ''})`
            )
        );
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:P${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(startRow).height = 40;

        const headerRowNum = startRow + 2;
        const headers = [
            getTranslatedLabel('accounting.payments.list.paymentId', 'Payment Number'),
            getTranslatedLabel('accounting.payments.list.paymentType', 'Payment Type'),
            getTranslatedLabel('accounting.payments.list.orderId', 'Order ID'),
            getTranslatedLabel('accounting.payments.list.certificateNumber', 'Certificate Number'),
            getTranslatedLabel('accounting.payments.list.from', 'From Party'),
            getTranslatedLabel('accounting.payments.list.to', 'To Party'),
            getTranslatedLabel('accounting.payments.list.date', 'Payment Date'),
            getTranslatedLabel('accounting.payments.list.dueStatus', 'Due Status'),
            getTranslatedLabel('accounting.payments.list.status', 'Status'),
            getTranslatedLabel('accounting.payments.list.buildingNumber', 'Building Number'),
            getTranslatedLabel('accounting.payments.list.productId', 'Product ID'),
            getTranslatedLabel('accounting.payments.list.bankName', 'Bank Name'),
            getTranslatedLabel('accounting.payments.list.chequeNumber', 'chequeNumber'),
            getTranslatedLabel('accounting.payments.list.amount', 'Amount'),
            getTranslatedLabel('accounting.payments.list.projectName', 'Project'),
            getTranslatedLabel('accounting.payments.list.comments', 'Comments'),
        ];

        const headerRow = ws.getRow(headerRowNum);
        headers.forEach((h, i) => {
            const cell = headerRow.getCell(i + 1);
            cell.value = utils.rtlEmbed(h);
            cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
            cell.font = { name: 'Amiri', bold: true };
            cell.alignment = { horizontal: 'center', vertical: 'middle' };
            cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
        });

        data.data.forEach((p, idx) => {
            const row = ws.getRow(headerRowNum + 1 + idx);
            const values = [
                p.paymentId,
                p.paymentTypeDescription,
                p.orderId,
                p.certificateNumber,
                p.partyIdFromName,
                p.partyIdToName,
                utils.formatDate(p.effectiveDate),
                p.dueStatusArabic,
                p.statusDescription,
                p.buildingNumber,
                p.productId,
                p.bankName,
                p.chequeNumber,
                p.amount,
                p.projectName,
                p.comments,
            ];

            values.forEach((v, i) => {
                const cell = row.getCell(i + 1);
                cell.value = typeof v === 'string' ? utils.rtlEmbed(v) : v;
                cell.font = { name: 'Amiri' };
                cell.alignment = { horizontal: i === 13 ? 'right' : 'center', vertical: 'middle' };
                cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
                if (i === 13) cell.numFmt = '#,##0.00';
            });

            // Colorize Due Status cell (column 8)
            const dueStatusCell = row.getCell(8);
            if (p.statusId === "PMNT_NOT_PAID") {
                const days = p.daysUntilDue ?? 0;
                if (days < 0) {
                    dueStatusCell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFFFEBEE' } }; // Light red
                    dueStatusCell.font = { color: { argb: 'FFC62828' }, bold: true, name: 'Amiri' };
                } else if (days <= 7) {
                    dueStatusCell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFFFF3E0' } }; // Light orange
                    dueStatusCell.font = { color: { argb: 'FFEF6C00' }, bold: true, name: 'Amiri' };
                } else {
                    dueStatusCell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE8F5E8' } }; // Light green
                    dueStatusCell.font = { color: { argb: 'FF2E7D32' }, bold: true, name: 'Amiri' };
                }
            }
        });

        ws.columns.forEach(col => { col.width = 20; });
        ws.getColumn(14).width = 15; // Amount
        ws.getColumn(8).width = 30; // Due Status
        ws.getColumn(16).width = 40; // Comments

        return workbook;
    }, [getTranslatedLabel, startDate, endDate]);

    const handleGenerate = async () => {
        if (!startDate || !endDate) return;
        setIsGenerating(true);
        try {
            const response = await trigger({
                fromDate: startDate.format('YYYY-MM-DD'),
                toDate: endDate.format('YYYY-MM-DD'),
            }).unwrap();

            const wb = await generateExcel(response);
            if (wb) {
                const buffer = await wb.xlsx.writeBuffer();
                saveAs(new Blob([buffer]), `PaymentsDueStatus_${startDate.format('YYYY-MM-DD')}_to_${endDate.format('YYYY-MM-DD')}.xlsx`);
            }
        } catch (e) {
            console.error('Export failed:', e);
        } finally {
            setIsGenerating(false);
            setOpen(false);
        }
    };

    return (
        <LocalizationProvider dateAdapter={AdapterDayjs}>
            <Button
                variant="contained"
                color="primary"
                onClick={handleOpen}
                className="btn-primary"
                style={{ marginLeft: '10px' }}
            >
                {getTranslatedLabel('accounting.payments.list.exportByDateRange', 'Export to Excel by Date Range')}
            </Button>

            <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
                <DialogTitle>{getTranslatedLabel('accounting.payments.report.daterange.select', 'Select Date Range')}</DialogTitle>
                <DialogContent>
                    <Box sx={{ display: 'flex', gap: 2, mt: 2, flexDirection: 'column' }}>
                        <DesktopDatePicker
                            label={getTranslatedLabel('accounting.payments.report.daterange.start', 'Start Date')}
                            value={startDate}
                            onChange={(newValue) => setStartDate(newValue)}
                        />
                        <DesktopDatePicker
                            label={getTranslatedLabel('accounting.payments.report.daterange.end', 'End Date')}
                            value={endDate}
                            onChange={(newValue) => setEndDate(newValue)}
                        />
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose} disabled={isGenerating}>
                        {getTranslatedLabel('common.cancel', 'Cancel')}
                    </Button>
                    <Button
                        onClick={handleGenerate}
                        variant="contained"
                        disabled={isGenerating || isFetching}
                        startIcon={(isGenerating || isFetching) && <CircularProgress size={20} />}
                    >
                        {getTranslatedLabel('common.export', 'Export')}
                    </Button>
                </DialogActions>
            </Dialog>
        </LocalizationProvider>
    );
};
