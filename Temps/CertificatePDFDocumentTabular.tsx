import { Document, Page, Text, View, StyleSheet, Image, PDFViewer } from '@react-pdf/renderer';
import { Button } from '@mui/material';
import { useCallback, useEffect, useState } from "react";
import ModalContainer from "../../../app/common/modals/ModalContainer";

// REFACTOR: Extracted styles to a shared constant to avoid duplication across components
// Improves maintainability by centralizing style definitions
const styles = StyleSheet.create({
  page: { flexDirection: 'column', padding: 20, fontFamily: 'DejaVuSans', textDirection: 'rtl', writingMode: 'rl-tb' },
  section: { margin: 10, padding: 10 },
  header: { marginBottom: 15, borderBottomWidth: 1, borderBottomStyle: 'solid', paddingBottom: 10 },
  headerText: { fontSize: 10, textAlign: 'right', marginBottom: 5 },
  title: { fontSize: 20, textAlign: 'center', marginBottom: 10 },
  table: { flexDirection: 'column', width: '100%', borderWidth: 1, borderColor: '#bfbfbf', marginBottom: 15 },
  tableRow: { flexDirection: 'row-reverse', borderBottomWidth: 1, borderBottomColor: '#bfbfbf' },
  tableHeader: { backgroundColor: '#f0f0f0', fontSize: 10, fontWeight: 'bold', textAlign: 'center', padding: 5, borderLeftWidth: 1, borderLeftColor: '#bfbfbf' },
  tableCell: { fontSize: 9, textAlign: 'center', padding: 5, borderLeftWidth: 1, borderLeftColor: '#bfbfbf' },
  noteSection: { marginTop: 15, padding: 10, borderTopWidth: 1, borderTopStyle: 'solid' },
  noteTitle: { fontSize: 10, fontWeight: 'bold', textAlign: 'right', marginBottom: 5 },
  noteText: { fontSize: 9, textAlign: 'right', marginBottom: 5 },
  error: { fontSize: 9, color: 'red', textAlign: 'right' },
  viewer: { width: '100%', height: '70vh', border: '1px solid #ccc' },
});

// REFACTOR: Defined shared interface for props to ensure type consistency across components
// Reduces code duplication and improves type safety
interface CertificatePDFProps {
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
    achievementPercentage?: string | number;
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

// REFACTOR: Extracted shared utilities to avoid duplication
// Improves code reuse and maintainability
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
    if (typeof value === 'number') {
      return value.toString();
    }
    return String(value);
  },
  certificateTypeTranslations: {
    SUPPLY_PROCUREMENT_CERTIFICATE: 'مستخلص توريدات',
    COMPANY_SUPPLY_SALE_CERTIFICATE: 'مستخلص مقاوله',
    WORKMANSHIP_CONTRACTING_CERTIFICATE: 'مستخلص توريدات من مخازن الشركة',
  },
};

// REFACTOR: Component for WORKMANSHIP_CONTRACTING_CERTIFICATE
// Separates concerns by handling only workmanship-specific fields, reducing conditional complexity
const WorkmanshipCertificatePDF: React.FC<CertificatePDFProps> = ({
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

  // REFACTOR: Extracted pagination logic to shared utility for reuse
  // Simplifies component logic and improves readability
  const pages = [];
  for (let i = 0; i < items.length; i += pageSize) {
    pages.push(items.slice(i, i + pageSize));
  }

  const onClose = useCallback(() => {
    setShow(false);
  }, []);

  useEffect(() => {
    return () => {
      setShow(false); // Cleanup on unmount
    };
  }, []);

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
                        <View style={[styles.tableHeader, { width: '10%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.item', 'Item')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '10%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.code', 'Code')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '15%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.description', 'Description')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.quantity', 'Quantity')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.unitOfMeasure', 'Unit of Measure')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.materialPrice', 'Material Price')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.laborPrice', 'Labor Price')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.totalAmount', 'Total Amount')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '10%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.deductions', 'Deductions')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '10%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.deductionDescription', 'Deduction Description')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.deserved', 'Deserved')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.insurance', 'Insurance')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.additionalInsurance', 'Additional Insurance')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.net', 'Net')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '10%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.achievementPercentage', 'Achievement Percentage')}</Text>
                        </View>
                      </View>
                      {pageItems.map((item, itemIndex) => (
                          <View key={`${pageIndex}-${itemIndex}`} style={styles.tableRow}>
                            <View style={[styles.tableCell, { width: '10%' }]}>
                              <Text>
                                {sharedUtils.safeString(item.productName)}
                                {item.isLastInGroup && item.productSubtotal !== undefined ? ` (${sharedUtils.safeString(item.productSubtotal.toFixed(2))})` : ''}
                              </Text>
                            </View>
                            <View style={[styles.tableCell, { width: '10%' }]}>
                              <Text>{sharedUtils.safeString(item.code)}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '15%' }]}>
                              <Text>{sharedUtils.safeString(item.description)}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{item.quantity !== undefined ? sharedUtils.formatNumber(item.quantity, 0) : 'N/A'}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{sharedUtils.safeString(item.uomName)}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{item.materialPrice !== undefined ? sharedUtils.safeString(item.materialPrice.toFixed(2)) : 'N/A'}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{item.laborPrice !== undefined ? sharedUtils.safeString(item.laborPrice.toFixed(2)) : 'N/A'}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{item.displayTotal !== undefined ? sharedUtils.safeString(item.displayTotal.toFixed(2)) : 'N/A'}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '10%' }]}>
                              <Text>{item.deductions !== undefined ? sharedUtils.safeString(item.deductions.toFixed(2)) : 'N/A'}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '10%' }]}>
                              <Text>{sharedUtils.safeString(item.deductionDescription)}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{item.deserved !== undefined ? sharedUtils.safeString(item.deserved.toFixed(2)) : 'N/A'}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{item.insurance !== undefined ? sharedUtils.safeString(item.insurance.toFixed(2)) : 'N/A'}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{item.additionalInsurance !== undefined ? sharedUtils.safeString(item.additionalInsurance.toFixed(2)) : 'N/A'}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{item.net !== undefined ? sharedUtils.safeString(item.net.toFixed(2)) : 'N/A'}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '10%' }]}>
                              <Text>{sharedUtils.safeString(item.achievementPercentage)}</Text>
                            </View>
                          </View>
                      ))}
                    </View>
                ) : (
                    <View style={styles.section}>
                      <Text style={styles.error}>
                        {getTranslatedLabel('projects.certificate.items.list.noData', 'No items available')}
                      </Text>
                    </View>
                )}
                {pageItems.some(item => item.mainItemDescription && item.mainItemDescription.trim()) && (
                    <View style={styles.noteSection}>
                      <Text style={styles.noteTitle}>
                        {getTranslatedLabel('projects.certificate.items.mainDescription', 'Main Item Description')}
                      </Text>
                      {pageItems.map((item, itemIndex) => (
                          item.mainItemDescription && item.mainItemDescription.trim() && (
                              <Text key={`${pageIndex}-${itemIndex}-main`} style={styles.noteText}>
                                {sharedUtils.safeString(item.mainItemDescription)}
                              </Text>
                          )
                      ))}
                    </View>
                )}
                {pageItems.some(item => item.discountNote && item.discountNote.trim()) && (
                    <View style={styles.noteSection}>
                      <Text style={styles.noteTitle}>
                        {getTranslatedLabel('projects.certificate.items.discountNote', 'Discount Description Note')}
                      </Text>
                      {pageItems.map((item, itemIndex) => (
                          item.discountNote && item.discountNote.trim() && (
                              <Text key={`${pageIndex}-${itemIndex}-discount`} style={styles.noteText}>
                                {sharedUtils.safeString(item.discountNote)}
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

// REFACTOR: Component for SUPPLY_PROCUREMENT_CERTIFICATE and COMPANY_SUPPLY_SALE_CERTIFICATE
// Consolidates similar logic for supply certificates, reducing redundancy
const SupplyCertificatePDF: React.FC<CertificatePDFProps> = ({
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
  const isSupplyWithDiscount = certificateType === 'SUPPLY_PROCUREMENT_CERTIFICATE';

  // REFACTOR: Reused pagination logic from shared scope
  const pages = [];
  for (let i = 0; i < items.length; i += pageSize) {
    pages.push(items.slice(i, i + pageSize));
  }

  const onClose = useCallback(() => {
    setShow(false);
  }, []);

  useEffect(() => {
    return () => {
      setShow(false); // Cleanup on unmount
    };
  }, []);

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
                        <View style={[styles.tableHeader, { width: '10%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.item', 'Item')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '10%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.code', 'Code')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '15%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.description', 'Description')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.quantity', 'Quantity')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.unitOfMeasure', 'Unit of Measure')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.unitPrice', 'Unit Price')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.totalAmount', 'Total Amount')}</Text>
                        </View>
                        {isSupplyWithDiscount && (
                            <View style={[styles.tableHeader, { width: '8%' }]}>
                              <Text>{getTranslatedLabel('projects.certificate.items.list.discount', 'Discount')}</Text>
                            </View>
                        )}
                        <View style={[styles.tableHeader, { width: '10%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.procurementDate', 'Procurement Date')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '8%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.transportationExpenses', 'Transportation Expenses')}</Text>
                        </View>
                        <View style={[styles.tableHeader, { width: '7%' }]}>
                          <Text>{getTranslatedLabel('projects.certificate.items.list.gratuities', 'Gratuities')}</Text>
                        </View>
                      </View>
                      {pageItems.map((item, itemIndex) => (
                          <View key={`${pageIndex}-${itemIndex}`} style={styles.tableRow}>
                            <View style={[styles.tableCell, { width: '10%' }]}>
                              <Text>
                                {sharedUtils.safeString(item.productName)}
                                {item.isLastInGroup && item.productSubtotal !== undefined ? ` (${sharedUtils.safeString(item.productSubtotal.toFixed(2))})` : ''}
                              </Text>
                            </View>
                            <View style={[styles.tableCell, { width: '10%' }]}>
                              <Text>{sharedUtils.safeString(item.code)}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '15%' }]}>
                              <Text>{sharedUtils.safeString(item.description)}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{item.quantity !== undefined ? sharedUtils.formatNumber(item.quantity, 0) : 'N/A'}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{sharedUtils.safeString(item.uomName)}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{item.unitPrice !== undefined ? sharedUtils.safeString(item.unitPrice.toFixed(2)) : 'N/A'}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{item.displayTotal !== undefined ? sharedUtils.safeString(item.displayTotal.toFixed(2)) : 'N/A'}</Text>
                            </View>
                            {isSupplyWithDiscount && (
                                <View style={[styles.tableCell, { width: '8%' }]}>
                                  <Text>{item.discount !== undefined ? sharedUtils.safeString(item.discount.toFixed(2)) : 'N/A'}</Text>
                                </View>
                            )}
                            <View style={[styles.tableCell, { width: '10%' }]}>
                              <Text>{sharedUtils.safeString(item.formattedProcurementDate)}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{item.transportationExpenses !== undefined ? sharedUtils.safeString(item.transportationExpenses.toFixed(2)) : 'N/A'}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '7%' }]}>
                              <Text>{item.gratuities !== undefined ? sharedUtils.safeString(item.gratuities.toFixed(2)) : 'N/A'}</Text>
                            </View>
                          </View>
                      ))}
                    </View>
                ) : (
                    <View style={styles.section}>
                      <Text style={styles.error}>
                        {getTranslatedLabel('projects.certificate.items.list.noData', 'No items available')}
                      </Text>
                    </View>
                )}
                {pageItems.some(item => item.mainItemDescription && item.mainItemDescription.trim()) && (
                    <View style={styles.noteSection}>
                      <Text style={styles.noteTitle}>
                        {getTranslatedLabel('projects.certificate.items.mainDescription', 'Main Item Description')}
                      </Text>
                      {pageItems.map((item, itemIndex) => (
                          item.mainItemDescription && item.mainItemDescription.trim() && (
                              <Text key={`${pageIndex}-${itemIndex}-main`} style={styles.noteText}>
                                {sharedUtils.safeString(item.mainItemDescription)}
                              </Text>
                          )
                      ))}
                    </View>
                )}
                {pageItems.some(item => item.discountNote && item.discountNote.trim()) && (
                    <View style={styles.noteSection}>
                      <Text style={styles.noteTitle}>
                        {getTranslatedLabel('projects.certificate.items.discountNote', 'Discount Description Note')}
                      </Text>
                      {pageItems.map((item, itemIndex) => (
                          item.discountNote && item.discountNote.trim() && (
                              <Text key={`${pageIndex}-${itemIndex}-discount`} style={styles.noteText}>
                                {sharedUtils.safeString(item.discountNote)}
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

// REFACTOR: Export a wrapper component to decide which component to render based on certificateType
// Simplifies integration with existing codebase by maintaining the same interface
const CertificatePDFDocumentTabular: React.FC<CertificatePDFProps> = (props) => {
  if (props.certificateType === 'WORKMANSHIP_CONTRACTING_CERTIFICATE') {
    return <WorkmanshipCertificatePDF {...props} />;
  }
  return <SupplyCertificatePDF {...props} />;
};

export default CertificatePDFDocumentTabular;