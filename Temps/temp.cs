import { useSelector } from 'react-redux';
import { RootState } from '../../../app/store/configureStore';
import CertificatePDFDocument from './CertificatePDFDocument';

// Inside ProjectCertificateForm.tsx component
const ProjectCertificateForm = () => {
    // Assuming currentCertificateType is accessed from Redux
    const currentCertificateType = useSelector((state: RootState) => state.certificateUi.currentCertificateType);
    const certificateReport = useSelector(certificateReportSelector);

    // Other logic...

    return (
        // Other JSX...
        <CertificatePDFDocument
        certificate={certificateReport.certificate}
    items={certificateReport.items}
    getTranslatedLabel={getTranslatedLabel}
    subtotal={subtotal}
    isGrouped={currentCertificateType === 'WORKMANSHIP_CONTRACTING_CERTIFICATE'}
    isSubmitting={isSubmitting}
    isAddCertificateLoading={isAddCertificateLoading}
    isUpdateCertificateLoading={isUpdateCertificateLoading}
    isReceiveLoading={isReceiveLoading}
    certificateNumber={certificateReport.certificate.certificateNumber}
    certificateType={currentCertificateType} // Add this prop
    />
    );
};