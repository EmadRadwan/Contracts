const ledgerItems = useMemo((): LedgerRow[] => {
    if (!data) return [];

    const rows: LedgerRow[] = [];
    let runningBalance = 0;  // ← هيبقى بالمنطق المصري: موجب = لصالحنا

    const fmt = (d: string) => new Date(d).toISOString().split('T')[0];

    // 1. Invoices + Applied Payments
    data.invoicesApplPayments?.forEach((inv) => {
        const invTotal = inv.total || 0;
        const invApplied = inv.amountApplied || 0;

        // فاتورة شراء → تزيد الدين علينا → سالب في المنطق المصري → لكن بنعكسها
        runningBalance -= invTotal;  // ← عكس اللي كان موجود
        rows.push({
            date: fmt(inv.invoiceDate),
            description: `فاتورة رقم ${inv.invoiceId}`,
            invoiceNumber: inv.invoiceId,
            value: invTotal,
            toPay: invTotal,
            paid: 0,
            balance: runningBalance,   // ← دلوقتي سالب لو فاتورة شراء
            notes: inv.invoiceTypeId === 'PURCHASE_INVOICE' ? 'شراء' : 'بيع',
        });

        if (inv.paymentId && invApplied > 0) {
            runningBalance += invApplied;  // ← الدفع يقلل الدين → موجب
            rows.push({
                date: fmt(inv.paymentEffectiveDate),
                description: `دفعة رقم ${inv.paymentId}`,
                paymentNumber: inv.paymentId,
                value: 0,
                toPay: 0,
                paid: invApplied,
                balance: runningBalance,
                notes: '',
            });
        }
    });

    // 2. Unapplied Payments (مقدمات)
    data.unappliedPayments?.forEach((pay) => {
        const amount = pay.unappliedAmount || pay.amount || 0;
        runningBalance += amount;  // ← دفعة غير موزعة = مقدم = لصالحنا = موجب
        rows.push({
            date: fmt(pay.effectiveDate),
            description: `دفعة غير موزعة ${pay.paymentId}`,
            paymentNumber: pay.paymentId,
            value: 0,
            toPay: 0,
            paid: amount,
            balance: runningBalance,
            notes: pay.paymentTypeDescription || 'غير موزعة',
        });
    });

    return rows;
}, [data]);