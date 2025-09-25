// ... (previous imports and styles remain unchanged)

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
                                                                           certificateType,
                                                                           pageSize = 15,
                                                                       }) => {
    // REFACTOR: Adjusted pagination to handle list-based items
    // Purpose: Ensures items are split across pages without breaking list structure
    // Improvement: Maintains readability for large item sets
    const pages = [];
    for (let i = 0; i < items.length; i += pageSize) {
        pages.push(items.slice(i, i + pageSize));
    }

    const isSupplyWithDiscount = certificateType === 'SUPPLY_PROCUREMENT_CERTIFICATE';
    const isSupplyCertificate = ['SUPPLY_PROCUREMENT_CERTIFICATE', 'COMPANY_SUPPLY_SALE_CERTIFICATE'].includes(certificateType);
    const partyField = isGrouped ? certificate.partyIdContractor ?? 'N/A' : certificate.partyIdSupplier ?? 'N/A';

    // REFACTOR: Updated translation keys to match JSON structure
    // Purpose: Aligns with provided Arabic translations under projects.certificate
    // Improvement: Ensures correct Arabic labels are used
    const partyLabelKey = isGrouped ? 'projects.certificate.form.contractor' : 'projects.certificate.form.supplier';
    const partyLabelDefault = isGrouped ? 'Contractor' : 'Supplier';

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
                                <Text style={[styles.headerText, { textAlign: 'left' }]}>{new Date().toLocaleDateString('en-US', { year: 'numeric', month: 'numeric', day: 'numeric' })}</Text>
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
                            {!isGrouped && (
                                <View style={{ flexDirection: 'row-reverse', alignItems: 'baseline' }}>
                                    <Text style={[styles.headerText, { textAlign: 'right' }]}>
                                        {getTranslatedLabel('projects.certificate.form.facility', 'Facility')}
                                    </Text>
                                    <Text style={styles.headerText}>:</Text>
                                    <Text style={[styles.headerText, { textAlign: 'left' }]}>{certificate.facilityName ?? 'N/A'}</Text>
                                </View>
                            )}
                        </View>

                        {/* REFACTOR: Replaced table with list-based item rendering */}
                        {/* Purpose: Displays items as a vertical list to match client’s layout */}
                        {/* Improvement: Reduces horizontal crowding and aligns with header style */}
                        {pageItems && pageItems.length > 0 ? (
                            pageItems.map((item, itemIndex) => (
                                <View key={`${pageIndex}-${itemIndex}`} style={styles.itemSection}>
                                    <Text style={styles.itemTitle}>
                                        {getTranslatedLabel('projects.certificate.items.list.item', 'Item')} {itemIndex + 1}: {item.productName ?? 'N/A'}
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
                                    {!isGrouped && (
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
                                            {isSupplyCertificate && (
                                                <>
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

                        {/* REFACTOR: Retained main item description and discount note sections */}
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
        <PDFDownloadLink document={<MyDocument />} fileName={`Certificate_${certificateNumber}.pdf`}>
            {({ loading }) => (
                <Button
                    color="primary"
                    variant="outlined"
                    disabled={isSubmitting || isAddCertificateLoading || isUpdateCertificateLoading || isReceiveLoading || loading}
                    aria-label={loading ? 'Generating PDF' : 'Export to PDF'}
                >
                    {loading ? getTranslatedLabel('projects.certificate.generating', 'Generating PDF...') : getTranslatedLabel('projects.certificate.export', 'Export to PDF')}
                </Button>
            )}
        </PDFDownloadLink>
    );
};

export default CertificatePDFDocument;