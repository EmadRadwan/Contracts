import React, { useCallback, useState } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';
import ModalContainer from '../../../app/common/modals/ModalContainer';

interface WorkmanshipCertificatePDFProps {
    certificate: { certificateNumber: string; description: string; partyIdContractor: string };
    items: { productName: string; code: string; description: string; quantity: number; uomName: string; materialPrice: number; laborPrice: number; displayTotal: number; deductions: number; deductionDescription: string; deserved: number; insurance: number; additionalInsurance: number; net: number; achievementPercentage: string | number; isLastInGroup: boolean; productSubtotal: number; mainItemDescription: string; discountNote: string; }[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    subtotal: number;
    isSubmitting: boolean;
    isAddCertificateLoading: boolean;
    isUpdateCertificateLoading: boolean;
    isReceiveLoading: boolean;
    pageSize?: number;
    isFetching?: boolean; // REFACTOR: Add prop for fetch state
    // Purpose: Passed from parent to disable during fetch
    // Improvement: Prevents render with stale/empty items on switch
    // Context: Logs show PDF triggers before fetch completes
}

// REFACTOR: Shared utilities with RTL embedding
// Purpose: Adds Unicode RTL embedding (\u202B) for Arabic text
// Improvement: Forces RTL direction, fixing bidi reordering bugs in #1571, #2306
// Context: Wraps text to prevent 'id' error in reorderLine for mixed content
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
        // REFACTOR: Add \u202B embedding
        // Purpose: Forces RTL for Arabic, preventing reversal in mixed text
        // Improvement: Fixes number flipping (e.g., "2 *" becomes "* 2") as in SO posts
        // Context: From #2306 discussion; test for Arabic
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
const validateItems = (items: WorkmanshipCertificatePDFProps['items']) => {
    const validationResults = items.map((item, index) => {
        const errors: string[] = [];
        if (!item.productName) errors.push('productName is missing');
        if (!item.code) errors.push('code is missing');
        if (item.quantity === undefined || item.quantity < 0) errors.push('quantity is invalid');
        if (item.materialPrice === undefined || item.laborPrice === undefined) errors.push('materialPrice/laborPrice missing for WORKMANSHIP');
        if (typeof item.achievementPercentage === 'number' && isNaN(item.achievementPercentage)) errors.push('achievementPercentage is NaN');
        return { index, errors, item };
    });
    const invalidItems = validationResults.filter(result => result.errors.length > 0);
    if (invalidItems.length > 0) {
        console.error('Invalid items detected:', invalidItems);
    }
    return { isValid: invalidItems.length === 0, invalidItems };
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

    console.log('Rendering WorkmanshipCertificateExcel with items:', { items, certificateType, isFetching });

    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        const pages = [];
        for (let i = 0; i < items.length; i += pageSize) {
            pages.push(items.slice(i, i + pageSize));
        }

        const { isValid } = validateItems(items);
        if (!isValid || isFetching) {
            console.error('Cannot generate Excel: Invalid items or fetching');
            return null;
        }

        // Fetch logo from public folder
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
        } catch (error) {
            console.warn('Logo fetch failed:', error);
        }

        pages.forEach((pageItems, pageIndex) => {
            const worksheet = workbook.addWorksheet(`Page ${pageIndex + 1}`);

            worksheet.pageSetup = { paperSize: 9, orientation: 'landscape' };
            worksheet.views = [{ rightToLeft: true }];
            worksheet.getColumn(1).font = { name: 'Amiri', size: 10 };

            // Add logo to worksheet
            if (logoImageId !== null) {
                worksheet.addImage(logoImageId, {
                    tl: { col: 0, row: 0 },
                    ext: { width: 100, height: 100 },
                    editAs: 'absolute',
                });
                worksheet.getRow(1).height = 75; // Approx 100px at 96 DPI
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

            // Header Section
            worksheet.addRow([getTranslatedLabel('projects.certificate.report.title', 'Certificate Report') + ': ' + sharedUtilsUpdated.safeString(certificate.certificateNumber)]);
            worksheet.mergeCells(`A${logoImageId !== null ? 4 : 2}:O${logoImageId !== null ? 4 : 2}`);
            worksheet.getRow(logoImageId !== null ? 4 : 2).font = { name: 'Amiri', size: 14, bold: true };
            worksheet.getRow(logoImageId !== null ? 4 : 2).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };

            worksheet.addRow([
                getTranslatedLabel('projects.certificate.type', 'Type') + ': ' + (sharedUtilsUpdated.certificateTypeTranslations[certificateType] || certificateType),
                getTranslatedLabel('projects.certificate.date', 'Date') + ': ' + new Date().toLocaleDateString('en-UK'),
            ]);
            worksheet.addRow([
                getTranslatedLabel('projects.certificate.description', 'Description') + ': ' + sharedUtilsUpdated.safeString(certificate.description),
                getTranslatedLabel('projects.certificate.form.contractor', 'Contractor') + ': ' + sharedUtilsUpdated.safeString(certificate.partyIdContractor),
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
            headerRow.eachCell(cell => {
                cell.border = {
                    top: { style: 'thin' },
                    left: { style: 'thin' },
                    bottom: { style: 'thin' },
                    right: { style: 'thin' },
                };
            });

            worksheet.columns = [
                { width: 40 }, // Item
                { width: 40 }, // Code
                { width: 25 }, // Description
                { width: 8 }, // Quantity
                { width: 12 }, // Unit of Measure
                { width: 10 }, // Material Price
                { width: 10 }, // Labor Price
                { width: 10 }, // Total Amount
                { width: 10 }, // Deductions
                { width: 15 }, // Deduction Description
                { width: 11 }, // Deserved
                { width: 8 }, // Insurance
                { width: 10 }, // Additional Insurance
                { width: 8 }, // Net
                { width: 12 }, // Achievement Percentage
            ];

            // REFACTOR: Ensure numeric columns use raw numbers with numFmt
            // Purpose: Allows Excel to apply locale-based formatting (e.g., commas) for numeric cells
            // Improvement: Raw numbers enable Excel's built-in formatting; numFmt ensures consistent display
            // Context: Sample Excel shows numbers without commas, but user expects commas; numFmt handles this
            worksheet.getColumn(4).numFmt = '0'; // Quantity
            worksheet.getColumn(6).numFmt = '#,##0.00'; // Material Price
            worksheet.getColumn(7).numFmt = '#,##0.00'; // Labor Price
            worksheet.getColumn(8).numFmt = '#,##0.00'; // Total Amount
            worksheet.getColumn(9).numFmt = '#,##0.00'; // Deductions
            worksheet.getColumn(11).numFmt = '#,##0.00'; // Deserved
            worksheet.getColumn(12).numFmt = '#,##0.00'; // Insurance
            worksheet.getColumn(13).numFmt = '#,##0.00'; // Additional Insurance
            worksheet.getColumn(14).numFmt = '#,##0.00'; // Net
            worksheet.getColumn(15).numFmt = '0%'; // Achievement Percentage

            // Table Rows
            pageItems.forEach((item, index) => {
                let achievementValue: number | string = item.achievementPercentage;
                const achievementStr = sharedUtilsUpdated.safeString(item.achievementPercentage);
                if (achievementStr.endsWith('%')) {
                    const parsed = parseFloat(achievementStr.slice(0, -1));
                    if (!isNaN(parsed)) {
                        achievementValue = parsed / 100;
                    }
                } else if (typeof item.achievementPercentage === 'number') {
                    achievementValue = item.achievementPercentage / 100;
                }

                const rowData = [
                    item.isLastInGroup && item.productSubtotal !== undefined
                        ? `${sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.productName))} (${sharedUtilsUpdated.formatNumber(item.productSubtotal, 2)})`
                        : sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.productName)),
                    sharedUtilsUpdated.safeString(item.code),
                    sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.description)),
                    item.quantity !== undefined ? item.quantity : 'N/A',
                    sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.uomName)),
                    item.materialPrice !== undefined ? item.materialPrice : 'N/A',
                    item.laborPrice !== undefined ? item.laborPrice : 'N/A',
                    item.displayTotal !== undefined ? item.displayTotal : 'N/A',
                    item.deductions !== undefined ? item.deductions : 'N/A',
                    sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.deductionDescription)),
                    item.deserved !== undefined ? item.deserved : 'N/A',
                    item.insurance !== undefined ? item.insurance : 'N/A',
                    item.additionalInsurance !== undefined ? item.additionalInsurance : 'N/A',
                    item.net !== undefined ? item.net : 'N/A',
                    achievementValue,
                ];
                const row = worksheet.addRow(rowData);
                row.font = { name: 'Amiri', size: 9 };
                row.alignment = { horizontal: 'right', vertical: 'middle', wrapText: true };
                row.eachCell(cell => {
                    cell.border = {
                        top: { style: 'thin' },
                        left: { style: 'thin' },
                        bottom: { style: 'thin' },
                        right: { style: 'thin' },
                    };
                });
            });

            // Notes Sections
            if (pageItems.some(item => item.mainItemDescription && item.mainItemDescription.trim())) {
                worksheet.addRow([]);
                worksheet.addRow([getTranslatedLabel('projects.certificate.items.mainDescription', 'Main Item Description')]);
                worksheet.getRow(worksheet.lastRow!.number).font = { name: 'Amiri', size: 10, bold: true };
                pageItems.forEach(item => {
                    if (item.mainItemDescription && item.mainItemDescription.trim()) {
                        worksheet.addRow([sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.mainItemDescription))]);
                        worksheet.getRow(worksheet.lastRow!.number).font = { name: 'Amiri', size: 9 };
                        worksheet.getRow(worksheet.lastRow!.number).alignment = { horizontal: 'right', wrapText: true };
                    }
                });
            }

            if (pageItems.some(item => item.discountNote && item.discountNote.trim())) {
                worksheet.addRow([]);
                worksheet.addRow([getTranslatedLabel('projects.certificate.items.discountNote', 'Discount Description Note')]);
                worksheet.getRow(worksheet.lastRow!.number).font = { name: 'Amiri', size: 10, bold: true };
                pageItems.forEach(item => {
                    if (item.discountNote && item.discountNote.trim()) {
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

    const onClose = useCallback(() => setShow(false), []);

    const handleDownload = useCallback(async () => {
        const buffer = await generateExcel();
        if (buffer) {
            const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            saveAs(blob, `WorkmanshipCertificate_${certificate.certificateNumber}.xlsx`);
        }
    }, [generateExcel, certificate.certificateNumber]);

    const disabled = isSubmitting || isAddCertificateLoading || isUpdateCertificateLoading || isReceiveLoading || isFetching;

    return (
        <div>
            <Button
                color="primary"
                variant="outlined"
                disabled={disabled}
                onClick={handleDownload}
                style={{ marginRight: 10 }}
            >
                {getTranslatedLabel('projects.certificate.excel', 'Download Excel')}
            </Button>
            
            {show && (
                <ModalContainer show={show} onClose={onClose} width={1200}>
                    <div>
                        {getTranslatedLabel('projects.certificate.excel.preview', 'Excel preview is not supported in browsers. Please download the file.')}
                    </div>
                </ModalContainer>
            )}
        </div>
    );
};