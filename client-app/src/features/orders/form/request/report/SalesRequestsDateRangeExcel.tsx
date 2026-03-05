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
import { useLazyFetchSalesRequestsByDateRangeQuery } from "../../../../../app/store/apis/salesRequestApi";

interface SalesRequestsDateRangeExcelProps {
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined | null, dec = 2) =>
        v == null ? '0.00' : v.toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined | null) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

export const SalesRequestsDateRangeExcel: React.FC<SalesRequestsDateRangeExcelProps> = ({
                                                                                          getTranslatedLabel,
                                                                                      }) => {
    const [open, setOpen] = useState(false);
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf('month'));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [trigger, { isFetching }] = useLazyFetchSalesRequestsByDateRangeQuery();
    const [isGenerating, setIsGenerating] = useState(false);

    const handleOpen = () => setOpen(true);
    const handleClose = () => {
        setOpen(false);
    };

    const generateExcel = useCallback(async (data: any[]) => {
        if (!data || data.length === 0) return null;

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
        const safeSheetName = `Sales Requests ${period}`.replace(/[*\?\\:\[\]\/]/g, '_').slice(0, 31);
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
                'salesRequest.report.daterange.title',
                `Sales Requests (${startDate?.format('DD/MM/YYYY') || ''} - ${endDate?.format('DD/MM/YYYY') || ''})`
            )
            .replace('{0}', startDate?.format('DD/MM/YYYY') || '')
            .replace('{1}', endDate?.format('DD/MM/YYYY') || '')
        );
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:J${startRow}`); // 10 columns
        ws.getRow(startRow).font = { name: 'Amiri', size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(startRow).height = 40;

        const headerRowNum = startRow + 2;
        const headers = [
            getTranslatedLabel('salesRequest.list.id', 'Request ID'),
            getTranslatedLabel('salesRequest.list.apartment', 'Apartment'),
            getTranslatedLabel('salesRequest.list.customer', 'Customer'),
            getTranslatedLabel('salesRequest.list.employee', 'Employee'),
            getTranslatedLabel('salesRequest.list.status', 'Status'),
            getTranslatedLabel('salesRequest.list.saleDate', 'Sale Date'),
            getTranslatedLabel('salesRequest.list.total', 'Total'),
            getTranslatedLabel('salesRequest.list.advance', 'Advance'),
            getTranslatedLabel('salesRequest.list.project', 'Project'),
            getTranslatedLabel('salesRequest.list.comments', 'Comments'),
        ];

        ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

        data.forEach((sr: any) => {
            const row = ws.addRow([
                utils.rtlEmbed(utils.safeString(sr.salesRequestId)),
                utils.rtlEmbed(utils.safeString(sr.apartmentName ?? '')),
                utils.rtlEmbed(utils.safeString(sr.fromPartyName ?? '')),
                utils.rtlEmbed(utils.safeString(sr.employeeName ?? '')),
                utils.rtlEmbed(utils.safeString(sr.statusDescription ?? '')),
                utils.formatDate(sr.saleDate),
                utils.formatNumber(sr.totalPrice),
                utils.formatNumber(sr.advancePayment),
                utils.rtlEmbed(utils.safeString(sr.projectName ?? '')),
                utils.rtlEmbed(utils.safeString(sr.comments ?? '')),
            ]);
            row.font = { name: 'Amiri', size: 10 };
            row.alignment = { horizontal: 'right', wrapText: true };
            row.height = 22;
        });

        if (data.length > 0) {
            const totalSum = data.reduce((sum: number, sr: any) => sum + (sr.totalPrice || 0), 0);
            const totalRowNum = headerRowNum + data.length + 1;
            ws.addRow([
                '', '', '', '', '',
                utils.rtlEmbed(getTranslatedLabel('common.total', 'Total')),
                utils.formatNumber(totalSum),
                '', '', ''
            ]);
            ws.mergeCells(`A${totalRowNum}:F${totalRowNum}`);
            ws.getRow(totalRowNum).font = { name: 'Amiri', size: 12, bold: true };
            ws.getCell(`G${totalRowNum}`).font = { bold: true };
        }

        ws.columns = [
            { width: 15 }, // A Request ID
            { width: 25 }, // B Apartment
            { width: 25 }, // C Customer
            { width: 25 }, // D Employee
            { width: 20 }, // E Status
            { width: 15 }, // F Sale Date
            { width: 15 }, // G Total
            { width: 15 }, // H Advance
            { width: 25 }, // I Project
            { width: 35 }  // J Comments
        ];
        ws.getColumn(7).numFmt = '#,##0.00'; 
        ws.getColumn(8).numFmt = '#,##0.00';

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
                fromDate: startDate.format('YYYY-MM-DD'),
                toDate: endDate.format('YYYY-MM-DD'),
            }).unwrap();

            const buffer = await generateExcel(result);
            if (buffer) {
                const fileName = `SalesRequests_${startDate.format('YYYYMMDD')}_to_${endDate.format('YYYYMMDD')}.xlsx`;
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
    }, [trigger, generateExcel, startDate, endDate]);

    const isLoading = isFetching || isGenerating;

    return (
        <>
            <Button variant="contained" color="success" onClick={handleOpen} sx={{ ml: 1 }}>
                {getTranslatedLabel('salesRequest.report.daterange.excel', 'Export by Date Range')}
            </Button>

            <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel('salesRequest.report.daterange.title', 'Select Date Range for Export')}
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
