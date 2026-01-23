const initialValues = useMemo((): Partial<MultiPaymentItem> => {
    const base = {
        workEffortId: multiPaymentItem?.workEffortId || "",
        glAccountId: multiPaymentItem?.glAccountId || "",
        workEffortIdParent: workEffortId || "",
        itemType: multiPaymentItem?.itemType || "",
        serviceId: multiPaymentItem?.serviceId
            ? { ProductId: multiPaymentItem.serviceId, ProductName: multiPaymentItem.serviceName || "" }
            : null,
        productId: multiPaymentItem?.productId
            ? { ProductId: multiPaymentItem.productId, ProductName: multiPaymentItem.productName || "" }
            : null,
        description: multiPaymentItem?.description || "",
        amount: multiPaymentItem?.amount ?? 0,
        discount: multiPaymentItem?.discount ?? 0,
        discountMode: multiPaymentItem?.discountMode || "value",
        transportationExpenses: multiPaymentItem?.transportationExpenses ?? 0,
        gratuities: multiPaymentItem?.gratuities ?? 0,
        total: multiPaymentItem?.total ?? 0,
    };

    // ── Add supplier & contractor ────────────────────────────────
    let partyIdSupplier = null;
    let partyIdContractor = null;

    if (multiPaymentItem) {
        if (multiPaymentItem.partyIdSupplier && multiPaymentItem.partyIdSupplierName) {
            partyIdSupplier = {
                fromPartyId: multiPaymentItem.partyIdSupplier,
                fromPartyName: multiPaymentItem.partyIdSupplierName,
                // If your ComboBox expects more fields, add them here with fallback empty strings
                // e.g. partyCode: multiPaymentItem.partyCodeSupplier || "",
                //      address:    multiPaymentItem.addressSupplier    || "",
            };
        }

        if (multiPaymentItem.partyIdContractor && multiPaymentItem.partyIdContractorName) {
            partyIdContractor = {
                fromPartyId: multiPaymentItem.partyIdContractor,
                fromPartyName: multiPaymentItem.partyIdContractorName,
                // same as above — add other fields if needed
            };
        }
    }

    return {
        ...base,
        partyIdSupplier,
        partyIdContractor,
    };
}, [multiPaymentItem, workEffortId]);