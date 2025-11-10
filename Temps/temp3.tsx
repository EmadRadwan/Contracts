// REFACTOR: Send only the string value of debitCreditFlag
// Purpose: DropDownList returns the whole data item ({text, value}); backend needs "D"/"C"
// Improvement: Prevents object serialization error
const handleSubmit = useCallback(
    async (data: any) => {
        if (!data.isValid) return;

        const {
            glAccountId,
            amount,
            description,
            debitCreditFlag,   // <-- this is now the selected data item
        } = data.values;

        try {
            const result = await saveInitialBalanceTrans({
                CreateInitialBalanceTransParams: {
                    AcctgTransTypeId: "INITIAL_BALANCE",
                    TransactionDate: headerValues.transactionDate,
                    OrganizationPartyId: companyId,
                    HeaderDescription: headerValues.headerDescription,
                    GlFiscalTypeId: "ACTUAL",
                    IsPosted: "N",
                },
                Entry: {
                    glAccountId: glAccountId!,
                    amount: amount!,
                    description: description || "",
                    // REFACTOR: Extract the primitive value
                    debitCreditFlag: debitCreditFlag?.value ?? debitCreditFlag,
                },
            });

            setTransactionId(result.acctgTransId);
            toast.success(getTranslatedLabel("general.success", "Initial balance saved"));
            setFormResetCounter((prev) => prev + 1);
        } catch {
            // error already toasted in hook
        }
    },
    [headerValues, companyId, saveInitialBalanceTrans, getTranslatedLabel]
);