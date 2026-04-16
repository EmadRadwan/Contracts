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
    Checkbox
} from '@mui/material';
import { DesktopDatePicker } from '@mui/x-date-pickers/DesktopDatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';
import { useLazyFetchProjectReportQuery, ProjectReportDto } from "../../../app/store/apis/projectsApi";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";

interface ProjectReportExcelProps {
    projectId: string;
    projectName: string;
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

export const ProjectReportExcel: React.FC<ProjectReportExcelProps> = ({
                                                                          projectId,
                                                                          projectName,
                                                                          open,
                                                                          onClose
                                                                      }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf('month'));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [allData, setAllData] = useState(false);
    const [trigger, { isFetching }] = useLazyFetchProjectReportQuery();
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

        const period = allData ? 'All_Data' :
            `${startDate?.format('YYYY-MM-DD')}_to_${endDate?.format('YYYY-MM-DD')}`;

        // ====================== EXPENSES SHEET ======================
        const wsExp = workbook.addWorksheet('المصاريف');
        wsExp.views = [{ rightToLeft: true }];
        wsExp.pageSetup = { orientation: 'landscape', paperSize: 9 };

        let currentRow = 1;

        // Logo
        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: 'jpeg' });
            wsExp.addImage(imageId, { tl: { col: 0, row: 0 }, ext: { width: 140, height: 90 } });
            wsExp.getRow(1).height = 70;
            currentRow = 6;
        } else {
            wsExp.getCell('A1').value = 'Golden Land';
            wsExp.getCell('A1').font = { name: 'Amiri', size: 18, bold: true };
            currentRow = 3;
        }

        // Main Title
        const titleCell = wsExp.getCell(`A${currentRow}`);
        titleCell.value = utils.rtlEmbed(`${projectName} - الثروة الخضراء - المصاريف (${period})`);
        titleCell.font = { name: 'Amiri', size: 16, bold: true, color: { argb: 'FFFFFFFF' } };
        titleCell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF1E40AF' } };
        titleCell.alignment = { horizontal: 'center', vertical: 'middle' };
        wsExp.mergeCells(`A${currentRow}:O${currentRow}`);
        wsExp.getRow(currentRow).height = 45;

        currentRow += 2;

        // Headers
        const expHeaders = [
            'رقم الشهادة', 'رقم الدفعة', 'اسم الطرف', 'المنتج/الخدمة', 'التاريخ',
            'النوع', 'الوصف', 'وصف البند', 'الكمية', 'السعر', 'الإجمالي',
            'الخصم', 'الاستقطاعات', 'التأمين', 'صافي المعتمد'
        ];

        const headerRow = wsExp.addRow(expHeaders.map(h => utils.rtlEmbed(h)));
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFDBEAFE' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

        // Data + Total calculation
        let totalNet = 0;

        data.expenses.forEach((exp, idx) => {
            const net = exp.netCertifiedAmount ??
                ((exp.grossAmount || 0) - (exp.discountAmount || 0) -
                    (exp.deductionsAmount || 0) - (exp.insuranceAmount || 0));

            const row = wsExp.addRow([
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
                net
            ]);

            // Format monetary columns (I to O) - adjusted index from 10-16 to 9-15
            for (let i = 9; i <= 15; i++) {
                const cell = row.getCell(i);
                cell.numFmt = '#,##0.00';
                cell.alignment = { horizontal: 'right' };
            }

            if (idx % 2 === 1) {
                row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFF1F5F9' } };
            }

            totalNet += net;
        });

        // Total Row for Expenses
        const totalRow = wsExp.addRow(['', '', '', '', '', '', '', 'الإجمالي الكلي', '', '', '', '', '', '', totalNet]);
        totalRow.font = { name: 'Amiri', size: 12, bold: true };
        totalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFBFDBFE' } };
        totalRow.getCell(15).numFmt = '#,##0.00';
        totalRow.getCell(15).alignment = { horizontal: 'right' };

        let grandTotalExpenses = totalNet;

        // Column widths
        const expColWidths = [18, 16, 32, 28, 14, 25, 38, 45, 12, 16, 18, 16, 16, 16, 20];
        wsExp.columns.forEach((col, i) => col.width = expColWidths[i] || 15);

        // ====================== DIRECT PAYMENTS SECTION ======================
        if (data.directPayments && data.directPayments.length > 0) {
            currentRow = wsExp.rowCount + 3;

            const directTitleCell = wsExp.getCell(`A${currentRow}`);
            directTitleCell.value = utils.rtlEmbed('مصاريف مباشرة أخرى (دفعات)');
            directTitleCell.font = { name: 'Amiri', size: 14, bold: true, color: { argb: 'FFFFFFFF' } };
            directTitleCell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF10B981' } };
            directTitleCell.alignment = { horizontal: 'center', vertical: 'middle' };
            wsExp.mergeCells(`A${currentRow}:O${currentRow}`);
            wsExp.getRow(currentRow).height = 30;

            currentRow++;

            const directHeaders = [
                'رقم الدفعة', 'النوع', 'من طرف', 'إلى طرف', 'الحالة', 'التاريخ',
                'المبلغ', 'رقم المرجع', 'طريقة الدفع', 'الشيك', 'تاريخ الشيك',
                'مركز التكلفة', 'ملاحظات', '', ''
            ];

            const directHeaderRow = wsExp.addRow(directHeaders.map(h => utils.rtlEmbed(h)));
            directHeaderRow.font = { name: 'Amiri', size: 11, bold: true };
            directHeaderRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFD1FAE5' } };
            directHeaderRow.alignment = { horizontal: 'center', vertical: 'middle' };

            let totalDirect = 0;
            data.directPayments.forEach((pyt: any, idx) => {
                const row = wsExp.addRow([
                    utils.safeString(pyt.paymentId),
                    utils.safeString(pyt.paymentTypeDescription),
                    utils.safeString(pyt.partyIdFromName),
                    utils.safeString(pyt.partyIdToName),
                    utils.safeString(pyt.dueStatusArabic || pyt.statusDescription),
                    utils.formatDate(pyt.effectiveDate),
                    pyt.amount || 0,
                    utils.safeString(pyt.paymentRefNum || ''),
                    utils.safeString(pyt.paymentMethodTypeDescription),
                    utils.safeString(pyt.chequeNumber),
                    utils.formatDate(pyt.chequeDate),
                    utils.safeString(pyt.costCenterDescription),
                    utils.safeString(pyt.comments),
                    '',
                    ''
                ]);

                row.getCell(7).numFmt = '#,##0.00';
                row.getCell(7).alignment = { horizontal: 'right' };
                row.getCell(8).alignment = { horizontal: 'right' };

                if (idx % 2 === 1) {
                    row.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFF0FDF4' } };
                }
                totalDirect += Number(pyt.amount) || 0;
            });

            const directTotalRow = wsExp.addRow(['', '', '', '', '', 'الإجمالي', totalDirect, '', '', '', '', '', '', '', '']);
            directTotalRow.font = { name: 'Amiri', size: 12, bold: true };
            directTotalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFA7F3D0' } };
            directTotalRow.getCell(7).numFmt = '#,##0.00';
            directTotalRow.getCell(7).alignment = { horizontal: 'right' };

            grandTotalExpenses += totalDirect;
        }

        // ====================== GRAND TOTAL FOR EXPENSES ======================
        wsExp.addRow([]); // Empty row for spacing
        const grandTotalRow = wsExp.addRow(['', '', '', '', '', 'إجمالي مصاريف المشروع', '', '', '', '', '', '', '', '', grandTotalExpenses]);
        grandTotalRow.font = { name: 'Amiri', size: 14, bold: true };
        grandTotalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF065F46' } }; // Dark green
        grandTotalRow.font.color = { argb: 'FFFFFFFF' };
        grandTotalRow.getCell(15).numFmt = '#,##0.00';
        grandTotalRow.getCell(15).alignment = { horizontal: 'right' };
        
        wsExp.mergeCells(`A${grandTotalRow.number}:N${grandTotalRow.number}`);
        grandTotalRow.getCell(1).alignment = { horizontal: 'center' };
        grandTotalRow.getCell(15).alignment = { horizontal: 'right' };
        grandTotalRow.height = 30;

        // ====================== REVENUES SHEET ======================
        const wsRev = workbook.addWorksheet('الإيرادات');
        wsRev.views = [{ rightToLeft: true }];

        let revRow = 1;
        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: 'jpeg' });
            wsRev.addImage(imageId, { tl: { col: 0, row: 0 }, ext: { width: 140, height: 90 } });
            wsRev.getRow(1).height = 70;
            revRow = 6;
        }

        const revTitleCell = wsRev.getCell(`A${revRow}`);
        revTitleCell.value = utils.rtlEmbed(`${projectName} - الثروة الخضراء - الإيرادات (${period})`);
        revTitleCell.font = { name: 'Amiri', size: 16, bold: true, color: { argb: 'FFFFFFFF' } };
        revTitleCell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF1E40AF' } };
        revTitleCell.alignment = { horizontal: 'center', vertical: 'middle' };
        wsRev.mergeCells(`A${revRow}:L${revRow}`);
        wsRev.getRow(revRow).height = 45;

        revRow += 2;

        const revHeaders = [
            'رقم الدفعة', 'المبنى', 'الوحدة', 'العميل', 'الفئة', 'المجدول',
            'المحصل', 'المتبقي', 'الحالة', 'شريحة التأخير', 'حالة الاستحقاق', 'تاريخ الاستحقاق'
        ];

        const revHeaderRow = wsRev.addRow(revHeaders.map(h => utils.rtlEmbed(h)));
        revHeaderRow.font = { name: 'Amiri', size: 11, bold: true };
        revHeaderRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFDBEAFE' } };

        let totalScheduled = 0, totalCollected = 0, totalOutstanding = 0;

        data.revenues.forEach(rev => {
            const row = wsRev.addRow([
                rev.paymentId,
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
                utils.formatDate(rev.dueDate)
            ]);

            [6, 7, 8].forEach(col => {
                const cell = row.getCell(col);
                cell.numFmt = '#,##0.00';
                cell.alignment = { horizontal: 'right' };
            });

            totalScheduled += rev.scheduledAmount || 0;
            totalCollected += rev.collectedAmount || 0;
            totalOutstanding += rev.outstandingAmount || 0;
        });

        // Total Row for Revenues
        const revTotalRow = wsRev.addRow(['', '', '', '', 'الإجمالي', totalScheduled, totalCollected, totalOutstanding, '', '', '', '']);
        revTotalRow.font = { name: 'Amiri', size: 12, bold: true };
        revTotalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFBFDBFE' } };
        for (let i = 6; i <= 8; i++) {
            revTotalRow.getCell(i).numFmt = '#,##0.00';
        }

        // Column widths for Revenue
        const revWidths = [16, 14, 14, 32, 20, 18, 18, 18, 16, 24, 25, 16];
        wsRev.columns.forEach((col, i) => col.width = revWidths[i] || 15);

        return await workbook.xlsx.writeBuffer();
    }, [projectName, allData, startDate, endDate]);

    const handleDownload = useCallback(async () => {
        setIsGenerating(true);
        try {
            const result = await trigger({
                projectId,
                startDate: allData ? undefined : startDate?.format('YYYY-MM-DD'),
                endDate: allData ? undefined : endDate?.format('YYYY-MM-DD'),
                allData
            }).unwrap();

            const buffer = await generateExcel(result);

            // Safe filename generation
            const safeProjectName = projectName.replace(/[^a-zA-Z0-9\u0600-\u06FF\s-]/g, '_').trim();
            const fileName = `Project_Report_${safeProjectName}_${allData ? 'All' : startDate?.format('YYYYMMDD')}.xlsx`;

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
    }, [trigger, generateExcel, projectId, projectName, startDate, endDate, allData, onClose]);

    const isLoading = isFetching || isGenerating;

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>تصدير تقرير المشروع - {projectName}</DialogTitle>
            <DialogContent>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3, mt: 2 }}>
                    <FormControlLabel
                        control={<Checkbox checked={allData} onChange={(e) => setAllData(e.target.checked)} />}
                        label="كل البيانات"
                    />
                    <LocalizationProvider dateAdapter={AdapterDayjs}>
                        <DesktopDatePicker
                            label="من تاريخ"
                            value={startDate}
                            onChange={setStartDate}
                            disabled={allData}
                            slotProps={{ textField: { fullWidth: true } }}
                        />
                        <DesktopDatePicker
                            label="إلى تاريخ"
                            value={endDate}
                            minDate={startDate ?? undefined}
                            onChange={setEndDate}
                            disabled={allData}
                            slotProps={{ textField: { fullWidth: true } }}
                        />
                    </LocalizationProvider>
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>إلغاء</Button>
                <Button
                    onClick={handleDownload}
                    variant="contained"
                    disabled={isLoading || (!allData && (!startDate || !endDate))}
                    startIcon={isLoading ? <CircularProgress size={20} /> : null}
                >
                    {isLoading ? 'جاري الإنشاء...' : 'تحميل التقرير'}
                </Button>
            </DialogActions>
        </Dialog>
    );
};