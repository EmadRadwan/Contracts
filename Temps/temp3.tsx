const ledgerItems = useMemo((): LedgerRow[] => {
    if (!data) return [];

    const rows: LedgerRow[] = [];
    let balance = 0;

    const fmt = (d: string | null | undefined) =>
        d ? new Date(d).toISOString().split('T')[0] : '';

    // 1. Opening Balance (same as before)
    const openingEntries = data.openingBalances || [];
    const openingImpact = openingEntries.reduce((sum, ob) => sum + ob.impactOnBalance, 0);
    if (openingImpact !== 0) {
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

    // 2. Collect all transactions
    interface Transaction {
        date: string;
        displayDate: string;
        type: 'invoice' | 'payment' | 'rentalPosting' | 'partnerAccrual';
        id?: string;
        description: string;
        amount: number;
        impact: number;           // signed: + = party owes more
        notes?: string;
    }

    const transactions: Transaction[] = [];

    // ── Invoices ──
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
            description: `فاتورة ${id}`,
            amount: inv.total,
            impact: inv.total,        // usually positive
        });
    });

    // ── Payments ──
    const paymentMap = new Map<string, { amount: number; date: string }>();
    // ... (your existing payment collection logic remains the same)

    paymentMap.forEach((pay, id) => {
        const isUnapplied = data.unappliedPayments?.some(up => up.paymentId === id);
        transactions.push({
            date: pay.date,
            displayDate: fmt(pay.date),
            type: 'payment',
            id,
            description: `دفعة ${id}`,
            amount: pay.amount,
            impact: -pay.amount,
            notes: isUnapplied ? 'غير مطبقة' : undefined,
        });
    });

    // ── Rental Property Postings (debits mostly) ──
    data.rentalPropertyPostings?.forEach(rp => {
        if (!rp.transactionDate) return;
        transactions.push({
            date: rp.transactionDate,
            displayDate: fmt(rp.transactionDate),
            type: 'rentalPosting',
            description: `إيجار - ${rp.glAccountTypeId || 'عقاري'}`,
            amount: Math.abs(rp.impactOnBalance),
            impact: rp.impactOnBalance,
            notes: rp.description || 'تسجيل إيجاري',
        });
    });

    // ── NEW: Partner Accruals (usually credits → partner share) ──
    const partnerAccruals = data.partnerAccrualPostings || []; // ← you need to add this field in PartyFinancialHistoryDetails
    partnerAccruals.forEach(pa => {
        if (!pa.transactionDate) return;
        transactions.push({
            date: pa.transactionDate,
            displayDate: fmt(pa.transactionDate),
            type: 'partnerAccrual',
            description: `استحقاق شركاء - ${pa.glAccountTypeId || 'إيراد عقاري'}`,
            amount: Math.abs(pa.impactOnBalance),
            impact: pa.impactOnBalance,           // most likely negative
            notes: pa.description || 'استحقاق للشركاء',
        });
    });

    // 3. Sort all transactions chronologically
    transactions.sort((a, b) => a.date.localeCompare(b.date));

    // 4. Build ledger rows
    transactions.forEach(t => {
        balance -= t.impact; // ← very important: positive impact increases what party owes

        if (t.type === 'invoice') {
            rows.push({
                date: t.displayDate,
                description: t.description,
                invoiceNumber: t.id,
                value: t.amount,
                toPay: t.amount,
                paid: 0,
                balance,
                transactionType: t.type,
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
                transactionType: t.type,
            });
        } else if (t.type === 'rentalPosting' || t.type === 'partnerAccrual') {
            rows.push({
                date: t.displayDate,
                description: t.description,
                value: t.amount,
                toPay: t.impact > 0 ? t.amount : 0,
                paid: t.impact < 0 ? Math.abs(t.impact) : 0,
                balance,
                notes: t.notes,
                transactionType: t.type,
            });
        }
    });

    return rows;
}, [data]);