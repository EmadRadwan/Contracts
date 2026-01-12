// 4. Build rows – explicit periodChange per transaction type
transactions.forEach((t) => {
    let madin = 0; // مدين
    let dain = 0;  // دائن
    let periodChange = 0;

    if (t.type === "invoice") {
        if (t.isSalesInvoice) {
            dain = t.grossAmount;      // sales → party owes us more → balance increases
            periodChange = +t.grossAmount;  // positive
        } else if (t.isPurchaseInvoice) {
            madin = t.grossAmount;     // purchase → we owe more → balance decreases
            periodChange = -t.grossAmount;  // negative
        }
    } else if (t.type === "payment") {
        if (t.isReceiptFromParty) {
            dain = t.grossAmount;      // party paid us → we owe more → balance decreases
            periodChange = -t.grossAmount;  // negative
        } else if (t.isDisbursementToParty) {
            madin = t.grossAmount;     // we paid party → owe less → balance increases
            periodChange = +t.grossAmount;  // positive
        }
    } else if (t.type === "rental") {
        const flag = (t.debitCreditFlag || "").trim().toUpperCase();
        if (flag === "D") {
            madin = t.grossAmount;
            periodChange = -t.grossAmount;  // rental debit → owe more → negative
        } else if (flag === "C") {
            dain = t.grossAmount;
            periodChange = +t.grossAmount;  // rental credit → owe less → positive
        }
    } else if (t.type === "partnerAccrual") {
        const flag = (t.debitCreditFlag || "").trim().toUpperCase();
        if (flag === "D") {
            madin = t.grossAmount;
            periodChange = -t.grossAmount;  // accrual debit → owe less → positive
        } else if (flag === "C") {
            dain = t.grossAmount;
            periodChange = +t.grossAmount;  // accrual credit → owe more → negative
        }
    }

    runningBalance += periodChange;

    const notes =
        t.type === "payment" &&
        data.unappliedPayments?.some((p) => p.paymentId === t.id)
            ? "غير مطبقة"
            : undefined;

    rows.push({
        date: formatDate(t.date),
        description: t.description,
        invoiceNumber: t.type === "invoice" ? t.id : undefined,
        paymentNumber: t.type === "payment" ? t.id : undefined,
        value: t.grossAmount,
        toPay: dain,
        paid: madin,
        balance: Math.round(runningBalance * 100) / 100,
        notes,
        transactionType: t.type,
    });
});