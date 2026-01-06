const handleSaveTransaction = useCallback(
    async () => {
        if (transEntries.length === 0) {
            toast.error(getTranslatedLabel("general.error", "No entries to save"));
            return;
        }

        // Optional: extra safety check (button already disables if unbalanced)
        if (totalDebit !== totalCredit) {
            toast.warn("Debits and credits must balance before saving.");
            return;
        }

        try {
            const result = await saveMultiAcctgTransWithEntries({
                CreateMultiAcctgTransParams: {
                    AcctgTransTypeId: "_NA_",
                    TransactionDate: headerValues.transactionDate,
                    OrganizationPartyId: companyId,
                    HeaderDescription: headerValues.headerDescription || "",
                    Description: transEntries[0]?.description || headerValues.headerDescription || "",
                    IsPosted: "N",
                    GlFiscalTypeId: "ACTUAL",
                    partyId: headerValues.party?.fromPartyId || undefined, // ← correctly extracted
                },
                Entries: transEntries.map((entry) => ({
                    debitGlAccountId: entry.debitGlAccountId || undefined,
                    creditGlAccountId: entry.creditGlAccountId || undefined,
                    amount: entry.amount,
                    description: entry.description || "",
                    debitCreditFlag: entry.debitCreditFlag,
                })),
            });

            setTransactionId(result.acctgTransId);
            toast.success(
                getTranslatedLabel("general.success", "Transaction saved successfully")
            );

            // Do NOT reset entries or header here — user can continue editing or post
        } catch (error) {
            // Error is typically already toasted in the hook
            toast.error(getTranslatedLabel("general.error", "Failed to save transaction"));
            console.error("Save transaction error:", error);
        }
    },
    [
        transEntries,
        totalDebit,
        totalCredit,
        headerValues,
        companyId,
        saveMultiAcctgTransWithEntries,
        getTranslatedLabel,
    ]
);