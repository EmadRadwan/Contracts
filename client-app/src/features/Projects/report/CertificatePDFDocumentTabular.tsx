import { Document, Page, Text, View, StyleSheet, Image, PDFViewer } from '@react-pdf/renderer';
import { Button } from '@mui/material';
import { useCallback, useState } from "react";
import ModalContainer from "../../../app/common/modals/ModalContainer";

// REFACTOR: Updated styles to support custom table structure without external table components
// Improvement: Ensures compatibility with @react-pdf/renderer, maintains RTL layout
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
    textAlign: 'center', // REFACTOR: Centered title for better visual hierarchy
    marginBottom: 10,
  },
  table: {
    flexDirection: 'column',
    width: '100%',
    borderWidth: 1,
    borderColor: '#bfbfbf',
    marginBottom: 15,
  },
  tableRow: {
    flexDirection: 'row-reverse', // REFACTOR: Supports RTL layout for table rows
    borderBottomWidth: 1,
    borderBottomColor: '#bfbfbf',
  },
  tableHeader: {
    backgroundColor: '#f0f0f0',
    fontSize: 10,
    fontWeight: 'bold',
    textAlign: 'center',
    padding: 5,
    borderLeftWidth: 1,
    borderLeftColor: '#bfbfbf',
  },
  tableCell: {
    fontSize: 9,
    textAlign: 'center',
    padding: 5,
    borderLeftWidth: 1,
    borderLeftColor: '#bfbfbf',
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
    height: '70vh',
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
console.log('certificate', certificate)
  console.log('items', items)
  
  // Improvement: Prevents unnecessary re-renders
  const onClose = useCallback(() => {
    setShow(false);
  }, []);

  // Improvement: Maintains readability for large item sets
  const pages = [];
  for (let i = 0; i < items.length; i += pageSize) {
    pages.push(items.slice(i, i + pageSize));
  }

  // Improvement: More explicit logic for conditional fields
  const isSupplyWithDiscount = certificateType === 'SUPPLY_PROCUREMENT_CERTIFICATE';
  const isSupplyCertificate = ['SUPPLY_PROCUREMENT_CERTIFICATE', 'COMPANY_SUPPLY_SALE_CERTIFICATE'].includes(certificateType);
  const isWorkmanshipCertificate = certificateType === 'WORKMANSHIP_CONTRACTING_CERTIFICATE';

  const partyField = isWorkmanshipCertificate ? certificate.partyIdContractor ?? 'N/A' : certificate.partyIdSupplier ?? 'N/A';
  const partyLabelKey = isWorkmanshipCertificate ? 'projects.certificate.form.contractor' : 'projects.certificate.form.supplier';
  const partyLabelDefault = isWorkmanshipCertificate ? 'Contractor' : 'Supplier';

  const certificateTypeTranslations: { [key: string]: string } = {
    SUPPLY_PROCUREMENT_CERTIFICATE: 'شهادة توريد',
    COMPANY_SUPPLY_SALE_CERTIFICATE: 'شهادة بيع توريد الشركة',
    WORKMANSHIP_CONTRACTING_CERTIFICATE: 'شهادة أعمال المقاولة',
  };

  // Improvement: Ensures all Text components receive valid strings, preventing layout errors
  // Context: Addresses TypeError in @react-pdf/renderer by safely handling undefined/null values
  const safeString = (value: any): string => {
    if (value === null || value === undefined) return 'N/A';
    return String(value);
  };

  const MyDocument = () => (
      <Document>
        {pages.map((pageItems, pageIndex) => (
            <Page key={pageIndex} size="A4" orientation="landscape" style={styles.page}>
              <View style={styles.section}>
                <View style={styles.header}>
                  <Image style={{ width: 100, height: 100, marginBottom: 10 }} src="/goldenlandlogo.jpg" />
                  <Text style={styles.title}>
                    {getTranslatedLabel('projects.certificate.report.title', 'Certificate Report')}: {safeString(certificate.certificateNumber)}
                  </Text>
                  <View style={{ flexDirection: 'row-reverse', justifyContent: 'space-between', marginBottom: 5 }}>
                    <Text style={styles.headerText}>
                      {getTranslatedLabel('projects.certificate.type', 'Type')}: {certificateTypeTranslations[certificateType] || certificateType}
                    </Text>
                    <Text style={styles.headerText}>
                      {getTranslatedLabel('projects.certificate.date', 'Date')}: {new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'numeric', day: 'numeric' })}
                    </Text>
                  </View>
                  <View style={{ flexDirection: 'row-reverse', justifyContent: 'space-between', marginBottom: 5 }}>
                    <Text style={styles.headerText}>
                      {getTranslatedLabel('projects.certificate.description', 'Description')}: {safeString(certificate.description)}
                    </Text>
                    <Text style={styles.headerText}>
                      {getTranslatedLabel(partyLabelKey, partyLabelDefault)}: {safeString(partyField)}
                    </Text>
                  </View>
                  <View style={{ flexDirection: 'row-reverse', justifyContent: 'space-between' }}>
                    <Text style={styles.headerText}>
                      {getTranslatedLabel('projects.certificate.total', 'Total')}: {safeString(subtotal.toFixed(2))}
                    </Text>
                    {isSupplyCertificate && (
                        <Text style={styles.headerText}>
                          {getTranslatedLabel('projects.certificate.form.facility', 'Facility')}: {safeString(certificate.facilityName)}
                        </Text>
                    )}
                  </View>
                </View>

                {pageItems && pageItems.length > 0 ? (
                    <View style={styles.table}>
                      {/* Table Header */}
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
                        {isSupplyCertificate && (
                            <>
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
                            </>
                        )}
                        {isWorkmanshipCertificate && (
                            <>
                              <View style={[styles.tableHeader, { width: '8%' }]}>
                                <Text>{getTranslatedLabel('projects.certificate.items.list.materialPrice', 'Material Price')}</Text>
                              </View>
                              <View style={[styles.tableHeader, { width: '8%' }]}>
                                <Text>{getTranslatedLabel('projects.certificate.items.list.laborPrice', 'Labor Price')}</Text>
                              </View>
                              <View style={[styles.tableHeader, { width: '8%' }]}>
                                <Text>{getTranslatedLabel('projects.certificate.items.list.totalAmount', 'Total Amount')}</Text>
                              </View>
                              <View style={[styles.tableHeader, { width: '8%' }]}>
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
                            </>
                        )}
                      </View>
                      {/* Table Body */}
                      {pageItems.map((item, itemIndex) => (
                          <View key={`${pageIndex}-${itemIndex}`} style={styles.tableRow}>
                            <View style={[styles.tableCell, { width: '10%' }]}>
                              {/* REFACTOR: Added safeString to handle undefined productName and productSubtotal */}
                              <Text>
                                {safeString(item.productName)}
                                {item.isLastInGroup && item.productSubtotal !== undefined ? ` (${safeString(item.productSubtotal.toFixed(2))})` : ''}
                              </Text>
                            </View>
                            <View style={[styles.tableCell, { width: '10%' }]}>
                              <Text>{safeString(item.code)}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '15%' }]}>
                              <Text>{safeString(item.description)}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{safeString(item.quantity)}</Text>
                            </View>
                            <View style={[styles.tableCell, { width: '8%' }]}>
                              <Text>{safeString(item.uomName)}</Text>
                            </View>
                            {isSupplyCertificate && (
                                <>
                                  <View style={[styles.tableCell, { width: '8%' }]}>
                                    <Text>{item.unitPrice !== undefined ? safeString(item.unitPrice.toFixed(2)) : 'N/A'}</Text>
                                  </View>
                                  <View style={[styles.tableCell, { width: '8%' }]}>
                                    <Text>{item.displayTotal !== undefined ? safeString(item.displayTotal.toFixed(2)) : 'N/A'}</Text>
                                  </View>
                                  {isSupplyWithDiscount && (
                                      <View style={[styles.tableCell, { width: '8%' }]}>
                                        <Text>{item.discount !== undefined ? safeString(item.discount.toFixed(2)) : 'N/A'}</Text>
                                      </View>
                                  )}
                                  <View style={[styles.tableCell, { width: '10%' }]}>
                                    <Text>{safeString(item.formattedProcurementDate)}</Text>
                                  </View>
                                  <View style={[styles.tableCell, { width: '8%' }]}>
                                    <Text>{item.transportationExpenses !== undefined ? safeString(item.transportationExpenses.toFixed(2)) : 'N/A'}</Text>
                                  </View>
                                  <View style={[styles.tableCell, { width: '7%' }]}>
                                    <Text>{item.gratuities !== undefined ? safeString(item.gratuities.toFixed(2)) : 'N/A'}</Text>
                                  </View>
                                </>
                            )}
                            {isWorkmanshipCertificate && (
                                <>
                                  <View style={[styles.tableCell, { width: '8%' }]}>
                                    <Text>{item.materialPrice !== undefined ? safeString(item.materialPrice.toFixed(2)) : 'N/A'}</Text>
                                  </View>
                                  <View style={[styles.tableCell, { width: '8%' }]}>
                                    <Text>{item.laborPrice !== undefined ? safeString(item.laborPrice.toFixed(2)) : 'N/A'}</Text>
                                  </View>
                                  <View style={[styles.tableCell, { width: '8%' }]}>
                                    <Text>{item.displayTotal !== undefined ? safeString(item.displayTotal.toFixed(2)) : 'N/A'}</Text>
                                  </View>
                                  <View style={[styles.tableCell, { width: '8%' }]}>
                                    <Text>{item.deductions !== undefined ? safeString(item.deductions.toFixed(2)) : 'N/A'}</Text>
                                  </View>
                                  <View style={[styles.tableCell, { width: '10%' }]}>
                                    <Text>{safeString(item.deductionDescription)}</Text>
                                  </View>
                                  <View style={[styles.tableCell, { width: '8%' }]}>
                                    <Text>{item.deserved !== undefined ? safeString(item.deserved.toFixed(2)) : 'N/A'}</Text>
                                  </View>
                                  <View style={[styles.tableCell, { width: '8%' }]}>
                                    <Text>{item.insurance !== undefined ? safeString(item.insurance.toFixed(2)) : 'N/A'}</Text>
                                  </View>
                                  <View style={[styles.tableCell, { width: '8%' }]}>
                                    <Text>{item.additionalInsurance !== undefined ? safeString(item.additionalInsurance.toFixed(2)) : 'N/A'}</Text>
                                  </View>
                                  <View style={[styles.tableCell, { width: '8%' }]}>
                                    <Text>{item.net !== undefined ? safeString(item.net.toFixed(2)) : 'N/A'}</Text>
                                  </View>
                                  <View style={[styles.tableCell, { width: '10%' }]}>
                                    <Text>{safeString(item.achievementPercentage)}</Text>
                                  </View>
                                </>
                            )}
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

                {/* REFACTOR: Added check for valid mainItemDescription to prevent empty note sections */}
                {pageItems.some(item => item.mainItemDescription && item.mainItemDescription.trim()) && (
                    <View style={styles.noteSection}>
                      <Text style={styles.noteTitle}>
                        {getTranslatedLabel('projects.certificate.items.mainDescription', 'Main Item Description')}
                      </Text>
                      {pageItems.map((item, itemIndex) => (
                          item.mainItemDescription && item.mainItemDescription.trim() && (
                              <Text key={`${pageIndex}-${itemIndex}-main`} style={styles.noteText}>
                                {safeString(item.mainItemDescription)}
                              </Text>
                          )
                      ))}
                    </View>
                )}

                {/* REFACTOR: Added check for valid discountNote to prevent empty note sections */}
                {pageItems.some(item => item.discountNote && item.discountNote.trim()) && (
                    <View style={styles.noteSection}>
                      <Text style={styles.noteTitle}>
                        {getTranslatedLabel('projects.certificate.items.discountNote', 'Discount Description Note')}
                      </Text>
                      {pageItems.map((item, itemIndex) => (
                          item.discountNote && item.discountNote.trim() && (
                              <Text key={`${pageIndex}-${itemIndex}-discount`} style={styles.noteText}>
                                {safeString(item.discountNote)}
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

export default CertificatePDFDocument;