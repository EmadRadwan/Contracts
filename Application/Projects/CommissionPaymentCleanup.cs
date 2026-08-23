using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

/// <summary>
/// Removes the payments a sales commission generated when it was approved, together with every
/// artifact those payments spawned (ledger entries, bank-side transactions, reconciliation rows,
/// attributes and applications).
///
/// Shared by <see cref="ResetSalesCommission"/> and <see cref="DeleteSalesCommission"/> so the two
/// paths can never drift apart.
///
/// NOTE: this is a hard delete and it does NOT spare already-disbursed payments (PMNT_SENT /
/// PMNT_CONFIRMED). Both callers warn the user up front that approved commission payments and their
/// accounting entries are wiped, so real financial history is discarded on their explicit
/// confirmation rather than reversed. Caller is responsible for the transaction and SaveChanges.
/// </summary>
internal static class CommissionPaymentCleanup
{
    public const string CommissionPaymentTypeId = "COMMISSION_PAYMENT";

    public static async Task PurgeAsync(DataContext context, string? salesRequestId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(salesRequestId)) return;

        var payments = await context.Payments
            .Where(p => p.SalesRequestId == salesRequestId
                        && p.PaymentTypeId == CommissionPaymentTypeId)
            .ToListAsync(ct);

        if (!payments.Any()) return;

        var paymentIds = payments.Select(p => p.PaymentId).ToList();

        // Bank-side transactions (cheque withdrawals). Grab their ids first — the CHECK_ISSUED
        // AcctgTrans hangs off the FinAccountTrans rather than off the payment.
        var finAccountTrans = await context.FinAccountTrans
            .Where(fat => fat.PaymentId != null && paymentIds.Contains(fat.PaymentId))
            .ToListAsync(ct);
        var finAccountTransIds = finAccountTrans.Select(fat => fat.FinAccountTransId).ToList();

        var acctgTransList = await context.AcctgTrans
            .Include(t => t.AcctgTransEntries)
            .Include(t => t.AcctgTransAttributes)
            .Where(t => (t.PaymentId != null && paymentIds.Contains(t.PaymentId))
                        || (t.FinAccountTransId != null && finAccountTransIds.Contains(t.FinAccountTransId)))
            .ToListAsync(ct);
        var acctgTransIds = acctgTransList.Select(t => t.AcctgTransId).ToList();

        // GL reconciliation rows sit on top of the ledger entries, so they go first.
        var reconciliationEntries = await context.GlReconciliationEntries
            .Where(e => acctgTransIds.Contains(e.AcctgTransId))
            .ToListAsync(ct);
        context.GlReconciliationEntries.RemoveRange(reconciliationEntries);

        foreach (var tran in acctgTransList)
        {
            context.AcctgTransEntries.RemoveRange(tran.AcctgTransEntries);
            context.AcctgTransAttributes.RemoveRange(tran.AcctgTransAttributes);
        }

        context.AcctgTrans.RemoveRange(acctgTransList);

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

        // Payroll records can point at a commission payment (a commission settled against an
        // advance or a deduction). Those are HR records in their own right, so detach rather than
        // delete — this only stops the FK from blocking the purge.
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
