import React, { useCallback, useState } from "react";
import {
    Autocomplete,
    Button,
    Checkbox,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Box,
    FormControlLabel,
    TextField,
} from "@mui/material";
import ExcelJS from "exceljs";
import { saveAs } from "file-saver";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import { useAppDispatch, useAppSelector } from "../../../app/store/configureStore";
import { multiPaymentCertificateApi } from "../../../app/store/apis/multiPaymentCertificateApi";
import { useFetchGlAccountOrgCashOrEquivalentLovQuery } from "../../../app/store/apis";
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

// Columns shared between both layouts
const COL_COUNT_SUMMARY = 4;   // cert-only: ID | Date | Description | Total
const COL_COUNT_FULL    = 10;  // with items: ID | Date | Cert Desc | Item GL | Item Desc | Amount | Type | Supplier | Project | Cost Center

export default function MultiPaymentItemsByGlAccountFilterExcel() {
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();
    const { user } = useAppSelector((state) => state.account);
    const companyId = (user as any)?.organizationPartyId || "";

    const [open, setOpen] = useState(false);
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().subtract(1, 'month'));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [selectedGlAccount, setSelectedGlAccount] = useState<any | null>(null);
    const [includeItems, setIncludeItems] = useState(true);
    const [isGenerating, setIsGenerating] = useState(false);

    const { data: glAccounts, isLoading: isLoadingGlAccounts } = useFetchGlAccountOrgCashOrEquivalentLovQuery(
        companyId,
        { skip: !companyId || !open }
    );

    const handleOpen = () => setOpen(true);
    const handleClose = () => {
        setOpen(false);
        setSelectedGlAccount(null);
    };

    const generateExcel = useCallback(async (
        certs: any[],
        items: any[] | null,
        glAccountName: string,
    ) => {
        if (!certs || certs.length === 0) return null;

        const withItems = items !== null;
        const colCount = withItems ? COL_COUNT_FULL : COL_COUNT_SUMMARY;
        const lastCol = String.fromCharCode(64 + colCount); // A=1 → 'A', 10 → 'J'

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
        const safeSheetName = `GL ${glAccountName} ${period}`.replace(/[*\?\\:\[\]\/]/g, "_").slice(0, 31);
        const ws = workbook.addWorksheet(safeSheetName);
        ws.pageSetup = { paperSize: 9, orientation: "landscape" };
        ws.views = [{ rightToLeft: true }];

        ws.getRow(1).height = logoBuffer ? 75 : 30;
        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: "jpeg" });
            ws.addImage(imageId, { tl: { col: 0, row: 0 }, ext: { width: 120, height: 100 } });
        } else {
            ws.getCell("A1").value = "Golden Land";
            ws.getCell("A1").font = { name: "Amiri", size: 16, bold: true };
        }

        const startRow = logoBuffer ? 5 : 3;

        // Report title
        const title = `‫مستندات الصرف - حساب: ${glAccountName} (${startDate?.format("DD/MM/YYYY") || ""} - ${endDate?.format("DD/MM/YYYY") || ""})`;
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:${lastCol}${startRow}`);
        ws.getRow(startRow).font = { name: "Amiri", size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: "center", vertical: "middle" };
        ws.getRow(startRow).height = 40;

        // Column headers
        const headerRowNum = startRow + 2;

        if (!withItems) {
            // ── Summary layout ──────────────────────────────────────────────────
            const headers = [
                getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.workEffortId", "رقم المستند"),
                getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.date", "تاريخ المستند"),
                getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.description", "وصف المستند"),
                getTranslatedLabel("projects.multiPaymentCertificate.items.amount", "الإجمالي"),
            ];
            ws.addRow(headers.map(h => utils.rtlEmbed(h)));
            const hr = ws.getRow(headerRowNum);
            hr.font = { name: "Amiri", size: 11, bold: true };
            hr.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFE0E0E0" } };
            hr.alignment = { horizontal: "center", vertical: "middle" };
            hr.height = 22;

            let grandTotal = 0;
            certs.forEach((cert: any) => {
                const row = ws.addRow([
                    utils.rtlEmbed(utils.safeString(cert.workEffortId)),
                    utils.formatDate(cert.date),
                    utils.rtlEmbed(utils.safeString(cert.description)),
                    utils.formatNumber(cert.amount),
                ]);
                row.font = { name: "Amiri", size: 10 };
                row.alignment = { horizontal: "right", wrapText: true };
                row.height = 22;
                grandTotal += cert.amount || 0;
            });

            const gtr = ws.addRow(["", "", `‫الإجمالي`, utils.formatNumber(grandTotal)]);
            gtr.font = { name: "Amiri", size: 12, bold: true };
            gtr.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FF9DC3E6" } };
            gtr.alignment = { horizontal: "right" };
            gtr.height = 26;

            ws.columns = [
                { width: 20 }, { width: 18 }, { width: 60 }, { width: 18 },
            ];
            ws.getColumn(4).numFmt = "#,##0.00";

        } else {
            // ── Full layout: certificate header row + item rows per cert ────────
            const headers = [
                getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.workEffortId", "رقم المستند"),
                getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.date", "تاريخ المستند"),
                getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.description", "وصف المستند"),
                getTranslatedLabel("projects.multiPaymentCertificate.items.glAccountName", "حساب دفتر الأستاذ"),
                getTranslatedLabel("projects.multiPaymentCertificate.items.description", "وصف البند"),
                getTranslatedLabel("projects.multiPaymentCertificate.items.amount", "المبلغ"),
                getTranslatedLabel("projects.multiPaymentCertificate.items.itemType", "نوع البند"),
                getTranslatedLabel("projects.certificate.form.supplier", "المورد"),
                getTranslatedLabel("projects.multiPaymentCertificate.items.project", "المشروع"),
                getTranslatedLabel("accounting.payments.form.costCenter", "مركز التكلفة"),
            ];
            ws.addRow(headers.map(h => utils.rtlEmbed(h)));
            const hr = ws.getRow(headerRowNum);
            hr.font = { name: "Amiri", size: 11, bold: true };
            hr.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFE0E0E0" } };
            hr.alignment = { horizontal: "center", vertical: "middle" };
            hr.height = 22;

            // Build a map: certId → items[]
            const itemsByCert = new Map<string, any[]>();
            (items || []).forEach((item: any) => {
                const id = utils.safeString(item.parentWorkEffortId);
                if (!itemsByCert.has(id)) itemsByCert.set(id, []);
                itemsByCert.get(id)!.push(item);
            });

            let grandTotal = 0;

            certs.forEach((cert: any) => {
                const certId = utils.safeString(cert.workEffortId);
                const certItems = itemsByCert.get(certId) || [];
                const certTotal = cert.amount || 0;

                // Certificate header row (orange background, spans all columns)
                const certRow = ws.addRow([
                    utils.rtlEmbed(certId),
                    utils.formatDate(cert.date),
                    utils.rtlEmbed(utils.safeString(cert.description)),
                    "", "", utils.formatNumber(certTotal),
                    "", "", "", "",
                ]);
                certRow.font = { name: "Amiri", size: 11, bold: true };
                certRow.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFFCE4D6" } };
                certRow.alignment = { horizontal: "right", vertical: "middle" };
                certRow.height = 24;

                // Item rows
                certItems.forEach((item: any) => {
                    const itemRow = ws.addRow([
                        "",
                        utils.formatDate(item.estimatedStartDate),
                        "",
                        utils.rtlEmbed(utils.safeString(item.glAccountName)),
                        utils.rtlEmbed(utils.safeString(item.description)),
                        utils.formatNumber(item.amount),
                        utils.rtlEmbed(utils.safeString(item.itemTypeDescription)),
                        utils.rtlEmbed(utils.safeString(item.partyIdSupplierName)),
                        utils.rtlEmbed(utils.safeString(item.projectName)),
                        utils.rtlEmbed(utils.safeString(item.costCenterName)),
                    ]);
                    itemRow.font = { name: "Amiri", size: 10 };
                    itemRow.alignment = { horizontal: "right", wrapText: true };
                    itemRow.height = 22;
                });

                grandTotal += certTotal;
            });

            const gtr = ws.addRow([
                "", "", "", "", `‫الإجمالي`,
                utils.formatNumber(grandTotal),
                "", "", "", "",
            ]);
            gtr.font = { name: "Amiri", size: 12, bold: true };
            gtr.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FF9DC3E6" } };
            gtr.alignment = { horizontal: "right" };
            gtr.height = 26;

            ws.columns = [
                { width: 18 }, { width: 15 }, { width: 40 }, { width: 25 },
                { width: 35 }, { width: 15 }, { width: 15 }, { width: 25 },
                { width: 25 }, { width: 20 },
            ];
            ws.getColumn(6).numFmt = "#,##0.00";
        }

        return await workbook.xlsx.writeBuffer();
    }, [getTranslatedLabel, startDate, endDate]);

    const handleExport = async () => {
        if (!startDate || !endDate) {
            toast.error(getTranslatedLabel("common.dateRangeError", "يرجى اختيار التواريخ"));
            return;
        }
        if (!selectedGlAccount) {
            toast.error(getTranslatedLabel("projects.multiPaymentCertificate.excel.glAccountRequired", "يرجى اختيار حساب دفتر الأستاذ."));
            return;
        }
        setIsGenerating(true);
        try {
            const params = {
                startDate: startDate.startOf('day').toISOString(),
                endDate: endDate.endOf('day').toISOString(),
                glAccountId: selectedGlAccount.glAccountId,
            };

            // Always fetch certificate headers; additionally fetch items when checkbox is checked
            const [certs, items] = await Promise.all([
                dispatch(multiPaymentCertificateApi.endpoints.fetchMultiPaymentCertificatesByDateRange.initiate(params)).unwrap(),
                includeItems
                    ? dispatch(multiPaymentCertificateApi.endpoints.fetchMultiPaymentItemsByDateRange.initiate(params)).unwrap()
                    : Promise.resolve(null),
            ]);

            if (!certs || certs.length === 0) {
                toast.warn(getTranslatedLabel("projects.multiPaymentCertificate.excel.glAccountNoData", "لا توجد بنود لهذا الحساب في الفترة المحددة."));
                setIsGenerating(false);
                return;
            }

            const buffer = await generateExcel(certs, items, selectedGlAccount.accountName || selectedGlAccount.glAccountId);
            if (buffer) {
                const blob = new Blob([buffer], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
                const safeName = (selectedGlAccount.glAccountId || "gl").replace(/[/\\?%*:|"<>]/g, "_");
                saveAs(blob, `MultiPaymentItems_GL_${safeName}_${startDate.format("YYYYMMDD")}_${endDate.format("YYYYMMDD")}.xlsx`);
                toast.success(getTranslatedLabel("projects.multiPaymentCertificate.excel.glAccountFilterSuccess", "تم إنشاء تقرير حساب دفتر الأستاذ بنجاح"));
            }
            handleClose();
        } catch (error) {
            console.error("Export failed", error);
            toast.error(getTranslatedLabel("projects.multiPaymentCertificate.excel.glAccountFilterError", "فشل إنشاء تقرير حساب دفتر الأستاذ"));
        } finally {
            setIsGenerating(false);
        }
    };

    return (
        <>
            <Button
                variant="outlined"
                color="secondary"
                onClick={handleOpen}
                disabled={isGenerating}
                style={{ margin: "5px" }}
            >
                {isGenerating
                    ? getTranslatedLabel("common.exporting", "جاري التصدير...")
                    : getTranslatedLabel("common.exportItemsByGlAccountFilter", "تصدير بنود حساب")}
            </Button>

            <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel("projects.multiPaymentCertificate.excel.glAccountFilterDialogTitle", "تصدير بنود حساب دفتر الأستاذ")}
                </DialogTitle>
                <DialogContent>
                    <LocalizationProvider dateAdapter={AdapterDayjs}>
                        <Box sx={{ display: "flex", flexDirection: "column", gap: 3, mt: 2 }}>
                            <DesktopDatePicker
                                label={getTranslatedLabel("common.fromDate", "من تاريخ")}
                                value={startDate}
                                format="DD/MM/YYYY"
                                onChange={(newValue) => setStartDate(newValue)}
                                slotProps={{ textField: { fullWidth: true } }}
                            />
                            <DesktopDatePicker
                                label={getTranslatedLabel("common.toDate", "إلى تاريخ")}
                                value={endDate}
                                format="DD/MM/YYYY"
                                minDate={startDate ?? undefined}
                                onChange={(newValue) => setEndDate(newValue)}
                                slotProps={{ textField: { fullWidth: true } }}
                            />
                            <Autocomplete
                                options={glAccounts || []}
                                loading={isLoadingGlAccounts}
                                getOptionLabel={(option) => option.accountName || option.glAccountId || ""}
                                value={selectedGlAccount}
                                onChange={(_, newValue) => setSelectedGlAccount(newValue)}
                                isOptionEqualToValue={(option, value) => option.glAccountId === value?.glAccountId}
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        label={getTranslatedLabel("projects.multiPaymentCertificate.items.glAccountName", "حساب دفتر الأستاذ")}
                                        fullWidth
                                        InputProps={{
                                            ...params.InputProps,
                                            endAdornment: (
                                                <>
                                                    {isLoadingGlAccounts ? <CircularProgress size={18} /> : null}
                                                    {params.InputProps.endAdornment}
                                                </>
                                            ),
                                        }}
                                    />
                                )}
                            />
                            <FormControlLabel
                                control={
                                    <Checkbox
                                        checked={includeItems}
                                        onChange={(e) => setIncludeItems(e.target.checked)}
                                    />
                                }
                                label={getTranslatedLabel("projects.multiPaymentCertificate.excel.includeItems", "تضمين بنود المستندات")}
                            />
                        </Box>
                    </LocalizationProvider>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose}>
                        {getTranslatedLabel("common.cancel", "إلغاء")}
                    </Button>
                    <Button
                        onClick={handleExport}
                        variant="contained"
                        disabled={isGenerating || !startDate || !endDate || !selectedGlAccount}
                        startIcon={isGenerating ? <CircularProgress size={20} /> : null}
                    >
                        {isGenerating
                            ? getTranslatedLabel("common.generating", "جاري التحميل...")
                            : getTranslatedLabel("common.download", "تحميل")}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
}
