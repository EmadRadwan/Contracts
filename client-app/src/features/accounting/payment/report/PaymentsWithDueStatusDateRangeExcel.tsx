import React, { useCallback, useState } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import {
    Button,
    Checkbox,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Box,
    CircularProgress,
    Divider,
    FormControl,
    FormControlLabel,
    FormLabel,
    RadioGroup,
    Radio,
} from '@mui/material';
import { DesktopDatePicker } from '@mui/x-date-pickers/DesktopDatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';
import {
    useLazyFetchPaymentsWithDueStatusByDateRangeQuery,
    useLazyFetchPaymentsWithDueStatusForExportQuery,
} from "../../../../app/store/apis";
import {
    CompositeFilterDescriptor,
    FilterDescriptor,
    State,
    toODataString,
} from '@progress/kendo-data-query';
import { toast } from 'react-toastify';

interface PaymentsWithDueStatusDateRangeExcelProps {
    companyName: string;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    dataState?: State;
    paymentFilter?: 'all' | 'incoming' | 'outgoing';
}

type SubtotalBy =
    | 'none'
    | 'paymentTypeDescription'
    | 'statusDescription'
    | 'partyIdFromName'
    | 'partyIdToName'
    | 'projectName'
    | 'costCenterDescription';

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? '' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `‫${t}` : t,
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString('en-GB') : '',
};

// 'amount' is COLUMN_DEFS index 13 (0-based) → ExcelJS column 14 (1-based)
const AMOUNT_COL_INDEX = 14;
// 'dueStatusArabic' is COLUMN_DEFS index 7 (0-based) → ExcelJS column 8 (1-based)
const DUE_STATUS_COL_INDEX = 8;
const LAST_COL_LETTER = 'Q'; // 17 columns

const COLUMN_DEFS = [
    { field: 'paymentId',              headerKey: 'accounting.payments.list.paymentId',              defaultHeader: 'Payment Number',    width: 16 },
    { field: 'paymentTypeDescription', headerKey: 'accounting.payments.list.paymentType',             defaultHeader: 'Payment Type',       width: 22 },
    { field: 'orderId',                headerKey: 'accounting.payments.list.orderId',                 defaultHeader: 'Order ID',           width: 14 },
    { field: 'certificateNumber',      headerKey: 'accounting.payments.list.certificateNumber',       defaultHeader: 'Certificate Number', width: 18 },
    { field: 'partyIdFromName',        headerKey: 'accounting.payments.list.from',                    defaultHeader: 'From Party',         width: 26 },
    { field: 'partyIdToName',          headerKey: 'accounting.payments.list.to',                      defaultHeader: 'To Party',           width: 26 },
    { field: 'effectiveDate',          headerKey: 'accounting.payments.list.date',                    defaultHeader: 'Payment Date',       width: 14, isDate: true },
    { field: 'dueStatusArabic',        headerKey: 'accounting.payments.list.dueStatus',               defaultHeader: 'Due Status',         width: 30 },
    { field: 'statusDescription',      headerKey: 'accounting.payments.list.status',                  defaultHeader: 'Status',             width: 16 },
    { field: 'buildingNumber',         headerKey: 'accounting.payments.list.buildingNumber',           defaultHeader: 'Building Number',    width: 16 },
    { field: 'productId',              headerKey: 'accounting.payments.list.productId',               defaultHeader: 'Product ID',         width: 14 },
    { field: 'bankName',               headerKey: 'accounting.payments.list.bankName',                defaultHeader: 'Bank Name',          width: 18 },
    { field: 'chequeNumber',           headerKey: 'accounting.payments.list.chequeNumber',            defaultHeader: 'Cheque Number',      width: 16 },
    { field: 'amount',                 headerKey: 'accounting.payments.list.amount',                  defaultHeader: 'Amount',             width: 16, isNumeric: true },
    { field: 'projectName',            headerKey: 'accounting.payments.list.projectName',             defaultHeader: 'Project',            width: 26 },
    { field: 'costCenterDescription',  headerKey: 'accounting.payments.list.costCenterDescription',   defaultHeader: 'Cost Center',        width: 22 },
    { field: 'comments',               headerKey: 'accounting.payments.list.comments',                defaultHeader: 'Comments',           width: 40 },
];

function buildCellValue(colDef: typeof COLUMN_DEFS[0], payment: any): any {
    if (colDef.isDate)    return utils.formatDate(payment[colDef.field]);
    if (colDef.isNumeric) return payment[colDef.field] ?? 0;
    return utils.rtlEmbed(utils.safeString(payment[colDef.field]));
}

function groupByField(data: any[], field: SubtotalBy): Map<string, any[]> {
    const groups = new Map<string, any[]>();
    [...data].sort((a, b) => (a[field] || '').localeCompare(b[field] || '', 'ar')).forEach(p => {
        const key = utils.safeString(p[field]) || 'N/A';
        if (!groups.has(key)) groups.set(key, []);
        groups.get(key)!.push(p);
    });
    return groups;
}

const FIELD_LABELS: Record<string, string> = {
    paymentId: "رقم الدفعة",
    paymentTypeDescription: "نوع الدفعة",
    orderId: "رقم الطلب",
    certificateNumber: "رقم الشهادة",
    partyIdFromName: "من طرف",
    partyIdToName: "إلى طرف",
    effectiveDate: "تاريخ الدفعة",
    dueStatusArabic: "حالة الاستحقاق",
    daysUntilDue: "أيام حتى الاستحقاق",
    statusDescription: "الحالة",
    buildingNumber: "رقم المبنى",
    productId: "رقم المنتج",
    bankName: "اسم البنك",
    chequeNumber: "رقم الشيك",
    amount: "المبلغ",
    projectName: "المشروع",
    costCenterDescription: "مركز التكلفة",
    comments: "ملاحظات",
};

const OPERATOR_LABELS: Record<string, string> = {
    eq: "=", neq: "≠",
    contains: "يحتوي على", doesnotcontain: "لا يحتوي على",
    startswith: "يبدأ بـ", endswith: "ينتهي بـ",
    gte: "≥", gt: ">", lte: "≤", lt: "<",
    isnull: "فارغ", isnotnull: "غير فارغ",
    isempty: "فارغ", isnotempty: "غير فارغ",
};

const NO_VALUE_OPS = new Set(["isnull", "isnotnull", "isempty", "isnotempty"]);

function describeFilter(f: CompositeFilterDescriptor | FilterDescriptor): string {
    if ("filters" in f) {
        const parts = (f as CompositeFilterDescriptor).filters
            .map(child => describeFilter(child as CompositeFilterDescriptor | FilterDescriptor))
            .filter(Boolean);
        if (parts.length === 0) return "";
        const sep = (f as CompositeFilterDescriptor).logic === "and" ? " و " : " أو ";
        return parts.length === 1 ? parts[0] : `(${parts.join(sep)})`;
    }
    const fd = f as FilterDescriptor;
    const label = FIELD_LABELS[fd.field as string] ?? String(fd.field);
    const op = OPERATOR_LABELS[fd.operator as string] ?? String(fd.operator);
    if (NO_VALUE_OPS.has(fd.operator as string)) return `${label} ${op}`;
    const val = fd.value instanceof Date
        ? new Date(fd.value).toLocaleDateString("en-GB")
        : fd.value == null ? "" : String(fd.value);
    return `${label} ${op} "${val}"`;
}

function applyDueStatusColor(cell: ExcelJS.Cell, payment: any) {
    if (payment.statusId !== "PMNT_NOT_PAID") return;
    const days = payment.daysUntilDue ?? 0;
    if (days < 0) {
        cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFFFEBEE' } };
        cell.font = { name: 'Amiri', size: 10, bold: true, color: { argb: 'FFC62828' } };
    } else if (days <= 7) {
        cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFFFF3E0' } };
        cell.font = { name: 'Amiri', size: 10, bold: true, color: { argb: 'FFEF6C00' } };
    } else {
        cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE8F5E8' } };
        cell.font = { name: 'Amiri', size: 10, bold: true, color: { argb: 'FF2E7D32' } };
    }
}

export const PaymentsWithDueStatusDateRangeExcel: React.FC<PaymentsWithDueStatusDateRangeExcelProps> = ({
    companyName,
    getTranslatedLabel,
    dataState,
    paymentFilter = 'all',
}) => {
    const [open, setOpen] = useState(false);
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf('month'));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [subtotalBy, setSubtotalBy] = useState<SubtotalBy>('paymentTypeDescription');
    const [respectFilters, setRespectFilters] = useState(false);
    const [trigger, { isFetching }] = useLazyFetchPaymentsWithDueStatusByDateRangeQuery();
    const [exportTrigger, { isFetching: isExportFetching }] = useLazyFetchPaymentsWithDueStatusForExportQuery();
    const [isGenerating, setIsGenerating] = useState(false);

    const generateExcel = useCallback(async (
        data: { data: any[]; total: number },
        groupBy: SubtotalBy,
        filterInfo: string,
    ) => {
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
        const safeSheetName = `DueStatus ${period}`.replace(/[*\?\\:\[\]\/]/g, '_').slice(0, 31);
        const ws = workbook.addWorksheet(safeSheetName);
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.views = [{ rightToLeft: true }];
        ws.getRow(1).height = logoBuffer ? 75 : 30;

        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: 'jpeg' });
            ws.addImage(imageId, { tl: { col: 0, row: 0 }, ext: { width: 120, height: 100 } });
        } else {
            ws.getCell('A1').value = 'Golden Land';
            ws.getCell('A1').font = { name: 'Amiri', size: 16, bold: true };
        }

        const startRow = logoBuffer ? 5 : 3;

        // Title
        const filterLabel = paymentFilter === 'outgoing' ? 'صادرة' : paymentFilter === 'incoming' ? 'واردة' : 'الكل';
        const title = utils.rtlEmbed(
            getTranslatedLabel(
                'accounting.payments.report.dueStatus.daterange.title',
                `حالة استحقاق المدفوعات - ${filterLabel} (${startDate?.format('DD/MM/YYYY') || ''} - ${endDate?.format('DD/MM/YYYY') || ''})`
            )
        );
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:${LAST_COL_LETTER}${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(startRow).height = 40;

        // Filter info row
        const filterInfoRow = ws.addRow([utils.rtlEmbed(filterInfo)]);
        ws.mergeCells(`A${startRow + 1}:${LAST_COL_LETTER}${startRow + 1}`);
        filterInfoRow.font = { name: 'Amiri', size: 9, italic: true, color: { argb: 'FF555555' } };
        filterInfoRow.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
        filterInfoRow.height = 18;

        // Header row
        const headerRowNum = startRow + 2;
        const headers = COLUMN_DEFS.map(c => utils.rtlEmbed(getTranslatedLabel(c.headerKey, c.defaultHeader)));
        ws.addRow(headers);
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };
        headerRow.height = 22;

        const subtotalLabel = getTranslatedLabel('accounting.payments.report.subtotal', 'المجموع الفرعي');
        const grandTotalLabel = getTranslatedLabel('accounting.payments.report.grandTotal', 'المجموع الكلي');
        const amountField = COLUMN_DEFS[AMOUNT_COL_INDEX - 1].field;
        // Column letter for the label cell (one before the amount column)
        const labelColLetter = String.fromCharCode(64 + AMOUNT_COL_INDEX - 1);
        // Column letter for the amount cell
        const amountColLetter = String.fromCharCode(64 + AMOUNT_COL_INDEX);

        const addDataRow = (payment: any) => {
            const row = ws.addRow(COLUMN_DEFS.map(c => buildCellValue(c, payment)));
            row.font = { name: 'Amiri', size: 10 };
            row.alignment = { horizontal: 'right', wrapText: true };
            row.height = 22;
            row.getCell(AMOUNT_COL_INDEX).numFmt = '#,##0.00';
            applyDueStatusColor(row.getCell(DUE_STATUS_COL_INDEX), payment);
            return row;
        };

        if (groupBy === 'none') {
            let firstDataRowNum = -1;
            let lastDataRowNum = -1;
            data.data.forEach(payment => {
                const row = addDataRow(payment);
                if (firstDataRowNum === -1) firstDataRowNum = row.number;
                lastDataRowNum = row.number;
            });

            const totalRowData = COLUMN_DEFS.map((_c, i) => {
                if (i === AMOUNT_COL_INDEX - 2) return utils.rtlEmbed(grandTotalLabel);
                return '';
            });
            const totalRow = ws.addRow(totalRowData);
            ws.mergeCells(`A${totalRow.number}:${labelColLetter}${totalRow.number}`);
            totalRow.font = { name: 'Amiri', size: 13, bold: true };
            totalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFFFF3E0' } };
            totalRow.alignment = { horizontal: 'right', vertical: 'middle' };
            totalRow.getCell(AMOUNT_COL_INDEX).value = { formula: `SUBTOTAL(9,${amountColLetter}${firstDataRowNum}:${amountColLetter}${lastDataRowNum})` };
            totalRow.getCell(AMOUNT_COL_INDEX).numFmt = '#,##0.00';
            totalRow.height = 24;
        } else {
            const groups = groupByField(data.data, groupBy);
            let grandTotal = 0;

            groups.forEach((rows, groupName) => {
                const groupTotal = rows.reduce((s, p) => s + (p[amountField] || 0), 0);
                grandTotal += groupTotal;

                // Group header row
                const groupHeaderRow = ws.addRow([utils.rtlEmbed(groupName)]);
                ws.mergeCells(`A${groupHeaderRow.number}:${LAST_COL_LETTER}${groupHeaderRow.number}`);
                groupHeaderRow.font = { name: 'Amiri', size: 11, bold: true, color: { argb: 'FF1A237E' } };
                groupHeaderRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE8EAF6' } };
                groupHeaderRow.alignment = { horizontal: 'right', vertical: 'middle' };
                groupHeaderRow.height = 20;

                rows.forEach(payment => addDataRow(payment));

                // Subtotal row
                const subtotalRowData = COLUMN_DEFS.map((_c, i) => {
                    if (i === AMOUNT_COL_INDEX - 2) return utils.rtlEmbed(`${subtotalLabel}: ${groupName}`);
                    if (i === AMOUNT_COL_INDEX - 1) return groupTotal;
                    return '';
                });
                const subtotalRow = ws.addRow(subtotalRowData);
                ws.mergeCells(`A${subtotalRow.number}:${labelColLetter}${subtotalRow.number}`);
                subtotalRow.font = { name: 'Amiri', size: 11, bold: true, color: { argb: 'FF1B5E20' } };
                subtotalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE8F5E9' } };
                subtotalRow.alignment = { horizontal: 'right', vertical: 'middle' };
                subtotalRow.getCell(AMOUNT_COL_INDEX).numFmt = '#,##0.00';
                subtotalRow.height = 20;

                ws.addRow([]);
            });

            // Grand total row
            const grandTotalRowData = COLUMN_DEFS.map((_c, i) => {
                if (i === AMOUNT_COL_INDEX - 2) return utils.rtlEmbed(grandTotalLabel);
                if (i === AMOUNT_COL_INDEX - 1) return grandTotal;
                return '';
            });
            const grandTotalRow = ws.addRow(grandTotalRowData);
            ws.mergeCells(`A${grandTotalRow.number}:${labelColLetter}${grandTotalRow.number}`);
            grandTotalRow.font = { name: 'Amiri', size: 13, bold: true };
            grandTotalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFFFF3E0' } };
            grandTotalRow.alignment = { horizontal: 'right', vertical: 'middle' };
            grandTotalRow.getCell(AMOUNT_COL_INDEX).numFmt = '#,##0.00';
            grandTotalRow.height = 24;
        }

        ws.columns = COLUMN_DEFS.map(c => ({ width: c.width }));

        return await workbook.xlsx.writeBuffer();
    }, [getTranslatedLabel, startDate, endDate, paymentFilter]);

    const handleDownload = useCallback(async () => {
        if (!startDate || !endDate) return;
        setIsGenerating(true);
        try {
            let result: { data: any[]; total: number };

            if (respectFilters && dataState) {
                const oDataQuery = toODataString({
                    ...(dataState.filter ? { filter: dataState.filter } : {}),
                    sort: dataState.sort,
                });
                const flat = await exportTrigger({
                    oDataQuery,
                    fromDate: startDate.format('YYYY-MM-DD'),
                    toDate: endDate.format('YYYY-MM-DD'),
                }, false).unwrap();
                result = { data: flat, total: flat.length };
            } else {
                result = await trigger({
                    fromDate: startDate.format('YYYY-MM-DD'),
                    toDate: endDate.format('YYYY-MM-DD'),
                }, false).unwrap();
            }

            if (!result || result.data.length === 0) {
                toast.info(getTranslatedLabel('accounting.payments.report.noData', 'لا توجد دفعات للفترة المحددة'));
                return;
            }

            const filterParts: string[] = [
                `الفترة: ${startDate.format("DD/MM/YYYY")} - ${endDate.format("DD/MM/YYYY")}`,
            ];
            if (respectFilters && dataState?.filter) {
                const gridText = describeFilter(dataState.filter as CompositeFilterDescriptor);
                if (gridText) filterParts.push(`الفلاتر: ${gridText}`);
            }
            const filterInfo = filterParts.join("  |  ");

            const buffer = await generateExcel(result, subtotalBy, filterInfo);
            if (buffer) {
                const fileName = `PaymentsDueStatus_${startDate.format('YYYYMMDD')}_to_${endDate.format('YYYYMMDD')}.xlsx`;
                saveAs(
                    new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }),
                    fileName
                );
            }
            setOpen(false);
        } catch (err: any) {
            console.error('Excel generation failed:', err);
            const msg = err?.data?.error || err?.message || 'Failed to generate report.';
            toast.error(msg);
        } finally {
            setIsGenerating(false);
        }
    }, [trigger, exportTrigger, generateExcel, startDate, endDate, subtotalBy, respectFilters, dataState, getTranslatedLabel]);

    const isLoading = isFetching || isExportFetching || isGenerating;

    return (
        <>
            <Button variant="contained" color="primary" onClick={() => setOpen(true)}>
                {getTranslatedLabel('accounting.payments.list.exportByDateRange', 'Export to Excel by Date Range')}
            </Button>

            <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel('accounting.payments.report.daterange.select', 'اختر نطاق التاريخ')}
                </DialogTitle>
                <DialogContent>
                    <LocalizationProvider dateAdapter={AdapterDayjs}>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, mt: 2 }}>
                            <DesktopDatePicker
                                label={getTranslatedLabel('common.fromDate', 'من تاريخ')}
                                value={startDate}
                                onChange={(v) => setStartDate(v)}
                                slotProps={{ textField: { fullWidth: true } }}
                            />
                            <DesktopDatePicker
                                label={getTranslatedLabel('common.toDate', 'إلى تاريخ')}
                                value={endDate}
                                minDate={startDate ?? undefined}
                                onChange={(v) => setEndDate(v)}
                                slotProps={{ textField: { fullWidth: true } }}
                            />

                            <Divider />

                            <FormControl component="fieldset">
                                <FormLabel component="legend">
                                    {getTranslatedLabel('accounting.payments.report.subtotalBy', 'المجموع الفرعي حسب')}
                                </FormLabel>
                                <RadioGroup
                                    value={subtotalBy}
                                    onChange={(e) => setSubtotalBy(e.target.value as SubtotalBy)}
                                >
                                    <FormControlLabel value="none" control={<Radio />}
                                        label={getTranslatedLabel('accounting.payments.report.subtotalByNone', 'بدون تجميع')} />
                                    <FormControlLabel value="paymentTypeDescription" control={<Radio />}
                                        label={getTranslatedLabel('accounting.payments.report.subtotalByPaymentType', 'نوع الدفعة')} />
                                    <FormControlLabel value="statusDescription" control={<Radio />}
                                        label={getTranslatedLabel('accounting.payments.report.subtotalByStatus', 'الحالة')} />
                                    <FormControlLabel value="partyIdFromName" control={<Radio />}
                                        label={getTranslatedLabel('accounting.payments.report.subtotalByFromParty', 'من طرف')} />
                                    <FormControlLabel value="partyIdToName" control={<Radio />}
                                        label={getTranslatedLabel('accounting.payments.report.subtotalByToParty', 'إلى طرف')} />
                                    <FormControlLabel value="projectName" control={<Radio />}
                                        label={getTranslatedLabel('accounting.payments.report.subtotalByProject', 'المشروع')} />
                                    <FormControlLabel value="costCenterDescription" control={<Radio />}
                                        label={getTranslatedLabel('accounting.payments.report.subtotalByCostCenter', 'مركز التكلفة')} />
                                </RadioGroup>
                            </FormControl>

                            {dataState && (
                                <FormControlLabel
                                    control={
                                        <Checkbox
                                            checked={respectFilters}
                                            onChange={(e) => setRespectFilters(e.target.checked)}
                                        />
                                    }
                                    label={getTranslatedLabel(
                                        "accounting.payments.report.respectFilters",
                                        "مراعاة فلاتر قائمة الدفعات"
                                    )}
                                />
                            )}
                        </Box>
                    </LocalizationProvider>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setOpen(false)} disabled={isLoading}>
                        {getTranslatedLabel('common.cancel', 'Cancel')}
                    </Button>
                    <Button
                        onClick={handleDownload}
                        variant="contained"
                        disabled={isLoading || !startDate || !endDate}
                        startIcon={isLoading ? <CircularProgress size={20} /> : null}
                    >
                        {isLoading
                            ? getTranslatedLabel('common.generating', 'Generating...')
                            : getTranslatedLabel('common.download', 'Download')}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
};
