// 4. Build rows – correct sign inversion for external view
transactions.forEach((t) => {
    let madin = 0; // مدين
    let dain = 0;  // دائن

    if (t.type === "invoice") {
        if (t.isSalesInvoice) {
            // Sales: they owe us more → in external view = we owe them less → dain increases balance
            dain = t.grossAmount;
        } else if (t.isPurchaseInvoice) {
            // Purchase: we owe them → dain (external) or madin (company)
            isExternalView ? (dain = t.grossAmount) : (madin = t.grossAmount);
        }
    } else if (t.type === "payment") {
        if (t.isReceiptFromParty) {
            // They paid us → we owe them more → balance decreases
            isExternalView ? (dain = t.grossAmount) : (madin = t.grossAmount);
        } else if (t.isDisbursementToParty) {
            // We paid them → we owe them less → balance increases
            isExternalView ? (madin = t.grossAmount) : (dain = t.grossAmount);
        }
    } else if (t.type === "rental" && t.isRentalDebit) {
        // Rental debit: they owe us more → dain
        dain = t.grossAmount;
    } else if (t.type === "partnerAccrual" && t.isPartnerCredit) {
        // Partner accrual: we owe them more → balance decreases → dain in external
        isExternalView ? (dain = t.grossAmount) : (madin = t.grossAmount);
    }

    // Critical: in external view, credit (dain) means we owe more → subtract from balance
    // in company view, credit (dain) means they owe more → add to balance
    const periodChange = isExternalView ? (madin - dain) : (dain - madin);
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
        toPay: dain,   // دائن column
        paid: madin,   // مدين column
        balance: Math.round(runningBalance * 100) / 100,
        notes,
        transactionType: t.type,
    });
});