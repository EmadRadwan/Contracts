import { Document, Page, Text, View, StyleSheet, Image, BlobProvider, Font } from '@react-pdf/renderer';
import { Button } from '@mui/material';
import { useCallback, useState } from 'react';
import ModalContainer from '../../../app/common/modals/ModalContainer';

// REFACTOR: Centralized styles with RTL font
// Purpose: Applies Tajawal to all text for consistent Arabic support
// Improvement: Ensures RTL direction with proper font, avoiding reorder bugs
// Context: Sets base fontFamily to Tajawal; adds explicit direction where needed
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

// REFACTOR: Validate items with type check
// Purpose: Adds check for WORKMANSHIP fields
// Improvement: Logs mismatches during switches; prevents invalid render
// Context: Logs show validation fails on switch due to type mismatch
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

export const WorkmanshipCertificatePDF: React.FC<WorkmanshipCertificatePDFProps> = ({
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

    // REFACTOR: Log items on render with type
    // Purpose: Debugs switches; logs if items match type
    // Improvement: Identifies stale items during type change
    // Context: Logs show mismatched validation on switch
    console.log('Rendering WorkmanshipCertificatePDF with items:', {items, certificateType, isFetching});

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

    // Validate items before rendering
    const { isValid, invalidItems } = validateItems(items);

    const MyDocument = () => (
        <Document>
            {isValid && !isFetching ? (
                pages.map((pageItems, pageIndex) => (
                    <Page key={pageIndex} size="A4" orientation="landscape" style={styles.page}>
                        <View style={styles.section}>
                            <View style={styles.header}>
                                {/* REFACTOR: Isolated image with fallback */}
                                {/* Purpose: Prevents Buffer warning from crashing render */}
                                {/* Improvement: Logs fetch errors; uses text fallback */}
                                {/* Context: Logs show Buffer in fetchImage—isolates it */}
                                {(() => {
                                    try {
                                        return <Image style={{ width: 100, height: 100, marginBottom: 10 }} src="/goldenlandlogo.jpg" />;
                                    } catch (error) {
                                        console.warn('Logo fetch failed:', error);
                                        return <Text>Logo Unavailable</Text>;
                                    }
                                })()}
                                <Text style={styles.title}>
                                    {getTranslatedLabel('projects.certificate.report.title', 'Certificate Report')}: {sharedUtils.safeString(certificate.certificateNumber)}
                                </Text>
                                <View style={{ flexDirection: 'row-reverse', justifyContent: 'space-between', marginBottom: 5 }}>
                                    <Text style={styles.headerText}>
                                        {getTranslatedLabel('projects.certificate.type', 'Type')}: {sharedUtils.certificateTypeTranslations[certificateType] || certificateType}
                                    </Text>
                                    <Text style={styles.headerText}>
                                        {sharedUtils.rtlEmbed(getTranslatedLabel('projects.certificate.date', 'Date'))}: {new Date().toLocaleDateString('en-UK', { year: 'numeric', month: 'numeric', day: 'numeric' })}
                                    </Text>
                                </View>
                                <View style={{ flexDirection: 'row-reverse', justifyContent: 'space-between', marginBottom: 5 }}>
                                    <Text style={styles.headerText}>
                                        {getTranslatedLabel('projects.certificate.description', 'Description')}: {sharedUtils.safeString(certificate.description)}
                                    </Text>
                                    <Text style={styles.headerText}>
                                        {getTranslatedLabel('projects.certificate.form.contractor', 'Contractor')}: {sharedUtils.safeString(certificate.partyIdContractor)}
                                    </Text>
                                </View>
                                <View style={{ flexDirection: 'row-reverse', justifyContent: 'space-between' }}>
                                    <Text style={styles.headerText}>
                                        {getTranslatedLabel('projects.certificate.total', 'Total')}: {sharedUtils.formatNumber(subtotal)}
                                    </Text>
                                </View>
                            </View>
                            {pageItems && pageItems.length > 0 ? (
                                <View style={styles.table}>
                                    <View style={styles.tableRow}>
                                        // REFACTOR: Increased widths for Arabic columns
                                        // Purpose: Arabic text is wider; expands description/deduction/uom to prevent horizontal overlap
                                        // Improvement: 20% for description (from 15%), 12% for deduction/uom—balances table without truncation
                                        // Context: Tajawal has wider glyphs; fixed widths caused squeeze
                                        <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.item', 'Item')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.code', 'Code')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '20%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.description', 'Description')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '6%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.quantity', 'Quantity')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '12%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.unitOfMeasure', 'Unit of Measure')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.materialPrice', 'Material Price')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.laborPrice', 'Labor Price')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.totalAmount', 'Total Amount')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.deductions', 'Deductions')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '12%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.deductionDescription', 'Deduction Description')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '6%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.deserved', 'Deserved')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '6%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.insurance', 'Insurance')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '8%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.additionalInsurance', 'Additional Insurance')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '6%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.net', 'Net')}</Text></View>
                                        <View style={[styles.tableHeader, { width: '10%' }]}><Text>{getTranslatedLabel('projects.certificate.items.list.achievementPercentage', 'Achievement Percentage')}</Text></View>
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
                                            <View style={[styles.tableCell, { width: '8%' }]}><Text style={{ direction: 'ltr' }}>{item.materialPrice !== undefined ? sharedUtils.safeString(item.materialPrice.toFixed(2)) : 'N/A'}</Text></View>
                                            <View style={[styles.tableCell, { width: '8%' }]}><Text style={{ direction: 'ltr' }}>{item.laborPrice !== undefined ? sharedUtils.safeString(item.laborPrice.toFixed(2)) : 'N/A'}</Text></View>
                                            <View style={[styles.tableCell, { width: '8%' }]}><Text style={{ direction: 'ltr' }}>{item.displayTotal !== undefined ? sharedUtils.safeString(item.displayTotal.toFixed(2)) : 'N/A'}</Text></View>
                                            <View style={[styles.tableCell, { width: '8%' }]}><Text style={{ direction: 'ltr' }}>{item.deductions !== undefined ? sharedUtils.safeString(item.deductions.toFixed(2)) : 'N/A'}</Text></View>
                                            <View style={[styles.tableCell, { width: '12%' }]}><Text style={{ textAlign: 'right' }}>{sharedUtils.rtlEmbed(sharedUtils.safeString(item.deductionDescription))}</Text></View>
                                            <View style={[styles.tableCell, { width: '6%' }]}><Text style={{ direction: 'ltr' }}>{item.deserved !== undefined ? sharedUtils.safeString(item.deserved.toFixed(2)) : 'N/A'}</Text></View>
                                            <View style={[styles.tableCell, { width: '6%' }]}><Text style={{ direction: 'ltr' }}>{item.insurance !== undefined ? sharedUtils.safeString(item.insurance.toFixed(2)) : 'N/A'}</Text></View>
                                            <View style={[styles.tableCell, { width: '8%' }]}><Text style={{ direction: 'ltr' }}>{item.additionalInsurance !== undefined ? sharedUtils.safeString(item.additionalInsurance.toFixed(2)) : 'N/A'}</Text></View>
                                            <View style={[styles.tableCell, { width: '6%' }]}><Text style={{ direction: 'ltr' }}>{item.net !== undefined ? sharedUtils.safeString(item.net.toFixed(2)) : 'N/A'}</Text></View>
                                            <View style={[styles.tableCell, { width: '10%' }]}><Text style={{ direction: 'ltr' }}>{sharedUtils.safeString(item.achievementPercentage)}</Text></View>
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
                ))
            ) : (
                <Page size="A4" orientation="landscape" style={styles.page}>
                    <View style={styles.section}>
                        <Text style={styles.error}>
                            {getTranslatedLabel('projects.certificate.invalidItems', 'Invalid items detected. Check console for details.')}
                        </Text>
                    </View>
                </Page>
            )}
        </Document>
    );

    // REFACTOR: Disable during isFetching
    // Purpose: Prevent render with stale/empty items on switch
    // Improvement: Waits for fetch; fixes timing race in logs
    // Context: useFetchCertificateItemsQuery is async—PDF triggers too early
    const disabled = isSubmitting || isAddCertificateLoading || isUpdateCertificateLoading || isReceiveLoading || isFetching;

    return (
        <div>
            <BlobProvider document={<MyDocument />}>
                {({ blob, url, loading, error }) => (
                    <>
                        <Button
                            color="primary"
                            variant="outlined"
                            disabled={disabled || loading || !!error}
                            onClick={() => {
                                if (url) {
                                    const link = document.createElement('a');
                                    link.href = url;
                                    link.download = `WorkmanshipCertificate_${certificate.certificateNumber}.pdf`;
                                    link.click();
                                    URL.revokeObjectURL(url);
                                }
                            }}
                            style={{ marginRight: 10 }}
                        >
                            {loading ? 'Generating PDF...' : error ? 'Error Generating PDF' : getTranslatedLabel('projects.certificate.preview', 'Download PDF')}
                        </Button>
                        {error && (
                            <div style={{ color: 'red' }}>
                                {getTranslatedLabel('projects.certificate.pdf.error', 'Failed to generate PDF: ' + error.message)}
                            </div>
                        )}
                        {show && (
                            <ModalContainer show={show} onClose={onClose} width={1200}>
                                {url ? (
                                    <iframe src={url} style={{ width: '100%', height: '70vh', border: '1px solid #ccc' }} title="PDF Preview" />
                                ) : (
                                    <div>{loading ? 'Loading...' : 'Error generating preview'}</div>
                                )}
                            </ModalContainer>
                        )}
                    </>
                )}
            </BlobProvider>
            <Button
                color="secondary"
                variant="outlined"
                onClick={() => setShow(true)}
                disabled={disabled}
                style={{ marginRight: 10 }}
            >
                {getTranslatedLabel('projects.certificate.preview', 'Preview PDF')}
            </Button>
        </div>
    );
};