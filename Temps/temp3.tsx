// In MultiPaymentCertificatesList.tsx
const handleSelectCertificate = useCallback(
    (workEffortId?: string) => {
        if (!workEffortId) return;
        const selectedCert = certificates.data.find(
            (cert: MultiPaymentCertificate) => cert.workEffortId === workEffortId
        );
        if (!selectedCert) return;
        setPaymentCertificate(selectedCert);
        // REFACTOR: Set editMode to 2 for editing an existing certificate
        // This ensures the form operates in edit mode, aligning with the hook's logic
        setFormEditMode(2); // Changed from 1 to 2
        setViewMode("form");
    },
    [certificates.data]
);

const handleCreateNew = useCallback(() => {
    // REFACTOR: Clear paymentCertificate to ensure a clean slate for new certificate
    // This prevents stale data from persisting in the form
    setPaymentCertificate(null);
    setFormEditMode(1);
    setViewMode("form");
}, []);

const cancelEdit = useCallback(() => {
    // REFACTOR: Clear paymentCertificate when canceling to reset form state
    // This ensures no stale certificate data persists when returning to list view
    setPaymentCertificate(null);
    setFormEditMode(0);
    setViewMode("list");
}, []);

// REFACTOR: Add useEffect to clear paymentCertificate when viewMode changes to 'list'
// This ensures the form starts fresh when re-opened
useEffect(() => {
    if (viewMode === "list") {
        setPaymentCertificate(null);
    }
}, [viewMode]);