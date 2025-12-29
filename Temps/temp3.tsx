const salesRequestValidator = useCallback((values: any): KeyValue<string> | undefined => {
    const t = getTranslatedLabel;

    const apt = values.productId;
    const currentSalesRequestId = values.salesRequestId;
    const aptStatusId = typeof apt === "object" ? apt?.apartmentStatusId : null;

    if (aptStatusId && aptStatusId !== APARTMENT_AVAILABLE) {
        const reservedByThisRequest = apt.reservedBySalesRequestId === currentSalesRequestId;
        if (!reservedByThisRequest) {
            return {
                VALIDATION_SUMMARY: t(
                    "salesRequest.form.validation.apartmentNotAvailable",
                    "Cannot proceed: this apartment is already SOLD or RESERVED by another sales request."
                )
            };
        }
    }

    // All other validation (payment plan, advance match, etc.) moved to handleSubmitData
    return;
}, [getTranslatedLabel]); // ← Removed autoSetDerivedFields from deps!