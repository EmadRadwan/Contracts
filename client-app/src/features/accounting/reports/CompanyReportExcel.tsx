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
    FormControlLabel,
    Checkbox,
    Typography,
    Divider
} from '@mui/material';
import { DesktopDatePicker } from '@mui/x-date-pickers/DesktopDatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';
import { useLazyFetchCompanyReportQuery, ProjectReportDto } from "../../../app/store/apis/projectsApi";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

interface CompanyReportExcelProps {
    open: boolean;
    onClose: () => void;
}

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? '' : String(v).trim(),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined | null, dec = 2) =>
        v == null ? 0 : Number(v).toLocaleString('en-US', {
            minimumFractionDigits: dec,
            maximumFractionDigits: dec
        }),
    formatDate: (d: string | Date | undefined | null) => {
        if (!d) return '';
        const date = new Date(d);
        return isNaN(date.getTime()) ? '' : date.toLocaleDateString('en-GB');
    },
};

export const CompanyReportExcel: React.FC<CompanyReportExcelProps> = ({
                                                                          open,
                                                                          onClose
                                                                      }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const [expensesStartDate, setExpensesStartDate] = useState<Dayjs | null>(dayjs().startOf('year'));
    const [expensesEndDate, setExpensesEndDate] = useState<Dayjs | null>(dayjs());
    const [expensesAllData, setExpensesAllData] = useState(false);

    const [revenuesStartDate, setRevenuesStartDate] = useState<Dayjs | null>(dayjs().startOf('year'));
    const [revenuesEndDate, setRevenuesEndDate] = useState<Dayjs | null>(dayjs());
    const [revenuesAllData, setRevenuesAllData] = useState(false);

    const [salesStartDate, setSalesStartDate] = useState<Dayjs | null>(dayjs().startOf('year'));
    const [salesEndDate, setSalesEndDate] = useState<Dayjs | null>(dayjs());
    const [salesAllData, setSalesAllData] = useState(false);

    const [trigger, { isFetching }] = useLazyFetchCompanyReportQuery();
    const [isGenerating, setIsGenerating] = useState(false);

    const generateExcel = useCallback(async (data: ProjectReportDto) => {
        const workbook = new ExcelJS.Workbook();
        workbook.creator = 'Golden Land System';
        workbook.created = new Date();

        let logoBuffer: ArrayBuffer | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) logoBuffer = await resp.blob().then(b => b.arrayBuffer());
        } catch (e) {
            console.warn('Logo not found:', e);
        }

        const expPeriod = expensesAllData ? 'All_Data' :
            `${expensesStartDate?.format('YYYY-MM-DD')}_to_${expensesEndDate?.format('YYYY-MM-DD')}`;
        const revPeriod = revenuesAllData ? 'All_Data' :
            `${revenuesStartDate?.format('YYYY-MM-DD')}_to_${revenuesEndDate?.format('YYYY-MM-DD')}`;
        const salPeriod = salesAllData ? 'All_Data' :
            `${salesStartDate?.format('YYYY-MM-DD')}_to_${salesEndDate?.format('YYYY-MM-DD')}`;

        // ---------- helpers ----------
        const num = (v: any) => Number(v) || 0;
        const sum = (arr: any[] | undefined, sel: (x: any) => any) =>
            (arr || []).reduce((s, x) => s + num(sel(x)), 0);
        const expenseNet = (e: any) => e.netCertifiedAmount ??
            ((e.grossAmount || 0) - (e.discountAmount || 0) - (e.deductionsAmount || 0) - (e.insuranceAmount || 0));

        const addSheetHeader = (ws: ExcelJS.Worksheet, title: string, lastCol: number, fill: string) => {
            let r = 1;
            if (logoBuffer) {
                const imageId = workbook.addImage({ buffer: logoBuffer, extension: 'jpeg' });
                ws.addImage(imageId, { tl: { col: 0, row: 0 }, ext: { width: 140, height: 90 } });
                ws.getRow(1).height = 70;
                r = 6;
            } else {
                ws.getCell('A1').value = 'Golden Land';
                ws.getCell('A1').font = { name: 'Amiri', size: 18, bold: true };
                r = 3;
            }
            const t = ws.getCell(`A${r}`);
            t.value = utils.rtlEmbed(title);
            t.font = { name: 'Amiri', size: 16, bold: true, color: { argb: 'FFFFFFFF' } };
            t.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: fill } };
            t.alignment = { horizontal: 'center', vertical: 'middle' };
            ws.mergeCells(r, 1, r, lastCol);
            ws.getRow(r).height = 45;
            return r + 2;
        };

        // Generic payment-style block on its OWN sheet (own header/autofilter/total). amountCol is 1-based.
        const addPaymentSheet = (
            sheetName: string, title: string, titleFill: string, headerFill: string, altFill: string,
            headers: string[], items: any[], rowMapper: (x: any) => any[], amountCol: number, widths: number[]
        ) => {
            const ws = workbook.addWorksheet(sheetName);
            ws.views = [{ rightToLeft: true }];
            ws.pageSetup = { orientation: 'landscape', paperSize: 9 };
            addSheetHeader(ws, title, headers.length, titleFill);

            const headerRow = ws.addRow(headers.map(h => utils.rtlEmbed(h)));
            headerRow.font = { name: 'Amiri', size: 11, bold: true };
            headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: headerFill } };
            headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

            const dataStart = headerRow.number + 1;
            items.forEach((it, idx) => {
                const row = ws.addRow(rowMapper(it));
                const c = row.getCell(amountCol);
                c.numFmt = '#,##0.00';
                c.alignment = { horizontal: 'right' };
                if (idx % 2 === 1) row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: altFill } };
            });
            const dataEnd = ws.rowCount;

            const amountLetter = ws.getColumn(amountCol).letter;
            const totalArr: any[] = headers.map(() => '');
            totalArr[amountCol - 2] = utils.rtlEmbed('الإجمالي');
            totalArr[amountCol - 1] = { formula: `SUBTOTAL(109,${amountLetter}${dataStart}:${amountLetter}${dataEnd})` };
            const totalRow = ws.addRow(totalArr);
            totalRow.font = { name: 'Amiri', size: 12, bold: true };
            totalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: headerFill } };
            totalRow.getCell(amountCol).numFmt = '#,##0.00';
            totalRow.getCell(amountCol).alignment = { horizontal: 'right' };

            ws.autoFilter = { from: { row: headerRow.number, column: 1 }, to: { row: dataEnd, column: headers.length } };
            ws.columns.forEach((col, i) => (col.width = widths[i] || 15));
            return ws;
        };

        // ---------- pre-computed section totals (for the summary sheet) ----------
        const expTotal = sum(data.expenses, expenseNet);
        const directTotal = sum(data.directPayments, p => p.amount);
        const opTotal = sum(data.operatingExpenses, p => p.amount);
        const payrollTotal = sum(data.payroll, p => p.amount);
        const grandExpenses = expTotal + directTotal + opTotal + payrollTotal;
        const revScheduled = sum(data.revenues, r => r.scheduledAmount);
        const revCollected = sum(data.revenues, r => r.collectedAmount);
        const revOutstanding = sum(data.revenues, r => r.outstandingAmount);
        const soldRows = (data.apartmentSales || []).filter((s: any) => s.isSold);
        const availRows = (data.apartmentSales || []).filter((s: any) => !s.isSold);
        const soldTotal = sum(soldRows, s => s.totalPrice);
        const soldCollectedAdvance = sum(soldRows, s => s.advancePayment);

        // ====================== SUMMARY SHEET (first) ======================
        const wsSum = workbook.addWorksheet('ملخص التقرير');
        wsSum.views = [{ rightToLeft: true }];
        addSheetHeader(wsSum, `ملخص تقرير الشركة (${expPeriod})`, 4, 'FF1E40AF');

        const sumSection = (label: string, fill: string) => {
            const row = wsSum.addRow([utils.rtlEmbed(label), '']);
            wsSum.mergeCells(row.number, 1, row.number, 4);
            row.font = { name: 'Amiri', size: 13, bold: true, color: { argb: 'FFFFFFFF' } };
            row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: fill } };
            row.alignment = { horizontal: 'center', vertical: 'middle' };
            row.height = 26;
        };
        const sumLine = (label: string, value: number, opts: { bold?: boolean; fill?: string; int?: boolean } = {}) => {
            const row = wsSum.addRow([utils.rtlEmbed(label), value]);
            row.font = { name: 'Amiri', size: 12, bold: !!opts.bold };
            if (opts.fill) row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: opts.fill } };
            const v = row.getCell(2);
            v.numFmt = opts.int ? '#,##0' : '#,##0.00';
            v.alignment = { horizontal: 'right' };
            v.font = { name: 'Amiri', size: 12, bold: !!opts.bold };
        };

        sumSection('المصاريف', 'FF1E40AF');
        sumLine('المستخلصات', expTotal);
        sumLine('الدفعات المباشرة', directTotal);
        sumLine('المصاريف التشغيلية', opTotal);
        sumLine('الرواتب', payrollTotal);
        sumLine('إجمالي مصاريف الشركة', grandExpenses, { bold: true, fill: 'FFBFDBFE' });
        wsSum.addRow([]);

        sumSection('الإيرادات', 'FF065F46');
        sumLine('المجدول', revScheduled);
        sumLine('المحصل', revCollected);
        sumLine('المتبقي', revOutstanding);
        wsSum.addRow([]);

        sumSection('مبيعات الوحدات', 'FF0D9488');
        sumLine('عدد الوحدات المباعة', soldRows.length, { int: true });
        sumLine('إجمالي قيمة المبيعات', soldTotal);
        sumLine('إجمالي المقدمات المحصلة', soldCollectedAdvance);
        sumLine('عدد الوحدات المتاحة', availRows.length, { int: true });
        wsSum.addRow([]);

        sumSection('الصافي', 'FF7C3AED');
        sumLine('صافي (المحصل من العملاء - مصاريف الشركة)', revCollected - grandExpenses, { bold: true, fill: 'FFEDE9FE' });
        wsSum.getColumn(1).width = 46;
        wsSum.getColumn(2).width = 24;
        wsSum.getColumn(3).width = 4;
        wsSum.getColumn(4).width = 4;

        // ====================== CERTIFICATE EXPENSES SHEET ======================
        const wsExp = workbook.addWorksheet('المصاريف - المستخلصات');
        wsExp.views = [{ rightToLeft: true }];
        wsExp.pageSetup = { orientation: 'landscape', paperSize: 9 };
        addSheetHeader(wsExp, `المستخلصات للشركة (${expPeriod})`, 16, 'FF1E40AF');

        const expHeaders = [
            'المشروع', 'رقم الشهادة', 'رقم الدفعة', 'اسم الطرف', 'المنتج/الخدمة', 'التاريخ',
            'النوع', 'الوصف', 'وصف البند', 'الكمية', 'السعر', 'الإجمالي',
            'الخصم', 'الاستقطاعات', 'التأمين', 'صافي المعتمد'
        ];
        const headerRow = wsExp.addRow(expHeaders.map(h => utils.rtlEmbed(h)));
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFDBEAFE' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

        const dataStartRow = headerRow.number + 1;
        data.expenses.forEach((exp, idx) => {
            const row = wsExp.addRow([
                utils.safeString(exp.projectId),
                utils.safeString(exp.certificateNumber),
                utils.safeString(exp.paymentId),
                utils.safeString(exp.partyName || exp.partyId),
                utils.safeString(exp.productName || exp.productId),
                utils.formatDate(exp.expenseDate),
                utils.safeString(exp.certificateTypeArabic || exp.certificateType),
                utils.safeString(exp.certificateDescription),
                utils.safeString(exp.itemDescription),
                exp.quantity || 1,
                exp.unitRate || 0,
                exp.grossAmount || 0,
                exp.discountAmount || 0,
                exp.deductionsAmount || 0,
                exp.insuranceAmount || 0,
                expenseNet(exp)
            ]);
            for (let i = 11; i <= 16; i++) {
                const cell = row.getCell(i);
                cell.numFmt = '#,##0.00';
                cell.alignment = { horizontal: 'right' };
            }
            if (idx % 2 === 1) row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFF1F5F9' } };
        });
        const dataEndRow = wsExp.rowCount;

        const totalArr: any[] = new Array(16).fill('');
        totalArr[8] = utils.rtlEmbed('الإجمالي الكلي');
        totalArr[15] = { formula: `SUBTOTAL(109,P${dataStartRow}:P${dataEndRow})` };
        const totalRow = wsExp.addRow(totalArr);
        totalRow.font = { name: 'Amiri', size: 12, bold: true };
        totalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFBFDBFE' } };
        totalRow.getCell(16).numFmt = '#,##0.00';
        totalRow.getCell(16).alignment = { horizontal: 'right' };

        wsExp.autoFilter = { from: { row: headerRow.number, column: 1 }, to: { row: dataEndRow, column: 16 } };
        // @ts-ignore  disable filter buttons on numeric columns 10-16
        wsExp.autoFilter.columns = [
            {}, {}, {}, {}, {}, {}, {}, {}, {},
            { showButton: false }, { showButton: false }, { showButton: false },
            { showButton: false }, { showButton: false }, { showButton: false }, { showButton: false }
        ];
        const expColWidths = [15, 18, 16, 32, 28, 14, 25, 38, 45, 12, 16, 18, 16, 16, 16, 20];
        wsExp.columns.forEach((col, i) => (col.width = expColWidths[i] || 15));

        // ====================== DIRECT PAYMENTS SHEET ======================
        if (data.directPayments && data.directPayments.length > 0) {
            addPaymentSheet(
                'الدفعات المباشرة',
                `الدفعات المباشرة للشركة (${expPeriod})`,
                'FF10B981', 'FFD1FAE5', 'FFF0FDF4',
                ['المشروع', 'رقم الدفعة', 'النوع', 'من طرف', 'إلى طرف', 'الحالة', 'التاريخ', 'المبلغ',
                    'رقم المرجع', 'طريقة الدفع', 'الشيك', 'تاريخ الشيك', 'مركز التكلفة', 'ملاحظات'],
                data.directPayments,
                (p: any) => [
                    utils.safeString(p.projectId), utils.safeString(p.paymentId), utils.safeString(p.paymentTypeDescription),
                    utils.safeString(p.partyIdFromName), utils.safeString(p.partyIdToName),
                    utils.safeString(p.dueStatusArabic || p.statusDescription), utils.formatDate(p.effectiveDate),
                    p.amount || 0, utils.safeString(p.paymentRefNum || ''), utils.safeString(p.paymentMethodTypeDescription),
                    utils.safeString(p.chequeNumber), utils.formatDate(p.chequeDate),
                    utils.safeString(p.costCenterDescription), utils.safeString(p.comments)
                ],
                8, [15, 18, 20, 32, 32, 18, 14, 18, 18, 20, 16, 16, 25, 45]
            );
        }

        // ====================== OPERATING EXPENSES SHEET ======================
        if (data.operatingExpenses && data.operatingExpenses.length > 0) {
            addPaymentSheet(
                'المصاريف التشغيلية',
                `المصاريف التشغيلية للشركة (${expPeriod})`,
                'FF6366F1', 'FFE0E7FF', 'FFF5F3FF',
                ['المشروع', 'رقم الدفعة', 'النوع', 'من طرف', 'إلى طرف', 'الحالة', 'التاريخ', 'المبلغ',
                    'رقم المرجع', 'طريقة الدفع', 'الشيك', 'تاريخ الشيك', 'مركز التكلفة', 'ملاحظات'],
                data.operatingExpenses,
                (p: any) => [
                    utils.safeString(p.projectId), utils.safeString(p.paymentId), utils.safeString(p.paymentTypeDescription),
                    utils.safeString(p.partyIdFromName), utils.safeString(p.partyIdToName),
                    utils.safeString(p.dueStatusArabic || p.statusDescription), utils.formatDate(p.effectiveDate),
                    p.amount || 0, utils.safeString(p.paymentRefNum || ''), utils.safeString(p.paymentMethodTypeDescription),
                    utils.safeString(p.chequeNumber), utils.formatDate(p.chequeDate),
                    utils.safeString(p.costCenterDescription), utils.safeString(p.comments)
                ],
                8, [15, 18, 20, 32, 32, 18, 14, 18, 18, 20, 16, 16, 25, 45]
            );
        }

        // ====================== PAYROLL SHEET ======================
        if (data.payroll && data.payroll.length > 0) {
            addPaymentSheet(
                'الرواتب',
                `رواتب الشركة (${expPeriod})`,
                'FF0D9488', 'FFCCFBF1', 'FFF0FDFA',
                ['الحساب', 'رقم القيد', 'النوع', 'الموظف', 'إلى طرف', 'الحالة', 'التاريخ', 'المبلغ', 'ملاحظات'],
                data.payroll,
                (p: any) => [
                    utils.safeString(p.projectName), utils.safeString(p.paymentId), utils.safeString(p.paymentTypeDescription),
                    utils.safeString(p.partyIdFromName), utils.safeString(p.partyIdToName),
                    utils.safeString(p.dueStatusArabic || p.statusDescription), utils.formatDate(p.effectiveDate),
                    p.amount || 0, utils.safeString(p.comments)
                ],
                8, [22, 20, 22, 28, 24, 18, 14, 18, 45]
            );
        }

        // ====================== REVENUES SHEET ======================
        const wsRev = workbook.addWorksheet('الإيرادات');
        wsRev.views = [{ rightToLeft: true }];
        addSheetHeader(wsRev, `تقرير الإيرادات للشركة (${revPeriod})`, 19, 'FF1E40AF');

        const revHeaders = [
            'المشروع', 'رقم الدفعة', 'السنة', 'الربع', 'المبنى', 'الوحدة', 'العميل', 'الفئة', 'المجدول',
            'المحصل', 'المتبقي', 'الحالة', 'شريحة التأخير', 'حالة الاستحقاق', 'تاريخ الاستحقاق',
            'مستحق اليوم', 'مستحق خلال أسبوع', 'مستحق خلال شهر', 'متأخر'
        ];
        const revHeaderRow = wsRev.addRow(revHeaders.map(h => utils.rtlEmbed(h)));
        revHeaderRow.font = { name: 'Amiri', size: 11, bold: true };
        revHeaderRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFDBEAFE' } };
        revHeaderRow.alignment = { horizontal: 'center', vertical: 'middle' };

        const revDataStartRow = revHeaderRow.number + 1;
        data.revenues.forEach((rev: any) => {
            const row = wsRev.addRow([
                rev.projectName || rev.projectId,
                rev.paymentId,
                rev.year,
                rev.quarter,
                rev.buildingNumber,
                rev.apartmentId,
                utils.safeString(rev.customerName),
                utils.safeString(rev.revenueCategory),
                rev.scheduledAmount || 0,
                rev.collectedAmount || 0,
                rev.outstandingAmount || 0,
                utils.safeString(rev.paymentStatus),
                utils.safeString(rev.overdueBucket),
                utils.safeString(rev.dueStatusArabic),
                utils.formatDate(rev.dueDate),
                utils.safeString(rev.deservedToday),
                utils.safeString(rev.deservedWithinWeek),
                utils.safeString(rev.deservedWithinMonth),
                utils.safeString(rev.lateDue)
            ]);
            [9, 10, 11].forEach(col => {
                const cell = row.getCell(col);
                cell.numFmt = '#,##0.00';
                cell.alignment = { horizontal: 'right' };
            });
        });
        const revDataEndRow = wsRev.rowCount;

        const revTotalRow = wsRev.addRow([
            '', '', '', '', '', '', '', 'الإجمالي',
            { formula: `SUBTOTAL(109,I${revDataStartRow}:I${revDataEndRow})` },
            { formula: `SUBTOTAL(109,J${revDataStartRow}:J${revDataEndRow})` },
            { formula: `SUBTOTAL(109,K${revDataStartRow}:K${revDataEndRow})` },
            '', '', '', '', '', '', '', ''
        ]);
        revTotalRow.font = { name: 'Amiri', size: 12, bold: true };
        revTotalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFBFDBFE' } };
        for (let i = 9; i <= 11; i++) revTotalRow.getCell(i).numFmt = '#,##0.00';

        wsRev.autoFilter = { from: { row: revHeaderRow.number, column: 1 }, to: { row: revDataEndRow, column: revHeaders.length } };
        const revWidths = [18, 16, 10, 10, 14, 14, 32, 20, 18, 18, 18, 16, 24, 25, 16, 16, 18, 18, 12];
        wsRev.columns.forEach((col, i) => (col.width = revWidths[i] || 15));

        // ====================== APARTMENT SALES SHEET (company-wide) ======================
        if (data.apartmentSales && data.apartmentSales.length > 0) {
            const NOT_SOLD_FILL = 'FFFCE8B2';   // light amber = available/reserved, not sold
            const NOT_SOLD_TEXT = 'FF8A6D00';

            const wsSales = workbook.addWorksheet('مبيعات الوحدات');
            wsSales.views = [{ rightToLeft: true }];

            const salesTitleCell = wsSales.getCell('A1');
            salesTitleCell.value = utils.rtlEmbed(`تقرير مبيعات الوحدات للشركة (${salPeriod})`);
            salesTitleCell.font = { name: 'Amiri', size: 16, bold: true, color: { argb: 'FFFFFFFF' } };
            salesTitleCell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF0D9488' } }; // Teal
            salesTitleCell.alignment = { horizontal: 'center', vertical: 'middle' };
            wsSales.mergeCells('A1:Q1');
            wsSales.getRow(1).height = 30;

            wsSales.getCell('A2').value = utils.rtlEmbed('الصفوف الكهرمانية = وحدات بدون طلب بيع (متاحة/محجوزة)، غير مباعة.');
            wsSales.mergeCells('A2:Q2');
            wsSales.getRow(2).font = { name: 'Amiri', size: 10, italic: true, color: { argb: NOT_SOLD_TEXT } };
            wsSales.getRow(2).alignment = { horizontal: 'center', vertical: 'middle' };

            const salesHeaders = [
                'رقم الطلب', 'الوحدة', 'المبنى', 'الطابق', 'العميل', 'الموظف', 'الحالة', 'حالة الوحدة',
                'تاريخ البيع', 'الإجمالي', 'المقدم', 'وديعة الصيانة', 'مساحة الوحدة', 'مساحة الحديقة',
                'سعر المتر', 'المشروع', 'ملاحظات'
            ];
            const salesHeaderRow = wsSales.addRow(salesHeaders.map(h => utils.rtlEmbed(h)));
            salesHeaderRow.font = { name: 'Amiri', size: 11, bold: true };
            salesHeaderRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFCCFBF1' } }; // Light Teal
            salesHeaderRow.alignment = { horizontal: 'center', vertical: 'middle' };

            const salesDataStartRow = salesHeaderRow.number + 1;

            data.apartmentSales.forEach((sr: any) => {
                const isSold = !!sr.isSold;
                const row = wsSales.addRow([
                    utils.safeString(sr.salesRequestId),
                    utils.safeString(sr.apartmentName),
                    utils.safeString(sr.buildingNumber),
                    utils.safeString(sr.floorNumber),
                    utils.safeString(sr.fromPartyName),
                    utils.safeString(sr.employeeName),
                    utils.safeString(sr.statusDescription),
                    utils.safeString(sr.apartmentStatusDescription),
                    isSold ? utils.formatDate(sr.saleDate) : '',
                    isSold ? (sr.totalPrice ?? 0) : '',
                    isSold ? (sr.advancePayment ?? 0) : '',
                    isSold ? (sr.maintenanceDeposit ?? 0) : '',
                    sr.apartmentSpaceM2 ?? 0,
                    sr.gardenSpaceM2 ?? 0,
                    sr.apartmentPricePerM2 ?? 0,
                    utils.safeString(sr.projectName),
                    utils.safeString(sr.comments),
                ]);
                row.font = { name: 'Amiri', size: 10 };
                row.alignment = { horizontal: 'right', wrapText: true };
                [10, 11, 12, 13, 14, 15].forEach(col => {
                    row.getCell(col).numFmt = '#,##0.00';
                    row.getCell(col).alignment = { horizontal: 'right' };
                });

                if (!isSold) {
                    row.eachCell({ includeEmpty: true }, cell => {
                        cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: NOT_SOLD_FILL } };
                    });
                    row.getCell(7).font = { name: 'Amiri', size: 10, bold: true, color: { argb: NOT_SOLD_TEXT } };
                }
            });

            const salesDataEndRow = wsSales.rowCount;

            const salesTotalRow = wsSales.addRow([
                '', '', '', '', '', '', 'الإجمالي', '', '',
                { formula: `SUBTOTAL(109,J${salesDataStartRow}:J${salesDataEndRow})` },
                { formula: `SUBTOTAL(109,K${salesDataStartRow}:K${salesDataEndRow})` },
                { formula: `SUBTOTAL(109,L${salesDataStartRow}:L${salesDataEndRow})` },
                { formula: `SUBTOTAL(109,M${salesDataStartRow}:M${salesDataEndRow})` },
                { formula: `SUBTOTAL(109,N${salesDataStartRow}:N${salesDataEndRow})` },
                '', '', ''
            ]);
            salesTotalRow.font = { name: 'Amiri', size: 12, bold: true };
            salesTotalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF99F6E4' } };
            [10, 11, 12, 13, 14].forEach(col => salesTotalRow.getCell(col).numFmt = '#,##0.00');

            wsSales.autoFilter = {
                from: { row: salesHeaderRow.number, column: 1 },
                to: { row: salesDataEndRow, column: 17 }
            };

            const salesWidths = [15, 22, 16, 16, 22, 22, 18, 16, 14, 15, 15, 16, 18, 18, 16, 22, 32];
            wsSales.columns.forEach((col, i) => col.width = salesWidths[i] || 15);
        }

        return await workbook.xlsx.writeBuffer();
    }, [expensesAllData, expensesStartDate, expensesEndDate, revenuesAllData, revenuesStartDate, revenuesEndDate, salesAllData, salesStartDate, salesEndDate]);

    const handleDownload = useCallback(async () => {
        setIsGenerating(true);
        try {
            const result = await trigger({
                expensesStartDate: expensesAllData ? undefined : expensesStartDate?.format('YYYY-MM-DD'),
                expensesEndDate: expensesAllData ? undefined : expensesEndDate?.format('YYYY-MM-DD'),
                expensesAllData,
                revenuesStartDate: revenuesAllData ? undefined : revenuesStartDate?.format('YYYY-MM-DD'),
                revenuesEndDate: revenuesAllData ? undefined : revenuesEndDate?.format('YYYY-MM-DD'),
                revenuesAllData,
                salesStartDate: salesAllData ? undefined : salesStartDate?.format('YYYY-MM-DD'),
                salesEndDate: salesAllData ? undefined : salesEndDate?.format('YYYY-MM-DD'),
                salesAllData
            }).unwrap();

            const buffer = await generateExcel(result);

            const fileName = `Company_Report_${expensesAllData ? 'All' : expensesStartDate?.format('YYYYMMDD')}.xlsx`;

            const blob = new Blob([buffer], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
            });

            saveAs(blob, fileName);
            onClose();
        } catch (err) {
            console.error('Excel generation failed:', err);
            alert('فشل في إنشاء التقرير. يرجى المحاولة مرة أخرى.');
        } finally {
            setIsGenerating(false);
        }
    }, [trigger, generateExcel, expensesStartDate, expensesEndDate, expensesAllData, revenuesStartDate, revenuesEndDate, revenuesAllData, salesStartDate, salesEndDate, salesAllData, onClose]);

    const isLoading = isFetching || isGenerating;

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>تصدير تقرير الشركة - مصاريف مقابل إيرادات</DialogTitle>
            <DialogContent>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, mt: 2 }}>
                    <Box>
                        <Typography variant="subtitle1" gutterBottom sx={{ fontWeight: 'bold' }}>
                            المصاريف (Expenses)
                        </Typography>
                        <FormControlLabel
                            control={<Checkbox checked={expensesAllData} onChange={(e) => setExpensesAllData(e.target.checked)} />}
                            label="كل البيانات"
                        />
                        <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
                            <LocalizationProvider dateAdapter={AdapterDayjs}>
                                <DesktopDatePicker
                                    label="من تاريخ"
                                    value={expensesStartDate}
                                    onChange={setExpensesStartDate}
                                    disabled={expensesAllData}
                                    slotProps={{ textField: { fullWidth: true } }}
                                />
                                <DesktopDatePicker
                                    label="إلى تاريخ"
                                    value={expensesEndDate}
                                    minDate={expensesStartDate ?? undefined}
                                    onChange={setExpensesEndDate}
                                    disabled={expensesAllData}
                                    slotProps={{ textField: { fullWidth: true } }}
                                />
                            </LocalizationProvider>
                        </Box>
                    </Box>

                    <Divider />

                    <Box>
                        <Typography variant="subtitle1" gutterBottom sx={{ fontWeight: 'bold' }}>
                            الإيرادات (Revenues)
                        </Typography>
                        <FormControlLabel
                            control={<Checkbox checked={revenuesAllData} onChange={(e) => setRevenuesAllData(e.target.checked)} />}
                            label="كل البيانات"
                        />
                        <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
                            <LocalizationProvider dateAdapter={AdapterDayjs}>
                                <DesktopDatePicker
                                    label="من تاريخ"
                                    value={revenuesStartDate}
                                    onChange={setRevenuesStartDate}
                                    disabled={revenuesAllData}
                                    slotProps={{ textField: { fullWidth: true } }}
                                />
                                <DesktopDatePicker
                                    label="إلى تاريخ"
                                    value={revenuesEndDate}
                                    minDate={revenuesStartDate ?? undefined}
                                    onChange={setRevenuesEndDate}
                                    disabled={revenuesAllData}
                                    slotProps={{ textField: { fullWidth: true } }}
                                />
                            </LocalizationProvider>
                        </Box>
                    </Box>

                    <Divider />

                    <Box>
                        <Typography variant="subtitle1" gutterBottom sx={{ fontWeight: 'bold' }}>
                            مبيعات الوحدات (Apartment Sales)
                        </Typography>
                        <FormControlLabel
                            control={<Checkbox checked={salesAllData} onChange={(e) => setSalesAllData(e.target.checked)} />}
                            label="كل البيانات"
                        />
                        <Box sx={{ display: 'flex', gap: 2, mt: 1 }}>
                            <LocalizationProvider dateAdapter={AdapterDayjs}>
                                <DesktopDatePicker
                                    label="من تاريخ"
                                    value={salesStartDate}
                                    onChange={setSalesStartDate}
                                    disabled={salesAllData}
                                    slotProps={{ textField: { fullWidth: true } }}
                                />
                                <DesktopDatePicker
                                    label="إلى تاريخ"
                                    value={salesEndDate}
                                    minDate={salesStartDate ?? undefined}
                                    onChange={setSalesEndDate}
                                    disabled={salesAllData}
                                    slotProps={{ textField: { fullWidth: true } }}
                                />
                            </LocalizationProvider>
                        </Box>
                    </Box>
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>إلغاء</Button>
                <Button
                    onClick={handleDownload}
                    variant="contained"
                    disabled={isLoading || (!expensesAllData && (!expensesStartDate || !expensesEndDate)) || (!revenuesAllData && (!revenuesStartDate || !revenuesEndDate)) || (!salesAllData && (!salesStartDate || !salesEndDate))}
                    startIcon={isLoading ? <CircularProgress size={20} /> : null}
                >
                    {isLoading ? 'جاري الإنشاء...' : 'تحميل التقرير'}
                </Button>
            </DialogActions>
        </Dialog>
    );
};
