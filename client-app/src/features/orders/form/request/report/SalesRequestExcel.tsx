// src/features/orders/form/request/report/SalesRequestExcel.tsx
import React, { useCallback } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';
import { SalesRequest } from '../../../../../app/models/order/SalesRequest';

interface Installment {
    dueDate: string | Date;
    amount: number;
    isAdvance: boolean;
}

interface SalesRequestExcelProps {
    salesRequest: SalesRequest;
    installments: Installment[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    language: string;
}

const utils = {
    safeString: (v: any) => (v == null || typeof v === 'object') ? 'N/A' : String(v),
    rtlEmbed: (t: string) => /\p{Script=Arabic}/u.test(t) ? `\u202B${t}` : t,
    formatNumber: (v: number | undefined | null, dec = 2) =>
        v == null ? '0.00' : v.toLocaleString('en-US', { minimumFractionDigits: dec, maximumFractionDigits: dec }),
    formatDate: (d: string | Date | undefined | null) => d ? new Date(d).toLocaleDateString('en-GB') : 'N/A',
};

export const SalesRequestExcel: React.FC<SalesRequestExcelProps> = ({
                                                                        salesRequest,
                                                                        installments,
                                                                        getTranslatedLabel,
                                                                        language,
                                                                    }) => {
    const localizationKey = "salesRequest.report.excel";

    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        // --- FETCH LOGO ---
        let logoBuffer: ArrayBuffer | null = null;
        try {
            const resp = await fetch('/goldenlandlogo.jpg');
            if (resp.ok) {
                logoBuffer = await (await resp.blob()).arrayBuffer();
            }
        } catch (e) {
            console.warn('Logo fetch error:', e);
        }

        const safeSheet = salesRequest.salesRequestId
            ? `SR_${salesRequest.salesRequestId}`
            : 'Sales Request';
        
        const ws = workbook.addWorksheet(safeSheet, { views: [{ rightToLeft: language === 'ar' }] });
        ws.pageSetup = { paperSize: 9, orientation: 'landscape' };
        ws.getColumn(1).font = { name: 'Amiri', size: 10 };

        // === ADD LOGO ===
        if (logoBuffer) {
            const imageId = workbook.addImage({
                buffer: logoBuffer,
                extension: 'jpeg',
            });
            ws.getRow(1).height = 75;
            ws.addImage(imageId, {
                tl: { col: 0, row: 0 },
                ext: { width: 100, height: 100 },
            });
            ws.addRow([]); ws.addRow([]); ws.addRow([]);
        } else {
            const fallbackRow = ws.getRow(1);
            fallbackRow.getCell(1).value = 'Logo Unavailable';
            fallbackRow.font = { name: 'Amiri', size: 10, color: { argb: 'FFFF0000' } };
            fallbackRow.alignment = { horizontal: 'center', vertical: 'middle' };
        }

        const startRow = logoBuffer ? 5 : 2;

        // === TITLE ===
        const title = utils.rtlEmbed(getTranslatedLabel('salesRequest.form.new2', 'Sales Request')) + ' - ' + (salesRequest.salesRequestId || '');
        ws.addRow([title]);
        ws.mergeCells(`A${startRow}:F${startRow}`);
        ws.getRow(startRow).font = { name: 'Amiri', size: 16, bold: true };
        ws.getRow(startRow).alignment = { horizontal: 'center', vertical: 'middle' };
        ws.addRow([]); // spacer

        // === HEADER DATA ===
        const headerStart = ws.lastRow!.number + 1;
        ws.addRow([
            getTranslatedLabel('salesRequest.form.saleDate', 'Sale Date'),
            getTranslatedLabel('salesRequest.form.from', 'Customer'),
            getTranslatedLabel('salesRequest.form.product', 'Product'),
            getTranslatedLabel('salesRequest.form.project', 'Project'),
            getTranslatedLabel('salesRequest.form.status', 'Status'),
        ]);
        const headerLabelRow = ws.getRow(headerStart);
        headerLabelRow.font = { name: 'Amiri', size: 10, bold: true };
        headerLabelRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        headerLabelRow.alignment = { horizontal: 'center' };

        ws.addRow([
            utils.formatDate(salesRequest.saleDate),
            utils.rtlEmbed(salesRequest.fromPartyName || ''),
            utils.rtlEmbed(salesRequest.apartmentName || ''),
            utils.rtlEmbed(salesRequest.projectName || ''),
            utils.rtlEmbed(salesRequest.statusDescription || ''),
        ]);
        ws.getRow(ws.lastRow!.number).font = { name: 'Amiri', size: 10 };
        ws.getRow(ws.lastRow!.number).alignment = { horizontal: 'center' };
        ws.addRow([]); // spacer

        // === PRICING DATA ===
        const pricingStart = ws.lastRow!.number + 1;
        ws.addRow([
            getTranslatedLabel('salesRequest.form.totalPrice', 'Total Price'),
            getTranslatedLabel('salesRequest.form.discount', 'Discount'),
            getTranslatedLabel('salesRequest.form.advance', 'Advance'),
            getTranslatedLabel('salesRequest.form.maintenanceDeposit', 'Maintenance Deposit'),
        ]);
        const pricingLabelRow = ws.getRow(pricingStart);
        pricingLabelRow.font = { name: 'Amiri', size: 10, bold: true };
        pricingLabelRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'E6F3FF' } };
        pricingLabelRow.alignment = { horizontal: 'center' };

        ws.addRow([
            salesRequest.totalPrice || 0,
            salesRequest.discount || 0,
            salesRequest.advancePayment || 0,
            salesRequest.maintenanceDeposit || 0,
        ]);
        ws.getRow(ws.lastRow!.number).font = { name: 'Amiri', size: 10 };
        ws.getRow(ws.lastRow!.number).alignment = { horizontal: 'center' };
        ws.addRow([]); // spacer

        // === PAYMENT SCHEDULE ===
        const scheduleStart = ws.lastRow!.number + 1;
        ws.addRow([getTranslatedLabel('salesRequest.form.paymentPlan', 'Payment Schedule')]);
        ws.mergeCells(`A${scheduleStart}:F${scheduleStart}`);
        ws.getRow(scheduleStart).font = { name: 'Amiri', size: 12, bold: true };
        ws.getRow(scheduleStart).alignment = { horizontal: 'center' };

        ws.addRow([
            '#',
            getTranslatedLabel('salesRequest.form.dueDate', 'Due Date'),
            getTranslatedLabel('salesRequest.form.amount', 'Amount'),
            getTranslatedLabel('salesRequest.form.type', 'Type'),
        ]);
        const scheduleHeaderRow = ws.getRow(ws.lastRow!.number);
        scheduleHeaderRow.font = { name: 'Amiri', size: 10, bold: true };
        scheduleHeaderRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'D9EAD3' } };
        scheduleHeaderRow.alignment = { horizontal: 'center' };
        scheduleHeaderRow.eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

        installments.forEach((inst, index) => {
            ws.addRow([
                index + 1,
                utils.formatDate(inst.dueDate),
                inst.amount || 0,
                inst.isAdvance 
                    ? getTranslatedLabel('salesRequest.form.advance', 'Advance') 
                    : getTranslatedLabel('salesRequest.form.installment', 'Installment'),
            ]);
        });
        ws.getRow(ws.lastRow!.number).eachCell(c => c.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } });

        // === COLUMN WIDTHS ===
        ws.columns = [
            { width: 10 }, // #
            { width: 30 }, // Column 2
            { width: 20 }, // Column 3
            { width: 25 }, // Column 4
            { width: 20 }, // Column 5
            { width: 20 }, // Column 6
        ];
        
        // Format numeric columns
        ws.getColumn(3).numFmt = '#,##0.00'; // Pricing values and installment amount (depending on layout)
        // Adjust formatting based on row indices if needed, but for simplicity:
        ws.getRow(pricingStart + 1).eachCell((cell, colNumber) => {
            if (colNumber <= 4) cell.numFmt = '#,##0.00';
        });

        return await workbook.xlsx.writeBuffer();
    }, [salesRequest, installments, getTranslatedLabel, language]);

    const handleDownload = useCallback(async () => {
        const buf = await generateExcel();
        if (buf) {
            const blob = new Blob([buf], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            saveAs(blob, `SalesRequest_${salesRequest.salesRequestId}.xlsx`);
        }
    }, [generateExcel, salesRequest.salesRequestId]);

    return (
        <Button
            variant="contained"
            color="success"
            onClick={handleDownload}
            sx={{ mr: 1 }}
        >
            {getTranslatedLabel(
                'projects.multiPaymentCertificate.report.excel',
                'Export Excel'
            )}
        </Button>
    );
};
