const partyValidator = (values: Partial<MultiPaymentItem>): KeyValue<string> | undefined => {
    const hasSupplier   = !!values.partyIdSupplier   && values.partyIdSupplier !== "";
    const hasContractor = !!values.partyIdContractor && values.partyIdContractor !== "";

    // ── Most important rule ───────────────────────────────────────
    if (hasSupplier && hasContractor) {
        return {
            VALIDATION_SUMMARY: getTranslatedLabel(
                `${localizationKey}.validation.partyExclusive`,
                "Please select either a Supplier or a Contractor, not both."
            ),
        };
    }

    // No error if:
    //   - both are empty   → allowed
    //   - only supplier    → allowed
    //   - only contractor  → allowed

    return undefined;
};