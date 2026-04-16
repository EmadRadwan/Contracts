const handleCreate = async (data: { values: any; menuItem: string }) => {
    const { values } = data;
    const isDisbursement = values.isDisbursement ?? false;

    const customerId = isDisbursement
        ? values.partyIdTo?.fromPartyId
        : values.partyIdFrom?.fromPartyId;

    const organizationId = values.organizationPartyId;
    const org = companies.find((c) => c.organizationPartyId === organizationId);
    const orgName = org?.organizationPartyName ?? "";

    const newPayment: Payment = {
        paymentId: "",
        paymentTypeId: values.paymentTypeId,
        paymentMethodId: values.paymentMethodId,
        statusId: PAYMENT_STATUSES.NOT_PAID,

        partyIdFrom: isDisbursement ? organizationId : customerId,
        partyIdFromName: isDisbursement
            ? orgName
            : values.partyIdFrom?.fromPartyName ?? "",

        partyIdTo: isDisbursement ? customerId : organizationId,
        partyIdToName: isDisbursement
            ? values.partyIdTo?.fromPartyName ?? ""
            : orgName,

        amount: values.amount,
        effectiveDate: normalizeToDateString(values.effectiveDate),   // ← Clean!

        comments: values.comments ?? "",
        organizationPartyId: organizationId,
        isDepositWithDrawPayment: values.isDepositWithDrawPayment ? "Y" : "N",
        finAccountTransTypeId: isDisbursement ? "WITHDRAWAL" : "DEPOSIT",
        isDisbursement,

        chequeNumber: values.chequeNumber ?? "",
        chequeDate: normalizeToISOString(values.chequeDate),         // ← Clean!

        overrideGlAccountId: values.overrideGlAccountId,
        projectId: values.projectId?.projectId || null,
        projectName: values.projectId?.projectName || null,
        costCenterId: values.costCenterId || null,
        isBankTransfer: values.isBankTransfer || false,
        paymentRefNum: values.paymentRefNum || "",
    };

    await createPayment(newPayment);
};