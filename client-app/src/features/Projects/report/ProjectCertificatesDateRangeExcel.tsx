import React, { useCallback, useState } from "react";
import {
    Button,
    Checkbox,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Box,
    CircularProgress,
    FormControlLabel,
} from "@mui/material";
import ExcelJS from "exceljs";
import { saveAs } from "file-saver";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import { useAppDispatch } from "../../../app/store/configureStore";
import { projectsApi } from "../../../app/store/apis/projectsApi";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { DesktopDatePicker } from "@mui/x-date-pickers/DesktopDatePicker";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs, { Dayjs } from "dayjs";
import {
    CompositeFilterDescriptor,
    FilterDescriptor,
    State,
    toODataString,
} from "@progress/kendo-data-query";

const utils = {
    safeString: (v: any) => (v == null || typeof v === "object") ? "N/A" : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? "0.00" : v.toLocaleString("en-US", { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString("en-GB") : "N/A",
};

const FIELD_LABELS: Record<string, string> = {
    certificateNumber: "\u0631\u0642\u0645 \u0627\u0644\u0634\u0647\u0627\u062F\u0629",
    projectName: "\u0627\u0633\u0645 \u0627\u0644\u0645\u0634\u0631\u0648\u0639",
    certificateCategoryDescription: "\u0627\u0644\u0646\u0648\u0639",
    statusDescription: "\u0627\u0644\u062D\u0627\u0644\u0629",
    totalAmount: "\u0627\u0644\u0645\u0628\u0644\u063A \u0627\u0644\u0625\u062C\u0645\u0627\u0644\u064A",
    partyIdSupplier: "\u0631\u0642\u0645 \u0627\u0644\u0645\u0648\u0631\u062F",
    partyNameSupplier: "\u0627\u0633\u0645 \u0627\u0644\u0645\u0648\u0631\u062F",
    partyIdContractor: "\u0631\u0642\u0645 \u0627\u0644\u0645\u0642\u0627\u0648\u0644",
    partyNameContractor: "\u0627\u0633\u0645 \u0627\u0644\u0645\u0642\u0627\u0648\u0644",
    description: "\u0627\u0644\u0648\u0635\u0641",
    estimatedStartDate: "\u062A\u0627\u0631\u064A\u062E \u0627\u0644\u0628\u062F\u0627\u064A\u0629",
    estimatedCompletionDate: "\u062A\u0627\u0631\u064A\u062E \u0627\u0644\u0625\u062A\u0645\u0627\u0645",
    facilityName: "\u0627\u0633\u0645 \u0627\u0644\u0645\u0646\u0634\u0623\u0629",
};

const OPERATOR_LABELS: Record<string, string> = {
    eq: "=",
    neq: "\u2260",
    contains: "\u064A\u062D\u062A\u0648\u064A \u0639\u0644\u0649",
    doesnotcontain: "\u0644\u0627 \u064A\u062D\u062A\u0648\u064A \u0639\u0644\u0649",
    startswith: "\u064A\u0628\u062F\u0623 \u0628\u0640",
    endswith: "\u064A\u0646\u062A\u0647\u064A \u0628\u0640",
    gte: "\u2265",
    gt: ">",
    lte: "\u2264",
    lt: "<",
    isnull: "\u0641\u0627\u0631\u063A",
    isnotnull: "\u063A\u064A\u0631 \u0641\u0627\u0631\u063A",
    isempty: "\u0641\u0627\u0631\u063A",
    isnotempty: "\u063A\u064A\u0631 \u0641\u0627\u0631\u063A",
};

const NO_VALUE_OPS = new Set(["isnull", "isnotnull", "isempty", "isnotempty"]);

function describeFilter(f: CompositeFilterDescriptor | FilterDescriptor): string {
    if ("filters" in f) {
        const parts = (f as CompositeFilterDescriptor).filters
            .map(child => describeFilter(child as CompositeFilterDescriptor | FilterDescriptor))
            .filter(Boolean);
        if (parts.length === 0) return "";
        const sep = (f as CompositeFilterDescriptor).logic === "and" ? " \u0648 " : " \u0623\u0648 ";
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

interface Props {
    dataState?: State;
}

export default function ProjectCertificatesDateRangeExcel({ dataState }: Props) {
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();
    const [open, setOpen] = useState(false);
    const [startDate, setStartDate] = useState<Dayjs | null>(dayjs().startOf("month"));
    const [endDate, setEndDate] = useState<Dayjs | null>(dayjs());
    const [isGenerating, setIsGenerating] = useState(false);
    const [respectFilters, setRespectFilters] = useState(false);

    const handleOpen = () => setOpen(true);
    const handleClose = () => setOpen(false);

    const generateExcel = useCallback(async (data: any[], filterInfo: string) => {
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
                "projects.certificate.excel.title",
                `Project Certificates (${startDate?.format("DD/MM/YYYY") || ""} - ${endDate?.format("DD/MM/YYYY") || ""})`
            )
        );
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:K${startRow}`);
        ws.getRow(startRow).font = { name: "Amiri", size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: "center", vertical: "middle" };
        ws.getRow(startRow).height = 40;

        // Filter info row — addRow places it at startRow+1 (next after the getCell-defined title row),
        // which also corrects the pre-existing off-by-one where headerRowNum = startRow+2 was unreachable.
        const filterInfoRow = ws.addRow([utils.rtlEmbed(filterInfo)]);
        ws.mergeCells(`A${startRow + 1}:K${startRow + 1}`);
        filterInfoRow.font = { name: "Amiri", size: 9, italic: true, color: { argb: "FF555555" } };
        filterInfoRow.alignment = { horizontal: "right", vertical: "middle", wrapText: true };
        filterInfoRow.height = 18;

        const headerRowNum = startRow + 2;
        const headers = [
            getTranslatedLabel("projects.certificate.list.certificateNumber", "Certificate Number"),
            getTranslatedLabel("projects.certificate.list.projectName", "Project Name"),
            getTranslatedLabel("projects.certificate.list.certificateType", "Type"),
            getTranslatedLabel("projects.certificate.list.statusDescription", "Status"),
            getTranslatedLabel("projects.certificate.list.totalAmount", "Total Amount"),
            getTranslatedLabel("projects.certificate.list.supplierPartyName", "Supplier"),
            getTranslatedLabel("projects.certificate.list.contractorPartyName", "Contractor"),
            getTranslatedLabel("projects.certificate.list.startDate", "Start Date"),
            getTranslatedLabel("projects.certificate.list.completionDate", "Completion Date"),
            getTranslatedLabel("projects.certificate.list.facilityName", "Facility"),
            getTranslatedLabel("projects.certificate.list.description", "Description"),
        ];

        ws.addRow(headers.map(h => utils.rtlEmbed(h)));
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: "Amiri", size: 11, bold: true };
        headerRow.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFE0E0E0" } };
        headerRow.alignment = { horizontal: "center", vertical: "middle" };

        const dataStartRow = headerRowNum + 1;
        data.forEach((item: any) => {
            const row = ws.addRow([
                utils.rtlEmbed(utils.safeString(item.certificateNumber)),
                utils.rtlEmbed(utils.safeString(item.projectName)),
                utils.rtlEmbed(utils.safeString(item.certificateCategoryDescription)),
                utils.rtlEmbed(utils.safeString(item.statusDescription)),
                utils.formatNumber(item.totalAmount),
                utils.rtlEmbed(utils.safeString(item.partyNameSupplier)),
                utils.rtlEmbed(utils.safeString(item.partyNameContractor)),
                utils.formatDate(item.estimatedStartDate),
                utils.formatDate(item.estimatedCompletionDate),
                utils.rtlEmbed(utils.safeString(item.facilityName)),
                utils.rtlEmbed(utils.safeString(item.description)),
            ]);
            row.font = { name: "Amiri", size: 10 };
            row.alignment = { horizontal: "right", wrapText: true };
            row.height = 22;
        });

        if (data.length > 0) {
            const totalAmount = data.reduce((sum: number, item: any) => sum + (item.totalAmount || 0), 0);
            const totalRowNum = dataStartRow + data.length;
            ws.addRow([
                "", "", "", 
                utils.rtlEmbed(getTranslatedLabel("common.total", "Total")),
                utils.formatNumber(totalAmount),
                "", "", "", "", "", ""
            ]);
            ws.mergeCells(`A${totalRowNum}:C${totalRowNum}`);
            ws.getRow(totalRowNum).font = { name: "Amiri", size: 12, bold: true };
            ws.getCell(`E${totalRowNum}`).font = { bold: true };
        }

        ws.columns = [
            { width: 20 }, // Certificate Number
            { width: 30 }, // Project Name
            { width: 30 }, // Type
            { width: 15 }, // Status
            { width: 15 }, // Total Amount
            { width: 25 }, // Supplier
            { width: 25 }, // Contractor
            { width: 15 }, // Start Date
            { width: 15 }, // Completion Date
            { width: 20 }, // Facility
            { width: 40 }, // Description
        ];
        ws.getColumn(5).numFmt = "#,##0.00";

        return await workbook.xlsx.writeBuffer();
    }, [getTranslatedLabel, startDate, endDate]);

    const handleExport = async () => {
        if (!startDate || !endDate) {
            toast.error(getTranslatedLabel("common.dateRangeError", "Please select both dates."));
            return;
        }
        setIsGenerating(true);
        try {
            let result: any[];

            // Pass date range as dedicated URL params to avoid OData DateTime parsing edge cases.
            // Only the grid's non-date filters go into the OData $filter so ApplyTo stays reliable.
            const oDataQuery = toODataString({
                ...(respectFilters && dataState?.filter ? { filter: dataState.filter } : {}),
                sort: dataState?.sort,
            });
            result = await dispatch(
                projectsApi.endpoints.fetchProjectCertificatesForExport.initiate({
                    oDataQuery,
                    fromDate: startDate.format('YYYY-MM-DD'),
                    toDate: endDate.format('YYYY-MM-DD'),
                })
            ).unwrap();

            if (!result || result.length === 0) {
                toast.warn(getTranslatedLabel("projects.certificate.excel.noData", "No data found for the selected range"));
                setIsGenerating(false);
                return;
            }

            const filterParts: string[] = [
                `الفترة: ${startDate!.format("DD/MM/YYYY")} - ${endDate!.format("DD/MM/YYYY")}`,
            ];
            if (respectFilters && dataState?.filter) {
                const gridText = describeFilter(dataState.filter as CompositeFilterDescriptor);
                if (gridText) filterParts.push(`الفلاتر: ${gridText}`);
            }
            const filterInfo = filterParts.join("  |  ");

            const buffer = await generateExcel(result, filterInfo);
            if (buffer) {
                const blob = new Blob([buffer], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
                saveAs(blob, `ProjectCertificates_${startDate.format("YYYYMMDD")}_to_${endDate.format("YYYYMMDD")}.xlsx`);
                toast.success(getTranslatedLabel("projects.certificate.excel.success", "Excel report generated successfully"));
            }
            handleClose();
        } catch (error) {
            console.error("Export failed", error);
            toast.error(getTranslatedLabel("projects.certificate.excel.error", "Failed to generate Excel report"));
        } finally {
            setIsGenerating(false);
        }
    };

    return (
        <>
            <Button
                variant="contained"
                color="primary"
                onClick={handleOpen}
                disabled={isGenerating}
            >
                {isGenerating ? getTranslatedLabel("common.exporting", "Exporting...") : getTranslatedLabel("common.exportToExcel", "Export by Date Range")}
            </Button>

            <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel("projects.certificate.excel.dialogTitle", "Select Date Range for Project Certificates")}
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
                            {dataState && (
                                <FormControlLabel
                                    control={
                                        <Checkbox
                                            checked={respectFilters}
                                            onChange={(e) => setRespectFilters(e.target.checked)}
                                        />
                                    }
                                    label={getTranslatedLabel(
                                        "projects.certificate.excel.respectFilters",
                                        "مراعاة فلاتر قائمة الشهادات"
                                    )}
                                />
                            )}
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
