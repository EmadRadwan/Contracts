const handleSaveTransaction = useCallback(
    async () => {
        if (!selectedAcctgTrans?.acctgTransId) {
            toast.error(getTranslatedLabel("general.error", "No transaction ID provided"));
            return;
        }
        if (transEntries.length === 0) {
            toast.error(getTranslatedLabel("general.error", "No entries to save"));
            return;
        }
        try {
            await handleUpdateMultiAcctgTransWithEntries({
                acctgTransId: selectedAcctgTrans.acctgTransId,
                UpdateMultiAcctgTransParams: {
                    AcctgTransId: selectedAcctgTrans.acctgTransId, // REFACTOR: Include AcctgTransId in UpdateMultiAcctgTransParams
                    // Purpose: Match backend validation requirement for AcctgTransId
                    // Improvement: Ensures payload structure aligns with backend expectations
                    AcctgTransTypeId: "_NA_",
                    TransactionDate: headerValues.transactionDate,
                    OrganizationPartyId: companyId,
                    HeaderDescription: headerValues.headerDescription,
                    Description: transEntries[0]?.description || "",
                    IsPosted: selectedAcctgTrans?.isPosted || "N",
                    GlFiscalTypeId: "ACTUAL",
                },
                Entries: transEntries.map((entry) => ({
                    acctgTransEntrySeqId: entry.acctgTransEntrySeqId,
                    debitGlAccountId: entry.debitGlAccountId,
                    creditGlAccountId: entry.creditGlAccountId,
                    amount: entry.amount,
                    description: entry.description,
                    debitCreditFlag: entry.debitCreditFlag,
                })),
            });
            toast.success(getTranslatedLabel("general.success", "Transaction updated successfully"));
            router.navigate("/orgGl");
        } catch (error) {
            toast.error(getTranslatedLabel("general.error", "Failed to update transaction"));
        }
    },
    [transEntries, companyId, handleUpdateMultiAcctgTransWithEntries, getTranslatedLabel, headerValues, selectedAcctgTrans]
);