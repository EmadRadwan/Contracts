const handleStatusUpdate = useCallback(
    async (action: string) => {
        if (!selectedCertificate?.workEffortId) {
            toast.error(getTranslatedLabel("certificate.noWorkEffortId", "No certificate selected"));
            return;
        }
        setIsSubmitting(true);
        // Purpose: Ensure status transitions use WEPR_CREATED, WEPR_APPROVED, WEPR_COMPLETE
        // Context: Aligns with editModeMap and backend expectations
        setSelectedMenuItem(action);
        const statusUpdate = {
            values: {
                workEffortId: selectedCertificate.workEffortId,
                currentStatusId: action === 'Approve Certificate' ? CertificateStatus.APPROVED : CertificateStatus.COMPLETE,
            },
            selectedMenuItem: action,
        };
        try {
            // REFACTOR: Check handleCreate result to avoid incorrect success toast
            // Purpose: Only show success toast if handleCreate indicates success
            // Improvement: Prevents success toast when INSUFFICIENT_INVENTORY error occurs
            // Context: Uses return value from handleCreate to determine outcome
            const result = await handleCreate(statusUpdate);
            if (result.success) {
                toast.success(
                    getTranslatedLabel(
                        action === 'Approve Certificate' ? 'certificate.approved' : 'certificate.completed',
                        action === 'Approve Certificate' ? 'Certificate approved' : 'Certificate completed'
                    )
                );
            }
        } catch (error) {
            toast.error(getTranslatedLabel("certificate.statusUpdate.error", "Failed to update certificate status"));
        } finally {
            setIsSubmitting(false);
            setSelectedMenuItem("");
        }
    },
    [handleCreate, selectedCertificate, getTranslatedLabel]
);