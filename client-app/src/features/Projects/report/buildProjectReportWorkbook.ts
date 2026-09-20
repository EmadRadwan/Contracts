import ExcelJS from 'exceljs';
import { ProjectReportDto } from '../../../app/store/apis/projectsApi';

/**
 * Builds the multi-sheet project-report workbook from a ProjectReportDto.
 *
 * Pure — no React, no component state. The dialog (ProjectReportExcel.tsx) and, later, the
 * in-app report screen both call this so there is exactly one Excel implementation.
 *
 * Summary totals are read from `data.summary` (computed server-side in
 * GetProjectReport / ProjectReportSummaryDto.Build) — this module never re-sums the sections,
 * so the summary sheet, the screen and any PDF export cannot drift.
 */
export interface ProjectReportWorkbookOptions {
    projectName: string;
    /** Pre-formatted period label for sheet titles, e.g. "2026-01-01_to_2026-09-10" or "All_Data". */
    expensesPeriod: string;
    revenuesPeriod: string;
    salesPeriod: string;
    commissionsPeriod: string;
    mgmtFeePeriod: string;
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

export async function buildProjectReportWorkbook(
    data: ProjectReportDto,
    opts: ProjectReportWorkbookOptions
) {
    const projectName = opts.projectName;
    const expPeriod = opts.expensesPeriod;
    const revPeriod = opts.revenuesPeriod;
    const salPeriod = opts.salesPeriod;
    const comPeriod = opts.commissionsPeriod;
    const feePeriod = opts.mgmtFeePeriod;

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

    // ---------- helpers ----------
    const expenseNet = (e: any) => e.netCertifiedAmount ??
        ((e.grossAmount || 0) - (e.discountAmount || 0) - (e.deductionsAmount || 0) - (e.insuranceAmount || 0));

    // Adds the logo (if any) + a coloured title row; returns the next free row index.
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

    // Generic "payment-style" block on its OWN sheet (own header/autofilter/total so the
    // user can sort & filter each block independently). amountCol is 1-based.
    //
    // `groups` (optional) partitions the rows into contiguous blocks, in the order given, and
    // adds one subtotal row per block ahead of the grand total. Each subtotal is a SUBTOTAL(109)
    // over that block's own row range, so it stays filter-aware like the grand total; the rows
    // are written block-by-block precisely so those ranges are contiguous.
    const addPaymentSheet = (
        sheetName: string, title: string, titleFill: string, headerFill: string, altFill: string,
        headers: string[], items: any[], rowMapper: (x: any) => any[], amountCol: number, widths: number[],
        groups?: { label: string; match: (x: any) => boolean }[]
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
        const blocks: { label: string; items: any[]; start: number; end: number }[] = groups
            ? groups.map(g => ({ label: g.label, items: items.filter(g.match), start: 0, end: 0 }))
            : [{ label: '', items, start: 0, end: 0 }];
        let idx = 0;
        blocks.forEach(b => {
            b.start = ws.rowCount + 1;
            b.items.forEach(it => {
                const row = ws.addRow(rowMapper(it));
                const c = row.getCell(amountCol);
                c.numFmt = '#,##0.00';
                c.alignment = { horizontal: 'right' };
                if (idx % 2 === 1) row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: altFill } };
                idx++;
            });
            b.end = ws.rowCount;
        });
        const dataEnd = ws.rowCount;

        const amountLetter = ws.getColumn(amountCol).letter;
        const addTotalRow = (label: string, from: number, to: number, bold: boolean, fill: string) => {
            const arr: any[] = headers.map(() => '');
            arr[amountCol - 2] = utils.rtlEmbed(label);
            arr[amountCol - 1] = to >= from
                ? { formula: `SUBTOTAL(109,${amountLetter}${from}:${amountLetter}${to})` }
                : 0;
            const row = ws.addRow(arr);
            row.font = { name: 'Amiri', size: bold ? 12 : 11, bold: true };
            row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: fill } };
            row.getCell(amountCol).numFmt = '#,##0.00';
            row.getCell(amountCol).alignment = { horizontal: 'right' };
        };
        if (groups) {
            blocks.forEach(b => addTotalRow(`${b.label} (${b.items.length})`, b.start, b.end, false, altFill));
        }
        addTotalRow('الإجمالي', dataStart, dataEnd, true, headerFill);

        ws.autoFilter = { from: { row: headerRow.number, column: 1 }, to: { row: dataEnd, column: headers.length } };
        ws.columns.forEach((col, i) => (col.width = widths[i] || 15));
        return ws;
    };

    // Direct payments: paid vs. still-open (PMNT_NOT_PAID). Mirrors ProjectReportSummaryDto.IsUnpaid.
    const isUnpaid = (p: any) => p.statusId === 'PMNT_NOT_PAID';
    const settlementLabel = (p: any) => (isUnpaid(p) ? 'غير مدفوعة' : 'مدفوعة');

    // ---------- data partitions still needed for the detail sheets ----------
    // Maintenance deposit is custodial, not project revenue — it gets its own sheet.
    const isMaintenance = (r: any) =>
        r.paymentTypeId === 'RECEIPT_MAINTENANCE_AMOUNT' || r.revenueCategory === 'Maintenance Deposit';
    // The revenues sheet lists every receipt in the revenue window — advance, installments AND
    // the maintenance receipts — as two contiguous blocks with their own subtotals, so the first
    // block still reconciles to the summary's الإيراد المتفق عليه (maintenance is custodial and
    // stays out of that figure and of the management-fee base).
    const agreedRevenues = (data.revenues || []).filter(r => !isMaintenance(r));
    const maintenanceReceipts = (data.revenues || []).filter(isMaintenance);
    // The وديعة الصيانة sheet itself is built from the sales requests (data.maintenanceDeposits).
    const maintenanceDeposits = data.maintenanceDeposits || [];
    const commissions = data.paidCommissions || [];
    const commissionEntries = data.paidCommissionAcctgEntries || [];

    // Summary totals come from the server (ProjectReportDto.Summary). This module never re-sums.
    const s = data.summary;
    if (!s) {
        throw new Error('التقرير لا يحتوي على ملخص محسوب من الخادم. يرجى تحديث الصفحة والمحاولة مرة أخرى.');
    }

    // ====================== SUMMARY SHEET (first) ======================
    const wsSum = workbook.addWorksheet('ملخص التقرير');
    wsSum.views = [{ rightToLeft: true }];
    addSheetHeader(wsSum, `${projectName} - الثروة الخضراء - ملخص التقرير`, 4, 'FF1E40AF');

    const sumSection = (label: string, fill: string) => {
        const row = wsSum.addRow([utils.rtlEmbed(label), '']);
        wsSum.mergeCells(row.number, 1, row.number, 4);
        row.font = { name: 'Amiri', size: 13, bold: true, color: { argb: 'FFFFFFFF' } };
        row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: fill } };
        row.alignment = { horizontal: 'center', vertical: 'middle' };
        row.height = 26;
    };
    const sumLine = (label: string, value: number, o: { bold?: boolean; fill?: string; int?: boolean } = {}) => {
        const row = wsSum.addRow([utils.rtlEmbed(label), value]);
        row.font = { name: 'Amiri', size: 12, bold: !!o.bold };
        if (o.fill) row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: o.fill } };
        const v = row.getCell(2);
        v.numFmt = o.int ? '#,##0' : '#,##0.00';
        v.alignment = { horizontal: 'right' };
        v.font = { name: 'Amiri', size: 12, bold: !!o.bold };
    };

    sumSection('المصاريف', 'FF1E40AF');
    sumLine('المستخلصات', s.certificateExpenses);
    sumLine(`الدفعات المباشرة — مدفوعة (${s.directPaymentsPaidCount ?? 0})`, s.directPaymentsPaid ?? s.directPayments);
    sumLine(`الدفعات المباشرة — غير مدفوعة (${s.directPaymentsUnpaidCount ?? 0})`, s.directPaymentsUnpaid ?? 0);
    sumLine('إجمالي الدفعات المباشرة', s.directPayments);
    sumLine('قيود محاسبية', s.accountingTransactions);
    sumLine('رواتب المشروع', s.projectPayroll);
    sumLine('المصاريف التشغيلية', s.operatingExpenses);
    sumLine('إجمالي مصاريف المشروع', s.totalProjectExpenses, { bold: true, fill: 'FFBFDBFE' });
    wsSum.addRow([]);

    sumSection('الإيرادات', 'FF065F46');
    sumLine('الإيراد المتفق عليه', s.revenueScheduled);
    sumLine('المحصل', s.revenueCollected);
    sumLine('المتبقي', s.revenueOutstanding);
    wsSum.addRow([]);

    sumSection('وديعة الصيانة', 'FF0F766E');
    sumLine(`عدد الوحدات (${s.maintenanceUnitsCollected ?? 0} محصلة بالكامل)`, s.maintenanceUnits ?? 0, { int: true });
    sumLine('إجمالي مبلغ الصيانة (حسب طلبات البيع)', s.maintenanceScheduled);
    sumLine('مبالغ الصيانة المحصلة', s.maintenanceCollected);
    sumLine('مبالغ الصيانة المتبقية', s.maintenanceOutstanding);
    wsSum.addRow([]);

    sumSection('مبيعات الوحدات', 'FF0D9488');
    sumLine('عدد الوحدات المباعة', s.unitsSold, { int: true });
    sumLine('إجمالي قيمة المبيعات', s.unitsSoldValue);
    sumLine('إجمالي المقدمات المحصلة', s.unitsAdvanceCollected);
    sumLine('عدد الوحدات المتاحة', s.unitsAvailable, { int: true });
    wsSum.addRow([]);

    sumSection('العمولات', 'FFB45309');
    sumLine('عدد دفعات العمولة', s.commissionPaymentCount, { int: true });
    sumLine('إجمالي العمولات المدفوعة', s.commissionsPaid);
    sumLine('إجمالي العمولات المستحقة', s.commissionsPending);
    sumLine('إجمالي العمولات', s.commissionsPaid + s.commissionsPending, { bold: true, fill: 'FFFDE9C8' });
    wsSum.addRow([]);

    sumSection(`مبلغ الإدارة (${feePeriod})`, 'FF1E40AF');
    const excludedLabel = s.mgmtExcludedBuildings.length
        ? ` (عدا ${s.mgmtExcludedBuildings.join('، ')})`
        : '';
    sumLine(`الإيراد المتفق عليه${excludedLabel}`, s.mgmtFeeBase);
    sumLine(`نسبة الإدارة (${s.mgmtFeePercent}%)`, s.mgmtFee);
    sumLine('يُخصم: المصاريف التشغيلية (فترة مبلغ الإدارة)', -(s.mgmtFeeOperatingExpenses ?? s.operatingExpenses));
    sumLine('صافي مبلغ الإدارة المتبقي', s.mgmtFeeNet, { bold: true, fill: 'FFBFDBFE' });
    wsSum.addRow([]);

    sumSection('الصافي', 'FF7C3AED');
    sumLine('صافي (المحصل من العملاء - مصاريف المشروع)', s.netAfterExpenses, { bold: true, fill: 'FFEDE9FE' });
    sumLine('الصافي بعد خصم العمولات المدفوعة', s.netAfterPaidCommissions, { bold: true, fill: 'FFEDE9FE' });
    wsSum.getColumn(1).width = 46;
    wsSum.getColumn(2).width = 24;
    wsSum.getColumn(3).width = 4;
    wsSum.getColumn(4).width = 4;

    // ====================== CERTIFICATE EXPENSES SHEET ======================
    const wsExp = workbook.addWorksheet('المصاريف - المستخلصات');
    wsExp.views = [{ rightToLeft: true }];
    wsExp.pageSetup = { orientation: 'landscape', paperSize: 9 };
    addSheetHeader(wsExp, `${projectName} - الثروة الخضراء - المستخلصات (${expPeriod})`, 15, 'FF1E40AF');

    // Column order per client request, matching the "الترتيب المطلوب" sheet they provided
    // (Project_Report_نسيم..._20260101.xlsx): date/description/party/net up front, ids and
    // amounts follow. Kept as one array so header row, data rows, widths and the net-column
    // total/formula can never drift out of sync when the order changes again.
    const expColumnDefs: { header: string; width: number; numeric?: boolean; get: (exp: any) => any }[] = [
        { header: 'التاريخ', width: 14, get: exp => utils.formatDate(exp.expenseDate) },
        { header: 'وصف البند', width: 45, get: exp => utils.safeString(exp.itemDescription) },
        { header: 'اسم الطرف', width: 32, get: exp => utils.safeString(exp.partyName || exp.partyId) },
        { header: 'صافي المعتمد', width: 20, numeric: true, get: exp => expenseNet(exp) },
        { header: 'رقم الدفعة', width: 16, get: exp => utils.safeString(exp.paymentId) },
        { header: 'رقم الشهادة', width: 18, get: exp => utils.safeString(exp.certificateNumber) },
        { header: 'المنتج/الخدمة', width: 28, get: exp => utils.safeString(exp.productName || exp.productId) },
        { header: 'النوع', width: 25, get: exp => utils.safeString(exp.certificateTypeArabic || exp.certificateType) },
        { header: 'الوصف', width: 38, get: exp => utils.safeString(exp.certificateDescription) },
        { header: 'الكمية', width: 12, numeric: true, get: exp => exp.quantity || 1 },
        { header: 'السعر', width: 16, numeric: true, get: exp => exp.unitRate || 0 },
        { header: 'الإجمالي', width: 18, numeric: true, get: exp => exp.grossAmount || 0 },
        { header: 'الخصم', width: 16, numeric: true, get: exp => exp.discountAmount || 0 },
        { header: 'الاستقطاعات', width: 16, numeric: true, get: exp => exp.deductionsAmount || 0 },
        { header: 'التأمين', width: 16, numeric: true, get: exp => exp.insuranceAmount || 0 }
    ];
    const netColIndex = expColumnDefs.findIndex(c => c.header === 'صافي المعتمد') + 1; // 1-based

    const headerRow = wsExp.addRow(expColumnDefs.map(c => utils.rtlEmbed(c.header)));
    headerRow.font = { name: 'Amiri', size: 11, bold: true };
    headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFDBEAFE' } };
    headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

    const dataStartRow = headerRow.number + 1;
    data.expenses.forEach((exp, idx) => {
        const row = wsExp.addRow(expColumnDefs.map(c => c.get(exp)));
        expColumnDefs.forEach((c, i) => {
            if (!c.numeric) return;
            const cell = row.getCell(i + 1);
            cell.numFmt = '#,##0.00';
            cell.alignment = { horizontal: 'right' };
        });
        if (idx % 2 === 1) row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFF1F5F9' } };
    });
    const dataEndRow = wsExp.rowCount;

    const netColLetter = wsExp.getColumn(netColIndex).letter;
    const totalArr: any[] = expColumnDefs.map(() => '');
    totalArr[netColIndex - 2] = utils.rtlEmbed('الإجمالي الكلي');
    totalArr[netColIndex - 1] = { formula: `SUBTOTAL(109,${netColLetter}${dataStartRow}:${netColLetter}${dataEndRow})` };
    const totalRow = wsExp.addRow(totalArr);
    totalRow.font = { name: 'Amiri', size: 12, bold: true };
    totalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFBFDBFE' } };
    totalRow.getCell(netColIndex).numFmt = '#,##0.00';
    totalRow.getCell(netColIndex).alignment = { horizontal: 'right' };

    wsExp.autoFilter = { from: { row: headerRow.number, column: 1 }, to: { row: dataEndRow, column: expColumnDefs.length } };
    // @ts-ignore  disable filter buttons on numeric columns
    wsExp.autoFilter.columns = expColumnDefs.map(c => (c.numeric ? { showButton: false } : {}));
    wsExp.columns.forEach((col, i) => (col.width = expColumnDefs[i]?.width || 15));

    // ====================== DIRECT PAYMENTS SHEET ======================
    if (data.directPayments && data.directPayments.length > 0) {
        addPaymentSheet(
            'الدفعات المباشرة',
            `${projectName} - الدفعات المباشرة (${expPeriod})`,
            'FF10B981', 'FFD1FAE5', 'FFF0FDF4',
            ['رقم الدفعة', 'النوع', 'من طرف', 'إلى طرف', 'حالة السداد', 'الحالة', 'التاريخ', 'المبلغ', 'رقم المرجع',
                'طريقة الدفع', 'الشيك', 'تاريخ الشيك', 'مركز التكلفة', 'ملاحظات'],
            data.directPayments,
            (p: any) => [
                utils.safeString(p.paymentId), utils.safeString(p.paymentTypeDescription),
                utils.safeString(p.partyIdFromName), utils.safeString(p.partyIdToName),
                settlementLabel(p),
                utils.safeString(p.dueStatusArabic || p.statusDescription), utils.formatDate(p.effectiveDate),
                p.amount || 0, utils.safeString(p.paymentRefNum || ''),
                utils.safeString(p.paymentMethodTypeDescription), utils.safeString(p.chequeNumber),
                utils.formatDate(p.chequeDate), utils.safeString(p.costCenterDescription), utils.safeString(p.comments)
            ],
            8, [18, 20, 32, 32, 14, 18, 14, 18, 18, 20, 16, 16, 25, 45],
            [
                { label: 'إجمالي المدفوعة', match: (p) => !isUnpaid(p) },
                { label: 'إجمالي غير المدفوعة', match: isUnpaid },
            ]
        );
    }

    // ====================== ACCOUNTING TRANSACTIONS SHEET ======================
    if (data.accountingTransactions && data.accountingTransactions.length > 0) {
        addPaymentSheet(
            'قيود محاسبية',
            `${projectName} - قيود محاسبية (${expPeriod})`,
            'FF6366F1', 'FFE0E7FF', 'FFF5F3FF',
            ['رقم القيد', 'النوع', 'من طرف', 'إلى طرف', 'الحالة', 'التاريخ', 'المبلغ', 'رقم المرجع', 'ملاحظات'],
            data.accountingTransactions,
            (t: any) => [
                utils.safeString(t.paymentId), utils.safeString(t.paymentTypeDescription),
                utils.safeString(t.partyIdFromName), utils.safeString(t.partyIdToName),
                utils.safeString(t.dueStatusArabic || t.statusDescription), utils.formatDate(t.effectiveDate),
                t.amount || 0, utils.safeString(t.paymentRefNum || ''), utils.safeString(t.comments)
            ],
            7, [20, 22, 28, 28, 18, 14, 18, 18, 45]
        );
    }

    // ====================== PROJECT PAYROLL SHEET ======================
    if (data.payroll && data.payroll.length > 0) {
        addPaymentSheet(
            'رواتب المشروع',
            `${projectName} - رواتب المشروع (${expPeriod})`,
            'FF0D9488', 'FFCCFBF1', 'FFF0FDFA',
            ['رقم القيد', 'النوع', 'الموظف', 'المشروع', 'الحالة', 'التاريخ', 'المبلغ', 'ملاحظات'],
            data.payroll,
            (p: any) => [
                utils.safeString(p.paymentId), utils.safeString(p.paymentTypeDescription),
                utils.safeString(p.partyIdFromName), utils.safeString(p.partyIdToName),
                utils.safeString(p.dueStatusArabic || p.statusDescription), utils.formatDate(p.effectiveDate),
                p.amount || 0, utils.safeString(p.comments)
            ],
            7, [22, 22, 28, 28, 18, 14, 18, 45]
        );
    }

    // ====================== OPERATING EXPENSES SHEET ======================
    if (data.operatingExpenses && data.operatingExpenses.length > 0) {
        addPaymentSheet(
            'المصاريف التشغيلية',
            `${projectName} - الثروة الخضراء - المصاريف التشغيلية (${expPeriod})`,
            'FF6366F1', 'FFE0E7FF', 'FFF5F3FF',
            ['رقم الدفعة', 'النوع', 'من طرف', 'إلى طرف', 'الحالة', 'التاريخ', 'المبلغ', 'رقم المرجع',
                'طريقة الدفع', 'الشيك', 'تاريخ الشيك', 'مركز التكلفة', 'ملاحظات'],
            data.operatingExpenses,
            (p: any) => [
                utils.safeString(p.paymentId), utils.safeString(p.paymentTypeDescription),
                utils.safeString(p.partyIdFromName), utils.safeString(p.partyIdToName),
                utils.safeString(p.dueStatusArabic || p.statusDescription), utils.formatDate(p.effectiveDate),
                p.amount || 0, utils.safeString(p.paymentRefNum || ''),
                utils.safeString(p.paymentMethodTypeDescription), utils.safeString(p.chequeNumber),
                utils.formatDate(p.chequeDate), utils.safeString(p.costCenterDescription), utils.safeString(p.comments)
            ],
            7, [18, 20, 32, 32, 18, 14, 18, 18, 20, 16, 16, 25, 45]
        );
    }

    // ====================== REVENUES SHEETS ======================
    // Agreed revenue (advance + installments) and the maintenance deposit are shown on their
    // own sheets so each carries its own total; maintenance is custodial, not project revenue.
    const revHeaders = [
        'رقم الدفعة', 'السنة', 'الربع', 'المبنى', 'الوحدة', 'العميل', 'الفئة', 'الإيراد المتفق عليه',
        'المحصل', 'المتبقي', 'الحالة', 'شريحة التأخير', 'حالة الاستحقاق', 'تاريخ الاستحقاق',
        'مستحق اليوم', 'مستحق خلال أسبوع', 'مستحق خلال شهر', 'متأخر'
    ];
    const revWidths = [16, 10, 10, 14, 14, 32, 20, 20, 18, 18, 16, 24, 25, 16, 16, 18, 18, 12];

    // `blocks` are written contiguously in order, each followed (after the data) by its own
    // SUBTOTAL(109) row over its range, then the grand total — same pattern as the direct payments.
    const addRevenueSheet = (
        sheetName: string, title: string, blocks: { label: string; rows: any[] }[], amountHeader: string
    ) => {
        const ws = workbook.addWorksheet(sheetName);
        ws.views = [{ rightToLeft: true }];
        const headers = revHeaders.slice();
        headers[7] = amountHeader;
        addSheetHeader(ws, title, headers.length, 'FF1E40AF');

        const hRow = ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        hRow.font = { name: 'Amiri', size: 11, bold: true };
        hRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFDBEAFE' } };
        hRow.alignment = { horizontal: 'center', vertical: 'middle' };

        const dataStart = hRow.number + 1;
        const ranges: { label: string; n: number; start: number; end: number }[] = [];
        blocks.forEach(b => {
            const start = ws.rowCount + 1;
            b.rows.forEach((rev: any) => {
                const row = ws.addRow([
                    rev.paymentId, rev.year, rev.quarter, rev.buildingNumber, rev.apartmentId,
                    utils.safeString(rev.customerName), utils.safeString(rev.revenueCategory),
                    rev.scheduledAmount || 0, rev.collectedAmount || 0, rev.outstandingAmount || 0,
                    utils.safeString(rev.paymentStatus), utils.safeString(rev.overdueBucket),
                    utils.safeString(rev.dueStatusArabic), utils.formatDate(rev.dueDate),
                    utils.safeString(rev.deservedToday), utils.safeString(rev.deservedWithinWeek),
                    utils.safeString(rev.deservedWithinMonth), utils.safeString(rev.lateDue)
                ]);
                [8, 9, 10].forEach(col => {
                    const cell = row.getCell(col);
                    cell.numFmt = '#,##0.00';
                    cell.alignment = { horizontal: 'right' };
                });
            });
            ranges.push({ label: b.label, n: b.rows.length, start, end: ws.rowCount });
        });
        const dataEnd = ws.rowCount;

        const addTotal = (label: string, from: number, to: number, bold: boolean, fill: string) => {
            const sub = (col: string) => (to >= from ? { formula: `SUBTOTAL(109,${col}${from}:${col}${to})` } : 0);
            const tRow = ws.addRow([
                '', '', '', '', '', '', utils.rtlEmbed(label), sub('H'), sub('I'), sub('J'),
                '', '', '', '', '', '', '', ''
            ]);
            tRow.font = { name: 'Amiri', size: bold ? 12 : 11, bold: true };
            tRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: fill } };
            for (let i = 8; i <= 10; i++) tRow.getCell(i).numFmt = '#,##0.00';
        };
        if (blocks.length > 1) {
            ranges.forEach(r => addTotal(`${r.label} (${r.n})`, r.start, r.end, false, 'FFEFF6FF'));
        }
        addTotal('الإجمالي', dataStart, dataEnd, true, 'FFBFDBFE');

        ws.autoFilter = { from: { row: hRow.number, column: 1 }, to: { row: dataEnd, column: headers.length } };
        // @ts-ignore  disable filter buttons on numeric columns
        ws.autoFilter.columns = [
            {}, { showButton: false }, { showButton: false }, {}, {}, {}, {},
            { showButton: false }, { showButton: false }, { showButton: false },
            {}, {}, {}, {}, {}, {}, {}, {}
        ];
        ws.columns.forEach((col, i) => (col.width = revWidths[i] || 15));
    };

    addRevenueSheet('الإيرادات', `${projectName} - الثروة الخضراء - الإيرادات (${revPeriod})`,
        [
            { label: 'إجمالي الإيراد المتفق عليه', rows: agreedRevenues },
            { label: 'إجمالي وديعة الصيانة', rows: maintenanceReceipts },
        ],
        'الإيراد المتفق عليه');

    // ====================== MAINTENANCE DEPOSIT SHEET ======================
    // One row per SOLD unit (same population and sales window as مبيعات الوحدات): the deposit
    // agreed on the sales request vs. what the maintenance receipts have collected so far.
    if (maintenanceDeposits.length > 0) {
        const ws = workbook.addWorksheet('وديعة الصيانة');
        ws.views = [{ rightToLeft: true }];
        ws.pageSetup = { orientation: 'landscape', paperSize: 9 };
        const headers = [
            'رقم الطلب', 'المبنى', 'الوحدة', 'الطابق', 'العميل', 'تاريخ البيع', 'إجمالي البيع', 'نسبة الصيانة %',
            'وديعة الصيانة', 'المحصل', 'المتبقي', 'حالة التحصيل', 'حالة الاستحقاق', 'تاريخ الاستحقاق',
            'عدد الإيصالات', 'الإيصالات المحصلة'
        ];
        const widths = [14, 10, 22, 16, 32, 14, 18, 14, 18, 18, 18, 16, 26, 16, 14, 16];
        addSheetHeader(ws, `${projectName} - الثروة الخضراء - وديعة الصيانة (${salPeriod})`, headers.length, 'FF0F766E');

        const hRow = ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        hRow.font = { name: 'Amiri', size: 11, bold: true };
        hRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFCCFBF1' } };
        hRow.alignment = { horizontal: 'center', vertical: 'middle' };

        const dataStart = hRow.number + 1;
        maintenanceDeposits.forEach((m, idx) => {
            const row = ws.addRow([
                utils.safeString(m.salesRequestId), utils.safeString(m.buildingNumber), utils.safeString(m.apartmentName),
                utils.safeString(m.floorNumber), utils.safeString(m.customerName), utils.formatDate(m.saleDate),
                m.totalPrice ?? 0, m.maintenancePercent != null ? Number(m.maintenancePercent) * 100 : '',
                m.maintenanceDeposit || 0, m.collectedAmount || 0, m.outstandingAmount || 0,
                utils.safeString(m.collectionStatusArabic), utils.safeString(m.dueStatusArabic),
                utils.formatDate(m.nextDueDate), m.receiptCount ?? 0, m.receivedCount ?? 0
            ]);
            [7, 9, 10, 11].forEach(col => {
                const cell = row.getCell(col);
                cell.numFmt = '#,##0.00';
                cell.alignment = { horizontal: 'right' };
            });
            row.getCell(8).numFmt = '0.00';
            if (idx % 2 === 1) row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFF0FDFA' } };
        });
        const dataEnd = ws.rowCount;

        const tRow = ws.addRow([
            '', '', '', '', '', '', '', 'الإجمالي',
            { formula: `SUBTOTAL(109,I${dataStart}:I${dataEnd})` },
            { formula: `SUBTOTAL(109,J${dataStart}:J${dataEnd})` },
            { formula: `SUBTOTAL(109,K${dataStart}:K${dataEnd})` },
            '', '', '', '', ''
        ]);
        tRow.font = { name: 'Amiri', size: 12, bold: true };
        tRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFCCFBF1' } };
        [9, 10, 11].forEach(col => {
            tRow.getCell(col).numFmt = '#,##0.00';
            tRow.getCell(col).alignment = { horizontal: 'right' };
        });

        ws.autoFilter = { from: { row: hRow.number, column: 1 }, to: { row: dataEnd, column: headers.length } };
        ws.columns.forEach((col, i) => (col.width = widths[i] || 15));
    }

    // ====================== APARTMENT SALES SHEET ======================
    if (data.apartmentSales && data.apartmentSales.length > 0) {
        const NOT_SOLD_FILL = 'FFFCE8B2';   // light amber = available/reserved, not sold
        const NOT_SOLD_TEXT = 'FF8A6D00';

        const wsSales = workbook.addWorksheet('مبيعات الوحدات');
        wsSales.views = [{ rightToLeft: true }];

        const salesTitleCell = wsSales.getCell('A1');
        salesTitleCell.value = utils.rtlEmbed(`${projectName} - الثروة الخضراء - مبيعات الوحدات (${salPeriod})`);
        salesTitleCell.font = { name: 'Amiri', size: 16, bold: true, color: { argb: 'FFFFFFFF' } };
        salesTitleCell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF0D9488' } }; // Teal
        salesTitleCell.alignment = { horizontal: 'center', vertical: 'middle' };
        wsSales.mergeCells('A1:Q1');
        wsSales.getRow(1).height = 30;

        // Legend explaining the amber (not-sold) rows.
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

    // ====================== PAID COMMISSIONS SHEET ======================
    // One row per commission payee-payment. Filterable by commission number, sales-request
    // number, unit, building, payee, amount and status (paid vs. still owed).
    if (commissions.length > 0) {
        addPaymentSheet(
            'العمولات المدفوعة',
            `${projectName} - الثروة الخضراء - العمولات المدفوعة (${comPeriod})`,
            'FFB45309', 'FFFDE9C8', 'FFFEF6E7',
            ['رقم العمولة', 'رقم طلب البيع', 'الوحدة', 'المبنى', 'المستفيد', 'المبلغ', 'حالة الدفع',
                'حالة العمولة', 'طريقة الدفع', 'التاريخ', 'رقم الشيك', 'رقم الدفعة',
                'مركز التكلفة', 'وصف مركز التكلفة',
                'رقم الحساب البديل', 'كود الحساب البديل', 'اسم الحساب البديل', 'ملاحظات'],
            commissions,
            (c: any) => [
                utils.safeString(c.salesCommissionId), utils.safeString(c.salesRequestId),
                utils.safeString(c.apartmentName), utils.safeString(c.buildingNumber),
                utils.safeString(c.payeeName || c.payeePartyId),
                c.amount || 0,
                utils.safeString(c.paymentStatusArabic),
                utils.safeString(c.commissionStatusArabic),
                utils.safeString(c.paymentMethodTypeArabic),
                utils.formatDate(c.effectiveDate), utils.safeString(c.chequeNumber),
                utils.safeString(c.paymentId),
                utils.safeString(c.costCenterId), utils.safeString(c.costCenterDescription),
                utils.safeString(c.overrideGlAccountId), utils.safeString(c.overrideGlAccountCode),
                utils.safeString(c.overrideGlAccountNameArabic),
                utils.safeString(c.comments)
            ],
            6, [16, 16, 26, 12, 30, 16, 18, 18, 18, 14, 16, 16, 16, 26, 18, 16, 30, 55]
        );
    }

    // ====================== PAID COMMISSIONS — LEDGER ENTRIES SHEET ======================
    // Companion to العمولات المدفوعة: one row per AcctgTransEntry of every accounting transaction
    // linked to those payments (including unposted / reversal transactions, so the auditor sees
    // the whole posting history). Debit and credit are separate columns with their own subtotals
    // so a filtered selection can be checked for balance directly in Excel.
    if (commissionEntries.length > 0) {
        const wsEnt = workbook.addWorksheet('قيود العمولات المدفوعة');
        wsEnt.views = [{ rightToLeft: true }];
        wsEnt.pageSetup = { orientation: 'landscape', paperSize: 9 };
        const entHeaders = [
            'رقم العمولة', 'رقم طلب البيع', 'الوحدة', 'المستفيد', 'رقم الدفعة',
            'رقم القيد', 'نوع القيد', 'تاريخ القيد', 'مرحّل', 'تاريخ الترحيل', 'النوع المالي',
            'وصف القيد', 'مركز التكلفة', 'وصف مركز التكلفة',
            'تسلسل', 'رقم الحساب', 'كود الحساب', 'اسم الحساب', 'مدين', 'دائن',
            'طرف البند', 'وصف البند'
        ];
        addSheetHeader(wsEnt, `${projectName} - الثروة الخضراء - قيود العمولات المدفوعة (${comPeriod})`,
            entHeaders.length, 'FF92400E');

        const entHeaderRow = wsEnt.addRow(entHeaders.map(h => utils.rtlEmbed(h)));
        entHeaderRow.font = { name: 'Amiri', size: 11, bold: true };
        entHeaderRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFFDE9C8' } };
        entHeaderRow.alignment = { horizontal: 'center', vertical: 'middle' };

        const DEBIT_COL = 19, CREDIT_COL = 20;
        const entDataStart = entHeaderRow.number + 1;
        // Alternate the band per accounting transaction (not per row) so the entries of one
        // trans read as a group.
        let band = 0;
        let lastTrans: string | undefined;
        commissionEntries.forEach((e: any) => {
            if (e.acctgTransId !== lastTrans) { band ^= 1; lastTrans = e.acctgTransId; }
            const row = wsEnt.addRow([
                utils.safeString(e.salesCommissionId), utils.safeString(e.salesRequestId),
                utils.safeString(e.apartmentName), utils.safeString(e.payeeName),
                utils.safeString(e.paymentId),
                utils.safeString(e.acctgTransId), utils.safeString(e.acctgTransTypeDescription),
                utils.formatDate(e.transactionDate),
                e.isPosted === 'Y' ? 'نعم' : 'لا',
                utils.formatDate(e.postedDate), utils.safeString(e.glFiscalTypeId),
                utils.safeString(e.transDescription),
                utils.safeString(e.costCenterId), utils.safeString(e.costCenterDescription),
                utils.safeString(e.acctgTransEntrySeqId),
                utils.safeString(e.glAccountId), utils.safeString(e.accountCode),
                utils.safeString(e.accountNameArabic || e.accountName),
                e.debit || 0, e.credit || 0,
                utils.safeString(e.entryPartyName || e.entryPartyId),
                utils.safeString(e.entryDescription)
            ]);
            row.font = { name: 'Amiri', size: 10 };
            [DEBIT_COL, CREDIT_COL].forEach(col => {
                row.getCell(col).numFmt = '#,##0.00';
                row.getCell(col).alignment = { horizontal: 'right' };
            });
            if (band === 1) row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFFEF6E7' } };
        });
        const entDataEnd = wsEnt.rowCount;

        const dL = wsEnt.getColumn(DEBIT_COL).letter;
        const cL = wsEnt.getColumn(CREDIT_COL).letter;
        const entTotalArr: any[] = entHeaders.map(() => '');
        entTotalArr[DEBIT_COL - 2] = utils.rtlEmbed('الإجمالي');
        entTotalArr[DEBIT_COL - 1] = { formula: `SUBTOTAL(109,${dL}${entDataStart}:${dL}${entDataEnd})` };
        entTotalArr[CREDIT_COL - 1] = { formula: `SUBTOTAL(109,${cL}${entDataStart}:${cL}${entDataEnd})` };
        const entTotalRow = wsEnt.addRow(entTotalArr);
        entTotalRow.font = { name: 'Amiri', size: 12, bold: true };
        entTotalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFFDE9C8' } };
        [DEBIT_COL, CREDIT_COL].forEach(col => {
            entTotalRow.getCell(col).numFmt = '#,##0.00';
            entTotalRow.getCell(col).alignment = { horizontal: 'right' };
        });

        wsEnt.autoFilter = {
            from: { row: entHeaderRow.number, column: 1 },
            to: { row: entDataEnd, column: entHeaders.length }
        };
        const entWidths = [16, 16, 26, 30, 16, 16, 22, 14, 8, 14, 12, 40, 16, 26, 8, 16, 14, 30, 16, 16, 30, 40];
        wsEnt.columns.forEach((col, i) => (col.width = entWidths[i] || 15));
    }

    return await workbook.xlsx.writeBuffer();
}
