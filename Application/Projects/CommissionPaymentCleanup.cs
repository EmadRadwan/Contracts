using Application.Accounting.Payments;
using Application.Accounting.Services;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

/// <summary>
/// Removes the commission payments generated for a sales request.
///
/// Step 3 of the auditor's soft-delete requirement (Sep 2026): a commission payment that is still
/// a draft is purged with everything hanging off it; one that has been disbursed is voided instead,
/// which keeps the row, reverses its ledger entries and cancels its bank transaction.
/// </summary>
internal static class CommissionPaymentCleanup
{
    public const string CommissionPaymentTypeId = "COMMISSION_PAYMENT";

    public class Outcome
    {
        public int Purged { get; set; }
        public int Voided { get; set; }
    }

    public static async Task<Outcome> PurgeOrVoidAsync(DataContext context, IPaymentVoidService voidService,
        string? salesRequestId, string reason, CancellationToken ct)
    {
        var outcome = new Outcome();
        if (string.IsNullOrEmpty(salesRequestId)) return outcome;

        var payments = await context.Payments
            .Where(p => p.SalesRequestId == salesRequestId
                        && p.PaymentTypeId == CommissionPaymentTypeId)
            .ToListAsync(ct);

        var drafts = new List<Payment>();
        foreach (var payment in payments)
        {
            if (payment.StatusId == "PMNT_VOID")
                continue; // already handled by an earlier void

            if (payment.StatusId == "PMNT_NOT_PAID")
            {
                drafts.Add(payment);
                continue;
            }

            var voided = await voidService.VoidPaymentAsync(payment.PaymentId, reason, ct);
            if (!voided.IsSuccess)
                throw new InvalidOperationException(voided.ErrorMessage);
            outcome.Voided++;
        }

        await PaymentArtifactCleanup.PurgePaymentsAsync(context, drafts, ct);
        outcome.Purged = drafts.Count;
        return outcome;
    }
}
