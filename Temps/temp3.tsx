// REFACTOR: FINAL VERSION – Invoices always first, same-day sorting by type
// Key Change: Normalize sortDate to DAY-ONLY (YYYY-MM-DD) to ignore timestamps
// Ensures: Invoice (0) → Payment (1) → Application (2) within same day
// Matches Egyptian accounting standards (كشف حساب رسمي)

const ledgerItems = useMemo((): LedgerRow[] => {
    if (!data) return [];

    const events: Array<{
        date: string; // Display date (YYYY-MM-DD)
        sortDate: Date; // Normalized to start of day (YYYY-MM-DD 00:00:00)
        secondarySort: number; // 0=invoice, 1=payment, 2=application
        invoiceId?: string;
        paymentId?: string;
        amount: number; // + = invoice (we owe), - = payment (we paid)
        description: string;
        notes?: string;
    }> = [];

    // Helper: Format date to YYYY-MM-DD
    const fmt = (d: string | null | undefined) => d ? new Date(d).toISOString().split('T')[0] : '';

    // Helper: Normalize to start of day (YYYY-MM-DD 00:00:00)
    const toDayStart = (d: string | null | undefined) => {
        if (!d) return new Date(0);
        const date = new Date(d);
        date.setHours(0, 0, 0, 0); // Reset to midnight
        return date;
    };

    // === 1. INVOICES – Priority 0 ===
    const invoiceMap = new Map<string, { total: number; date: string }>();

    data.invoicesApplPayments?.forEach(item => {
        if (!invoiceMap.has(item.invoiceId)) {
            invoiceMap.set(item.invoiceId, {
                total: item.total,
                date: item.invoiceDate!,
            });
        }
    });

    data.unappliedInvoices?.forEach(item => {
        if (!invoiceMap.has(item.invoiceId)) {
            invoiceMap.set(item.invoiceId, {
                total: item.amount,
                date: item.invoiceDate!,
            });
        }
    });

    invoiceMap.forEach((inv, id) => {
        events.push({
            date: fmt(inv.date),
            sortDate: toDayStart(inv.date),
            secondarySort: 0,
            invoiceId: id,
            amount: inv.total,
            description: `فاتورة شراء رقم ${id}`,
        });
    });

    // === 2. PAYMENTS – Priority 1 ===
    const paymentMap = new Map<string, { amount: number; date: string }>();

    data.invoicesApplPayments?.forEach(item => {
        if (item.paymentId && item.paymentAmount > 0) {
            paymentMap.set(item.paymentId, {
                amount: item.paymentAmount,
                date: item.paymentEffectiveDate || item.invoiceDate!,
            });
        }
    });

    data.unappliedPayments?.forEach(p => {
        paymentMap.set(p.paymentId, {
            amount: p.amount,
            date: p.effectiveDate!,
        });
    });

    paymentMap.forEach((pay, id) => {
        events.push({
            date: fmt(pay.date),
            sortDate: toDayStart(pay.date),
            secondarySort: 1,
            paymentId: id,
            amount: -pay.amount,
            description: `دفعة رقم ${id}`,
            notes: data.unappliedPayments?.some(p => p.paymentId === id) ? 'دفعة غير مطبقة' : undefined,
        });
    });

    // === 3. APPLICATIONS – Priority 2, use INVOICE date ===
    data.invoicesApplPayments?.forEach(app => {
        if (app.paymentId && app.amountApplied > 0) {
            // Use invoice date for application
            const invoice = data.invoicesApplPayments?.find(i => i.invoiceId === app.invoiceId);
            const appDate = invoice?.invoiceDate || app.invoiceDate;

            events.push({
                date: fmt(appDate),
                sortDate: toDayStart(appDate),
                secondarySort: 2,
                invoiceId: app.invoiceId,
                paymentId: app.paymentId,
                amount: 0,
                description: `تطبيق دفعة ${app.paymentId} على فاتورة ${app.invoiceId}`,
                notes: `مطبق: ${app.amountApplied.toFixed(3)}`,
            });
        }
    });

    // === SORT: By normalized day (YYYY-MM-DD), then by secondarySort ===
    events.sort((a, b) => {
        const dateDiff = a.sortDate.getTime() - b.sortDate.getTime();
        if (dateDiff !== 0) return dateDiff;
        return a.secondarySort - b.secondarySort; // 0=invoice, 1=payment, 2=application
    });

    // === Build ledger ===
    let balance = 0;
    return events.map(event => {
        balance += event.amount;
        return {
            date: event.date,
            description: event.description,
            invoiceNumber: event.invoiceId,
            paymentNumber: event.paymentId,
            value: event.amount > 0 ? event.amount : 0,
            toPay: event.amount > 0 ? event.amount : 0,
            paid: event.amount < 0 ? -event.amount : 0,
            balance,
            notes: event.notes,
        };
    });
}, [data]);