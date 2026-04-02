import React, { useCallback, useState } from "react";
import {
    Button,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Box,
    CircularProgress,
} from "@mui/material";
import ExcelJS from "exceljs";
import { saveAs } from "file-saver";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import { useAppDispatch } from "../../../app/store/configureStore";
import { multiPaymentCertificateApi } from "../../../app/store/apis/multiPaymentCertificateApi";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { DesktopDatePicker } from "@mui/x-date-pickers/DesktopDatePicker";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs, { Dayjs } from "dayjs";

const utils = {
    safeString: (v: any) => (v == null || typeof v === "object") ? "N/A" : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? "0.00" : v.toLocaleString("en-US", { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString("en-GB") : "N/A",
};

export default function MultiPaymentCertificatesDateRangeExcel() {
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();
    const [open, setOpen] = useState(false);
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf("month"));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [isGenerating, setIsGenerating] = useState(false);

    const handleOpen = () => setOpen(true);
    const handleClose = () => setOpen(false);

    const generateExcel = useCallback(async (data: any[]) => {
        if (!data || data.length === 0) return null;

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = "Golden Land System";

        let logoBuffer: ArrayBuffer | null = null;
        try {
            const resp = await fetch("/goldenlandlogo.jpg");
            if (resp.ok) logoBuffer = await resp.blob().then(b => b.arrayBuffer());
        } catch (e) {
            console.warn("Logo not found:", e);
        }

        const period = `${startDate?.format("YYYY-MM-DD") || "start"}_to_${endDate?.format("YYYY-MM-DD") || "end"}`;
        const safeSheetName = `Certificates ${period}`.replace(/[*\?\\:\[\]\/]/g, "_").slice(0, 31);
        const ws = workbook.addWorksheet(safeSheetName);
        ws.pageSetup = { paperSize: 9, orientation: "landscape" };
        ws.views = [{ rightToLeft: true }];

        ws.getRow(1).height = logoBuffer ? 75 : 30;

        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: "jpeg" });
            ws.addImage(imageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 120, height: 100 },
            });
        } else {
            ws.getCell("A1").value = "Golden Land";
            ws.getCell("A1").font = { name: "Amiri", size: 16, bold: true };
        }

        const startRow = logoBuffer ? 5 : 3;

        const title = utils.rtlEmbed(
            getTranslatedLabel(
                "projects.multiPaymentCertificate.excel.title",
                `Multi Payment Certificates (${startDate?.format("DD/MM/YYYY") || ""} - ${endDate?.format("DD/MM/YYYY") || ""})`
            )
        );
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:I${startRow}`);
        ws.getRow(startRow).font = { name: "Amiri", size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: "center", vertical: "middle" };
        ws.getRow(startRow).height = 40;

        const headerRowNum = startRow + 2;
        const headers = [
            getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.workEffortId", "Certificate ID"),
            getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.date", "Date"),
            getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.amount", "Amount"),
            getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.description", "Description"),
            getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.accountName", "Account Name"),
            getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.acctgTransId", "Transaction ID"),
            getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.paymentTo", "Payment To"),
            getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.referenceNum", "Reference"),
            getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.statusDescription", "Status"),
        ];

        ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: "Amiri", size: 11, bold: true };
        headerRow.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFE0E0E0" } };
        headerRow.alignment = { horizontal: "center", vertical: "middle" };

        data.forEach((item: any) => {
            const row = ws.addRow([
                utils.rtlEmbed(utils.safeString(item.workEffortId)),
                utils.formatDate(item.date),
                utils.formatNumber(item.amount),
                utils.rtlEmbed(utils.safeString(item.description)),
                utils.rtlEmbed(utils.safeString(item.accountName)),
                utils.rtlEmbed(utils.safeString(item.acctgTransId)),
                utils.rtlEmbed(utils.safeString(item.partyName)),
                utils.rtlEmbed(utils.safeString(item.notes)),
                utils.rtlEmbed(utils.safeString(item.statusDescription)),
            ]);
            row.font = { name: "Amiri", size: 10 };
            row.alignment = { horizontal: "right", wrapText: true };
            row.height = 22;
        });

        const dataStartRow = headerRowNum + 1;
        if (data.length > 0) {
            const totalAmount = data.reduce((sum: number, item: any) => sum + (item.amount || 0), 0);
            const totalRowNum = dataStartRow + data.length;
            ws.addRow([
                "", 
                utils.rtlEmbed(getTranslatedLabel("common.total", "Total")),
                utils.formatNumber(totalAmount),
                "", "", "", "", "", ""
            ]);
            ws.getRow(totalRowNum).font = { name: "Amiri", size: 12, bold: true };
            ws.getCell(`C${totalRowNum}`).font = { bold: true };
        }

        ws.columns = [
            { width: 20 }, // ID
            { width: 15 }, // Date
            { width: 15 }, // Amount
            { width: 40 }, // Description
            { width: 25 }, // Account
            { width: 15 }, // Trans ID
            { width: 25 }, // Payment To
            { width: 15 }, // Reference
            { width: 15 }, // Status
        ];
        ws.getColumn(3).numFmt = "#,##0.00";

        return await workbook.xlsx.writeBuffer();
    }, [getTranslatedLabel, startDate, endDate]);

    const handleExport = async () => {
        if (!startDate || !endDate) {
            toast.error(getTranslatedLabel("common.dateRangeError", "Please select both dates."));
            return;
        }
        setIsGenerating(true);
        try {
            const result = await dispatch(
                multiPaymentCertificateApi.endpoints.fetchMultiPaymentCertificatesByDateRange.initiate({
                    startDate: startDate.toISOString(),
                    endDate: endDate.toISOString(),
                })
            ).unwrap();

            if (!result || result.length === 0) {
                toast.warn(getTranslatedLabel("projects.multiPaymentCertificate.excel.noData", "No data found for the selected range"));
                setIsGenerating(false);
                return;
            }

            const buffer = await generateExcel(result);
            if (buffer) {
                const blob = new Blob([buffer], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
                saveAs(blob, `MultiPaymentCertificates_${startDate.format("YYYYMMDD")}_to_${endDate.format("YYYYMMDD")}.xlsx`);
                toast.success(getTranslatedLabel("projects.multiPaymentCertificate.excel.success", "Excel report generated successfully"));
            }
            handleClose();
        } catch (error) {
            console.error("Export failed", error);
            toast.error(getTranslatedLabel("projects.multiPaymentCertificate.excel.error", "Failed to generate Excel report"));
        } finally {
            setIsGenerating(false);
        }
    };

    return (
        <>
            <Button
                variant="contained"
                color="secondary"
                onClick={handleOpen}
                disabled={isGenerating}
                style={{ margin: "5px" }}
            >
                {isGenerating ? getTranslatedLabel("common.exporting", "Exporting...") : getTranslatedLabel("common.exportToExcel", "Export by Date Range")}
            </Button>

            <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel("projects.multiPaymentCertificate.excel.dialogTitle", "Select Date Range for Multi Payment Certificates")}
                </DialogTitle>
                <DialogContent>
                    <LocalizationProvider dateAdapter={AdapterDayjs}>
                        <Box sx={{ display: "flex", flexDirection: "column", gap: 3, mt: 2 }}>
                            <DesktopDatePicker
                                label={getTranslatedLabel("common.fromDate", "From Date")}
                                value={startDate}
                                onChange={(newValue) => setStartDate(newValue)}
                                slotProps={{ textField: { fullWidth: true } }}
                            />
                            <DesktopDatePicker
                                label={getTranslatedLabel("common.toDate", "To Date")}
                                value={endDate}
                                minDate={startDate ?? undefined}
                                onChange={(newValue) => setEndDate(newValue)}
                                slotProps={{ textField: { fullWidth: true } }}
                            />
                        </Box>
                    </LocalizationProvider>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose}>{getTranslatedLabel("common.cancel", "Cancel")}</Button>
                    <Button
                        onClick={handleExport}
                        variant="contained"
                        disabled={isGenerating || !startDate || !endDate}
                        startIcon={isGenerating ? <CircularProgress size={20} /> : null}
                    >
                        {isGenerating ? getTranslatedLabel("common.generating", "Generating...") : getTranslatedLabel("common.download", "Download")}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
}
