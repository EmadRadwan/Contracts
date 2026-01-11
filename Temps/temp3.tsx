const ledgerItems = useMemo((): LedgerRow[] => {
    if (!data) return [];

    const isExternalView = data.LedgerPerspective?.startsWith("External") ?? false;

    const rows: LedgerRow[] = [];
    let runningBalance = 0;

    const formatDate = (dateStr?: string | null) =>
        dateStr ? new Date(dateStr).toISOString().split("T")[0] : "";

    // 1. Opening Balance
    const openingTotalImpact = (data.openingBalances || []).reduce(
        (sum, ob) => sum + ob.ImpactOnBalance,
        0
    );

    if (openingTotalImpact !== 0) {
        const adjustedImpact = isExternalView ? -openingTotalImpact : openingTotalImpact;
        runningBalance += adjustedImpact;

        rows.push({
            date: "",
            description: getTranslatedLabel("openingBalance", "الرصيد الإفتتاحي"),
            value: 0,
            toPay: adjustedImpact > 0 ? adjustedImpact : 0,    // دائن
            paid: adjustedImpact < 0 ? -adjustedImpact : 0,    // مدين
            balance: Math.round(runningBalance * 100) / 100,
            notes: data.openingBalances?.length ? `${data.openingBalances.length} حركات` : "",
        });
    }

    // 2. Collect all valid transactions
    interface UnifiedTransaction {
        date: string;
        type: "invoice" | "payment" | "rental" | "partnerAccrual";
        id?: string;
        description: string;
        grossAmount: number;
        isSalesInvoice?: boolean;
        isPurchaseInvoice?: boolean;
        isReceiptFromParty?: boolean;
        isDisbursementToParty?: boolean;
        isRentalDebit?: boolean;
        isPartnerCredit?: boolean;
    }

    const transactions: UnifiedTransaction[] = [];

    // A. Invoices – safe version
    [...(data.invoicesApplPayments || []), ...(data.unappliedInvoices || [])].forEach((inv) => {
        if (!inv.InvoiceDate || !inv.InvoiceId) return; // skip invalid

        const isSales = inv.InvoiceTypeId?.startsWith("SALES") || false;

        if (!transactions.some((t) => t.id === inv.InvoiceId)) {
            transactions.push({
                date: inv.InvoiceDate,
                type: "invoice",
                id: inv.InvoiceId,
                description: `فاتورة ${inv.InvoiceId} ${inv.InvoiceTypeId || ""}${
                    inv.unappliedAmount ? " (غير مطبقة)" : ""
                }`,
                grossAmount: inv.Total ?? inv.Amount ?? 0,
                isSalesInvoice: isSales,
                isPurchaseInvoice: !isSales,
            });
        }
    });

    // B. Payments – safe & correct field names
    (data.unappliedPayments || []).forEach((pmt) => {
        if (!pmt.effectiveDate || !pmt.paymentId) return; // skip invalid

        const isReceipt = pmt.paymentParentTypeId === "RECEIPT";

        transactions.push({
            date: pmt.effectiveDate,
            type: "payment",
            id: pmt.paymentId,
            description: `دفعة ${pmt.paymentId} - ${pmt.paymentTypeDescription || "غير معروف"}`,
            grossAmount: pmt.amount || 0,
            isReceiptFromParty: isReceipt,
            isDisbursementToParty: !isReceipt,
        });
    });

    // C. Rental Property Postings – already safe
    (data.rentalPropertyPostings || []).forEach((rp) => {
        if (!rp.transactionDate) return;

        transactions.push({
            date: rp.transactionDate,
            type: "rental",
            description: rp.description || `تسجيل إيجاري ${rp.glAccountTypeId || ""}`,
            grossAmount: Math.abs(rp.impactOnBalance || 0),
            isRentalDebit: (rp.impactOnBalance || 0) > 0,
        });
    });

    // D. Partner Accrual Postings – already safe
    (data.partnerAccrualPostings || []).forEach((pa) => {
        if (!pa.transactionDate) return;

        transactions.push({
            date: pa.transactionDate,
            type: "partnerAccrual",
            description: pa.description || "استحقاق شركاء",
            grossAmount: Math.abs(pa.impactOnBalance || 0),
            isPartnerCredit: (pa.impactOnBalance || 0) > 0,
        });
    });

    // 3. Sort chronologically – safe version
    transactions.sort((a, b) => {
        const dateA = a.date || "9999-12-31";
        const dateB = b.date || "9999-12-31";
        return dateA.localeCompare(dateB);
    });

    // 4. Build rows with correct signs
    transactions.forEach((t) => {
        let madin = 0; // مدين
        let dain = 0;  // دائن

        if (t.type === "invoice") {
            if (t.isSalesInvoice) {
                // We invoiced them / they owe us → دائن in both views (standard for sales)
                dain = t.grossAmount;
            } else if (t.isPurchaseInvoice) {
                isExternalView ? dain = t.grossAmount : madin = t.grossAmount;
            }
        } else if (t.type === "payment") {
            if (t.isReceiptFromParty) {
                // They paid us
                isExternalView ? dain = t.grossAmount : madin = t.grossAmount;
            } else if (t.isDisbursementToParty) {
                // We paid them
                isExternalView ? madin = t.grossAmount : dain = t.grossAmount;
            }
        } else if (t.type === "rental" && t.isRentalDebit) {
            // Rental debit → increases what they owe us → دائن
            dain = t.grossAmount;
        } else if (t.type === "partnerAccrual" && t.isPartnerCredit) {
            // Partner accrual → increases what we owe → مدين in external view
            isExternalView ? madin = t.grossAmount : dain = t.grossAmount;
        }

        const periodChange = isExternalView ? (dain - madin) : (madin - dain);
        runningBalance += periodChange;

        const notes =
            t.type === "payment" && data.unappliedPayments?.some((p) => p.paymentId === t.id)
                ? "غير مطبقة"
                : undefined;

        rows.push({
            date: formatDate(t.date),
            description: t.description,
            invoiceNumber: t.type === "invoice" ? t.id : undefined,
            paymentNumber: t.type === "payment" ? t.id : undefined,
            value: t.grossAmount,
            toPay: dain,   // دائن
            paid: madin,   // مدين
            balance: Math.round(runningBalance * 100) / 100,
            notes,
            transactionType: t.type,
        });
    });

    return rows;
}, [data, isExternalView, getTranslatedLabel]);