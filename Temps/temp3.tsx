// ───────────────────────────────────────────────────────────────
// 3. Sort all transactions chronologically (safe)
// ───────────────────────────────────────────────────────────────
transactions.sort((a, b) => {
    const dateA = a.date || '9999-12-31'; // push missing dates to the end
    const dateB = b.date || '9999-12-31';
    return dateA.localeCompare(dateB);
});

// Or even better — filter first:
const validTransactions = transactions.filter(t => !!t.date);

validTransactions.sort((a, b) => a.date.localeCompare(b.date));

// Then use validTransactions in the .forEach loop instead:
validTransactions.forEach((t) => {
    // your existing row creation code...
});