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
    FormControl,
    FormLabel,
    RadioGroup,
    FormControlLabel,
    Radio,
} from '@mui/material';
import { useLazyFetchDailyPaymentsLazyQuery } from "../../../../app/store/apis";
import { toast } from 'react-toastify';

interface PaymentsDailyExcelProps {
    companyName: string;
    paymentType: 'incoming' | 'outgoing';
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

type SubtotalBy = 'paymentTypeDescription' | 'accountNameArabic' | 'paymentMethodDescription' | 'partyIdToName' | 'partyIdFromName';

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? '' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `‫${t}` : t,
    formatNumber: (v: number | undefined, dec = 2) =>
        v == null ? '' : v.toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined) => d ? new Date(d).toLocaleDateString('en-GB') : '',
    formatBoolean: (v: any) => (v === 1 || v === true) ? '✓' : '✕',
};

// Amount is at column index 6 (0-based) → Excel column G
const AMOUNT_COL_INDEX = 7; // 1-based for ExcelJS

const COLUMN_DEFS = [
    { field: 'paymentId',                   headerKey: 'accounting.payments.list.paymentId',                   defaultHeader: 'Payment Number',      width: 15 },
    { field: 'paymentRefNum',               headerKey: 'accounting.payments.list.paymentRefNum',               defaultHeader: 'Reference Number',     width: 18 },
    { field: 'paymentTypeDescription',      headerKey: 'accounting.payments.list.paymentType',                 defaultHeader: 'Payment Type',         width: 22 },
    { field: 'effectiveDate',               headerKey: 'accounting.payments.list.date',                        defaultHeader: 'Payment Date',         width: 14, isDate: true },
    { field: 'statusDescription',           headerKey: 'accounting.payments.list.status',                      defaultHeader: 'Status',               width: 14 },
    { field: 'dueStatusArabic',             headerKey: 'accounting.payments.list.dueStatus',                   defaultHeader: 'Due Status',           width: 25 },
    { field: 'amount',                      headerKey: 'accounting.payments.list.amount',                      defaultHeader: 'Amount',               width: 16, isNumeric: true },
    { field: 'paymentMethodTypeDescription',headerKey: 'accounting.payments.list.paymentMethodTypeDescription',defaultHeader: 'Payment Method Type',  width: 22 },
    { field: 'isBankTransfer',              headerKey: 'accounting.payments.list.isBankTransfer',              defaultHeader: 'Bank Transfer',        width: 14, isBoolean: true },
    { field: 'chequeNumber',                headerKey: 'accounting.payments.list.chequeNumber',                defaultHeader: 'Cheque Number',        width: 16 },
    { field: 'orderId',                     headerKey: 'accounting.payments.list.orderId',                     defaultHeader: 'Order ID',             width: 14 },
    { field: 'certificateNumber',           headerKey: 'accounting.payments.list.certificateNumber',           defaultHeader: 'Certificate No',       width: 18 },
    { field: 'salesRequestId',              headerKey: 'accounting.payments.list.salesRequestId',              defaultHeader: 'Sales Request ID',     width: 16 },
    { field: 'productId',                   headerKey: 'accounting.payments.list.productId',                   defaultHeader: 'Product ID',           width: 14 },
    { field: 'buildingNumber',              headerKey: 'accounting.payments.list.buildingNumber',              defaultHeader: 'Building Number',      width: 16 },
    { field: 'projectName',                 headerKey: 'accounting.payments.list.projectName',                 defaultHeader: 'Project',              width: 28 },
    { field: 'costCenterDescription',       headerKey: 'accounting.payments.list.costCenterDescription',       defaultHeader: 'Cost Center',          width: 26 },
    { field: 'partyIdFromName',             headerKey: 'accounting.payments.list.from',                        defaultHeader: 'From Party',           width: 26 },
    { field: 'partyIdToName',               headerKey: 'accounting.payments.list.to',                         defaultHeader: 'To Party',             width: 26 },
    { field: 'comments',                    headerKey: 'accounting.payments.list.comments',                    defaultHeader: 'Comments',             width: 32 },
    { field: 'paymentMethodDescription',    headerKey: 'accounting.payments.list.paymentMethodDescription',    defaultHeader: 'Payment Method',       width: 18 },
    { field: 'accountNameArabic',           headerKey: 'accounting.payments.list.accountNameArabic',           defaultHeader: 'Override Account',     width: 20 },
    { field: 'approvedByPartyName',         headerKey: 'accounting.payments.list.approvedByPartyName',         defaultHeader: 'Approved By',          width: 18 },
    { field: 'createdByPartyName',          headerKey: 'accounting.payments.list.createdByPartyName',          defaultHeader: 'Created By',           width: 18 },
];

const LAST_COL_LETTER = 'X'; // column 24 = X

function buildCellValue(colDef: typeof COLUMN_DEFS[0], payment: any): any {
    if (colDef.isDate)    return utils.formatDate(payment[colDef.field]);
    if (colDef.isBoolean) return utils.formatBoolean(payment[colDef.field]);
    if (colDef.isNumeric) return payment[colDef.field] ?? 0;
    return utils.rtlEmbed(utils.safeString(payment[colDef.field]));
}

function groupByField(data: any[], field: SubtotalBy): Map<string, any[]> {
    const groups = new Map<string, any[]>();
    [...data].sort((a, b) => (a[field] || '').localeCompare(b[field] || '', 'ar')).forEach(p => {
        const key = utils.safeString(p[field]) || 'N/A';
        if (!groups.has(key)) groups.set(key, []);
        groups.get(key)!.push(p);
    });
    return groups;
}

export const PaymentsDailyExcel: React.FC<PaymentsDailyExcelProps> = ({
    paymentType,
    getTranslatedLabel,
}) => {
    const [open, setOpen] = useState(false);
    const [subtotalBy, setSubtotalBy] = useState<SubtotalBy>('paymentTypeDescription');
    const [trigger, { isFetching }] = useLazyFetchDailyPaymentsLazyQuery();
    const [isGenerating, setIsGenerating] = useState(false);

    const generateExcel = useCallback(async (
        data: { data: any[]; total: number },
        groupBy: SubtotalBy
    ) => {
        if (!data || data.data.length === 0) return null;

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

        const safeSheetName = `${paymentType} Payments - ${new Date().toISOString().split('T')[0]}`.replace(/[*\?\\:\[\]\/]/g, '_').slice(0, 31);
        const ws = workbook.addWorksheet(safeSheetName);
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.views = [{ rightToLeft: true }];
        ws.getRow(1).height = logoBuffer ? 75 : 30;

        if (logoBuffer) {
            const imageId = workbook.addImage({ buffer: logoBuffer, extension: 'jpeg' });
            ws.addImage(imageId, { tl: { col: 0, row: 0 }, ext: { width: 120, height: 100 } });
        } else {
            ws.getCell('A1').value = 'Golden Land';
            ws.getCell('A1').font = { name: 'Amiri', size: 16, bold: true };
        }

        const startRow = logoBuffer ? 5 : 3;

        // Title
        const title = utils.rtlEmbed(
            getTranslatedLabel('accounting.payments.report.daily.title',
                `${paymentType === 'incoming' ? 'Incoming' : 'Outgoing'} Payments - Today`)
        );
        ws.getCell(`A${startRow}`).value = title;
        ws.mergeCells(`A${startRow}:${LAST_COL_LETTER}${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 18, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.getRow(startRow).height = 40;

        // Header row
        const headerRowNum = startRow + 2;
        const headers = COLUMN_DEFS.map(c => utils.rtlEmbed(getTranslatedLabel(c.headerKey, c.defaultHeader)));
        ws.addRow(headers);
        const headerRow = ws.getRow(headerRowNum);
        headerRow.font = { name: 'Amiri', size: 11, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE0E0E0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };
        headerRow.height = 22;

        // Data with subtotals
        const subtotalLabel = getTranslatedLabel('accounting.payments.report.subtotal', 'Subtotal');
        const grandTotalLabel = getTranslatedLabel('accounting.payments.report.grandTotal', 'Grand Total');
        const groups = groupByField(data.data, groupBy);
        let grandTotal = 0;

        groups.forEach((rows, groupName) => {
            const groupTotal = rows.reduce((s, p) => s + (p.amount || 0), 0);
            grandTotal += groupTotal;

            // Group header row
            const groupHeaderRow = ws.addRow([utils.rtlEmbed(groupName)]);
            ws.mergeCells(`A${groupHeaderRow.number}:${LAST_COL_LETTER}${groupHeaderRow.number}`);
            groupHeaderRow.font = { name: 'Amiri', size: 11, bold: true, color: { argb: 'FF1A237E' } };
            groupHeaderRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE8EAF6' } };
            groupHeaderRow.alignment = { horizontal: 'right', vertical: 'middle' };
            groupHeaderRow.height = 20;

            // Data rows
            rows.forEach(payment => {
                const row = ws.addRow(COLUMN_DEFS.map(c => buildCellValue(c, payment)));
                row.font = { name: 'Amiri', size: 10 };
                row.alignment = { horizontal: 'right', wrapText: true };
                row.height = 22;
                // Format amount cell
                row.getCell(AMOUNT_COL_INDEX).numFmt = '#,##0.00';
            });

            // Subtotal row
            const subtotalRowData = COLUMN_DEFS.map((_c, i) => {
                if (i === AMOUNT_COL_INDEX - 2) return utils.rtlEmbed(`${subtotalLabel}: ${groupName}`);
                if (i === AMOUNT_COL_INDEX - 1) return groupTotal;
                return '';
            });
            const subtotalRow = ws.addRow(subtotalRowData);
            ws.mergeCells(`A${subtotalRow.number}:F${subtotalRow.number}`);
            subtotalRow.font = { name: 'Amiri', size: 11, bold: true, color: { argb: 'FF1B5E20' } };
            subtotalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFE8F5E9' } };
            subtotalRow.alignment = { horizontal: 'right', vertical: 'middle' };
            subtotalRow.getCell(AMOUNT_COL_INDEX).numFmt = '#,##0.00';
            subtotalRow.height = 20;

            // Empty row between groups
            ws.addRow([]);
        });

        // Grand total row
        const grandTotalRowData = COLUMN_DEFS.map((_c, i) => {
            if (i === AMOUNT_COL_INDEX - 2) return utils.rtlEmbed(grandTotalLabel);
            if (i === AMOUNT_COL_INDEX - 1) return grandTotal;
            return '';
        });
        const grandTotalRow = ws.addRow(grandTotalRowData);
        ws.mergeCells(`A${grandTotalRow.number}:F${grandTotalRow.number}`);
        grandTotalRow.font = { name: 'Amiri', size: 13, bold: true };
        grandTotalRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FFFFF3E0' } };
        grandTotalRow.alignment = { horizontal: 'right', vertical: 'middle' };
        grandTotalRow.getCell(AMOUNT_COL_INDEX).numFmt = '#,##0.00';
        grandTotalRow.height = 24;

        // Column widths
        ws.columns = COLUMN_DEFS.map(c => ({ width: c.width }));

        return await workbook.xlsx.writeBuffer();
    }, [paymentType, getTranslatedLabel]);

    const handleDownload = useCallback(async () => {
        setIsGenerating(true);
        try {
            // preferCacheValue: false ensures a fresh fetch every time
            const result = await trigger({ paymentType }, false).unwrap();

            if (!result || result.data.length === 0) {
                toast.info(getTranslatedLabel('accounting.payments.report.noData', 'لا توجد دفعات لليوم'));
                return;
            }

            const buffer = await generateExcel(result, subtotalBy);
            if (buffer) {
                const today = new Date().toISOString().split('T')[0];
                saveAs(
                    new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }),
                    `${paymentType}_Daily_Payments_${today}.xlsx`
                );
                setOpen(false);
            }
        } catch (err: any) {
            console.error('Excel generation failed:', err);
            const msg = err?.data?.error || err?.message || 'Failed to generate report. Please try again.';
            toast.error(msg);
        } finally {
            setIsGenerating(false);
        }
    }, [trigger, generateExcel, paymentType, subtotalBy, getTranslatedLabel]);

    const isLoading = isFetching || isGenerating;

    return (
        <>
            <Button variant="contained" color="secondary" onClick={() => setOpen(true)} sx={{ ml: 1 }}>
                {getTranslatedLabel('accounting.payments.report.daily.excel', "Export Today's Payments")}
            </Button>

            <Dialog open={open} onClose={() => setOpen(false)} maxWidth="xs" fullWidth>
                <DialogTitle>
                    {getTranslatedLabel('accounting.payments.report.daily.dialogTitle', 'Daily Payments Report Options')}
                </DialogTitle>
                <DialogContent>
                    <Box sx={{ mt: 1 }}>
                        <FormControl component="fieldset">
                            <FormLabel component="legend">
                                {getTranslatedLabel('accounting.payments.report.subtotalBy', 'Subtotal By')}
                            </FormLabel>
                            <RadioGroup
                                value={subtotalBy}
                                onChange={(e) => setSubtotalBy(e.target.value as SubtotalBy)}
                            >
                                <FormControlLabel
                                    value="paymentTypeDescription"
                                    control={<Radio />}
                                    label={getTranslatedLabel('accounting.payments.report.subtotalByPaymentType', 'Payment Type')}
                                />
                                <FormControlLabel
                                    value="accountNameArabic"
                                    control={<Radio />}
                                    label={getTranslatedLabel('accounting.payments.report.subtotalByAccount', 'Override Account (GL Account)')}
                                />
                                <FormControlLabel
                                    value="paymentMethodDescription"
                                    control={<Radio />}
                                    label={getTranslatedLabel('accounting.payments.report.subtotalByPaymentMethod', 'Payment Method')}
                                />
                                <FormControlLabel
                                    value={paymentType === 'outgoing' ? 'partyIdToName' : 'partyIdFromName'}
                                    control={<Radio />}
                                    label={getTranslatedLabel('accounting.payments.report.subtotalByCounterparty', 'Counterparty')}
                                />
                            </RadioGroup>
                        </FormControl>
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setOpen(false)} disabled={isLoading}>
                        {getTranslatedLabel('common.cancel', 'Cancel')}
                    </Button>
                    <Button
                        onClick={handleDownload}
                        variant="contained"
                        disabled={isLoading}
                        startIcon={isLoading ? <CircularProgress size={20} /> : null}
                    >
                        {isLoading
                            ? getTranslatedLabel('common.generating', 'Generating...')
                            : getTranslatedLabel('common.download', 'Download')}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
};
