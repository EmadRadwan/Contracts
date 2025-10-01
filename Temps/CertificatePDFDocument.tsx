import { Document, Page, Text, View, StyleSheet, PDFDownloadLink, PDFViewer } from '@react-pdf/renderer';
import { Button } from '@mui/material';
import {useCallback, useState} from "react";
import ModalContainer from "../../../app/common/modals/ModalContainer";

// Purpose: Maintains consistent RTL layout for Arabic text, matching client’s Word document
// Improvement: Simplified styles for clarity and consistency
const styles = StyleSheet.create({
    page: {
        flexDirection: 'column',
        padding: 20,
        fontFamily: 'DejaVuSans',
        textDirection: 'rtl',
        writingMode: 'rl-tb',
    },
    section: {
        margin: 10,
        padding: 10,
    },
    header: {
        marginBottom: 15,
        borderBottomWidth: 1,
        borderBottomStyle: 'solid',
        paddingBottom: 10,
    },
    headerText: {
        fontSize: 10,
        textAlign: 'right',
        marginBottom: 5,
    },
    title: {
        fontSize: 20,
        textAlign: 'right',
        marginBottom: 10,
    },
    itemSection: {
        marginBottom: 15,
        padding: 10,
        borderBottomWidth: 1,
        borderBottomStyle: 'solid',
    },
    itemTitle: {
        fontSize: 12,
        fontWeight: 'bold',
        textAlign: 'right',
        marginBottom: 8,
    },
    itemText: {
        fontSize: 9,
        textAlign: 'right',
        marginBottom: 5,
    },
    noteSection: {
        marginTop: 15,
        padding: 10,
        borderTopWidth: 1,
        borderTopStyle: 'solid',
    },
    noteTitle: {
        fontSize: 10,
        fontWeight: 'bold',
        textAlign: 'right',
        marginBottom: 5,
    },
    noteText: {
        fontSize: 9,
        textAlign: 'right',
        marginBottom: 5,
    },
    error: {
        fontSize: 9,
        color: 'red',
        textAlign: 'right',
    },
    viewer: {
        width: '100%',
        height: '70vh', // Adjusted for modal display
        border: '1px solid #ccc',
    },
});

interface CertificatePDFDocumentProps {
    certificate: {
        certificateNumber: string;
        projectName?: string;
        partyIdContractor?: string;
        partyIdSupplier?: string;
        estimatedStartDate?: string;
        estimatedCompletionDate?: string;
        description?: string;
        status?: string;
        facilityName?: string;
    };
    items: {
        workEffortId?: string;
        code?: string;
        productName?: string;
        description?: string;
        quantity?: number;
        unitOfMeasure?: string;
        materialPrice?: number;
        laborPrice?: number;
        displayTotal?: number;
        deductions?: number;
        deductionDescription?: string;
        deserved?: number;
        insurance?: number;
        additionalInsurance?: number;
        net?: number;
        achievementPercentage?: string;
        unitPrice?: number;
        discount?: number;
        formattedProcurementDate?: string;
        transportationExpenses?: number;
        gratuities?: number;
        isLastInGroup?: boolean;
        productSubtotal?: number;
        mainItemDescription?: string;
        discountNote?: string;
    }[];
    getTranslatedLabel: (key: string, defaultValue: string) => string;
    subtotal: number;
    certificateType: string;
    isSubmitting: boolean;
    isAddCertificateLoading: boolean;
    isUpdateCertificateLoading: boolean;
    isReceiveLoading: boolean;
    certificateNumber: string;
    pageSize?: number;
}

const CertificatePDFDocument: React.FC<CertificatePDFDocumentProps> = ({
                                                                           certificate,
                                                                           items,
                                                                           getTranslatedLabel,
                                                                           subtotal,
                                                                           certificateType,
                                                                           isSubmitting,
                                                                           isAddCertificateLoading,
                                                                           isUpdateCertificateLoading,
                                                                           isReceiveLoading,
                                                                           certificateNumber,
                                                                           pageSize = 15,
                                                                       }) => {
    const [show, setShow] = useState(false);

    // Purpose: Memoizes the onClose function for consistent modal behavior
    // Improvement: Prevents unnecessary re-renders and aligns with CertificateItemsList
    const onClose = useCallback(() => {
        setShow(false);
    }, []);
    
    // Purpose: Ensures items are split across pages without breaking list structure
    // Improvement: Maintains readability for large item sets
    const pages = [];
    for (let i = 0; i < items.length; i += pageSize) {
        pages.push(items.slice(i, i + pageSize));
    }

    // Purpose: Defines which fields to display based on certificateType
    // Improvement: Clearer logic without intermediate flags
    const isSupplyWithDiscount = certificateType === 'SUPPLY_PROCUREMENT_CERTIFICATE';
    const isSupplyCertificate = ['SUPPLY_PROCUREMENT_CERTIFICATE', 'COMPANY_SUPPLY_SALE_CERTIFICATE'].includes(certificateType);
    const isWorkmanshipCertificate = certificateType === 'WORKMANSHIP_CONTRACTING_CERTIFICATE';

    // Purpose: Selects contractor or supplier based on certificateType
    // Improvement: More explicit and maintainable
    const partyField = isWorkmanshipCertificate ? certificate.partyIdContractor ?? 'N/A' : certificate.partyIdSupplier ?? 'N/A';
    const partyLabelKey = isWorkmanshipCertificate ? 'projects.certificate.form.contractor' : 'projects.certificate.form.supplier';
    const partyLabelDefault = isWorkmanshipCertificate ? 'Contractor' : 'Supplier';

    const MyDocument = () => (
        <Document>
            {pages.map((pageItems, pageIndex) => (
                <Page key={pageIndex} size="A4" orientation="landscape" style={styles.page}>
                    <View style={styles.section}>
                        {/* Header Section */}
                        <View style={styles.header}>
                            <Text style={styles.title}>
                                {getTranslatedLabel('projects.certificate.report.title', 'Certificate Report')}: {certificate.certificateNumber}
                            </Text>
                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                <Text style={[styles.headerText, { textAlign: 'right' }]}>
                                    {getTranslatedLabel('projects.certificate.type', 'Type')}
                                </Text>
                                <Text style={styles.headerText}>:</Text>
                                <Text style={[styles.headerText, { textAlign: 'left' }]}>{certificateType}</Text>
                            </View>
                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                <Text style={[styles.headerText, { textAlign: 'right' }]}>
                                    {getTranslatedLabel('projects.certificate.date', 'Date')}
                                </Text>
                                <Text style={styles.headerText}>:</Text>
                                <Text style={[styles.headerText, { textAlign: 'left' }]}>
                                    {new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'numeric', day: 'numeric' })}
                                </Text>
                            </View>
                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                <Text style={[styles.headerText, { textAlign: 'right' }]}>
                                    {getTranslatedLabel('projects.certificate.description', 'Description')}
                                </Text>
                                <Text style={styles.headerText}>:</Text>
                                <Text style={[styles.headerText, { textAlign: 'left' }]}>{certificate.description ?? 'N/A'}</Text>
                            </View>
                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                <Text style={[styles.headerText, { textAlign: 'right' }]}>
                                    {getTranslatedLabel(partyLabelKey, partyLabelDefault)}
                                </Text>
                                <Text style={styles.headerText}>:</Text>
                                <Text style={[styles.headerText, { textAlign: 'left' }]}>{partyField}</Text>
                            </View>
                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                <Text style={[styles.headerText, { textAlign: 'right' }]}>
                                    {getTranslatedLabel('projects.certificate.total', 'Total')}
                                </Text>
                                <Text style={styles.headerText}>:</Text>
                                <Text style={[styles.headerText, { textAlign: 'left' }]}>{subtotal.toFixed(2)}</Text>
                            </View>
                            {isSupplyCertificate && (
                                <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                    <Text style={[styles.headerText, { textAlign: 'right' }]}>
                                        {getTranslatedLabel('projects.certificate.form.facility', 'Facility')}
                                    </Text>
                                    <Text style={styles.headerText}>:</Text>
                                    <Text style={[styles.headerText, { textAlign: 'left' }]}>{certificate.facilityName ?? 'N/A'}</Text>
                                </View>
                            )}
                        </View>

                        {/* Purpose: Adds materialPrice, laborPrice, deductions, deductionDescription, deserved, insurance, additionalInsurance, net, and achievementPercentage */}
                        {/* Improvement: Aligns with WorkmanshipContractingForm and displayCertificateItemsSelector calculations */}
                        {pageItems && pageItems.length > 0 ? (
                            pageItems.map((item, itemIndex) => (
                                <View key={`${pageIndex}-${itemIndex}`} style={styles.itemSection}>
                                    <Text style={styles.itemTitle}>
                                        {getTranslatedLabel('projects.certificate.items.list.item', 'Item')} {itemIndex + 1}: {item.productName ?? 'N/A'}
                                        {item.isLastInGroup && item.productSubtotal !== undefined ? ` (${getTranslatedLabel('projects.certificate.items.list.productSubtotal', 'Subtotal')}: ${item.productSubtotal?.toFixed(2)})` : ''}
                                    </Text>
                                    <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                        <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                            {getTranslatedLabel('projects.certificate.items.list.code', 'Code')}
                                        </Text>
                                        <Text style={styles.itemText}>:</Text>
                                        <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.code ?? 'N/A'}</Text>
                                    </View>
                                    <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                        <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                            {getTranslatedLabel('projects.certificate.items.list.description', 'Description')}
                                        </Text>
                                        <Text style={styles.itemText}>:</Text>
                                        <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.description ?? 'N/A'}</Text>
                                    </View>
                                    <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                        <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                            {getTranslatedLabel('projects.certificate.items.list.quantity', 'Quantity')}
                                        </Text>
                                        <Text style={styles.itemText}>:</Text>
                                        <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.quantity ?? 'N/A'}</Text>
                                    </View>
                                    <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                        <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                            {getTranslatedLabel('projects.certificate.items.list.unitOfMeasure', 'Unit of Measure')}
                                        </Text>
                                        <Text style={styles.itemText}>:</Text>
                                        <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.unitOfMeasure ?? 'N/A'}</Text>
                                    </View>
                                    {isSupplyCertificate && (
                                        <>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.unitPrice', 'Unit Price')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.unitPrice?.toFixed(2) ?? 'N/A'}</Text>
                                            </View>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.totalAmount', 'Total Amount')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.displayTotal?.toFixed(2) ?? 'N/A'}</Text>
                                            </View>
                                            {isSupplyWithDiscount && (
                                                <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                    <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                        {getTranslatedLabel('projects.certificate.items.list.discount', 'Discount')}
                                                    </Text>
                                                    <Text style={styles.itemText}>:</Text>
                                                    <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.discount?.toFixed(2) ?? 'N/A'}</Text>
                                                </View>
                                            )}
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.procurementDate', 'Procurement Date')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.formattedProcurementDate ?? 'N/A'}</Text>
                                            </View>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.transportationExpenses', 'Transportation Expenses')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.transportationExpenses?.toFixed(2) ?? 'N/A'}</Text>
                                            </View>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.gratuities', 'Gratuities')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.gratuities?.toFixed(2) ?? 'N/A'}</Text>
                                            </View>
                                        </>
                                    )}
                                    {isWorkmanshipCertificate && (
                                        <>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.materialPrice', 'Material Price')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.materialPrice?.toFixed(2) ?? 'N/A'}</Text>
                                            </View>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.laborPrice', 'Labor Price')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.laborPrice?.toFixed(2) ?? 'N/A'}</Text>
                                            </View>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.totalAmount', 'Total Amount')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.displayTotal?.toFixed(2) ?? 'N/A'}</Text>
                                            </View>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.deductions', 'Deductions')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.deductions?.toFixed(2) ?? 'N/A'}</Text>
                                            </View>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.deductionDescription', 'Deduction Description')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.deductionDescription ?? 'N/A'}</Text>
                                            </View>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.deserved', 'Deserved')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.deserved?.toFixed(2) ?? 'N/A'}</Text>
                                            </View>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.insurance', 'Insurance')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.insurance?.toFixed(2) ?? 'N/A'}</Text>
                                            </View>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.additionalInsurance', 'Additional Insurance')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.additionalInsurance?.toFixed(2) ?? 'N/A'}</Text>
                                            </View>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.net', 'Net')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.net?.toFixed(2) ?? 'N/A'}</Text>
                                            </View>
                                            <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                                <Text style={[styles.itemText, { textAlign: 'right' }]}>
                                                    {getTranslatedLabel('projects.certificate.items.list.achievementPercentage', 'Achievement Percentage')}
                                                </Text>
                                                <Text style={styles.itemText}>:</Text>
                                                <Text style={[styles.itemText, { textAlign: 'left' }]}>{item.achievementPercentage ?? 'N/A'}</Text>
                                            </View>
                                        </>
                                    )}
                                </View>
                            ))
                        ) : (
                            <View style={styles.itemSection}>
                                <Text style={styles.error}>
                                    {getTranslatedLabel('projects.certificate.items.list.noData', 'No items available')}
                                </Text>
                            </View>
                        )}

                        {pageItems.some(item => item.mainItemDescription) && (
                            <View style={styles.noteSection}>
                                <Text style={styles.noteTitle}>
                                    {getTranslatedLabel('projects.certificate.items.mainDescription', 'Main Item Description')}
                                </Text>
                                {pageItems.map((item, itemIndex) => (
                                    item.mainItemDescription && (
                                        <Text key={`${pageIndex}-${itemIndex}-main`} style={styles.noteText}>
                                            {item.mainItemDescription}
                                        </Text>
                                    )
                                ))}
                            </View>
                        )}
                        {pageItems.some(item => item.discountNote) && (
                            <View style={styles.noteSection}>
                                <Text style={styles.noteTitle}>
                                    {getTranslatedLabel('projects.certificate.items.discountNote', 'Discount Description Note')}
                                </Text>
                                {pageItems.map((item, itemIndex) => (
                                    item.discountNote && (
                                        <Text key={`${pageIndex}-${itemIndex}-discount`} style={styles.noteText}>
                                            {item.discountNote}
                                        </Text>
                                    )
                                ))}
                            </View>
                        )}
                    </View>
                </Page>
            ))}
        </Document>
    );

   

    return (
        <div>
            <Button
                color="primary"
                variant="outlined"
                onClick={() => setShow(true)}
                disabled={isSubmitting || isAddCertificateLoading || isUpdateCertificateLoading || isReceiveLoading}
                aria-label="Preview PDF"
                style={{ marginRight: 10 }}
            >
                {getTranslatedLabel('projects.certificate.preview', 'Preview PDF')}
            </Button>

            {/* Purpose: Renders the PDF preview in a modal, aligning with existing UI patterns */}
            {/* Improvement: Provides a controlled, dismissible preview experience */}
            {show && (
                <ModalContainer show={show} onClose={onClose} width={1200}>
                    <PDFViewer style={styles.viewer}>
                        <MyDocument />
                    </PDFViewer>
                </ModalContainer>
            )}
        </div>
    );
};

export default CertificatePDFDocument;