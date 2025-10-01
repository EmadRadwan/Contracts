import { Document, Page, Text, View, StyleSheet, Image, BlobProvider, Font } from '@react-pdf/renderer';
import { Button } from '@mui/material';
import { useCallback, useState } from 'react';
import ModalContainer from '../../../app/common/modals/ModalContainer';

// REFACTOR: Centralized styles for consistency
// Purpose: Defines reusable styles for PDF layout
// Improvement: Maintains consistent RTL formatting; avoids duplication
const styles = StyleSheet.create({
    page: { flexDirection: 'column', padding: 20, fontFamily: 'Amiri', textDirection: 'rtl', writingMode: 'rl-tb' },
    section: { margin: 10, padding: 10 },
    header: { marginBottom: 15, borderBottomWidth: 1, borderBottomStyle: 'solid', paddingBottom: 10 },
    headerText: { fontSize: 10, textAlign: 'right', marginBottom: 5 },
    title: { fontSize: 20, textAlign: 'center', marginBottom: 10 },
    table: { flexDirection: 'column', width: '100%', borderWidth: 1, borderColor: '#bfbfbf', marginBottom: 15 },
    tableRow: { flexDirection: 'row-reverse', borderBottomWidth: 1, borderBottomColor: '#bfbfbf' },
    tableHeader: { backgroundColor: '#f0f0f0', fontSize: 10, fontWeight: 'bold', textAlign: 'center', padding: 5, borderLeftWidth: 1, borderLeftColor: '#bfbfbf' },
    // REFACTOR: Enhanced tableCell style for vertical expansion
    // Purpose: Enables text wrapping and vertical expansion for long Arabic text in narrow columns
    // Improvement: Adds flexWrap: 'wrap' to allow multi-line text; sets minHeight to 'auto' for dynamic row height; increases lineHeight to 1.5 for better spacing
    // Context: Without wrap, long text overflows horizontally; flexWrap breaks lines; auto height expands rows vertically; higher lineHeight prevents glyph stacking in Arabic
    tableCell: { fontSize: 9, textAlign: 'center', padding: 8, borderLeftWidth: 1, borderLeftColor: '#bfbfbf', lineHeight: 1.5, flexWrap: 'wrap', minHeight: 'auto' },
    noteSection: { marginTop: 15, padding: 10, borderTopWidth: 1, borderTopStyle: 'solid' },
    noteTitle: { fontSize: 10, fontWeight: 'bold', textAlign: 'right', marginBottom: 5 },
    noteText: { fontSize: 9, textAlign: 'right', marginBottom: 5 },
    error: { fontSize: 9, color: 'red', textAlign: 'right' },
});

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

// REFACTOR: Shared utilities for formatting
// Purpose: Centralizes formatting logic for numbers and strings
// Improvement: Ensures consistent output, prevents errors with undefined values
const sharedUtils = {
    formatNumber: (value: number | undefined, decimals: number = 2): string => {
        if (value === undefined || value === null) return 'N/A';
        return value.toLocaleString('en-US', { minimumFractionDigits: decimals, maximumFractionDigits: decimals });
    },
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

export const SupplyCertificatePDF: React.FC<SupplyCertificatePDFProps> = ({
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
    console.log('Rendering SupplyCertificatePDF with items:', items);

    // REFACTOR: Extracted pagination logic
    // Purpose: Splits items into pages for large datasets
    // Improvement: Simplifies rendering, improves readability
    const pages = [];
    for (let i = 0; i < items.length; i += pageSize) {
        pages.push(items.slice(i, i + pageSize));
    }

    // REFACTOR: Memoized onClose for performance
    // Purpose: Prevents unnecessary re-renders of modal close handler
    // Improvement: Optimizes component lifecycle
    const onClose = useCallback(() => setShow(false), []);

    const MyDocument = () => (
        <Document>
            {pages.map((pageItems, pageIndex) => (
                <Page key={pageIndex} size="A4" orientation="landscape" style={styles.page}>
                    <View style={styles.section}>
                        <View style={styles.header}>
                            <Image style={{ width: 100, height: 100, marginBottom: 10 }} src="/goldenlandlogo.jpg" />
                            <Text style={styles.title}>
                                {getTranslatedLabel('projects.certificate.report.title', 'Certificate Report')}: {sharedUtils.safeString(certificate.certificateNumber)}
                            </Text>
                            <View style={{ flexDirection: 'row-reverse', justifyContent: 'space-between', marginBottom: 5 }}>
                                <Text style={styles.headerText}>
                                    {getTranslatedLabel('projects.certificate.type', 'Type')}: {sharedUtils.certificateTypeTranslations[certificateType] || certificateType}
                                </Text>
                                <Text style={styles.headerText}>
                                    {getTranslatedLabel('projects.certificate.date', 'Date')}: {new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'numeric', day: 'numeric' })}
                                </Text>
                            </View>
                            <View style={{ flexDirection: 'row-reverse', justifyContent: 'space-between', marginBottom: 5 }}>
                                <Text style={styles.headerText}>
                                    {getTranslatedLabel('projects.certificate.description', 'Description')}: {sharedUtils.safeString(certificate.description)}
                                </Text>
                                <Text style={styles.headerText}>
                                    {getTranslatedLabel('projects.certificate.form.supplier', 'Supplier')}: {sharedUtils.safeString(certificate.partyIdSupplier)}
                                </Text>
                            </View>
                            <View style={{ flexDirection: 'row-reverse', justifyContent: 'space-between' }}>
                                <Text style={styles.headerText}>
                                    {getTranslatedLabel('projects.certificate.total', 'Total')}: {sharedUtils.formatNumber(subtotal)}
                                </Text>
                                <Text style={styles.headerText}>
                                    {getTranslatedLabel('projects.certificate.form.facility', 'Facility')}: {sharedUtils.safeString(certificate.facilityName)}
                                </Text>
                            </View>
                        </View>
                        {pageItems && pageItems.length > 0 ? (
                            <View style={styles.table}>
                                <View style={styles.tableRow}>
                                    // REFACTOR: Increased widths for Arabic columns
                                    // Purpose: Arabic text is wider; expands description/uom to prevent horizontal overlap
                                    // Improvement: 20% for description (from 15%), 12% for uom—balances table without truncation
                                    // Context: Tajawal has wider glyphs; fixed widths caused squeeze
                                    <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.item', 'Item')}</Text></View>
                                    <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.code', 'Code')}</Text></View>
                                    <View style={[styles.tableHeader, { width: '20%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.description', 'Description')}</Text></View>
                                    <View style={[styles.tableHeader, { width: '6%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.quantity', 'Quantity')}</Text></View>
                                    <View style={[styles.tableHeader, { width: '12%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.unitOfMeasure', 'Unit of Measure')}</Text></View>
                                    <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.unitPrice', 'Unit Price')}</Text></View>
                                    <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.totalAmount', 'Total Amount')}</Text></View>
                                    {isSupplyWithDiscount && (
                                        <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.discount', 'Discount')}</Text></View>
                                    )}
                                    <View style={[styles.tableHeader, { width: '10%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.procurementDate', 'Procurement Date')}</Text></View>
                                    <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.transportationExpenses', 'Transportation Expenses')}</Text></View>
                                    <View style={[styles.tableHeader, { width: '7%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.gratuities', 'Gratuities')}</Text></View>
                                </View>
                                {pageItems.map((item, itemIndex) => (
                                    <View key={`${pageIndex}-${itemIndex}`} style={styles.tableRow}>
                                        <View style={[styles.tableCell, { width: '8%' }]}>
                                            <Text>
                                                <Text style={{ textAlign: 'right' }}>{sharedUtils.rtlEmbed(sharedUtils.safeString(item.productName))}</Text>
                                                {item.isLastInGroup && item.productSubtotal !== undefined ? (
                                                    <Text style={{ direction: 'ltr' }}>{` (${sharedUtils.formatNumber(item.productSubtotal, 2)})`}</Text>
                                                ) : null}
                                            </Text>
                                        </View>
                                        <View style={[styles.tableCell, { width: '8%' }]}><Text>{sharedUtils.safeString(item.code)}</Text></View>
                                        <View style={[styles.tableCell, { width: '20%' }]}>
                                            // REFACTOR: Changed textAlign to 'right' for Arabic descriptions
                                            // Purpose: Aligns Arabic to right, preventing centering overlaps
                                            // Improvement: Arabic reads better right-aligned; reduces visual clutter in cells
                                            // Context: Centering caused shifts in mixed text; right align matches RTL flow
                                            <Text style={{ textAlign: 'right' }}>
                                                {sharedUtils.safeString(item.description)
                                                    .split('\n')
                                                    .filter(line => line.trim().length > 0)
                                                    .map((para, idx) => (
                                                        <Text key={idx} style={{ marginBottom: 2 }}>{sharedUtils.rtlEmbed(para)}</Text>
                                                    ))}
                                            </Text>
                                        </View>
                                        <View style={[styles.tableCell, { width: '6%' }]}><Text style={{ direction: 'ltr' }}>{item.quantity !== undefined ? sharedUtils.formatNumber(item.quantity, 0) : 'N/A'}</Text></View>
                                        <View style={[styles.tableCell, { width: '12%' }]}><Text style={{ textAlign: 'right' }}>{sharedUtils.rtlEmbed(sharedUtils.safeString(item.uomName))}</Text></View>
                                        <View style={[styles.tableCell, { width: '8%' }]}><Text style={{ direction: 'ltr' }}>{item.unitPrice !== undefined ? sharedUtils.safeString(item.unitPrice.toFixed(2)) : 'N/A'}</Text></View>
                                        <View style={[styles.tableCell, { width: '8%' }]}><Text style={{ direction: 'ltr' }}>{item.displayTotal !== undefined ? sharedUtils.safeString(item.displayTotal.toFixed(2)) : 'N/A'}</Text></View>
                                        {isSupplyWithDiscount && (
                                            <View style={[styles.tableCell, { width: '8%' }]}><Text style={{ direction: 'ltr' }}>{item.discount !== undefined ? sharedUtils.safeString(item.discount.toFixed(2)) : 'N/A'}</Text></View>
                                        )}
                                        <View style={[styles.tableCell, { width: '10%' }]}><Text style={{ direction: 'ltr' }}>{sharedUtils.safeString(item.formattedProcurementDate)}</Text></View>
                                        <View style={[styles.tableCell, { width: '8%' }]}><Text style={{ direction: 'ltr' }}>{item.transportationExpenses !== undefined ? sharedUtils.safeString(item.transportationExpenses.toFixed(2)) : 'N/A'}</Text></View>
                                        <View style={[styles.tableCell, { width: '7%' }]}><Text style={{ direction: 'ltr' }}>{item.gratuities !== undefined ? sharedUtils.safeString(item.gratuities.toFixed(2)) : 'N/A'}</Text></View>
                                    </View>
                                ))}
                            </View>
                        ) : (
                            <View style={styles.section}>
                                <Text style={styles.error}>{getTranslatedLabel('projects.certificate.items.list.noData', 'No items available')}</Text>
                            </View>
                        )}
                        {pageItems.some(item => item.mainItemDescription && item.mainItemDescription.trim()) && (
                            <View style={styles.noteSection}>
                                <Text style={styles.noteTitle}>{getTranslatedLabel('projects.certificate.items.mainDescription', 'Main Item Description')}</Text>
                                {pageItems.map((item, itemIndex) => (
                                    item.mainItemDescription && item.mainItemDescription.trim() && (
                                        <Text key={`${pageIndex}-${itemIndex}-main`} style={styles.noteText}>{sharedUtils.rtlEmbed(sharedUtils.safeString(item.mainItemDescription))}</Text>
                                    )
                                ))}
                            </View>
                        )}
                        {pageItems.some(item => item.discountNote && item.discountNote.trim()) && (
                            <View style={styles.noteSection}>
                                <Text style={styles.noteTitle}>{getTranslatedLabel('projects.certificate.items.discountNote', 'Discount Description Note')}</Text>
                                {pageItems.map((item, itemIndex) => (
                                    item.discountNote && item.discountNote.trim() && (
                                        <Text key={`${pageIndex}-${itemIndex}-discount`} style={styles.noteText}>{sharedUtils.rtlEmbed(sharedUtils.safeString(item.discountNote))}</Text>
                                    )
                                ))}
                            </View>
                        )}
                    </View>
                </Page>
            ))}
        </Document>
    );

    // REFACTOR: Use BlobProvider instead of PDFViewer
    // Purpose: Generates PDF as Blob for download, avoiding iframe render issues
    // Improvement: Bypasses render queue overlaps and bidi errors on type switches; simplifies error handling
    return (
        <BlobProvider document={<MyDocument />}>
            {({ blob, url, loading, error }) => {
                console.log('BlobProvider state:', { loading, error }); // Log to confirm error timing
                return (
                    <Button
                        color="primary"
                        variant="outlined"
                        disabled={isSubmitting || isAddCertificateLoading || isUpdateCertificateLoading || isReceiveLoading || loading || !!error}
                        onClick={() => {
                            if (url) {
                                const link = document.createElement('a');
                                link.href = url;
                                link.download = `SupplyCertificate_${certificate.certificateNumber}.pdf`;
                                link.click();
                                URL.revokeObjectURL(url);
                            }
                        }}
                    >
                        {loading ? 'Generating PDF...' : error ? `Error: ${error.message}` : getTranslatedLabel('projects.certificate.preview', 'Download PDF')}
                    </Button>
                );
            }}
        </BlobProvider>
    );
};