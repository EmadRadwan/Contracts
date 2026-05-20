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
import { useLazyFetchAbsenceDataQuery } from '../../../../app/store/apis/invoice/invoicesApi';
import { useAppSelector } from '../../../../app/store/configureStore';

const utils = {
    rtl: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    num: (v: number | undefined) => v == null ? 0 : Math.round(v * 100) / 100,
};

interface AbsenceReportProps {
    open: boolean;
    onClose: () => void;
}

const AbsenceReport: React.FC<AbsenceReportProps> = ({ open, onClose }) => {
    const { getTranslatedLabel } = useTranslationHelper();
    const { user } = useAppSelector((state) => state.account);
    const companyId = user?.organizationPartyId || "Company";

    const [startDate, setStartDate] = useState(dayjs().startOf('month'));
    const [endDate, setEndDate] = useState(dayjs());
    const [trigger, { isFetching }] = useLazyFetchAbsenceDataQuery();
    const [isGenerating, setIsGenerating] = useState(false);

    const generateExcel = useCallback(async (data: any[]) => {
        const wb = new ExcelJS.Workbook();
        const ws = wb.addWorksheet(`كشف غياب ${startDate.format('MM-YYYY')}`);

        ws.views = [{ rightToLeft: true }];
        ws.pageSetup = { orientation: 'portrait', paperSize: 9 };

        // === Headers ===
        const headers = [
            "م", "الاسم", "المهنة", "أيام الغياب", "رقم البصمة", "الملاحظات"
        ];

        ws.addRow(headers.map(h => utils.rtl(h)));
        const headerRow = ws.getRow(1);
        headerRow.font = { bold: true, size: 11, color: { argb: 'FFFFFFFF' } };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF1E3A8A' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };

        // === Employee Rows ===
        data.forEach((emp) => {
            const row = ws.addRow([
                emp.serial,
                utils.rtl(emp.employeeName),
                utils.rtl(emp.jobTitle || ''),
                utils.num(emp.absenceDays),
                emp.fingerPrintAttendanceId || '',
                utils.rtl(emp.notes || '')
            ]);

            if (emp.fingerPrintAttendanceId) {
                // Highlight row if it has fingerprint ID
                row.eachCell((cell) => {
                    cell.fill = {
                        type: 'pattern',
                        pattern: 'solid',
                        fgColor: { argb: 'FFE0F2F1' } // Light teal
                    };
                });
            }
        });

        // Column Widths
        ws.columns = [
            { width: 10 }, { width: 40 }, { width: 30 }, { width: 15 }, { width: 20 }, { width: 30 }
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
            const fileName = `كشف_غياب_${startDate.format('YYYYMM')}.xlsx`;
            saveAs(new Blob([buffer]), fileName);
        } catch (err) {
            console.error(err);
            alert('Failed to generate report');
        } finally {
            setIsGenerating(false);
        }
    }, [trigger, generateExcel, startDate, endDate, companyId]);

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>كشف الغياب</DialogTitle>
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
                    {isGenerating ? 'جاري التحميل...' : 'تحميل كشف الغياب'}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default AbsenceReport;
