import { Document, Page, Text, View, StyleSheet, PDFDownloadLink, Font } from '@react-pdf/renderer';
import { Button } from '@mui/material';

// REFACTOR: Register font for PDF rendering
// Purpose: Ensure consistent font usage for Arabic text in PDF
// Improvement: Moved font registration to the component file for encapsulation
Font.register({ family: 'Amiri', src: '/fonts/Amiri-Regular.ttf' });

// REFACTOR: Define styles for PDF document
// Purpose: Centralize styling for the PDF to ensure consistent layout
// Improvement: Extracted styles to the new component for better organization
const styles = StyleSheet.create({
    page: {
        flexDirection: 'column',
        backgroundColor: '#E4E4E4',
        padding: 20,
        fontFamily: 'Amiri',
        textDirection: 'rtl',
        writingMode: 'rl-tb',
    },
    section: {
        margin: 10,
        padding: 10,
    },
    table: {
        display: 'table',
        width: '100%',
        borderStyle: 'solid',
        borderWidth: 1,
        borderRightWidth: 0,
        borderBottomWidth: 0,
    },
    tableRow: {
        flexDirection: 'row-reverse',
    },
    tableCol: {
        width: '120pt',
        borderStyle: 'solid',
        borderWidth: 1,
        borderLeftWidth: 0,
        borderTopWidth: 0,
        padding: 10,
    },
    tableColWide: {
        width: '180pt',
        borderStyle: 'solid',
        borderWidth: 1,
        borderLeftWidth: 0,
        borderTopWidth: 0,
        padding: 10,
    },
    tableCell: {
        margin: 5,
        fontSize: 8,
        textAlign: 'right',
        wrap: true,
        maxLines: 3,
    },
    title: {
        fontSize: 20,
        marginBottom: 15,
        textAlign: 'right',
    },
    subtitle: {
        fontSize: 10,
        marginBottom: 15,
        textAlign: 'right',
    },
    error: {
        fontSize: 10,
        color: 'red',
        textAlign: 'right',
    },
});

interface CertificatePDFDocumentProps {
    certificate: any;
    items: any[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    subtotal: number;
    isGrouped: boolean;
    isSubmitting: boolean;
    isAddCertificateLoading: boolean;
    isUpdateCertificateLoading: boolean;
    isReceiveLoading: boolean;
    certificateNumber: string;
}

// REFACTOR: Create reusable PDF document component
// Purpose: Encapsulate PDF generation logic for modularity
// Improvement: Allows reuse across different forms and maintains single responsibility
const CertificatePDFDocument: React.FC<CertificatePDFDocumentProps> = ({
                                                                           certificate,
                                                                           items,
                                                                           getTranslatedLabel,
                                                                           subtotal,
                                                                           isGrouped,
                                                                           isSubmitting,
                                                                           isAddCertificateLoading,
                                                                           isUpdateCertificateLoading,
                                                                           isReceiveLoading,
                                                                           certificateNumber,
                                                                       }) => {
    const pageSize = 15;
    const pages = [];
    for (let i = 0; i < items.length; i += pageSize) {
        pages.push(items.slice(i, i + pageSize));
    }

    const isSupplyWithDiscount = [
        'SUPPLY_PROCUREMENT_CERTIFICATE',
        'EXTERNAL_SUPPLY_SALE_CERTIFICATE',
    ].includes(certificate.certificateCategory || '');
    const isSupplyWithoutDiscount = [
        'COMPANY_SUPPLY_SALE_CERTIFICATE',
        'CONTRACTOR_PURCHASE_CERTIFICATE',
    ].includes(certificate.certificateCategory || '');

    // REFACTOR: Move MyDocument into the component
    // Purpose: Keep PDF rendering logic self-contained
    // Improvement: Reduces clutter in the main form component
    const MyDocument = () => (
        <Document>
            {pages.map((pageItems, index) => (
                <Page key={index} size="A4" orientation="landscape" style={styles.page}>
                    <View style={styles.section}>
                        <Text style={styles.title}>
                            {getTranslatedLabel('certificate.report.title', 'Certificate Report')}: {certificate.certificateNumber}
                        </Text>
                        <Text style={styles.subtitle}>
                            {getTranslatedLabel('certificate.project', 'Project')}: {certificate.projectName}
                        </Text>
                        <Text style={styles.subtitle}>
                            {getTranslatedLabel('certificate.supplierOrContractor', 'Supplier/Contractor')}:{' '}
                            {certificate.partyIdSupplier || certificate.partyIdContractor}
                        </Text>
                        <Text style={styles.subtitle}>
                            {getTranslatedLabel('certificate.dates', 'Dates')}: {certificate.estimatedStartDate} to{' '}
                            {certificate.estimatedCompletionDate}
                        </Text>
                        <Text style={styles.subtitle}>
                            {getTranslatedLabel('certificate.description', 'Description')}: {certificate.description}
                        </Text>
                        <Text style={styles.subtitle}>
                            {getTranslatedLabel('certificate.status', 'Status')}: {certificate.status}
                        </Text>
                        <Text style={styles.subtitle}>
                            {getTranslatedLabel('certificate.facility', 'Facility')}: {certificate.facilityName}
                        </Text>
                        <Text style={styles.subtitle}>
                            {getTranslatedLabel('certificate.total', 'Total')}: {subtotal.toFixed(2)}
                        </Text>
                        <Text style={styles.subtitle}>
                            {getTranslatedLabel('product.products.list.generatedOn', 'Generated on')}:{' '}
                            {new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'numeric', day: 'numeric' })}
                        </Text>
                        <View style={styles.table}>
                            <View style={styles.tableRow}>
                                <View style={styles.tableCol}>
                                    <Text style={styles.tableCell}>{getTranslatedLabel('certificate.items.list.code', 'Code')}</Text>
                                </View>
                                <View style={styles.tableColWide}>
                                    <Text style={styles.tableCell}>
                                        {getTranslatedLabel('certificate.items.list.description', 'Product')}
                                    </Text>
                                </View>
                                <View style={styles.tableColWide}>
                                    <Text style={styles.tableCell}>
                                        {getTranslatedLabel('certificate.items.list.description', 'Description')}
                                    </Text>
                                </View>
                                <View style={styles.tableCol}>
                                    <Text style={styles.tableCell}>
                                        {getTranslatedLabel('certificate.items.list.quantity', 'Quantity')}
                                    </Text>
                                </View>
                                {isGrouped ? (
                                    <>
                                        <View style={styles.tableCol}>
                                            <Text style={styles.tableCell}>
                                                {getTranslatedLabel('certificate.items.list.materialPrice', 'Material Price')}
                                            </Text>
                                        </View>
                                        <View style={styles.tableCol}>
                                            <Text style={styles.tableCell}>
                                                {getTranslatedLabel('certificate.items.list.laborPrice', 'Labor Price')}
                                            </Text>
                                        </View>
                                        <View style={styles.tableCol}>
                                            <Text style={styles.tableCell}>
                                                {getTranslatedLabel('certificate.items.list.totalAmount', 'Total Amount')}
                                            </Text>
                                        </View>
                                        <View style={styles.tableCol}>
                                            <Text style={styles.tableCell}>
                                                {getTranslatedLabel('certificate.items.list.deductions', 'Deductions')}
                                            </Text>
                                        </View>
                                        <View style={styles.tableCol}>
                                            <Text style={styles.tableCell}>
                                                {getTranslatedLabel('certificate.items.list.deserved', 'Deserved')}
                                            </Text>
                                        </View>
                                        <View style={styles.tableCol}>
                                            <Text style={styles.tableCell}>
                                                {getTranslatedLabel('certificate.items.list.insurance', 'Insurance')}
                                            </Text>
                                        </View>
                                        <View style={styles.tableCol}>
                                            <Text style={styles.tableCell}>
                                                {getTranslatedLabel('certificate.items.list.additionalInsurance', 'Additional Insurance')}
                                            </Text>
                                        </View>
                                        <View style={styles.tableCol}>
                                            <Text style={styles.tableCell}>
                                                {getTranslatedLabel('certificate.items.list.net', 'Net')}
                                            </Text>
                                        </View>
                                        <View style={styles.tableCol}>
                                            <Text style={styles.tableCell}>
                                                {getTranslatedLabel('certificate.items.list.achievementPercentage', 'Achievement %')}
                                            </Text>
                                        </View>
                                    </>
                                ) : (
                                    <>
                                        <View style={styles.tableCol}>
                                            <Text style={styles.tableCell}>
                                                {getTranslatedLabel('certificate.items.list.unitPrice', 'Unit Price')}
                                            </Text>
                                        </View>
                                        <View style={styles.tableCol}>
                                            <Text style={styles.tableCell}>
                                                {getTranslatedLabel('certificate.items.list.totalAmount', 'Total Amount')}
                                            </Text>
                                        </View>
                                        {isSupplyWithDiscount && (
                                            <>
                                                <View style={styles.tableCol}>
                                                    <Text style={styles.tableCell}>
                                                        {getTranslatedLabel('certificate.items.list.discount', 'Discount')}
                                                    </Text>
                                                </View>
                                                <View style={styles.tableCol}>
                                                    <Text style={styles.tableCell}>
                                                        {getTranslatedLabel('certificate.items.list.procurementDate', 'Procurement Date')}
                                                    </Text>
                                                </View>
                                                <View style={styles.tableCol}>
                                                    <Text style={styles.tableCell}>
                                                        {getTranslatedLabel('certificate.items.list.transportationExpenses', 'Transportation Expenses')}
                                                    </Text>
                                                </View>
                                                <View style={styles.tableCol}>
                                                    <Text style={styles.tableCell}>
                                                        {getTranslatedLabel('certificate.items.list.gratuities', 'Gratuities')}
                                                    </Text>
                                                </View>
                                            </>
                                        )}
                                        {isSupplyWithoutDiscount && (
                                            <>
                                                <View style={styles.tableCol}>
                                                    <Text style={styles.tableCell}>
                                                        {getTranslatedLabel('certificate.items.list.procurementDate', 'Procurement Date')}
                                                    </Text>
                                                </View>
                                                <View style={styles.tableCol}>
                                                    <Text style={styles.tableCell}>
                                                        {getTranslatedLabel('certificate.items.list.transportationExpenses', 'Transportation Expenses')}
                                                    </Text>
                                                </View>
                                                <View style={styles.tableCol}>
                                                    <Text style={styles.tableCell}>
                                                        {getTranslatedLabel('certificate.items.list.gratuities', 'Gratuities')}
                                                    </Text>
                                                </View>
                                            </>
                                        )}
                                    </>
                                )}
                            </View>
                            {pageItems && pageItems.length > 0 ? (
                                pageItems.map((item) => {
                                    const codeText = item.isLastInGroup
                                        ? `${item.code || 'N/A'} (${getTranslatedLabel('certificate.items.list.productSubtotal', 'Subtotal')}: ${
                                            item.productSubtotal || 'N/A'
                                        })`
                                        : item.code || 'N/A';
                                    return (
                                        <View key={item.workEffortId || Math.random()} style={styles.tableRow}>
                                            <View style={styles.tableCol}>
                                                <Text style={styles.tableCell}>{codeText}</Text>
                                            </View>
                                            <View style={styles.tableColWide}>
                                                <Text style={styles.tableCell}>{item.productName || 'N/A'}</Text>
                                            </View>
                                            <View style={styles.tableColWide}>
                                                <Text style={styles.tableCell}>{item.description || 'N/A'}</Text>
                                            </View>
                                            <View style={styles.tableCol}>
                                                <Text style={styles.tableCell}>{item.quantity || 'N/A'}</Text>
                                            </View>
                                            {isGrouped ? (
                                                <>
                                                    <View style={styles.tableCol}>
                                                        <Text style={styles.tableCell}>{item.materialPrice?.toFixed(2) || 'N/A'}</Text>
                                                    </View>
                                                    <View style={styles.tableCol}>
                                                        <Text style={styles.tableCell}>{item.laborPrice?.toFixed(2) || 'N/A'}</Text>
                                                    </View>
                                                    <View style={styles.tableCol}>
                                                        <Text style={styles.tableCell}>{item.displayTotal?.toFixed(2) || 'N/A'}</Text>
                                                    </View>
                                                    <View style={styles.tableCol}>
                                                        <Text style={styles.tableCell}>{item.deductions?.toFixed(2) || 'N/A'}</Text>
                                                    </View>
                                                    <View style={styles.tableCol}>
                                                        <Text style={styles.tableCell}>{item.deserved?.toFixed(2) || 'N/A'}</Text>
                                                    </View>
                                                    <View style={styles.tableCol}>
                                                        <Text style={styles.tableCell}>{item.insurance?.toFixed(2) || 'N/A'}</Text>
                                                    </View>
                                                    <View style={styles.tableCol}>
                                                        <Text style={styles.tableCell}>{item.additionalInsurance?.toFixed(2) || 'N/A'}</Text>
                                                    </View>
                                                    <View style={styles.tableCol}>
                                                        <Text style={styles.tableCell}>{item.net?.toFixed(2) || 'N/A'}</Text>
                                                    </View>
                                                    <View style={styles.tableCol}>
                                                        <Text style={styles.tableCell}>{item.achievementPercentage || 'N/A'}</Text>
                                                    </View>
                                                </>
                                            ) : (
                                                <>
                                                    <View style={styles.tableCol}>
                                                        <Text style={styles.tableCell}>{item.unitPrice?.toFixed(2) || 'N/A'}</Text>
                                                    </View>
                                                    <View style={styles.tableCol}>
                                                        <Text style={styles.tableCell}>{item.displayTotal?.toFixed(2) || 'N/A'}</Text>
                                                    </View>
                                                    {isSupplyWithDiscount && (
                                                        <>
                                                            <View style={styles.tableCol}>
                                                                <Text style={styles.tableCell}>{item.discount?.toFixed(2) || 'N/A'}</Text>
                                                            </View>
                                                            <View style={styles.tableCol}>
                                                                <Text style={styles.tableCell}>{item.formattedProcurementDate || 'N/A'}</Text>
                                                            </View>
                                                            <View style={styles.tableCol}>
                                                                <Text style={styles.tableCell}>{item.transportationExpenses?.toFixed(2) || 'N/A'}</Text>
                                                            </View>
                                                            <View style={styles.tableCol}>
                                                                <Text style={styles.tableCell}>{item.gratuities?.toFixed(2) || 'N/A'}</Text>
                                                            </View>
                                                        </>
                                                    )}
                                                    {isSupplyWithoutDiscount && (
                                                        <>
                                                            <View style={styles.tableCol}>
                                                                <Text style={styles.tableCell}>{item.formattedProcurementDate || 'N/A'}</Text>
                                                            </View>
                                                            <View style={styles.tableCol}>
                                                                <Text style={styles.tableCell}>{item.transportationExpenses?.toFixed(2) || 'N/A'}</Text>
                                                            </View>
                                                            <View style={styles.tableCol}>
                                                                <Text style={styles.tableCell}>{item.gratuities?.toFixed(2) || 'N/A'}</Text>
                                                            </View>
                                                        </>
                                                    )}
                                                </>
                                            )}
                                        </View>
                                    );
                                })
                            ) : (
                                <View style={styles.tableRow}>
                                    <View style={styles.tableCol}>
                                        <Text style={styles.error}>
                                            {getTranslatedLabel('certificate.items.list.noData', 'No items available')}
                                        </Text>
                                    </View>
                                </View>
                            )}
                        </View>
                    </View>
                </Page>
            ))}
        </Document>
    );

    return (
        <PDFDownloadLink
            document={<MyDocument />}
            fileName={`Certificate_${certificateNumber}.pdf`}
        >
            {({ loading }) => (
                <Button
                    color="primary"
                    variant="outlined"
                    disabled={isSubmitting || isAddCertificateLoading || isUpdateCertificateLoading || isReceiveLoading || loading}
                >
                    {loading
                        ? getTranslatedLabel('certificate.generating', 'Generating PDF...')
                        : getTranslatedLabel('certificate.export', 'Export to PDF')}
                </Button>
            )}
        </PDFDownloadLink>
    );
};

export default CertificatePDFDocument;