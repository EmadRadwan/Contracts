import React, { useCallback, useState } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import {
    Button, Dialog, DialogTitle, DialogContent, DialogActions,
    Box, CircularProgress
} from '@mui/material';
import { DesktopDatePicker } from '@mui/x-date-pickers/DesktopDatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs from 'dayjs';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import { useLazyFetchPayrollDataQuery } from '../../../../app/store/apis/invoice/invoicesApi';
import { useAppSelector } from '../../../../app/store/configureStore';

const utils = {
    rtl: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    num: (v: number | undefined) => v == null ? 0 : Math.round(v * 100) / 100,
    dailyRate: (base: number) => base > 0 ? Math.round(base / 30) : 0,
};

interface PayrollReport2Props {
    open: boolean;
    onClose: () => void;
}

const PayrollReport2: React.FC<PayrollReport2Props> = ({ open, onClose }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "Company";

    const [startDate, setStartDate] = useState(dayjs().startOf('month'));
    const [endDate, setEndDate] = useState(dayjs());
    const [trigger, { isFetching }] = useLazyFetchPayrollDataQuery();
    const [isGenerating, setIsGenerating] = useState(false);

    const generateExcel = useCallback(async (data: any[]) => {
        const wb = new ExcelJS.Workbook();
        const ws = wb.addWorksheet(`كشف رواتب ${startDate.format('MM-YYYY')}`);

        ws.views = [{ rightToLeft: true }];
        ws.pageSetup = { orientation: 'landscape', paperSize: 9 };

        // === Headers ===
        const headers = [
            "م", "الاسم", "المهنة", "الراتب الاساسي", "معدل الحضور %",
            "الاجر باليوم", "أيام الإضافي", "الاضافي", "الاجمالي",
            "السلف", "أيام الغياب", "الغياب", "الخصومات", "صافي الراتب المستحق", "طريقة الصرف", "الملاحظات"
        ];

        ws.addRow(headers.map(h => utils.rtl(h)));
        const headerRow = ws.getRow(1);
        headerRow.font = { bold: true, size: 11, color: { argb: 'FFFFFFFF' } };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF1E3A8A' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

        // === Employee Rows ===
        data.forEach((emp) => {
            ws.addRow([
                emp.serial,
                utils.rtl(emp.employeeName),
                utils.rtl(emp.jobTitle || ''),
                utils.num(emp.baseSalary),
                utils.num(emp.attendanceRate),
                utils.dailyRate(emp.baseSalary),           // Rounded daily rate
                utils.num(emp.overtimeDays),
                utils.num(emp.overtimeValue),
                utils.num(emp.grossTotal),
                utils.num(emp.advances),
                utils.num(emp.absenceDays),
                utils.num(emp.absenceValue),
                utils.num(emp.totalDeductions),
                utils.num(emp.netSalary),
                utils.rtl(emp.paymentMethod || ''),
                utils.rtl(emp.notes || '')
            ]);
        });

        // === Main Total Row ===
        const totalBase = data.reduce((s, r) => s + r.baseSalary, 0);
        const totalGross = data.reduce((s, r) => s + r.grossTotal, 0);
        const totalAdvances = data.reduce((s, r) => s + r.advances, 0);
        const totalAbsence = data.reduce((s, r) => s + r.absenceValue, 0);
        const totalDeductions = data.reduce((s, r) => s + r.totalDeductions, 0);
        const totalNet = data.reduce((s, r) => s + r.netSalary, 0);

        const totalRow = ws.addRow([
            "الإجمالي", "", "", totalBase, "", "",
            data.reduce((s, r) => s + r.overtimeDays, 0),
            data.reduce((s, r) => s + r.overtimeValue, 0),
            totalGross,
            totalAdvances,
            data.reduce((s, r) => s + r.absenceDays, 0),
            totalAbsence,
            totalDeductions,
            totalNet,
            "",
            ""
        ]);
        totalRow.font = { bold: true, size: 12 };
        totalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFF5F5F5' } };

        let currentRow = ws.rowCount + 2;

        // === Financial Summary Breakdown ===
        ws.getCell(`A${currentRow}`).value = utils.rtl("البيان");
        ws.getCell(`B${currentRow}`).value = utils.rtl("المبلغ");
        ws.getCell(`D${currentRow}`).value = utils.rtl("البيان");
        ws.getCell(`E${currentRow}`).value = utils.rtl("المبلغ");
        currentRow++;

        const summary = [
            ["اجمالي المرتبات", totalBase],
            ["سلفيات الموظفين", -totalAdvances],
            ["صافي الرواتب", totalNet],
            ["الإضافي", data.reduce((s, r) => s + r.overtimeValue, 0)],
            ["اجمالي المرتبات مع الإضافي", totalGross],
            ["الغياب", -totalAbsence],
            ["اجمالي الخصومات", -totalDeductions],
        ];

        summary.forEach(([label, amount]) => {
            ws.getCell(`A${currentRow}`).value = utils.rtl(label as string);
            ws.getCell(`B${currentRow}`).value = Math.round(amount as number);
            currentRow++;
        });

        // Bank vs Cash
        const bankTotal = data
            .filter(e => e.paymentMethod?.includes("تحويل") || e.paymentMethod?.includes("BANK"))
            .reduce((s, r) => s + r.netSalary, 0);

        const cashTotal = totalNet - bankTotal;

        currentRow += 1;
        ws.getCell(`A${currentRow}`).value = utils.rtl("تحويل بنكي");
        ws.getCell(`B${currentRow}`).value = Math.round(bankTotal);

        currentRow++;
        ws.getCell(`A${currentRow}`).value = utils.rtl("يصرف نقدا");
        ws.getCell(`B${currentRow}`).value = Math.round(cashTotal);

        // Date & Final Net
        currentRow += 2;
        ws.getCell(`A${currentRow}`).value = utils.rtl("تاريخ كشف الرواتب");
        ws.getCell(`B${currentRow}`).value = dayjs().format('DD/MM/YYYY');
        ws.getCell(`D${currentRow}`).value = utils.rtl("صافي الرواتب");
        ws.getCell(`E${currentRow}`).value = Math.round(totalNet);

        // Column Widths
        ws.columns = [
            { width: 30 }, { width: 32 }, { width: 24 }, { width: 16 }, { width: 14 },
            { width: 14 }, { width: 12 }, { width: 14 }, { width: 16 },
            { width: 14 }, { width: 12 }, { width: 14 }, { width: 16 }, { width: 18 }, { width: 18 }, { width: 28 }
        ];

        return await wb.xlsx.writeBuffer();
    }, [startDate]);

    const handleDownload = useCallback(async () => {
        setIsGenerating(true);
        try {
            const result = await trigger({
                fromDate: startDate.format('YYYY-MM-DD'),
                toDate: endDate.format('YYYY-MM-DD'),
                organizationPartyId: companyId
            }).unwrap();

            const buffer = await generateExcel(result);
            const fileName = `كشف_رواتب_${startDate.format('YYYYMM')}.xlsx`;
            saveAs(new Blob([buffer]), fileName);
        } catch (err) {
            console.error(err);
            alert('Failed to generate report');
        } finally {
            setIsGenerating(false);
        }
    }, [trigger, generateExcel, startDate, endDate, companyId]);

    return (
        <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
            <DialogTitle>كشف الرواتب </DialogTitle>
            <DialogContent>
                <LocalizationProvider dateAdapter={AdapterDayjs}>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
                        <DesktopDatePicker label="من تاريخ" value={startDate} onChange={setStartDate} />
                        <DesktopDatePicker label="إلى تاريخ" value={endDate} onChange={setEndDate} />
                    </Box>
                </LocalizationProvider>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose} color="inherit">إلغاء</Button>
                <Button
                    variant="contained"
                    onClick={handleDownload}
                    disabled={isGenerating}
                    startIcon={isGenerating ? <CircularProgress size={20} /> : null}
                >
                    {isGenerating ? 'جاري التحميل...' : 'تحميل كشف الرواتب'}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default PayrollReport2;