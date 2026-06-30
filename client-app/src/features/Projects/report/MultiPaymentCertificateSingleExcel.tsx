import React, { useCallback, useState } from "react";
import {
    Button,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Box,
    CircularProgress,
    TextField,
} from "@mui/material";
import ExcelJS from "exceljs";
import { saveAs } from "file-saver";
import { useTranslationHelper } from "../../../app/hooks/useTranslationHelper";
import { toast } from "react-toastify";
import { useAppDispatch } from "../../../app/store/configureStore";
import { multiPaymentCertificateApi } from "../../../app/store/apis/multiPaymentCertificateApi";

const utils = {
    safeString: (v: any) => (v == null || typeof v === "object") ? "N/A" : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `‫${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? "0.00" : v.toLocaleString("en-US", { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString("en-GB") : "N/A",
};

export default function MultiPaymentCertificateSingleExcel() {
    const { getTranslatedLabel } = useTranslationHelper();
    const dispatch = useAppDispatch();
    const [open, setOpen] = useState(false);
    const [certificateId, setCertificateId] = useState("");
    const [isGenerating, setIsGenerating] = useState(false);

    const handleOpen = () => setOpen(true);
    const handleClose = () => {
        setOpen(false);
        setCertificateId("");
    };

    const generateExcel = useCallback(async (header: any, items: any[]) => {
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

        const safeSheetName = `Cert ${header.workEffortId}`.replace(/[*\?\\:\[\]\/]/g, "_").slice(0, 31);
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

        // Title
        const title = `‫مستند صرف متعدد`;
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:J${startRow}`);
        ws.getRow(startRow).font = { name: "Amiri", size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: "center", vertical: "middle" };
        ws.getRow(startRow).height = 40;

        // Header info box
        const infoStartRow = startRow + 2;
        const headerFields = [
            [getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.workEffortId", "Certificate ID"), utils.safeString(header.workEffortId)],
            [getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.date", "Date"), utils.formatDate(header.date)],
            [getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.description", "Description"), utils.safeString(header.description)],
            [getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.accountName", "Account Name"), utils.safeString(header.accountName)],
            [getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.acctgTransId", "Transaction ID"), utils.safeString(header.acctgTransId)],
            [getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.paymentTo", "Payment To"), utils.safeString(header.partyName)],
            [getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.referenceNum", "Reference"), utils.safeString(header.notes)],
            [getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.statusDescription", "Status"), utils.safeString(header.statusDescription)],
            [getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.amount", "Total Amount"), utils.formatNumber(header.amount)],
        ];

        headerFields.forEach(([label, value], i) => {
            const rowNum = infoStartRow + i;
            ws.getCell(`A${rowNum}`).value = utils.rtlEmbed(label);
            ws.getCell(`A${rowNum}`).font = { name: "Amiri", size: 10, bold: true };
            ws.getCell(`A${rowNum}`).fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFE0E0E0" } };
            ws.getCell(`B${rowNum}`).value = utils.rtlEmbed(value);
            ws.getCell(`B${rowNum}`).font = { name: "Amiri", size: 10 };
            ws.getRow(rowNum).alignment = { horizontal: "right", vertical: "middle" };
            ws.getRow(rowNum).height = 20;
        });

        // Items section header
        const itemsSectionRow = infoStartRow + headerFields.length + 2;
        ws.getCell(`A${itemsSectionRow}`).value = utils.rtlEmbed(
            getTranslatedLabel("projects.multiPaymentCertificate.items.sectionTitle", "Certificate Items")
        );
        ws.mergeCells(`A${itemsSectionRow}:J${itemsSectionRow}`);
        ws.getRow(itemsSectionRow).font = { name: "Amiri", size: 14, bold: true };
        ws.getRow(itemsSectionRow).alignment = { horizontal: "center", vertical: "middle" };
        ws.getRow(itemsSectionRow).height = 30;

        const itemHeaderRowNum = itemsSectionRow + 1;
        const itemHeaders = [
            getTranslatedLabel("projects.multiPaymentCertificate.items.glAccountName", "GL Account"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.description", "Item Description"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.amount", "Amount"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.itemType", "Item Type"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.estimatedStartDate", "Date"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.serviceName", "Service"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.product", "Product"),
            getTranslatedLabel("projects.certificate.form.supplier", "Supplier"),
            getTranslatedLabel("projects.multiPaymentCertificate.items.project", "Project"),
            getTranslatedLabel("accounting.payments.form.costCenter", "Cost Center"),
        ];

        ws.addRow(itemHeaders.map(h => utils.rtlEmbed(h)));
        const itemHeaderRow = ws.getRow(itemHeaderRowNum);
        itemHeaderRow.font = { name: "Amiri", size: 11, bold: true };
        itemHeaderRow.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFE0E0E0" } };
        itemHeaderRow.alignment = { horizontal: "center", vertical: "middle" };
        itemHeaderRow.height = 22;

        // Group items by GL account
        const groupedByGl: Record<string, any[]> = {};
        items.forEach((item: any) => {
            const glAccount = item.glAccountName || "";
            if (!groupedByGl[glAccount]) groupedByGl[glAccount] = [];
            groupedByGl[glAccount].push(item);
        });

        let grandTotal = 0;

        Object.entries(groupedByGl).forEach(([glAccount, glItems]) => {
            let glTotal = 0;

            glItems.forEach((item: any) => {
                const row = ws.addRow([
                    utils.rtlEmbed(utils.safeString(item.glAccountName)),
                    utils.rtlEmbed(utils.safeString(item.description)),
                    utils.formatNumber(item.amount),
                    utils.rtlEmbed(utils.safeString(item.itemTypeDescription)),
                    utils.formatDate(item.estimatedStartDate),
                    utils.rtlEmbed(utils.safeString(item.serviceName)),
                    utils.rtlEmbed(utils.safeString(item.productName)),
                    utils.rtlEmbed(utils.safeString(item.partyIdSupplierName)),
                    utils.rtlEmbed(utils.safeString(item.projectId)),
                    utils.rtlEmbed(utils.safeString(item.costCenterName)),
                ]);
                row.font = { name: "Amiri", size: 10 };
                row.alignment = { horizontal: "right", wrapText: true };
                row.height = 22;
                glTotal += item.amount || 0;
            });

            // GL account subtotal
            const subtotalRow = ws.addRow([
                utils.rtlEmbed(`${getTranslatedLabel("common.subtotal", "Subtotal")}: ${utils.safeString(glAccount)}`),
                "",
                utils.formatNumber(glTotal),
                "", "", "", "", "", "", "",
            ]);
            subtotalRow.font = { name: "Amiri", size: 10, italic: true, bold: true };
            subtotalRow.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFBDD7EE" } };
            subtotalRow.alignment = { horizontal: "right" };
            subtotalRow.height = 22;

            grandTotal += glTotal;
        });

        // Grand total
        const grandTotalRow = ws.addRow([
            utils.rtlEmbed(getTranslatedLabel("common.total", "Total")),
            "",
            utils.formatNumber(grandTotal),
            "", "", "", "", "", "", "",
        ]);
        grandTotalRow.font = { name: "Amiri", size: 12, bold: true };
        grandTotalRow.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FF9DC3E6" } };
        grandTotalRow.alignment = { horizontal: "right" };
        grandTotalRow.height = 26;

        ws.columns = [
            { width: 25 }, // GL Account
            { width: 35 }, // Item Description
            { width: 15 }, // Amount
            { width: 15 }, // Item Type
            { width: 15 }, // Date
            { width: 25 }, // Service
            { width: 25 }, // Product
            { width: 25 }, // Supplier
            { width: 25 }, // Project
            { width: 20 }, // Cost Center
        ];
        ws.getColumn(3).numFmt = "#,##0.00";

        return await workbook.xlsx.writeBuffer();
    }, [getTranslatedLabel]);

    const handleExport = async () => {
        if (!certificateId.trim()) {
            toast.error(getTranslatedLabel("projects.multiPaymentCertificate.excel.singleIdRequired", "Please enter a Certificate ID."));
            return;
        }
        setIsGenerating(true);
        try {
            const [headerResult, itemsResult] = await Promise.all([
                dispatch(
                    multiPaymentCertificateApi.endpoints.fetchMultiPaymentCertificateById.initiate(certificateId.trim())
                ).unwrap(),
                dispatch(
                    multiPaymentCertificateApi.endpoints.getMultiPaymentItems.initiate(certificateId.trim())
                ).unwrap(),
            ]);

            if (!headerResult) {
                toast.warn(getTranslatedLabel("projects.multiPaymentCertificate.excel.singleNotFound", "Certificate not found."));
                setIsGenerating(false);
                return;
            }

            const buffer = await generateExcel(headerResult, itemsResult || []);
            if (buffer) {
                const blob = new Blob([buffer], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
                saveAs(blob, `MultiPaymentCertificate_${certificateId.trim()}.xlsx`);
                toast.success(getTranslatedLabel("projects.multiPaymentCertificate.excel.singleSuccess", "Certificate report generated successfully"));
            }
            handleClose();
        } catch (error) {
            console.error("Export failed", error);
            toast.error(getTranslatedLabel("projects.multiPaymentCertificate.excel.singleError", "Failed to generate certificate report"));
        } finally {
            setIsGenerating(false);
        }
    };

    return (
        <>
            <Button
                variant="contained"
                color="success"
                onClick={handleOpen}
                disabled={isGenerating}
                style={{ margin: "5px" }}
            >
                {isGenerating
                    ? getTranslatedLabel("common.exporting", "Exporting...")
                    : getTranslatedLabel("common.exportSingleCertificate", "Export Certificate")}
            </Button>

            <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel("projects.multiPaymentCertificate.excel.singleDialogTitle", "Export Certificate Report")}
                </DialogTitle>
                <DialogContent>
                    <Box sx={{ mt: 2 }}>
                        <TextField
                            label={getTranslatedLabel("projects.multiPaymentCertificate.HeaderList.workEffortId", "Certificate ID")}
                            value={certificateId}
                            onChange={(e) => setCertificateId(e.target.value)}
                            fullWidth
                            autoFocus
                            onKeyDown={(e) => { if (e.key === "Enter") handleExport(); }}
                        />
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose}>{getTranslatedLabel("common.cancel", "Cancel")}</Button>
                    <Button
                        onClick={handleExport}
                        variant="contained"
                        disabled={isGenerating || !certificateId.trim()}
                        startIcon={isGenerating ? <CircularProgress size={20} /> : null}
                    >
                        {isGenerating
                            ? getTranslatedLabel("common.generating", "Generating...")
                            : getTranslatedLabel("common.download", "Download")}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
}
