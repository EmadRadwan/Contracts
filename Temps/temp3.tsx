const ledgerItems = useMemo((): LedgerRow[] => {
    if (!data) return [];

    const rows: LedgerRow[] = [];
    let balance = 0; // Positive = party owes us more, Negative = we owe party more

    const fmt = (d: string | null | undefined) => d ? new Date(d).toISOString().split('T')[0] : '';

    // === 1. Opening Balance (only traditional opening entries) ===
    const openingEntries = data.openingBalances || [];
    const openingImpact = openingEntries.reduce((sum, ob) => sum + ob.impactOnBalance, 0);

    if (openingImpact !== 0) {
        // Backend: positive impactOnBalance = customer owes us more
        // Ledger convention here: we apply the opposite to start correctly
        balance -= openingImpact;

        rows.push({
            date: '',
            description: 'الرصيد الافتتاحي',
            value: 0,
            toPay: openingImpact > 0 ? openingImpact : 0,
            paid: openingImpact < 0 ? -openingImpact : 0,
            balance,
            notes: openingEntries.length > 1 ? `${openingEntries.length} حركات افتتاحية` : 'حركة افتتاحية',
        });
    }

    // === 2. Unified transaction interface ===
    interface Transaction {
        date: string;           // ISO string for sorting
        displayDate: string;    // Formatted YYYY-MM-DD
        type: 'invoice' | 'payment' | 'rentalPosting';
        id?: string;
        description: string;
        amount: number;         // Absolute amount for display
        impact: number;         // Signed impact on balance (positive = customer owes more)
        notes?: string;
    }

    const transactions: Transaction[] = [];

    // Invoices
    const invoiceMap = new Map<string, { total: number; date: string }>();
    data.invoicesApplPayments?.forEach(i => {
        if (!invoiceMap.has(i.invoiceId)) {
            invoiceMap.set(i.invoiceId, { total: i.total, date: i.invoiceDate! });
        }
    });
    data.unappliedInvoices?.forEach(i => {
        if (!invoiceMap.has(i.invoiceId)) {
            invoiceMap.set(i.invoiceId, { total: i.amount, date: i.invoiceDate! });
        }
    });

    invoiceMap.forEach((inv, id) => {
        transactions.push({
            date: inv.date,
            displayDate: fmt(inv.date),
            type: 'invoice',
            id,
            description: `فاتورة شراء رقم ${id}`,
            amount: inv.total,
            impact: inv.total, // Invoice increases receivable
            notes: undefined,
        });
    });

    // Payments
    const paymentMap = new Map<string, { amount: number; date: string }>();
    data.invoicesApplPayments?.forEach(i => {
        if (i.paymentId) {
            paymentMap.set(i.paymentId, {
                amount: i.paymentAmount,
                date: i.paymentEffectiveDate || i.invoiceDate!,
            });
        }
    });
    data.unappliedPayments?.forEach(p => {
        paymentMap.set(p.paymentId, { amount: p.amount, date: p.effectiveDate! });
    });

    paymentMap.forEach((pay, id) => {
        const isUnapplied = data.unappliedPayments?.some(up => up.paymentId === id);
        transactions.push({
            date: pay.date,
            displayDate: fmt(pay.date),
            type: 'payment',
            id,
            description: `دفعة رقم ${id}`,
            amount: pay.amount,
            impact: -pay.amount, // Payment reduces receivable
            notes: isUnapplied ? 'دفعة غير مطبقة' : undefined,
        });
    });

    // Rental Property Postings
    const rentalEntries = data.rentalPropertyPostings || [];
    rentalEntries.forEach(rp => {
        if (!rp.transactionDate) return;

        transactions.push({
            date: rp.transactionDate,
            displayDate: fmt(rp.transactionDate),
            type: 'rentalPosting',
            description: `تسجيل إيجاري - ${rp.glAccountTypeId || 'إيجار'}`,
            amount: Math.abs(rp.impactOnBalance),
            impact: rp.impactOnBalance, // Already correctly signed from backend
            notes: rp.description || 'تسوية إيجارية',
        });
    });

    // === 3. Sort chronologically ===
    transactions.sort((a, b) => a.date.localeCompare(b.date));

    // === 4. Process all transactions and update balance ===
    transactions.forEach(t => {
        balance -= t.impact; // Apply signed impact

        if (t.type === 'invoice') {
            rows.push({
                date: t.displayDate,
                description: t.description,
                invoiceNumber: t.id,
                value: t.amount,
                toPay: t.amount,
                paid: 0,
                balance,
            });
        } else if (t.type === 'payment') {
            rows.push({
                date: t.displayDate,
                description: t.description,
                paymentNumber: t.id,
                value: 0,
                toPay: 0,
                paid: t.amount,
                balance,
                notes: t.notes,
            });
        } else if (t.type === 'rentalPosting') {
            rows.push({
                date: t.displayDate,
                description: t.description,
                value: t.amount,
                toPay: t.impact > 0 ? t.amount : 0,
                paid: t.impact < 0 ? t.amount : 0,
                balance,
                notes: t.notes,
            });
        }
    });

    return rows;
}, [data]);