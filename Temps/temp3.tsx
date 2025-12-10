// REFACTOR: Include Opening Balance as the very first ledger row
// Why: This is what accountants expect — "كشف الحساب" must start with the opening balance
// Without this, the running balance is wrong and doesn't match the final total
const ledgerItems = useMemo((): LedgerRow[] => {
    if (!data) return [];

    const rows: LedgerRow[] = [];
    let balance = 0;

    const fmt = (d: string | null | undefined) => d ? new Date(d).toISOString().split('T')[0] : '';

    // ===== 1. ADD OPENING BALANCE AS FIRST ROW (if exists) =====
    const openingBalanceEntries = data.openingBalances || [];
    if (openingBalanceEntries.length > 0) {
        // Sum all opening balance impacts (already correctly signed in backend)
        const openingTotal = openingBalanceEntries
            .reduce((sum, ob) => sum + ob.impactOnBalance, 0);

        if (openingTotal !== 0) {
            balance += openingTotal;

            rows.push({
                date: '', // or use earliest transaction date if available
                description: 'الرصيد الافتتاحي',
                invoiceNumber: undefined,
                paymentNumber: undefined,
                value: 0,
                toPay: openingTotal > 0 ? openingTotal : 0,     // إذا كان مدين (نحن ندين له)
                paid: openingTotal < 0 ? -openingTotal : 0,     // إذا كان دائن (هو مدين لنا)
                balance: balance,
                notes: `${openingBalanceEntries.length} حركة افتتاحية`,
            });
        }
    }

    // ===== 2. All invoices (applied + unapplied) =====
    const invoices = new Map<string, { id: string; total: number; date: string }>();
    data.invoicesApplPayments?.forEach(i => invoices.set(i.invoiceId, {
        id: i.invoiceId,
        total: i.total,
        date: i.invoiceDate!
    }));
    data.unappliedInvoices?.forEach(i => invoices.set(i.invoiceId, {
        id: i.invoiceId,
        total: i.amount,
        date: i.invoiceDate!
    }));

    Array.from(invoices.values())
        .sort((a, b) => a.date.localeCompare(b.date))
        .forEach(inv => {
            balance += inv.total;
            rows.push({
                date: fmt(inv.date),
                description: `فاتورة شراء رقم ${inv.id}`,
                invoiceNumber: inv.id,
                value: inv.total,
                toPay: inv.total,
                paid: 0,
                balance,
            });
        });

    // ===== 3. All payments =====
    const payments = new Map<string, { id: string; amount: number; date: string }>();
    data.invoicesApplPayments?.forEach(i => {
        if (i.paymentId) payments.set(i.paymentId, {
            id: i.paymentId,
            amount: i.paymentAmount,
            date: i.paymentEffectiveDate || i.invoiceDate!
        });
    });
    data.unappliedPayments?.forEach(p => payments.set(p.paymentId, {
        id: p.paymentId,
        amount: p.amount,
        date: p.effectiveDate!
    }));

    Array.from(payments.values())
        .sort((a, b) => a.date.localeCompare(b.date))
        .forEach(pay => {
            balance -= pay.amount;
            rows.push({
                date: fmt(pay.date),
                description: `دفعة رقم ${pay.id}`,
                paymentNumber: pay.id,
                value: 0,
                toPay: 0,
                paid: pay.amount,
                balance,
                notes: data.unappliedPayments?.some(p => p.paymentId === pay.id) ? 'دفعة غير مطبقة' : undefined,
            });
        });

    return rows;
}, [data]);