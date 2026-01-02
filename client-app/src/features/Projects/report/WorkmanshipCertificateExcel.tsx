import React, { useCallback, useState } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';
import ModalContainer from '../../../app/common/modals/ModalContainer';

interface WorkmanshipCertificatePDFProps {
    certificate: {
        certificateNumber: string;
        description: string;
        partyIdContractor: string;
    };
    items: {
        productName: string;
        code: string;
        description: string;
        quantity: number;
        uomName: string;
        materialPrice: number;
        laborPrice: number;
        displayTotal: number;
        deductions: number;
        deductionDescription: string;
        deserved: number;
        insurance: number;
        additionalInsurance: number;
        net: number;
        achievementPercentage: number; // ← NOW A NUMBER (40), not "40%"
        isLastInGroup: boolean;
        productSubtotal: number;
        mainItemDescription: string;
        discountNote: string;
    }[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    subtotal: number;
    isSubmitting: boolean;
    isAddCertificateLoading: boolean;
    isUpdateCertificateLoading: boolean;
    isReceiveLoading: boolean;
    pageSize?: number;
    isFetching?: boolean;
}

// REFACTOR: Shared utilities with RTL embedding
// Purpose: Adds Unicode RTL embedding (\u202B) for Arabic text
// Improvement: Forces RTL direction, fixing bidi reordering bugs
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
    certificateTypeTranslations: {
        SUPPLY_PROCUREMENT_CERTIFICATE: 'مستخلص توريدات',
        COMPANY_SUPPLY_SALE_CERTIFICATE: 'مستخلص مقاوله',
        WORKMANSHIP_CONTRACTING_CERTIFICATE: 'مستخلص توريدات من مخازن الشركة',
    },
};

const sharedUtilsUpdated = {
    ...sharedUtils,
    formatNumber: (value: number | undefined, decimals: number = 2): string => {
        if (value === undefined || value === null) return 'N/A';
        return value.toLocaleString('en-US', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals,
        });
    },
};

// REFACTOR: Accept achievementPercentage as string ("90%") or number (90)
// Purpose: Match real API data format
// Context: Prevents false validation errors
const validateItems = (items: WorkmanshipCertificatePDFProps['items']) => {
    const validationResults = items.map((item, index) => {
        const errors: string[] = [];

        if (!item.productName) errors.push('productName is missing');
        if (!item.code) errors.push('code is missing');
        if (item.quantity === undefined || item.quantity <= 0) errors.push('quantity must be > 0');

        // Accept both number and string like "90%"
        const percentageValue = typeof item.achievementPercentage === 'string'
            ? parseFloat(item.achievementPercentage.replace('%', ''))
            : item.achievementPercentage;

        if (isNaN(percentageValue) || percentageValue < 0 || percentageValue > 100) {
            errors.push(`achievementPercentage invalid: ${item.achievementPercentage}`);
        }

        return { index, errors, item };
    });

    const invalidItems = validationResults.filter(r => r.errors.length > 0);

    if (invalidItems.length > 0) {
        console.warn('Workmanship items validation warnings (non-blocking):', invalidItems);
    }

    // Always return true — we don't want to block printing for minor issues
    return { isValid: true, invalidItems };
};

export const WorkmanshipCertificateExcel: React.FC<WorkmanshipCertificatePDFProps> = ({
                                                                                          certificate,
                                                                                          items,
                                                                                          getTranslatedLabel,
                                                                                          subtotal,
                                                                                          certificateType = 'WORKMANSHIP_CONTRACTING_CERTIFICATE',
                                                                                          isSubmitting,
                                                                                          isAddCertificateLoading,
                                                                                          isUpdateCertificateLoading,
                                                                                          isReceiveLoading,
                                                                                          pageSize = 15,
                                                                                          isFetching = false,
                                                                                      }) => {
    const [show, setShow] = useState(false);

    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        const pages = [];
        for (let i = 0; i < items.length; i += pageSize) {
            pages.push(items.slice(i, i + pageSize));
        }

        validateItems(items);

        // Fetch logo
        let logoImageId: number | null = null;
        try {
            const response = await fetch('/goldenlandlogo.jpg');
            if (!response.ok) throw new Error('Failed to fetch logo');
            const blob = await response.blob();
            const arrayBuffer = await blob.arrayBuffer();
            logoImageId = workbook.addImage({ buffer: arrayBuffer, extension: 'jpeg' });
        } catch (error) {
            console.warn('Logo fetch failed:', error);
        }

        pages.forEach((pageItems, pageIndex) => {
            const worksheet = workbook.addWorksheet(`Page ${pageIndex + 1}`);
            worksheet.pageSetup = { paperSize: 9, orientation: 'landscape' };
            worksheet.views = [{ rightToLeft: true }];
            worksheet.getColumn(1).font = { name: 'Amiri', size: 10 };

            // Logo
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
            } else {
                worksheet.addRow(['Logo Unavailable']);
                worksheet.getRow(1).font = { name: 'Amiri', size: 10, color: { argb: 'FF0000' } };
                worksheet.getRow(1).alignment = { horizontal: 'center', vertical: 'middle' };
            }

            // Header
            worksheet.addRow([
                getTranslatedLabel('projects.certificate.report.title', 'Certificate Report') +
                ': ' +
                sharedUtilsUpdated.safeString(certificate.certificateNumber),
            ]);
            worksheet.mergeCells(`A${logoImageId !== null ? 4 : 2}:O${logoImageId !== null ? 4 : 2}`);
            worksheet.getRow(logoImageId !== null ? 4 : 2).font = { name: 'Amiri', size: 14, bold: true };
            worksheet.getRow(logoImageId !== null ? 4 : 2).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };

            worksheet.addRow([
                getTranslatedLabel('projects.certificate.type', 'Type') +
                ': ' +
                (sharedUtilsUpdated.certificateTypeTranslations[certificateType] || certificateType),
                getTranslatedLabel('projects.certificate.date', 'Date') + ': ' + new Date().toLocaleDateString('en-UK'),
            ]);

            const contractorName = certificate.partyIdContractor?.fromPartyName
                ?? certificate.partyIdContractor?.partyName
                ?? 'N/A';

            worksheet.addRow([
                getTranslatedLabel('projects.certificate.description', 'Description') +
                ': ' +
                sharedUtilsUpdated.safeString(certificate.description),
                getTranslatedLabel('projects.certificate.form.contractor', 'Contractor') +
                ': ' +
                sharedUtilsUpdated.safeString(contractorName),
            ]);

            worksheet.addRow([getTranslatedLabel('projects.certificate.total', 'Total') + ': ' + sharedUtilsUpdated.formatNumber(subtotal)]);

            worksheet.getRow(logoImageId !== null ? 5 : 3).font = { name: 'Amiri', size: 10 };
            worksheet.getRow(logoImageId !== null ? 6 : 4).font = { name: 'Amiri', size: 10 };
            worksheet.getRow(logoImageId !== null ? 7 : 5).font = { name: 'Amiri', size: 10 };
            worksheet.getRow(logoImageId !== null ? 5 : 3).alignment = { horizontal: 'right', wrapText: true };
            worksheet.getRow(logoImageId !== null ? 6 : 4).alignment = { horizontal: 'right', wrapText: true };
            worksheet.getRow(logoImageId !== null ? 7 : 5).alignment = { horizontal: 'right', wrapText: true };

            worksheet.addRow([]);

            // Table Headers
            const headers = [
                getTranslatedLabel('projects.certificate.items.list.item', 'Item'),
                getTranslatedLabel('projects.certificate.items.list.code', 'Code'),
                getTranslatedLabel('projects.certificate.items.list.description', 'Description'),
                getTranslatedLabel('projects.certificate.items.list.quantity', 'Quantity'),
                getTranslatedLabel('projects.certificate.items.list.unitOfMeasure', 'Unit of Measure'),
                getTranslatedLabel('projects.certificate.items.list.materialPrice', 'Material Price'),
                getTranslatedLabel('projects.certificate.items.list.laborPrice', 'Labor Price'),
                getTranslatedLabel('projects.certificate.items.list.totalAmount', 'Total Amount'),
                getTranslatedLabel('projects.certificate.items.list.deductions', 'Deductions'),
                getTranslatedLabel('projects.certificate.items.list.deductionDescription', 'Deduction Description'),
                getTranslatedLabel('projects.certificate.items.list.deserved', 'Deserved'),
                getTranslatedLabel('projects.certificate.items.list.insurance', 'Insurance'),
                getTranslatedLabel('projects.certificate.items.list.additionalInsurance', 'Additional Insurance'),
                getTranslatedLabel('projects.certificate.items.list.net', 'Net'),
                getTranslatedLabel('projects.certificate.items.list.achievementPercentage', 'Achievement Percentage'),
            ];

            worksheet.addRow(headers);
            const headerRow = worksheet.getRow(worksheet.lastRow!.number);
            headerRow.font = { name: 'Amiri', size: 10, bold: true };
            headerRow.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'F0F0F0' } };
            headerRow.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
            headerRow.eachCell((cell) => {
                cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
            });

            // Column widths
            worksheet.columns = [
                { width: 40 }, // Item
                { width: 40 }, // Code
                { width: 25 }, // Description
                { width: 8 },  // Quantity
                { width: 12 }, // UoM
                { width: 10 }, // Material
                { width: 10 }, // Labor
                { width: 10 }, // Total
                { width: 10 }, // Deductions
                { width: 15 }, // Deduction Desc
                { width: 11 }, // Deserved
                { width: 8 },  // Insurance
                { width: 10 }, // Add. Insurance
                { width: 8 },  // Net
                { width: 12 }, // Achievement %
            ];

            // Number formats
            worksheet.getColumn(4).numFmt = '0';           // Quantity
            worksheet.getColumn(6).numFmt = '#,##0.00';    // Material
            worksheet.getColumn(7).numFmt = '#,##0.00';    // Labor
            worksheet.getColumn(8).numFmt = '#,##0.00';    // Total
            worksheet.getColumn(9).numFmt = '#,##0.00';    // Deductions
            worksheet.getColumn(11).numFmt = '#,##0.00';   // Deserved
            worksheet.getColumn(12).numFmt = '#,##0.00';   // Insurance
            worksheet.getColumn(13).numFmt = '#,##0.00';   // Add. Insurance
            worksheet.getColumn(14).numFmt = '#,##0.00';   // Net
            worksheet.getColumn(15).numFmt = '0%';         // Achievement %

            // Rows
            pageItems.forEach((item) => {
                const rowData = [
                    item.isLastInGroup && item.productSubtotal !== undefined
                        ? `${sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.productName))} (${sharedUtilsUpdated.formatNumber(item.productSubtotal, 2)})`
                        : sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.productName)),
                    sharedUtilsUpdated.safeString(item.code),
                    sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.description)),
                    item.quantity ?? 'N/A',
                    sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.uomName)),
                    item.materialPrice ?? 0,
                    item.laborPrice ?? 0,
                    item.displayTotal ?? item.net ?? 0,
                    item.deductions ?? 0,
                    sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.deductionDescription)),
                    item.deserved ?? 0,
                    item.insurance ?? 0,
                    item.additionalInsurance ?? 0,
                    item.net ?? 0,
                    // Critical fix: convert "90%" → 90 → 0.90
                    (() => {
                        const val = typeof item.achievementPercentage === 'string'
                            ? parseFloat(item.achievementPercentage.replace('%', ''))
                            : item.achievementPercentage || 0;
                        return val / 100;
                    })(),
                ];

                const row = worksheet.addRow(rowData);
                row.font = { name: 'Amiri', size: 9 };
                row.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
                row.eachCell((cell) => {
                    cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
                });
            });

            // Notes
            if (pageItems.some((item) => item.mainItemDescription?.trim())) {
                worksheet.addRow([]);
                worksheet.addRow([getTranslatedLabel('projects.certificate.items.mainDescription', 'Main Item Description')]);
                worksheet.getRow(worksheet.lastRow!.number).font = { name: 'Amiri', size: 10, bold: true };
                pageItems.forEach((item) => {
                    if (item.mainItemDescription?.trim()) {
                        worksheet.addRow([sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.mainItemDescription))]);
                        worksheet.getRow(worksheet.lastRow!.number).font = { name: 'Amiri', size: 9 };
                        worksheet.getRow(worksheet.lastRow!.number).alignment = { horizontal: 'right', wrapText: true };
                    }
                });
            }

            if (pageItems.some((item) => item.discountNote?.trim())) {
                worksheet.addRow([]);
                worksheet.addRow([getTranslatedLabel('projects.certificate.items.discountNote', 'Discount Description Note')]);
                worksheet.getRow(worksheet.lastRow!.number).font = { name: 'Amiri', size: 10, bold: true };
                pageItems.forEach((item) => {
                    if (item.discountNote?.trim()) {
                        worksheet.addRow([sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.discountNote))]);
                        worksheet.getRow(worksheet.lastRow!.number).font = { name: 'Amiri', size: 9 };
                        worksheet.getRow(worksheet.lastRow!.number).alignment = { horizontal: 'right', wrapText: true };
                    }
                });
            }
        });

        const buffer = await workbook.xlsx.writeBuffer();
        return buffer;
    }, [items, certificate, subtotal, certificateType, pageSize, getTranslatedLabel, isFetching]);

    const handleDownload = useCallback(async () => {
        const buffer = await generateExcel();
        if (buffer) {
            const blob = new Blob([buffer], {
                type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
            });
            saveAs(blob, `WorkmanshipCertificate_${certificate.certificateNumber}.xlsx`);
        }
    }, [generateExcel, certificate.certificateNumber]);

    const disabled =
        isSubmitting || isAddCertificateLoading || isUpdateCertificateLoading || isReceiveLoading || isFetching;

    return (
        <div>
            <Button
                color="primary"
                variant="outlined"
                disabled={disabled}
                onClick={handleDownload}
                style={{ marginRight: 10 }}
            >
                {getTranslatedLabel('projects.certificate.excel', 'Excel Report')}
            </Button>
        </div>
    );
};