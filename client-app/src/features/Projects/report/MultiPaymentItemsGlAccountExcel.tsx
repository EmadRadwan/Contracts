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
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `‫${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? "0.00" : v.toLocaleString("en-US", { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString("en-GB") : "N/A",
};

export default function MultiPaymentItemsGlAccountExcel() {
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();
    const [open, setOpen] = useState(false);
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().subtract(1, 'month'));
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
        const safeSheetName = `GL Items ${period}`.replace(/[*\?\\:\[\]\/]/g, "_").slice(0, 31);
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

        const title = `‫بنود مستندات الصرف المتعددة حسب حساب دفتر الأستاذ (${startDate?.format("DD/MM/YYYY") || ""} - ${endDate?.format("DD/MM/YYYY") || ""})`;
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:N${startRow}`);
        ws.getRow(startRow).font = { name: "Amiri", size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: "center", vertical: "middle" };
        ws.getRow(startRow).height = 40;

        const headerRowNum = startRow + 2;
        const headers = [
            getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.workEffortId", "Certificate ID"),
            getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.date", "Certificate Date"),
            getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.description", "Certificate Description"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.glAccountName", "GL Account"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.description", "Item Description"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.amount", "Amount"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.itemType", "Item Type"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.estimatedStartDate", "Date"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.serviceName", "Service"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.product", "Product"),
            getTranslatedLabel("projects.certificate.form.supplier", "Supplier"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.project", "Project"),
            getTranslatedLabel("projects.certificate.form.subProject", "Sub Project"),
            getTranslatedLabel("accounting.payments.form.costCenter", "Cost Center"),
        ];

        ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: "Amiri", size: 11, bold: true };
        headerRow.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFE0E0E0" } };
        headerRow.alignment = { horizontal: "center", vertical: "middle" };

        // Group by glAccountName only
        const groupedByGl: Record<string, any[]> = {};
        data.forEach((item: any) => {
            const glAccount = item.glAccountName || "";
            if (!groupedByGl[glAccount]) groupedByGl[glAccount] = [];
            groupedByGl[glAccount].push(item);
        });

        let grandTotal = 0;

        Object.entries(groupedByGl).forEach(([glAccount, items]) => {
            let glTotal = 0;

            items.forEach((item: any) => {
                const row = ws.addRow([
                    utils.rtlEmbed(utils.safeString(item.parentWorkEffortId)),
                    utils.formatDate(item.certificateDate),
                    utils.rtlEmbed(utils.safeString(item.parentDescription)),
                    utils.rtlEmbed(utils.safeString(item.glAccountName)),
                    utils.rtlEmbed(utils.safeString(item.description)),
                    utils.formatNumber(item.amount),
                    utils.rtlEmbed(utils.safeString(item.itemTypeDescription)),
                    utils.formatDate(item.estimatedStartDate),
                    utils.rtlEmbed(utils.safeString(item.serviceName)),
                    utils.rtlEmbed(utils.safeString(item.productName)),
                    utils.rtlEmbed(utils.safeString(item.partyIdSupplierName)),
                    utils.rtlEmbed(utils.safeString(item.projectName)),
                    utils.rtlEmbed(utils.safeString(item.subProjectName)),
                    utils.rtlEmbed(utils.safeString(item.costCenterName)),
                ]);
                row.font = { name: "Amiri", size: 10 };
                row.alignment = { horizontal: "right", wrapText: true };
                row.height = 22;
                glTotal += item.amount || 0;
            });

            // GL Account subtotal row
            const glSubtotalRow = ws.addRow([
                "", "", "",
                utils.rtlEmbed(`${getTranslatedLabel("common.subtotal", "Subtotal")}: ${utils.safeString(glAccount)}`),
                "",
                utils.formatNumber(glTotal),
                "", "", "", "", "", "", "", "",
            ]);
            glSubtotalRow.font = { name: "Amiri", size: 11, bold: true };
            glSubtotalRow.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFBDD7EE" } };
            glSubtotalRow.alignment = { horizontal: "right" };
            glSubtotalRow.height = 24;

            grandTotal += glTotal;
        });

        // Grand total row
        const grandTotalRow = ws.addRow([
            "", "", "", "",
            utils.rtlEmbed(getTranslatedLabel("common.total", "Total")),
            utils.formatNumber(grandTotal),
            "", "", "", "", "", "", "", "",
        ]);
        grandTotalRow.font = { name: "Amiri", size: 12, bold: true };
        grandTotalRow.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FF9DC3E6" } };
        grandTotalRow.alignment = { horizontal: "right" };
        grandTotalRow.height = 26;

        ws.columns = [
            { width: 15 },
            { width: 15 },
            { width: 30 },
            { width: 25 },
            { width: 35 },
            { width: 15 },
            { width: 15 },
            { width: 15 },
            { width: 25 },
            { width: 25 },
            { width: 25 },
            { width: 25 },
            { width: 20 },
            { width: 20 },
        ];
        ws.getColumn(6).numFmt = "#,##0.00";

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
                multiPaymentCertificateApi.endpoints.fetchMultiPaymentItemsByDateRange.initiate({
                    startDate: startDate.startOf('day').toISOString(),
                    endDate: endDate.endOf('day').toISOString(),
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
                saveAs(blob, `MultiPaymentItems_ByGLAccount_${startDate.format("YYYYMMDD")}_to_${endDate.format("YYYYMMDD")}.xlsx`);
                toast.success(getTranslatedLabel("projects.multiPaymentCertificate.excel.itemsGlAccountSuccess", "GL Account items report generated successfully"));
            }
            handleClose();
        } catch (error) {
            console.error("Export failed", error);
            toast.error(getTranslatedLabel("projects.multiPaymentCertificate.excel.itemsGlAccountError", "Failed to generate GL Account items report"));
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
                {isGenerating
                    ? getTranslatedLabel("common.exporting", "Exporting...")
                    : getTranslatedLabel("common.exportItemsByGlAccount", "Export Items by GL Account")}
            </Button>

            <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel("projects.multiPaymentCertificate.excel.itemsGlAccountDialogTitle", "Select Date Range for Items by GL Account")}
                </DialogTitle>
                <DialogContent>
                    <LocalizationProvider dateAdapter={AdapterDayjs}>
                        <Box sx={{ display: "flex", flexDirection: "column", gap: 3, mt: 2 }}>
                            <DesktopDatePicker
                                label={getTranslatedLabel("common.fromDate", "From Date")}
                                value={startDate}
                                format="DD/MM/YYYY"
                                onChange={(newValue) => setStartDate(newValue)}
                                slotProps={{ textField: { fullWidth: true } }}
                            />
                            <DesktopDatePicker
                                label={getTranslatedLabel("common.toDate", "To Date")}
                                value={endDate}
                                format="DD/MM/YYYY"
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
