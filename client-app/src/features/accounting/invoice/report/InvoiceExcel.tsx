import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';
import { Invoice } from '../../../../app/models/accounting/invoice';
import { useFetchInvoiceItemsQuery } from '../../../../app/store/configureStore';

const sharedUtils = {
    safeString: (value: any): string => {
        if (value === null || value === undefined) return 'N/A';
        if (typeof value === 'object') {
            console.warn('safeString received object:', value);
            return 'N/A';
        }
        if (typeof value === 'number') return value.toString();
        return String(value);
    },
    rtlEmbed: (text: string): string => {
        return /\p{Script=Arabic}/u.test(text) ? `\u202B${text}` : text;
    },
};

interface InvoiceExcelProps {
    invoice: Invoice;
    total: number | null;
    outstandingAmount: number | null;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

export const InvoiceExcel: React.FC<InvoiceExcelProps> = ({ invoice, total, outstandingAmount, getTranslatedLabel }) => {
    const { data: invoiceItems } = useFetchInvoiceItemsQuery(invoice.invoiceId, { skip: !invoice.invoiceId });

    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';
        const worksheet = workbook.addWorksheet('Invoice Detail', {
            pageSetup: { paperSize: 9, orientation: 'landscape' },
            views: [{ rightToLeft: true }],
        });
        worksheet.getColumn(1).font = { name: 'Amiri', size: 10 };

        let logoImageId: number | null = null;
        try {
            const response = await fetch('/goldenlandlogo.jpg');
            if (!response.ok) throw new Error('Failed to fetch logo');
            const blob = await response.blob();
            const arrayBuffer = await blob.arrayBuffer();
            logoImageId = workbook.addImage({
                buffer: arrayBuffer,
                extension: 'jpeg',
            });
            worksheet.addImage(logoImageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 100, height: 100 },
                editAs: 'absolute',
            });
            worksheet.getRow(1).height = 75;
            worksheet.getRow(2).height = 20;
            worksheet.getRow(3).height = 20;
            worksheet.addRow([]);
            worksheet.addRow([]);
            worksheet.addRow([]);
        } catch (error) {
            console.warn('Logo fetch failed:', error);
            worksheet.addRow(['Logo Unavailable']);
            worksheet.getRow(1).font = { name: 'Amiri', size: 10, color: { argb: 'FF0000' } };
            worksheet.getRow(1).alignment = { horizontal: 'center', vertical: 'middle' };
        }

        // Header Section
        const titleRow = worksheet.addRow([
            getTranslatedLabel('accounting.invoices.report.title', 'Invoice Report') + ': ' + invoice.invoiceId,
        ]);
        worksheet.mergeCells(`A${logoImageId !== null ? 4 : 2}:E${logoImageId !== null ? 4 : 2}`);
        const titleCell = worksheet.getRow(logoImageId !== null ? 4 : 2);
        titleCell.font = { name: 'Amiri', size: 14, bold: true };
        titleCell.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };

        worksheet.addRow([
            getTranslatedLabel('accounting.invoices.report.date', 'Date') +
            ': ' +
            new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'numeric', day: 'numeric' }),
        ]);
        const dateCell = worksheet.getRow(logoImageId !== null ? 5 : 3);
        dateCell.font = { name: 'Amiri', size: 10 };
        dateCell.alignment = { horizontal: 'right', wrapText: true };
        worksheet.addRow([]);

        // Invoice Info
        const infoStartRow = worksheet.addRow([
            getTranslatedLabel('accounting.invoices.display.form.invoice-type', 'Invoice Type:'),
            invoice.invoiceTypeDescription || 'N/A',
            '',
            getTranslatedLabel('accounting.invoices.display.form.invoice-status', 'Status:'),
            invoice.statusDescription || 'N/A'
        ]);
        infoStartRow.font = { name: 'Amiri', size: 10, bold: true };

        worksheet.addRow([
            getTranslatedLabel('accounting.invoices.display.form.from-party', 'From Party:'),
            invoice.fromPartyName || 'N/A',
            '',
            getTranslatedLabel('accounting.invoices.display.form.to-party', 'Party To:'),
            invoice.toPartyName || 'N/A'
        ]);

        worksheet.addRow([
            getTranslatedLabel('accounting.invoices.display.form.invoice-date', 'Invoice Date:'),
            invoice.invoiceDate ? new Date(invoice.invoiceDate).toLocaleDateString() : 'N/A',
            '',
            getTranslatedLabel('accounting.invoices.display.form.total', 'Total:'),
            total?.toFixed(2) || '0.00'
        ]);

        worksheet.addRow([
            '',
            '',
            '',
            getTranslatedLabel('accounting.invoices.display.form.remaining-amount', 'Outstanding Amount:'),
            outstandingAmount?.toFixed(2) || '0.00'
        ]);

        if (invoice.description) {
            worksheet.addRow([
                getTranslatedLabel('accounting.invoices.display.form.description', 'Description:'),
                invoice.description
            ]);
        }

        worksheet.addRow([]);

        // Table Headers
        const headers = [
            getTranslatedLabel('accounting.invoices.display.form.columns.product', 'Product'),
            getTranslatedLabel('accounting.invoices.display.form.columns.item-type-desc', 'Item Type Description'),
            getTranslatedLabel('accounting.invoices.display.form.columns.quantity', 'Quantity'),
            getTranslatedLabel('accounting.invoices.display.form.columns.amount', 'Amount'),
            getTranslatedLabel('accounting.invoices.display.form.columns.description', 'Description'),
        ];
        const headerRow = worksheet.addRow(headers);
        headerRow.font = { name: 'Amiri', size: 10, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        headerRow.eachCell(cell => {
            cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
        });

        worksheet.columns = [
            { width: 30 }, // Product
            { width: 30 }, // Item Type
            { width: 15 }, // Quantity
            { width: 15 }, // Amount
            { width: 40 }, // Description
        ];

        // Add Items
        if (invoiceItems) {
            invoiceItems.forEach(item => {
                const rowData = [
                    sharedUtils.rtlEmbed(sharedUtils.safeString(item.productName || (item.productId ? 'Unnamed Product' : '— Service / Fee —'))),
                    sharedUtils.rtlEmbed(sharedUtils.safeString(item.invoiceItemTypeDescription)),
                    item.quantity,
                    item.amount,
                    sharedUtils.rtlEmbed(sharedUtils.safeString(item.description || '')),
                ];
                const row = worksheet.addRow(rowData);
                row.font = { name: 'Amiri', size: 9 };
                row.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
                row.eachCell(cell => {
                    cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
                });
            });
        }

        const buffer = await workbook.xlsx.writeBuffer();
        return buffer;
    }, [invoice, invoiceItems, getTranslatedLabel, total, outstandingAmount]);

    const handleDownload = useCallback(async () => {
        const buffer = await generateExcel();
        if (buffer) {
            const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            saveAs(blob, `Invoice_${invoice.invoiceId || 'New'}.xlsx`);
        }
    }, [generateExcel, invoice.invoiceId]);

    return (
        <Button
            color="primary"
            variant="outlined"
            onClick={handleDownload}
            fullWidth
            sx={{ mt: 1, textTransform: 'none' }}
        >
            {getTranslatedLabel('accounting.invoices.display.form.actions.exportExcel', 'Export to Excel')}
        </Button>
    );
};
