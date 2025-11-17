import React, { useCallback, useState } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';
import { useTranslationHelper } from '../../../../../app/hooks/useTranslationHelper';
import { SalesRequest } from '../../../../../app/models/order/SalesRequest';
import { Product } from '../../../../../app/models/product/product';

// REFACTOR: Extract shared utilities to avoid duplication across Excel components
// Purpose: Centralize RTL embedding, safe string handling, number formatting, and logo fetching
// Improvement: Guarantees consistent Arabic support, number commas, and logo placement
// Context: Reused from SupplyCertificateExcel, WorkmanshipCertificateExcel, etc.
const sharedUtils = {
    safeString: (value: any): string => {
        if (value === null || value === undefined) return 'N/A';
        return String(value);
    },
    rtlEmbed: (text: string): string => {
        return /\p{Script=Arabic}/u.test(text) ? `\u202B${text}` : text;
    },
    formatNumber: (value: number | undefined, decimals: number = 2): string => {
        if (value === undefined || value === null) return 'N/A';
        return value.toLocaleString('en-US', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals,
        });
    },
};

interface PaymentPlanExcelProps {
    salesRequest: SalesRequest;
    apartment?: Product;
    getTranslatedLabel: (key: string, defaultValue: string) => string;
}

export const PaymentPlanExcel: React.FC<PaymentPlanExcelProps> = ({
                                                                      salesRequest,
                                                                      apartment,
                                                                      getTranslatedLabel,
                                                                  }) => {
    const [loading, setLoading] = useState(false);
    const localizationKey = 'sales.request.paymentPlan';

    // REFACTOR: Re-use the same installment calculation logic from PaymentPlanModal
    // Purpose: Avoid code duplication and ensure Excel data matches modal exactly
    // Improvement: Guarantees identical due dates and amounts
    // Context: Mirrors the useMemo block in the modal
    const installments = React.useMemo(() => {
        const {
            totalPrice = 0,
            advancePayment = 0,
            numberOfInstallments = 0,
            dateOfFirstInstallment,
            monthsBetweenInstallments = 0,
        } = salesRequest;

        if (
            !totalPrice ||
            !advancePayment ||
            advancePayment >= totalPrice ||
            !numberOfInstallments ||
            !dateOfFirstInstallment ||
            !monthsBetweenInstallments
        ) {
            return [];
        }

        const remaining = totalPrice - advancePayment;
        const installmentAmount = remaining / numberOfInstallments;
        const result: Array<{ installmentNumber: number; dueDate: Date; amount: number }> = [];
        let currentDate = new Date(dateOfFirstInstallment);

        for (let i = 1; i <= numberOfInstallments; i++) {
            result.push({
                installmentNumber: i,
                dueDate: new Date(currentDate),
                amount: installmentAmount,
            });
            currentDate.setMonth(currentDate.getMonth() + monthsBetweenInstallments);
        }

        return result;
    }, [salesRequest]);

    // REFACTOR: Centralize Excel generation in a memoized function
    // Purpose: Prevent re-creation on every render; only rebuilds when data changes
    // Improvement: Performance + clean separation of concerns
    const generateExcel = useCallback(async () => {
        if (installments.length === 0) return null;

        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        const worksheet = workbook.addWorksheet(
            sharedUtils.rtlEmbed(
                getTranslatedLabel(`${localizationKey}.title`, 'Payment Plan Schedule')
            ).slice(0, 31) // Excel worksheet name limit
        );

        worksheet.pageSetup = { paperSize: 9, orientation: 'portrait', fitToPage: true };
        worksheet.views = [{ rightToLeft: true }];
        worksheet.getColumn(1).font = { name: 'Amiri', size: 10 };

        // REFACTOR: Fetch and embed logo (same pattern as SupplyCertificateExcel)
        // Purpose: Brand consistency across all reports
        // Improvement: Falls back gracefully if logo missing
        let logoImageId: number | null = null;
        try {
            const response = await fetch('/goldenlandlogo.jpg');
            if (response.ok) {
                const blob = await response.blob();
                const arrayBuffer = await blob.arrayBuffer();
                logoImageId = workbook.addImage({
                    buffer: arrayBuffer,
                    extension: 'jpeg',
                });
            }
        } catch (err) {
            console.warn('Logo fetch failed:', err);
        }

        let startRow = 1;
        if (logoImageId !== null) {
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
            startRow = 4;
        }

        // Header Title
        worksheet.mergeCells(`A${startRow}:C${startRow}`);
        const titleCell = worksheet.getCell(`A${startRow}`);
        titleCell.value = getTranslatedLabel(`${localizationKey}.title`, 'Payment Plan Schedule');
        titleCell.font = { name: 'Amiri', size: 14, bold: true };
        titleCell.alignment = { horizontal: 'center', vertical: 'middle' };
        startRow++;

        // Apartment Info
        if (apartment?.productName) {
            worksheet.mergeCells(`A${startRow}:C${startRow}`);
            const aptCell = worksheet.getCell(`A${startRow}`);
            aptCell.value = `${apartment.productName} - ${getTranslatedLabel(
                `${localizationKey}.totalPrice`,
                'Total'
            )}: ${sharedUtils.formatNumber(salesRequest.totalPrice)}`;
            aptCell.font = { name: 'Amiri', size: 11 };
            aptCell.alignment = { horizontal: 'right' };
            startRow++;
        }

        worksheet.addRow([]); // spacer
        startRow++;

        // Table Headers
        const headers = [
            getTranslatedLabel(`${localizationKey}.columns.installmentNumber`, '#'),
            getTranslatedLabel(`${localizationKey}.columns.dueDate`, 'Due Date'),
            getTranslatedLabel(`${localizationKey}.columns.amount`, 'Amount'),
        ];
        const headerRow = worksheet.addRow(headers);
        headerRow.font = { name: 'Amiri', size: 10, bold: true };
        headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
        headerRow.eachCell((cell) => {
            cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
        });

        // REFACTOR: Set column widths to match modal grid proportions
        // Purpose: Visual consistency between modal and exported file
        // Improvement: Prevents text cutoff in Arabic
        worksheet.columns = [
            { width: 15 }, // #
            { width: 20 }, // Due Date
            { width: 18 }, // Amount
        ];

        // Number formatting
        worksheet.getColumn(2).numFmt = '@'; // Due Date as text
        worksheet.getColumn(3).numFmt = '#,##0.00';

        // Table Rows
        installments.forEach((inst) => {
            const row = worksheet.addRow([
                inst.installmentNumber,
                inst.dueDate.toLocaleDateString('en-GB'), // dd/MM/yyyy
                inst.amount,
            ]);
            row.font = { name: 'Amiri', size: 9 };
            row.alignment = { horizontal: 'right', vertical: 'middle' };
            row.eachCell((cell) => {
                cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
            });
        });

        // Footer: Totals
        worksheet.addRow([]);
        const totalRow = worksheet.addRow([
            '',
            getTranslatedLabel(`${localizationKey}.totalRemaining`, 'Total Remaining'),
            salesRequest.totalPrice! - salesRequest.advancePayment!,
        ]);
        totalRow.font = { name: 'Amiri', size: 10, bold: true };
        totalRow.getCell(3).numFmt = '#,##0.00';

        const buffer = await workbook.xlsx.writeBuffer();
        return buffer;
    }, [installments, salesRequest, apartment, getTranslatedLabel]);

    const handleDownload = useCallback(async () => {
        setLoading(true);
        try {
            const buffer = await generateExcel();
            if (buffer) {
                const blob = new Blob([buffer], {
                    type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                });
                const fileName = `PaymentPlan_${apartment?.productName || 'Apartment'}_${salesRequest.salesRequestId || 'Unknown'}.xlsx`;
                saveAs(blob, fileName);
            }
        } catch (err) {
            console.error('Excel generation failed:', err);
        } finally {
            setLoading(false);
        }
    }, [generateExcel, apartment, salesRequest]);

    if (installments.length === 0) return null;

    return (
        <Button
            color="success"
            variant="contained"
            disabled={loading}
            onClick={handleDownload}
            sx={{ ml: 1 }}
        >
            {loading
                ? getTranslatedLabel('common.generating', 'Generating...')
                : getTranslatedLabel('sales.request.paymentPlan.excel', 'Excel Report')}
        </Button>
    );
};