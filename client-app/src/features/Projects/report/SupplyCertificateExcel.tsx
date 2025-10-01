import React, { useCallback, useState } from 'react';
import ExcelJS from 'exceljs';
import { saveAs } from 'file-saver';
import { Button } from '@mui/material';
import ModalContainer from '../../../app/common/modals/ModalContainer';


interface SupplyCertificatePDFProps {
    certificate: { certificateNumber: string; description: string; partyIdSupplier: string; facilityName: string };
    items: { productName: string; code: string; description: string; quantity: number; uomName: string; unitPrice: number; displayTotal: number; discount: number; formattedProcurementDate: string; transportationExpenses: number; gratuities: number; isLastInGroup: boolean; productSubtotal: number; mainItemDescription: string; discountNote: string; }[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    subtotal: number;
    isSubmitting: boolean;
    isAddCertificateLoading: boolean;
    isUpdateCertificateLoading: boolean;
    isReceiveLoading: boolean;
    pageSize?: number;
}

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


// REFACTOR: Use toLocaleString for number formatting
// Purpose: Adds thousands separators (commas) for readability, matching PDF and user expectations
// Improvement: Formats numbers like 790800.00 as 790,800.00 in header and product subtotal
// Context: Aligns with PDF's formatNumber and updated WorkmanshipCertificateExcel.tsx
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

// REFACTOR: Add validation for items
// Purpose: Ensures items have required fields to prevent errors in Excel generation
// Improvement: Catches missing or invalid data early, logs errors for debugging
// Context: Adapted from WorkmanshipCertificateExcel.tsx for SupplyCertificate props
const validateItems = (items: SupplyCertificatePDFProps['items']) => {
    const validationResults = items.map((item, index) => {
        const errors: string[] = [];
        if (!item.productName) errors.push('productName is missing');
        if (!item.code) errors.push('code is missing');
        if (item.quantity === undefined || item.quantity < 0) errors.push('quantity is invalid');
        if (item.unitPrice === undefined) errors.push('unitPrice is missing');
        if (item.displayTotal === undefined) errors.push('displayTotal is missing');
        return { index, errors, item };
    });
    const invalidItems = validationResults.filter(result => result.errors.length > 0);
    if (invalidItems.length > 0) {
        console.error('Invalid items detected:', invalidItems);
    }
    return { isValid: invalidItems.length === 0, invalidItems };
};

export const SupplyCertificateExcel: React.FC<SupplyCertificatePDFProps> = ({
                                                                                certificate,
                                                                                items,
                                                                                getTranslatedLabel,
                                                                                subtotal,
                                                                                certificateType = 'SUPPLY_PROCUREMENT_CERTIFICATE',
                                                                                isSubmitting,
                                                                                isAddCertificateLoading,
                                                                                isUpdateCertificateLoading,
                                                                                isReceiveLoading,
                                                                                pageSize = 15,
                                                                            }) => {
    const [show, setShow] = useState(false);
    const isSupplyWithDiscount = certificateType === 'SUPPLY_PROCUREMENT_CERTIFICATE';

    console.log('Rendering SupplyCertificateExcel with items:', { items, certificateType });

    const generateExcel = useCallback(async () => {
        const workbook = new ExcelJS.Workbook();
        workbook.created = new Date();
        workbook.creator = 'System';

        const pages = [];
        for (let i = 0; i < items.length; i += pageSize) {
            pages.push(items.slice(i, i + pageSize));
        }

        const { isValid } = validateItems(items);
        if (!isValid) {
            console.error('Cannot generate Excel: Invalid items');
            return null;
        }

        // REFACTOR: Fetch logo from public folder
        // Purpose: Loads goldenlandlogo.jpg from React public folder, matching PDF version
        // Improvement: Avoids embedding large base64 strings; uses browser-native fetch
        // Context: PDF uses /goldenlandlogo.jpg; fetching as buffer ensures compatibility with ExcelJS
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

            // REFACTOR: Add logo to worksheet
            // Purpose: Places logo at top-left (A1:B3), matching PDF's header placement
            // Improvement: Sets row height to accommodate logo; fallback text if logo fails
            // Context: PDF uses 100x100px logo; Excel uses similar dimensions (100px ~ 75pt)
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
            worksheet.mergeCells(`A${logoImageId !== null ? 4 : 2}:K${logoImageId !== null ? 4 : 2}`);
            worksheet.getRow(logoImageId !== null ? 4 : 2).font = { name: 'Amiri', size: 14, bold: true };
            worksheet.getRow(logoImageId !== null ? 4 : 2).alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };

            worksheet.addRow([
                getTranslatedLabel('projects.certificate.type', 'Type') + ': ' + (sharedUtilsUpdated.certificateTypeTranslations[certificateType] || certificateType),
                getTranslatedLabel('projects.certificate.date', 'Date') + ': ' + new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'numeric', day: 'numeric' }),
            ]);
            worksheet.addRow([
                getTranslatedLabel('projects.certificate.description', 'Description') + ': ' + sharedUtilsUpdated.safeString(certificate.description),
                getTranslatedLabel('projects.certificate.form.supplier', 'Supplier') + ': ' + sharedUtilsUpdated.safeString(certificate.partyIdSupplier),
            ]);
            worksheet.addRow([
                getTranslatedLabel('projects.certificate.total', 'Total') + ': ' + sharedUtilsUpdated.formatNumber(subtotal),
                getTranslatedLabel('projects.certificate.form.facility', 'Facility') + ': ' + sharedUtilsUpdated.safeString(certificate.facilityName),
            ]);
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
                getTranslatedLabel('projects.certificate.items.list.unitPrice', 'Unit Price'),
                getTranslatedLabel('projects.certificate.items.list.totalAmount', 'Total Amount'),
                ...(isSupplyWithDiscount ? [getTranslatedLabel('projects.certificate.items.list.discount', 'Discount')] : []),
                getTranslatedLabel('projects.certificate.items.list.procurementDate', 'Procurement Date'),
                getTranslatedLabel('projects.certificate.items.list.transportationExpenses', 'Transportation Expenses'),
                getTranslatedLabel('projects.certificate.items.list.gratuities', 'Gratuities'),
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

            // REFACTOR: Set column widths to match PDF table proportions
            // Purpose: Aligns column widths with PDF's percentages for consistent layout
            // Improvement: Wider Description (20%) and Unit of Measure (12%) for Arabic text; conditional Discount column
            // Context: PDF uses fixed percentages; Excel widths adjusted to approximate visual balance
            worksheet.columns = [
                { width: 40 }, // Item (8%)
                { width: 40 }, // Code (8%)
                { width: 100 }, // Description (20%)
                { width: 8 }, // Quantity (6%)
                { width: 12 }, // Unit of Measure (12%)
                { width: 10 }, // Unit Price (8%)
                { width: 10 }, // Total Amount (8%)
                ...(isSupplyWithDiscount ? [{ width: 10 }] : []), // Discount (8%)
                { width: 15 }, // Procurement Date (10%)
                { width: 10 }, // Transportation Expenses (8%)
                { width: 10 }, // Gratuities (7%)
            ];

            // REFACTOR: Apply comma formatting to numeric columns
            // Purpose: Ensures numbers display with thousands separators (e.g., 790,800.00)
            // Improvement: Matches PDF's toLocaleString and user expectations for readability
            // Context: Consistent with updated WorkmanshipCertificateExcel.tsx
            worksheet.getColumn(4).numFmt = '0'; // Quantity
            worksheet.getColumn(6).numFmt = '#,##0.00'; // Unit Price
            worksheet.getColumn(7).numFmt = '#,##0.00'; // Total Amount
            const discountCol = isSupplyWithDiscount ? 8 : 7;
            if (isSupplyWithDiscount) worksheet.getColumn(discountCol).numFmt = '#,##0.00'; // Discount
            worksheet.getColumn(discountCol + 1).numFmt = '@'; // Procurement Date (text)
            worksheet.getColumn(discountCol + 2).numFmt = '#,##0.00'; // Transportation Expenses
            worksheet.getColumn(discountCol + 3).numFmt = '#,##0.00'; // Gratuities

            // Table Rows
            pageItems.forEach((item, index) => {
                const rowData = [
                    item.isLastInGroup && item.productSubtotal !== undefined
                        ? `${sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.productName))} (${sharedUtilsUpdated.formatNumber(item.productSubtotal, 2)})`
                        : sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.productName)),
                    sharedUtilsUpdated.safeString(item.code),
                    sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.description)),
                    item.quantity !== undefined ? item.quantity : 'N/A',
                    sharedUtilsUpdated.rtlEmbed(sharedUtilsUpdated.safeString(item.uomName)),
                    item.unitPrice !== undefined ? item.unitPrice : 'N/A',
                    item.displayTotal !== undefined ? item.displayTotal : 'N/A',
                    ...(isSupplyWithDiscount ? [item.discount !== undefined ? item.discount : 'N/A'] : []),
                    sharedUtilsUpdated.safeString(item.formattedProcurementDate),
                    item.transportationExpenses !== undefined ? item.transportationExpenses : 'N/A',
                    item.gratuities !== undefined ? item.gratuities : 'N/A',
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
    }, [items, certificate, subtotal, certificateType, pageSize, getTranslatedLabel]);

    const onClose = useCallback(() => setShow(false), []);

    const handleDownload = useCallback(async () => {
        const buffer = await generateExcel();
        if (buffer) {
            const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            saveAs(blob, `SupplyCertificate_${certificate.certificateNumber}.xlsx`);
        }
    }, [generateExcel, certificate.certificateNumber]);

    const disabled = isSubmitting || isAddCertificateLoading || isUpdateCertificateLoading || isReceiveLoading;

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