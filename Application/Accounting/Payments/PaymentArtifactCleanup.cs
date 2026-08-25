using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Accounting.Payments;

/// <summary>
/// Hard-deletes a set of payments together with every artifact they spawned — ledger transactions
/// (both the ones hanging off the payment and the ones hanging off its bank-side transaction),
/// their entries, attributes and reconciliation rows, plus the payment's own satellite rows.
///
/// Extracted from the commission cleanup so the sales-request delete path cannot drift from it:
/// every caller that removes a payment has to remove the same fan-out of dependents, otherwise the
/// FK constraints (all NO ACTION in this schema) reject the whole SaveChanges.
///
/// NOTE: this is a hard delete and it does NOT spare already-disbursed payments (PMNT_SENT /
/// PMNT_CONFIRMED) — callers are expected to warn the user first. Caller owns the transaction and
/// calls SaveChanges.
/// </summary>
internal static class PaymentArtifactCleanup
{
    /// <summary>
    /// Removes the given accounting transactions along with their entries, attributes and GL
    /// reconciliation rows. Ids rather than entities so callers don't have to remember the Includes.
    /// </summary>
    public static async Task PurgeAcctgTransAsync(
        DataContext context, IReadOnlyCollection<string> acctgTransIds, CancellationToken ct)
    {
        if (acctgTransIds.Count == 0) return;

        // Materialise once — EF translates Contains against a List parameter reliably.
        var ids = acctgTransIds as List<string> ?? acctgTransIds.ToList();

        var acctgTransList = await context.AcctgTrans
            .Include(t => t.AcctgTransEntries)
            .Include(t => t.AcctgTransAttributes)
            .Where(t => ids.Contains(t.AcctgTransId))
            .ToListAsync(ct);

        if (!acctgTransList.Any()) return;

        // GL reconciliation rows sit on top of the ledger entries, so they go first.
        var reconciliationEntries = await context.GlReconciliationEntries
            .Where(e => ids.Contains(e.AcctgTransId))
            .ToListAsync(ct);
        context.GlReconciliationEntries.RemoveRange(reconciliationEntries);

        foreach (var tran in acctgTransList)
        {
            context.AcctgTransEntries.RemoveRange(tran.AcctgTransEntries);
            context.AcctgTransAttributes.RemoveRange(tran.AcctgTransAttributes);
        }

        context.AcctgTrans.RemoveRange(acctgTransList);
    }

    /// <summary>
    /// Removes the given payments and everything that points at them.
    /// </summary>
    public static async Task PurgePaymentsAsync(
        DataContext context, IReadOnlyCollection<Payment> payments, CancellationToken ct)
    {
        if (payments.Count == 0) return;

        var paymentIds = payments.Select(p => p.PaymentId).ToList();

        // Bank-side transactions (cheque withdrawals). Grab their ids first — the CHECK_ISSUED
        // AcctgTrans hangs off the FinAccountTrans rather than off the payment.
        var finAccountTrans = await context.FinAccountTrans
            .Where(fat => fat.PaymentId != null && paymentIds.Contains(fat.PaymentId))
            .ToListAsync(ct);
        var finAccountTransIds = finAccountTrans.Select(fat => fat.FinAccountTransId).ToList();

        var acctgTransIds = await context.AcctgTrans
            .Where(t => (t.PaymentId != null && paymentIds.Contains(t.PaymentId))
                        || (t.FinAccountTransId != null && finAccountTransIds.Contains(t.FinAccountTransId)))
            .Select(t => t.AcctgTransId)
            .ToListAsync(ct);

        await PurgeAcctgTransAsync(context, acctgTransIds, ct);

        var finAccountTransAttributes = await context.FinAccountTransAttributes
            .Where(a => finAccountTransIds.Contains(a.FinAccountTransId))
            .ToListAsync(ct);
        context.FinAccountTransAttributes.RemoveRange(finAccountTransAttributes);

        // Null the FK first to break the payment <-> FinAccountTrans cycle.
        foreach (var fat in finAccountTrans)
            fat.PaymentId = null;

        context.FinAccountTrans.RemoveRange(finAccountTrans);

        var paymentApplications = await context.PaymentApplications
            .Where(pa => (pa.PaymentId != null && paymentIds.Contains(pa.PaymentId))
                         || (pa.ToPaymentId != null && paymentIds.Contains(pa.ToPaymentId)))
            .ToListAsync(ct);
        context.PaymentApplications.RemoveRange(paymentApplications);

        var paymentAttributes = await context.PaymentAttributes
            .Where(a => paymentIds.Contains(a.PaymentId))
            .ToListAsync(ct);
        context.PaymentAttributes.RemoveRange(paymentAttributes);

        var paymentGroupMembers = await context.PaymentGroupMembers
            .Where(m => paymentIds.Contains(m.PaymentId))
            .ToListAsync(ct);
        context.PaymentGroupMembers.RemoveRange(paymentGroupMembers);

        var paymentBudgetAllocations = await context.PaymentBudgetAllocations
            .Where(b => paymentIds.Contains(b.PaymentId))
            .ToListAsync(ct);
        context.PaymentBudgetAllocations.RemoveRange(paymentBudgetAllocations);

        var paymentContents = await context.PaymentContents
            .Where(c => paymentIds.Contains(c.PaymentId))
            .ToListAsync(ct);
        context.PaymentContents.RemoveRange(paymentContents);

        // Payroll records can point at a payment (a commission settled against an advance or a
        // deduction). Those are HR records in their own right, so detach rather than delete — this
        // only stops the FK from blocking the purge.
        var deductions = await context.Deductions
            .Where(d => d.PaymentId != null && paymentIds.Contains(d.PaymentId))
            .ToListAsync(ct);
        foreach (var deduction in deductions)
            deduction.PaymentId = null;

        var employeeAdvances = await context.EmployeeAdvances
            .Where(a => a.PaymentId != null && paymentIds.Contains(a.PaymentId))
            .ToListAsync(ct);
        foreach (var advance in employeeAdvances)
            advance.PaymentId = null;

        context.Payments.RemoveRange(payments);
    }
}
