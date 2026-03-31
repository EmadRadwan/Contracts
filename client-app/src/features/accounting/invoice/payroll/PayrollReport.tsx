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
    Typography,
} from '@mui/material';
import { DesktopDatePicker } from '@mui/x-date-pickers/DesktopDatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { Dayjs } from 'dayjs';
import { useTranslationHelper } from '../../../../app/hooks/useTranslationHelper';
import { useLazyFetchPayrollDataQuery } from '../../../../app/store/apis/invoice/invoicesApi';
import { useAppSelector } from '../../../../app/store/configureStore';

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined, dec = 0) =>
        v == null ? 'N/A' : v.toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

interface PayrollReportProps {
    open: boolean;
    onClose: () => void;
}

const PayrollReport: React.FC<PayrollReportProps> = ({ open, onClose }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "Company";

    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf('month'));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [trigger, { isFetching }] = useLazyFetchPayrollDataQuery();
    const [isGenerating, setIsGenerating] = useState(false);

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
        const ws = workbook.addWorksheet(`Payroll ${period}`);
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
                'accounting.payroll.report.title',
                `Payroll Report (${startDate?.format('DD/MM/YYYY') || ''} - ${endDate?.format('DD/MM/YYYY') || ''})`
            )
        );
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:L${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(startRow).height = 40;

        const headerRowNum = startRow + 2;
        const headers = [
            getTranslatedLabel('accounting.payroll.run.employee-name', 'Employee Name'),
            getTranslatedLabel('accounting.payroll.run.base-salary', 'Base Salary'),
            getTranslatedLabel('accounting.payroll.run.salary-account', 'Salary Account'),
            getTranslatedLabel('accounting.payroll.run.payment-method', 'Payment Method'),
            getTranslatedLabel('accounting.payroll.run.absence-days', 'Absence Days'),
            getTranslatedLabel('accounting.payroll.run.absence-value', 'Absence Value'),
            getTranslatedLabel('accounting.payroll.run.overtime-days', 'Overtime Days'),
            getTranslatedLabel('accounting.payroll.run.overtime-value', 'Overtime Value'),
            getTranslatedLabel('accounting.payroll.run.advances', 'Advances'),
            getTranslatedLabel('accounting.payroll.run.net-salary', 'Net Salary'),
            getTranslatedLabel('accounting.invoices.display.form.invoice-date', 'Invoice Date'),
            getTranslatedLabel('accounting.invoices.list.invoiceId', 'Invoice Number'),
        ];

        ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

        data.forEach((emp: any) => {
            const row = ws.addRow([
                utils.rtlEmbed(utils.safeString(emp.employeeName)),
                emp.baseSalary,
                utils.rtlEmbed(utils.safeString(emp.salaryAccountNameArabic)),
                utils.rtlEmbed(
                    emp.preferredPayrollPaymentMethodId === "BANK_TRANSFER" 
                        ? getTranslatedLabel("accounting.payroll.run.bank-transfer", "Bank Transfer")
                        : emp.preferredPayrollPaymentMethodId === "CASH"
                        ? getTranslatedLabel("accounting.payroll.run.cash", "Cash")
                        : emp.preferredPayrollPaymentMethodId
                ),
                emp.absenceDays,
                emp.absenceValue,
                emp.overtimeDays,
                emp.overtimeValue,
                emp.totalAdvances,
                emp.netSalary,
                utils.formatDate(emp.invoiceDate),
                utils.safeString(emp.invoiceId),
            ]);
            row.font = { name: 'Amiri', size: 10 };
            row.alignment = { horizontal: 'right', wrapText: true };
        });

        const totalRowNum = headerRowNum + data.length + 1;
        if (data.length > 0) {
            const sum = (field: string) => data.reduce((s, item) => s + (item[field] || 0), 0);
            ws.addRow([
                utils.rtlEmbed(getTranslatedLabel('accounting.payroll.run.total', 'Total')),
                sum('baseSalary'),
                '', '',
                sum('absenceDays'),
                sum('absenceValue'),
                sum('overtimeDays'),
                sum('overtimeValue'),
                sum('totalAdvances'),
                sum('netSalary'),
                '', ''
            ]);
            ws.getRow(totalRowNum).font = { name: 'Amiri', size: 11, bold: true };
            ws.getRow(totalRowNum).fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFF5F5F5' } };
        }

        ws.columns = [
            { width: 30 }, // Name
            { width: 15 }, // Base Salary
            { width: 30 }, // Account
            { width: 20 }, // Payment Method
            { width: 12 }, // Absence Days
            { width: 15 }, // Absence Value
            { width: 12 }, // Overtime Days
            { width: 15 }, // Overtime Value
            { width: 15 }, // Advances
            { width: 15 }, // Net Salary
            { width: 15 }, // Date
            { width: 15 }, // Invoice ID
        ];

        return await workbook.xlsx.writeBuffer();
    }, [getTranslatedLabel, startDate, endDate]);

    const handleDownload = useCallback(async () => {
        if (!startDate || !endDate) return;
        setIsGenerating(true);
        try {
            const result = await trigger({
                fromDate: startDate.format('YYYY-MM-DD'),
                toDate: endDate.format('YYYY-MM-DD'),
                organizationPartyId: companyId
            }).unwrap();

            const buffer = await generateExcel(result);
            if (buffer) {
                const fileName = `Payroll_Report_${startDate.format('YYYYMMDD')}_to_${endDate.format('YYYYMMDD')}.xlsx`;
                const blob = new Blob([buffer], {
                    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                });
                saveAs(blob, fileName);
            }
        } catch (err) {
            console.error('Excel generation failed:', err);
            alert('Failed to generate report.');
        } finally {
            setIsGenerating(false);
        }
    }, [trigger, generateExcel, startDate, endDate, companyId]);

    const isLoading = isFetching || isGenerating;

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>
                {getTranslatedLabel("accounting.menu.payrollReport", "Payroll Report")}
            </DialogTitle>
            <DialogContent>
                <LocalizationProvider dateAdapter={AdapterDayjs}>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, mt: 2 }}>
                        <DesktopDatePicker
                            label={getTranslatedLabel('common.fromDate', 'From Date')}
                            value={startDate}
                            onChange={(newValue) => setStartDate(newValue)}
                        />
                        <DesktopDatePicker
                            label={getTranslatedLabel('common.toDate', 'To Date')}
                            value={endDate}
                            minDate={startDate ?? undefined}
                            onChange={(newValue) => setEndDate(newValue)}
                        />
                    </Box>
                </LocalizationProvider>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose} color="inherit">
                    {getTranslatedLabel('common.cancel', 'Cancel')}
                </Button>
                <Button
                    onClick={handleDownload}
                    variant="contained"
                    disabled={isLoading || !startDate || !endDate}
                    startIcon={isLoading ? <CircularProgress size={20} /> : null}
                >
                    {isLoading ? getTranslatedLabel('common.generating', 'Generating...') : getTranslatedLabel('accounting.payroll.report.generate', 'Generate Report')}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default PayrollReport;
