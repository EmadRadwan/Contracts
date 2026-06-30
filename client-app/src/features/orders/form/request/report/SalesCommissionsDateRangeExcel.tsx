import React, { useCallback, useState } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import {
    Button,
    Checkbox,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Divider,
    FormControl,
    FormControlLabel,
    FormLabel,
    Box,
    Radio,
    RadioGroup,
} from '@mui/material';
import { DesktopDatePicker } from '@mui/x-date-pickers/DesktopDatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';
import {
    CompositeFilterDescriptor,
    FilterDescriptor,
    State,
    toODataString,
} from '@progress/kendo-data-query';
import { useLazyFetchSalesCommissionsForExportQuery } from '../../../../../app/store/apis/salesCommissionsApi';
import { useTranslationHelper } from '../../../../../app/hooks/useTranslationHelper';
import { toast } from 'react-toastify';

interface Props {
    dataState?: State;
}

type SubtotalBy = 'projectName' | 'salesRepName' | 'saleTypeId' | 'statusId';

// ─── label maps (match the grid display values) ───────────────────────────────
const SALE_TYPE_LABELS: Record<string, string> = {
    COMM_SALE_DIRECT: "بيع مباشر",
    COMM_SALE_PERSONAL: "بيع شخصي",
    COMM_SALE_INDIRECT: "بيع غير مباشر",
};

const STATUS_LABELS: Record<string, string> = {
    COMMISSION_PENDING: "قيد الانتظار",
    COMMISSION_APPROVED: "معتمدة",
    COMMISSION_PAID: "مدفوعة",
};

// ─── utilities ────────────────────────────────────────────────────────────────
const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? '' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `‫${t}` : t,
    formatDate: (d: string | Date | undefined | null) => d ? new Date(d).toLocaleDateString('en-GB') : '',
    formatBoolean: (v: any) => (v === 1 || v === true) ? 'نعم' : 'لا',
};

// ─── column definitions ───────────────────────────────────────────────────────
// 46 columns → last column = AT
const LAST_COL_LETTER = 'AT';
// First 9 cols (A-I) are text/date — subtotal label merges these
const SUBTOTAL_LABEL_END = 'I';

interface ColDef {
    field: string;
    header: string;
    width: number;
    isDate?: boolean;
    isNumeric?: boolean;
    isAmount?: boolean; // subset of isNumeric — summed in subtotal/grand-total rows
    isBoolean?: boolean;
}

const COLUMN_DEFS: ColDef[] = [
    { field: 'salesCommissionId',         header: 'رقم العمولة',                width: 15 },
    { field: 'salesRequestId',            header: 'طلب البيع',                  width: 15 },
    { field: 'apartmentName',             header: 'الشقة',                      width: 22 },
    { field: 'projectName',               header: 'المشروع',                    width: 25 },
    { field: 'saleTypeId',                header: 'نوع البيع',                   width: 20 },
    { field: 'statusId',                  header: 'الحالة',                     width: 15 },
    { field: 'salesRepName',              header: 'المندوب',                    width: 22 },
    { field: 'managerName',               header: 'المدير',                     width: 22 },
    { field: 'commissionDate',            header: 'تاريخ العمولة',               width: 14, isDate: true },
    { field: 'salePrice',                 header: 'سعر البيع',                  width: 16, isNumeric: true, isAmount: true },
    { field: 'collectedAmount',           header: 'المبلغ المحصل',               width: 16, isNumeric: true, isAmount: true },
    { field: 'salesRepPercent',           header: 'نسبة المندوب %',              width: 14, isNumeric: true },
    { field: 'salesRepAmount',            header: 'مبلغ المندوب',               width: 16, isNumeric: true, isAmount: true },
    { field: 'salesRepNetAmount',         header: 'صافي المندوب',               width: 16, isNumeric: true, isAmount: true },
    { field: 'salesRep2Name',             header: 'المندوب الثاني',              width: 22 },
    { field: 'salesRep2Percent',          header: 'نسبة المندوب الثاني %',       width: 16, isNumeric: true },
    { field: 'salesRep2Amount',           header: 'مبلغ المندوب الثاني',        width: 16, isNumeric: true, isAmount: true },
    { field: 'salesRep2NetAmount',        header: 'صافي المندوب الثاني',        width: 16, isNumeric: true, isAmount: true },
    { field: 'managerPercent',            header: 'نسبة المدير %',              width: 14, isNumeric: true },
    { field: 'managerAmount',             header: 'مبلغ المدير',               width: 16, isNumeric: true, isAmount: true },
    { field: 'managerNetAmount',          header: 'صافي المدير',               width: 16, isNumeric: true, isAmount: true },
    { field: 'manager2Name',              header: 'المدير الثاني',              width: 22 },
    { field: 'manager2Percent',           header: 'نسبة المدير الثاني %',       width: 16, isNumeric: true },
    { field: 'manager2Amount',            header: 'مبلغ المدير الثاني',        width: 16, isNumeric: true, isAmount: true },
    { field: 'manager2NetAmount',         header: 'صافي المدير الثاني',        width: 16, isNumeric: true, isAmount: true },
    { field: 'externalCompanyName',       header: 'شركة الوسيط',               width: 22 },
    { field: 'externalCompanyPercent',    header: 'نسبة الوسيط %',             width: 14, isNumeric: true },
    { field: 'externalCompanyGrossAmount',header: 'إجمالي الوسيط',              width: 16, isNumeric: true, isAmount: true },
    { field: 'externalCompanyNetAmount',  header: 'صافي الوسيط',               width: 16, isNumeric: true, isAmount: true },
    { field: 'externalSalesRepName',      header: 'مندوب الوسيط',              width: 22 },
    { field: 'externalSalesRepPercent',   header: 'نسبة مندوب الوسيط %',       width: 16, isNumeric: true },
    { field: 'externalSalesRepAmount',                      header: 'مبلغ مندوب الوسيط',                  width: 16, isNumeric: true, isAmount: true },
    { field: 'externalSalesRepNetAmount',                   header: 'صافي مندوب الوسيط',                  width: 16, isNumeric: true, isAmount: true },
    { field: 'hasExternalSalesRepWithholdingTaxExemption',  header: 'إعفاء ض.استقطاع مندوب الوسيط',       width: 18, isBoolean: true },
    { field: 'externalSalesRepNationalId',                  header: 'الرقم القومي (مندوب الوسيط)',         width: 20 },
    { field: 'externalManagerName',                         header: 'مدير الوسيط',                        width: 22 },
    { field: 'externalManagerPercent',                      header: 'نسبة مدير الوسيط %',                 width: 16, isNumeric: true },
    { field: 'externalManagerAmount',                       header: 'مبلغ مدير الوسيط',                   width: 16, isNumeric: true, isAmount: true },
    { field: 'externalManagerNetAmount',                    header: 'صافي مدير الوسيط',                   width: 16, isNumeric: true, isAmount: true },
    { field: 'hasExternalManagerWithholdingTaxExemption',   header: 'إعفاء ض.استقطاع مدير الوسيط',        width: 18, isBoolean: true },
    { field: 'externalManagerNationalId',                   header: 'الرقم القومي (مدير الوسيط)',          width: 20 },
    { field: 'hasVatExemption',           header: 'إعفاء ض.ق.م',               width: 12, isBoolean: true },
    { field: 'hasWithholdingTaxExemption',header: 'إعفاء ض.استقطاع',            width: 14, isBoolean: true },
    { field: 'vatPercent',                header: 'نسبة ض.ق.م %',              width: 12, isNumeric: true },
    { field: 'withholdingTaxPercent',     header: 'نسبة ض.استقطاع %',           width: 14, isNumeric: true },
    { field: 'notes',                     header: 'ملاحظات',                   width: 25 },
];

function buildCellValue(col: ColDef, record: any): any {
    if (col.isDate)    return utils.formatDate(record[col.field]);
    if (col.isBoolean) return utils.formatBoolean(record[col.field]);
    if (col.isNumeric) return record[col.field] ?? 0;
    if (col.field === 'saleTypeId') return SALE_TYPE_LABELS[record.saleTypeId] ?? record.saleTypeId ?? '';
    if (col.field === 'statusId')   return STATUS_LABELS[record.statusId] ?? record.statusId ?? '';
    return utils.rtlEmbed(utils.safeString(record[col.field]));
}

function getGroupKey(record: any, field: SubtotalBy): string {
    if (field === 'saleTypeId') return SALE_TYPE_LABELS[record.saleTypeId] ?? record.saleTypeId ?? 'N/A';
    if (field === 'statusId')   return STATUS_LABELS[record.statusId] ?? record.statusId ?? 'N/A';
    return utils.safeString(record[field]) || 'N/A';
}

function groupBy(data: any[], field: SubtotalBy): Map<string, any[]> {
    const groups = new Map<string, any[]>();
    [...data]
        .sort((a, b) => getGroupKey(a, field).localeCompare(getGroupKey(b, field), 'ar'))
        .forEach(r => {
            const key = getGroupKey(r, field);
            if (!groups.has(key)) groups.set(key, []);
            groups.get(key)!.push(r);
        });
    return groups;
}

function sumAmounts(rows: any[]): Record<string, number> {
    const totals: Record<string, number> = {};
    for (const col of COLUMN_DEFS) {
        if (col.isAmount) {
            totals[col.field] = rows.reduce((s, r) => s + ((r[col.field] as number) ?? 0), 0);
        }
    }
    return totals;
}

// ─── filter description helpers ───────────────────────────────────────────────
const FIELD_LABELS: Record<string, string> = {
    salesCommissionId: "رقم العمولة",
    salesRequestId: "طلب البيع",
    apartmentName: "الشقة",
    projectName: "المشروع",
    saleTypeId: "نوع البيع",
    statusId: "الحالة",
    salesRepName: "المندوب",
    managerName: "المدير",
    commissionDate: "تاريخ العمولة",
    salePrice: "سعر البيع",
    collectedAmount: "المبلغ المحصل",
    salesRepPercent: "نسبة المندوب %",
    salesRepAmount: "مبلغ المندوب",
    salesRepNetAmount: "صافي المندوب",
    salesRep2Name: "المندوب الثاني",
    salesRep2Percent: "نسبة المندوب الثاني %",
    salesRep2Amount: "مبلغ المندوب الثاني",
    salesRep2NetAmount: "صافي المندوب الثاني",
    managerPercent: "نسبة المدير %",
    managerAmount: "مبلغ المدير",
    managerNetAmount: "صافي المدير",
    manager2Name: "المدير الثاني",
    manager2Percent: "نسبة المدير الثاني %",
    manager2Amount: "مبلغ المدير الثاني",
    manager2NetAmount: "صافي المدير الثاني",
    externalCompanyName: "شركة الوسيط",
    externalCompanyPercent: "نسبة الوسيط %",
    externalCompanyGrossAmount: "إجمالي الوسيط",
    externalCompanyNetAmount: "صافي الوسيط",
    externalSalesRepName: "مندوب الوسيط",
    externalSalesRepPercent: "نسبة مندوب الوسيط %",
    externalSalesRepAmount: "مبلغ مندوب الوسيط",
    externalSalesRepNetAmount: "صافي مندوب الوسيط",
    hasExternalSalesRepWithholdingTaxExemption: "إعفاء ض.استقطاع مندوب الوسيط",
    externalSalesRepNationalId: "الرقم القومي (مندوب الوسيط)",
    externalManagerName: "مدير الوسيط",
    externalManagerPercent: "نسبة مدير الوسيط %",
    externalManagerAmount: "مبلغ مدير الوسيط",
    externalManagerNetAmount: "صافي مدير الوسيط",
    hasExternalManagerWithholdingTaxExemption: "إعفاء ض.استقطاع مدير الوسيط",
    externalManagerNationalId: "الرقم القومي (مدير الوسيط)",
    hasVatExemption: "إعفاء ض.ق.م",
    hasWithholdingTaxExemption: "إعفاء ض.استقطاع",
    vatPercent: "نسبة ض.ق.م %",
    withholdingTaxPercent: "نسبة ض.استقطاع %",
    notes: "ملاحظات",
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
    const field = fd.field as string;
    const label = FIELD_LABELS[field] ?? field;
    const op = OPERATOR_LABELS[fd.operator as string] ?? String(fd.operator);
    if (NO_VALUE_OPS.has(fd.operator as string)) return `${label} ${op}`;
    const val = fd.value instanceof Date
        ? new Date(fd.value).toLocaleDateString("en-GB")
        : fd.value == null ? "" : String(fd.value);
    return `${label} ${op} "${val}"`;
}

// ─── component ────────────────────────────────────────────────────────────────
export default function SalesCommissionsDateRangeExcel({ dataState }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const [open, setOpen] = useState(false);
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf('month'));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [subtotalBy, setSubtotalBy] = useState<SubtotalBy>('projectName');
    const [respectFilters, setRespectFilters] = useState(false);
    const [isGenerating, setIsGenerating] = useState(false);
    const [trigger, { isFetching }] = useLazyFetchSalesCommissionsForExportQuery();

    const generateExcel = useCallback(async (data: any[], groupByField: SubtotalBy, filterInfo: string) => {
        if (!data || data.length === 0) return null;

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'Golden Land System';

        let logoBuffer: ArrayBuffer | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) logoBuffer = await resp.blob().then(b => b.arrayBuffer());
        } catch { /* no logo — continue */ }

        const period = `${startDate?.format('YYYY-MM-DD') || 'start'}_${endDate?.format('YYYY-MM-DD') || 'end'}`;
        const sheetName = `عمولات_${period}`.replace(/[*\?\\:\[\]\/]/g, '_').slice(0, 31);
        const ws = workbook.addWorksheet(sheetName);
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
        const title = utils.rtlEmbed(
            getTranslatedLabel(
                'salesCommission.report.daterange.title',
                `عمولات المبيعات (${startDate?.format('DD/MM/YYYY') || ''} - ${endDate?.format('DD/MM/YYYY') || ''})`
            )
        );
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:${LAST_COL_LETTER}${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(startRow).height = 40;

        // Filter info row (startRow+1) — fixes ExcelJS off-by-one on header styling
        const filterInfoRow = ws.addRow([utils.rtlEmbed(filterInfo)]);
        ws.mergeCells(`A${startRow + 1}:${LAST_COL_LETTER}${startRow + 1}`);
        filterInfoRow.font = { name: 'Amiri', size: 9, italic: true, color: { argb: 'FF555555' } };
        filterInfoRow.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
        filterInfoRow.height = 18;

        // Header row (startRow+2)
        const headerRowNum = startRow + 2;
        ws.addRow(COLUMN_DEFS.map(c => utils.rtlEmbed(c.header)));
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: 'Amiri', size: 10, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        headerRow.height = 24;

        // Data grouped with subtotals
        const subtotalLabel = getTranslatedLabel('salesCommission.report.subtotal', 'إجمالي');
        const grandTotalLabel = getTranslatedLabel('salesCommission.report.grandTotal', 'الإجمالي العام');
        const groups = groupBy(data, groupByField);
        const grandTotals: Record<string, number> = {};
        for (const col of COLUMN_DEFS) {
            if (col.isAmount) grandTotals[col.field] = 0;
        }

        groups.forEach((rows, groupName) => {
            const groupTotals = sumAmounts(rows);
            for (const col of COLUMN_DEFS) {
                if (col.isAmount) grandTotals[col.field] += groupTotals[col.field];
            }

            // Group header
            const groupHeaderRow = ws.addRow([utils.rtlEmbed(groupName)]);
            ws.mergeCells(`A${groupHeaderRow.number}:${LAST_COL_LETTER}${groupHeaderRow.number}`);
            groupHeaderRow.font = { name: 'Amiri', size: 10, bold: true, color: { argb: 'FF1A237E' } };
            groupHeaderRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE8EAF6' } };
            groupHeaderRow.alignment = { horizontal: 'right', vertical: 'middle' };
            groupHeaderRow.height = 20;

            // Data rows
            rows.forEach(record => {
                const row = ws.addRow(COLUMN_DEFS.map(c => buildCellValue(c, record)));
                row.font = { name: 'Amiri', size: 9 };
                row.alignment = { horizontal: 'right', wrapText: true };
                row.height = 20;
                COLUMN_DEFS.forEach((c, i) => {
                    if (c.isNumeric) row.getCell(i + 1).numFmt = '#,##0.00';
                });
            });

            // Subtotal row — label in A:I, amounts in their column positions
            const subtotalRowData = COLUMN_DEFS.map((c, i) => {
                if (i === 0) return utils.rtlEmbed(`${subtotalLabel}: ${groupName}`);
                if (i < 9)  return '';
                if (c.isAmount) return groupTotals[c.field];
                return '';
            });
            const subtotalRow = ws.addRow(subtotalRowData);
            ws.mergeCells(`A${subtotalRow.number}:${SUBTOTAL_LABEL_END}${subtotalRow.number}`);
            subtotalRow.font = { name: 'Amiri', size: 10, bold: true, color: { argb: 'FF1B5E20' } };
            subtotalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE8F5E9' } };
            subtotalRow.alignment = { horizontal: 'right', vertical: 'middle' };
            subtotalRow.height = 20;
            COLUMN_DEFS.forEach((c, i) => {
                if (c.isAmount) subtotalRow.getCell(i + 1).numFmt = '#,##0.00';
            });

            ws.addRow([]);
        });

        // Grand total row
        const grandTotalRowData = COLUMN_DEFS.map((c, i) => {
            if (i === 0) return utils.rtlEmbed(grandTotalLabel);
            if (i < 9)  return '';
            if (c.isAmount) return grandTotals[c.field];
            return '';
        });
        const grandTotalRow = ws.addRow(grandTotalRowData);
        ws.mergeCells(`A${grandTotalRow.number}:${SUBTOTAL_LABEL_END}${grandTotalRow.number}`);
        grandTotalRow.font = { name: 'Amiri', size: 11, bold: true };
        grandTotalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFFFF3E0' } };
        grandTotalRow.alignment = { horizontal: 'right', vertical: 'middle' };
        grandTotalRow.height = 24;
        COLUMN_DEFS.forEach((c, i) => {
            if (c.isAmount) grandTotalRow.getCell(i + 1).numFmt = '#,##0.00';
        });

        // Column widths
        ws.columns = COLUMN_DEFS.map(c => ({ width: c.width }));

        return await workbook.xlsx.writeBuffer();
    }, [getTranslatedLabel, startDate, endDate]);

    const handleDownload = useCallback(async () => {
        if (!startDate || !endDate) {
            alert(getTranslatedLabel('common.selectDates', 'الرجاء تحديد التاريخين'));
            return;
        }
        setIsGenerating(true);
        try {
            // Pass date range as dedicated URL params to avoid OData DateTime parsing edge cases.
            // Only the grid's non-date filters go into the OData $filter so ApplyTo stays reliable.
            const oDataQuery = toODataString({
                ...(respectFilters && dataState?.filter ? { filter: dataState.filter } : {}),
                sort: dataState?.sort,
            });

            const result = await trigger({
                oDataQuery,
                fromDate: startDate.format('YYYY-MM-DD'),
                toDate: endDate.format('YYYY-MM-DD'),
            }, false).unwrap();

            if (!result || result.length === 0) {
                toast.info(getTranslatedLabel('salesCommission.report.noData', 'لا توجد عمولات في الفترة المحددة'));
                return;
            }

            // Build filter info text
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
                const fileName = `SalesCommissions_${startDate.format('YYYYMMDD')}_${endDate.format('YYYYMMDD')}.xlsx`;
                saveAs(
                    new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }),
                    fileName
                );
                setOpen(false);
            }
        } catch (err: any) {
            console.error('Excel generation failed:', err);
            const msg = err?.data?.error || err?.message || 'Failed to generate report.';
            toast.error(msg);
        } finally {
            setIsGenerating(false);
        }
    }, [trigger, generateExcel, startDate, endDate, subtotalBy, respectFilters, dataState, getTranslatedLabel]);

    const isLoading = isFetching || isGenerating;

    return (
        <>
            <Button variant="contained" color="success" onClick={() => setOpen(true)} sx={{ ml: 1 }}>
                {getTranslatedLabel('salesCommission.report.button', 'تصدير Excel')}
            </Button>

            <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel('salesCommission.report.dialogTitle', 'تقرير عمولات المبيعات - Excel')}
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
                                    {getTranslatedLabel('salesCommission.report.subtotalBy', 'تجميع الإجماليات حسب')}
                                </FormLabel>
                                <RadioGroup
                                    value={subtotalBy}
                                    onChange={(e) => setSubtotalBy(e.target.value as SubtotalBy)}
                                >
                                    <FormControlLabel
                                        value="projectName"
                                        control={<Radio />}
                                        label={getTranslatedLabel('salesCommission.report.subtotalByProject', 'المشروع')}
                                    />
                                    <FormControlLabel
                                        value="salesRepName"
                                        control={<Radio />}
                                        label={getTranslatedLabel('salesCommission.report.subtotalBySalesRep', 'المندوب')}
                                    />
                                    <FormControlLabel
                                        value="saleTypeId"
                                        control={<Radio />}
                                        label={getTranslatedLabel('salesCommission.report.subtotalBySaleType', 'نوع البيع')}
                                    />
                                    <FormControlLabel
                                        value="statusId"
                                        control={<Radio />}
                                        label={getTranslatedLabel('salesCommission.report.subtotalByStatus', 'الحالة')}
                                    />
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
                                        'salesCommission.report.respectFilters',
                                        'مراعاة فلاتر قائمة العمولات'
                                    )}
                                />
                            )}
                        </Box>
                    </LocalizationProvider>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setOpen(false)} disabled={isLoading}>
                        {getTranslatedLabel('common.cancel', 'إلغاء')}
                    </Button>
                    <Button
                        onClick={handleDownload}
                        variant="contained"
                        disabled={isLoading || !startDate || !endDate}
                        startIcon={isLoading ? <CircularProgress size={20} /> : null}
                    >
                        {isLoading
                            ? getTranslatedLabel('common.generating', 'جاري الإنشاء...')
                            : getTranslatedLabel('common.download', 'تنزيل')}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
}
