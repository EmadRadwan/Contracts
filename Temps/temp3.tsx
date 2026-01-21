const ledgerItems = useMemo((): LedgerRow[] => {
    if (!data) return [];
    const rows: LedgerRow[] = [];
    let runningBalance = 0;

    const formatDate = (dateStr?: string | null) =>
        dateStr ? new Date(dateStr).toISOString().split("T")[0] : "";

    // ───────────────────────────────
    // 1. Opening Balance (unchanged)
    // ───────────────────────────────
    const openingEntries = data.openingBalances || [];
    if (openingEntries.length > 0) {
        let totalMadin = 0;
        let totalDain = 0;
        let netImpact = 0;
        let transactionDate = "";

        openingEntries.forEach((ob) => {
            const rawAmount = Number(ob?.amount || 0);
            const flag = (ob?.debitCreditFlag || "").trim().toUpperCase();
            transactionDate = ob.transactionDate || transactionDate;

            if (flag === "D") {
                totalMadin += rawAmount;
                netImpact += rawAmount;
            } else if (flag === "C") {
                totalDain += rawAmount;
                netImpact -= rawAmount;
            }
        });

        const viewAdjustedNet = netImpact;
        runningBalance += viewAdjustedNet;

        rows.push({
            date: formatDate(transactionDate),
            description: getTranslatedLabel("openingBalance", "الرصيد الإفتتاحي"),
            value: 0,
            toPay: totalDain,   // دائن
            paid: totalMadin,   // مدين
            balance: Math.round(runningBalance * 100) / 100,
        });
    }

    // ───────────────────────────────
    // 2. Unified Transactions
    // ───────────────────────────────
    interface UnifiedTransaction {
        date: string;
        type:
            | "invoice"
            | "payment"
            | "rentalPosting"
            | "partnerAccrual"
            | "apartmentSale"
            | "chequeIssued";     // ← NEW
        id?: string;
        description: string;
        grossAmount: number;
        isSalesInvoice?: boolean;
        isPurchaseInvoice?: boolean;
        isReceiptFromParty?: boolean;
        isDisbursementToParty?: boolean;
        debitCreditFlag?: string;
        transactionTypeId?: string;
        salesRequestId?: string;
    }

    const transactions: UnifiedTransaction[] = [];

    // ... existing invoice collection ...

    // ... existing payment collection ...

    // ... existing rentalPropertyPostings ...

    // ... existing partnerAccrualPostings ...

    // ... existing apartmentSalePostings ...

    // ───────────────────────────────────────────────
    //  NEW: Cheque Issued Postings (only debit side)
    // ───────────────────────────────────────────────
    (data.chequeIssuedPostings || []).forEach((ch) => {
        if (!ch.transactionDate || ch.amount <= 0) return;

        const flag = (ch.debitCreditFlag || "").trim().toUpperCase();
        if (flag !== "D") return; // we only included debits anyway, but safety

        transactions.push({
            date: ch.transactionDate,
            type: "chequeIssued",
            id: ch.transactionId,
            description:
                ch.description ||
                `إصدار شيك مؤجل - ${ch.glAccountId || "غير معروف"} - دفعة ${ch.transactionId}`,
            grossAmount: Number(ch.amount || 0),
            debitCreditFlag: ch.debitCreditFlag,
            transactionTypeId: ch.transactionTypeId,
        });
    });

    // 3. Sort all transactions by date
    transactions.sort((a, b) => {
        const dateA = a.date || "9999-12-31";
        const dateB = b.date || "9999-12-31";
        return dateA.localeCompare(dateB);
    });

    // 4. Build ledger rows
    transactions.forEach((t) => {
        let madin = 0;       // مدين
        let dain = 0;        // دائن
        let periodChange = 0;

        if (t.type === "invoice") {
            if (t.isSalesInvoice) {
                madin = t.grossAmount;
                periodChange = +t.grossAmount;
            } else if (t.isPurchaseInvoice) {
                dain = t.grossAmount;
                periodChange = -t.grossAmount;
            }
        } else if (t.type === "payment") {
            if (t.isReceiptFromParty) {
                dain = t.grossAmount;
                periodChange = -t.grossAmount;
            } else if (t.isDisbursementToParty) {
                madin = t.grossAmount;
                periodChange = +t.grossAmount;
            }
        } else if (t.type === "rentalPosting") {
            const flag = (t.debitCreditFlag || "").trim().toUpperCase();
            if (flag === "D") {
                madin = t.grossAmount;
                periodChange = +t.grossAmount;
            } else if (flag === "C") {
                dain = t.grossAmount;
                periodChange = -t.grossAmount;
            }
        } else if (t.type === "partnerAccrual") {
            const flag = (t.debitCreditFlag || "").trim().toUpperCase();
            if (flag === "D") {
                madin = t.grossAmount;
                periodChange = +t.grossAmount;
            } else if (flag === "C") {
                dain = t.grossAmount;
                periodChange = -t.grossAmount;
            }
        } else if (t.type === "apartmentSale") {
            const flag = (t.debitCreditFlag || "").trim().toUpperCase();
            if (flag === "D") {
                madin = t.grossAmount;
                periodChange = +t.grossAmount;
            } else if (flag === "C") {
                dain = t.grossAmount;
                periodChange = -t.grossAmount;
            }
        }
        // ──────────────── NEW ────────────────
        else if (t.type === "chequeIssued") {
            const flag = (t.debitCreditFlag || "").trim().toUpperCase();
            if (flag === "D") {
                madin = t.grossAmount;
                periodChange = +t.grossAmount;     // ← increases what contractor owes us
            } else if (flag === "C") {
                dain = t.grossAmount;
                periodChange = -t.grossAmount;
            }
        }

        runningBalance += periodChange;

        rows.push({
            date: formatDate(t.date),
            description: t.description,
            invoiceNumber: t.type === "invoice" ? t.id : undefined,
            paymentNumber: t.type === "payment" ? t.id : undefined,
            value: t.grossAmount,
            toPay: dain,           // دائن
            paid: madin,           // مدين
            balance: Math.round(runningBalance * 100) / 100,
            transactionType: t.type,
            notes: t.transactionTypeId ? `(${t.transactionTypeId})` : undefined,
        });
    });

    return rows;
}, [data, getTranslatedLabel]);