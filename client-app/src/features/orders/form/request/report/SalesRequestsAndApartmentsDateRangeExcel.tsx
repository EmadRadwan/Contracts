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
import 'dayjs/locale/en-gb';
import { useLazyFetchSalesRequestsAndAvailableApartmentsByDateRangeQuery } from "../../../../../app/store/apis/salesRequestApi";

interface SalesRequestsAndApartmentsDateRangeExcelProps {
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

const NOT_SOLD_ROW_FILL = 'FFFCE8B2'; // light amber - marks rows with no sales request
const NOT_SOLD_TEXT_COLOR = 'FF8A6D00';

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatDate: (d: string | Date | undefined | null) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

// Column layout (17 cols, A-Q). Kept in one place so header/data/format/subtotal/merge
// ranges below can't drift out of sync when a column is added or reordered.
type ColumnDef = { key: string; width: number; numFmt?: string };
const COLUMNS: ColumnDef[] = [
    { key: 'salesRequestId', width: 15 },              // A
    { key: 'apartmentName', width: 22 },                // B
    { key: 'buildingNumber', width: 16 },               // C
    { key: 'floorNumber', width: 16 },                  // D
    { key: 'fromPartyName', width: 22 },                // E
    { key: 'employeeName', width: 22 },                 // F
    { key: 'statusDescription', width: 18 },            // G
    { key: 'apartmentStatusDescription', width: 16 },   // H
    { key: 'saleDate', width: 14 },                     // I
    { key: 'totalPrice', width: 15, numFmt: '#,##0.00' },        // J
    { key: 'advancePayment', width: 15, numFmt: '#,##0.00' },    // K
    { key: 'maintenanceDeposit', width: 16, numFmt: '#,##0.00' },// L
    { key: 'apartmentSpaceM2', width: 18, numFmt: '#,##0.00' },  // M
    { key: 'gardenSpaceM2', width: 18, numFmt: '#,##0.00' },     // N
    { key: 'apartmentPricePerM2', width: 16, numFmt: '#,##0.00' }, // O
    { key: 'projectName', width: 22 },                  // P
    { key: 'comments', width: 32 },                     // Q
];
const SUBTOTAL_KEYS = ['totalPrice', 'advancePayment', 'maintenanceDeposit', 'apartmentSpaceM2', 'gardenSpaceM2'];

export const SalesRequestsAndApartmentsDateRangeExcel: React.FC<SalesRequestsAndApartmentsDateRangeExcelProps> = ({
                                                                                          getTranslatedLabel,
                                                                                      }) => {
    const [open, setOpen] = useState(false);
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf('month'));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [trigger, { isFetching }] = useLazyFetchSalesRequestsAndAvailableApartmentsByDateRangeQuery();
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
        const safeSheetName = `Sales+Available ${period}`.replace(/[*\?\\:\[\]\/]/g, '_').slice(0, 31);
        const ws = workbook.addWorksheet(safeSheetName);
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.views = [{ rightToLeft: true }];

        const lastCol = COLUMNS.length; // 17 -> column Q

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
                'salesRequest.report.daterangeWithApartments.title',
                `Sales Requests & Available Apartments (${startDate?.format('DD/MM/YYYY') || ''} - ${endDate?.format('DD/MM/YYYY') || ''})`
            )
            .replace('{0}', startDate?.format('DD/MM/YYYY') || '')
            .replace('{1}', endDate?.format('DD/MM/YYYY') || '')
        );
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(startRow, 1, startRow, lastCol);
        ws.getRow(startRow).font = { name: 'Amiri', size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(startRow).height = 40;

        // Legend explaining the amber row highlight, right below the title.
        const legendRowNum = startRow + 1;
        const legendText = utils.rtlEmbed(getTranslatedLabel(
            'salesRequest.report.daterangeWithApartments.legend',
            'Amber rows = apartments with no sales request yet (available or reserved inventory), not sold.'
        ));
        ws.getCell(`A${legendRowNum}`).value = legendText;
        ws.mergeCells(legendRowNum, 1, legendRowNum, lastCol);
        ws.getRow(legendRowNum).font = { name: 'Amiri', size: 10, italic: true, color: { argb: NOT_SOLD_TEXT_COLOR } };
        ws.getRow(legendRowNum).alignment = { horizontal: 'center', vertical: 'middle' };

        const headerRowNum = legendRowNum + 2;
        const headers = [
            getTranslatedLabel('salesRequest.list.id', 'Request ID'),
            getTranslatedLabel('salesRequest.list.apartment', 'Apartment'),
            getTranslatedLabel('salesRequest.list.buildingNumber', 'Building Number'),
            getTranslatedLabel('salesRequest.list.floorNumber', 'Floor'),
            getTranslatedLabel('salesRequest.list.customer', 'Customer'),
            getTranslatedLabel('salesRequest.list.employee', 'Employee'),
            getTranslatedLabel('salesRequest.list.status', 'Status'),
            getTranslatedLabel('salesRequest.list.apartmentStatus', 'Apartment Status'),
            getTranslatedLabel('salesRequest.list.saleDate', 'Sale Date'),
            getTranslatedLabel('salesRequest.list.total', 'Total'),
            getTranslatedLabel('salesRequest.list.advance', 'Advance'),
            getTranslatedLabel('salesRequest.list.maintenanceDeposit', 'Maintenance'),
            getTranslatedLabel('salesRequest.list.apartmentSpace', 'Apartment Space (m²)'),
            getTranslatedLabel('salesRequest.list.gardenSpace', 'Garden Space (m²)'),
            getTranslatedLabel('salesRequest.list.pricePerM2', 'Price / m²'),
            getTranslatedLabel('salesRequest.list.project', 'Project'),
            getTranslatedLabel('salesRequest.list.comments', 'Comments'),
        ];

        ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

        data.forEach((sr: any) => {
            const isSold = !!sr.isSold;
            const rowValues: Record<string, any> = {
                salesRequestId: utils.rtlEmbed(utils.safeString(sr.salesRequestId)),
                apartmentName: utils.rtlEmbed(utils.safeString(sr.apartmentName ?? '')),
                buildingNumber: utils.rtlEmbed(utils.safeString(sr.buildingNumber ?? '')),
                floorNumber: utils.rtlEmbed(utils.safeString(sr.floorNumber ?? '')),
                fromPartyName: utils.rtlEmbed(utils.safeString(sr.fromPartyName ?? '')),
                employeeName: utils.rtlEmbed(utils.safeString(sr.employeeName ?? '')),
                statusDescription: utils.rtlEmbed(utils.safeString(sr.statusDescription ?? '')),
                apartmentStatusDescription: utils.rtlEmbed(utils.safeString(sr.apartmentStatusDescription ?? '')),
                saleDate: isSold ? utils.formatDate(sr.saleDate) : '',
                totalPrice: isSold ? (sr.totalPrice ?? 0) : '',
                advancePayment: isSold ? (sr.advancePayment ?? 0) : '',
                maintenanceDeposit: isSold ? (sr.maintenanceDeposit ?? 0) : '',
                apartmentSpaceM2: sr.apartmentSpaceM2 ?? 0,
                gardenSpaceM2: sr.gardenSpaceM2 ?? 0,
                apartmentPricePerM2: sr.apartmentPricePerM2 ?? 0,
                projectName: utils.rtlEmbed(utils.safeString(sr.projectName ?? '')),
                comments: utils.rtlEmbed(utils.safeString(sr.comments ?? '')),
            };

            const row = ws.addRow(COLUMNS.map(c => rowValues[c.key]));
            row.font = { name: 'Amiri', size: 10 };
            row.alignment = { horizontal: 'right', wrapText: true };
            row.height = 22;

            if (!isSold) {
                row.eachCell({ includeEmpty: true }, cell => {
                    cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: NOT_SOLD_ROW_FILL } };
                });
                const statusCell = row.getCell(7); // G = Status
                statusCell.font = { name: 'Amiri', size: 10, bold: true, color: { argb: NOT_SOLD_TEXT_COLOR } };
            }
        });

        if (data.length > 0) {
            const dataStartRow = headerRowNum + 1;
            const dataEndRow = headerRowNum + data.length;
            const totalRowNum = dataEndRow + 1;

            // AutoFilter on the data range so the user can filter rows in Excel.
            ws.autoFilter = {
                from: { row: headerRowNum, column: 1 },
                to: { row: dataEndRow, column: lastCol },
            };

            const totalRowValues = COLUMNS.map(c =>
                c.key === 'statusDescription' ? utils.rtlEmbed(getTranslatedLabel('common.subtotal', 'Subtotal')) : ''
            );
            ws.addRow(totalRowValues);

            // SUBTOTAL (109 = SUM, ignoring filtered/hidden rows) so each value
            // recalculates live in Excel as the user applies filters.
            SUBTOTAL_KEYS.forEach(key => {
                const colIndex = COLUMNS.findIndex(c => c.key === key) + 1;
                const colLetter = ws.getColumn(colIndex).letter;
                const cell = ws.getCell(`${colLetter}${totalRowNum}`);
                cell.value = { formula: `SUBTOTAL(109,${colLetter}${dataStartRow}:${colLetter}${dataEndRow})` };
                cell.font = { bold: true };
            });

            ws.mergeCells(totalRowNum, 1, totalRowNum, 7); // through the Status column
            ws.getRow(totalRowNum).font = { name: 'Amiri', size: 12, bold: true };
        }

        ws.columns = COLUMNS.map(c => ({ width: c.width }));
        COLUMNS.forEach((c, i) => {
            if (c.numFmt) ws.getColumn(i + 1).numFmt = c.numFmt;
        });

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
                const fileName = `SalesRequestsAndApartments_${startDate.format('YYYYMMDD')}_to_${endDate.format('YYYYMMDD')}.xlsx`;
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
                {getTranslatedLabel('salesRequest.report.daterangeWithApartments.excel', 'Export by Date Range')}
            </Button>

            <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel('salesRequest.report.daterange.title', 'Select Date Range for Export')}
                </DialogTitle>
                <DialogContent>
                    <LocalizationProvider dateAdapter={AdapterDayjs} adapterLocale="en-gb">
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, mt: 2 }}>
                            <DesktopDatePicker
                                label={getTranslatedLabel('common.fromDate', 'From Date')}
                                value={startDate}
                                onChange={(newValue) => setStartDate(newValue)}
                                format="DD/MM/YYYY"
                                slotProps={{ textField: { fullWidth: true } }}
                            />
                            <DesktopDatePicker
                                label={getTranslatedLabel('common.toDate', 'To Date')}
                                value={endDate}
                                minDate={startDate ?? undefined}
                                onChange={(newValue) => setEndDate(newValue)}
                                format="DD/MM/YYYY"
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
