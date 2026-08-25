using Application.Accounting.Payments;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

/// <summary>
/// Removes the payments a sales commission generated when it was approved, together with every
/// artifact those payments spawned (ledger entries, bank-side transactions, reconciliation rows,
/// attributes and applications).
///
/// Shared by <see cref="ResetSalesCommission"/> and <see cref="DeleteSalesCommission"/> so the two
/// paths can never drift apart. The fan-out itself lives in <see cref="PaymentArtifactCleanup"/>,
/// which the sales-request delete path reuses; this class only decides *which* payments belong to a
/// commission.
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

        await PaymentArtifactCleanup.PurgePaymentsAsync(context, payments, ct);
    }
}
