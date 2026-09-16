using Application.Accounting.Services;
using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Microsoft.Extensions.Logging;

namespace Application.Accounting.Payments;

/// <summary>
/// Returns a sent or received payment to draft so it can be corrected and re-sent.
///
/// Step 3 of the auditor's soft-delete requirement (Sep 2026): the posted accounting transactions
/// are no longer deleted. Each is cancelled by a linked reversing transaction, so the ledger keeps
/// the original, the reversal, and later the re-posted entry. Payment applications are removed
/// through the service that also returns paid invoices to READY. The bank transaction row is left
/// alone: it is created with the payment at draft time and belongs to the payment, not to its
/// posting.
/// </summary>
public class ResetPayment
{
    public class Command : IRequest<Results<PaymentDto>>
    {
        public string PaymentId { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }

    public class Handler : IRequestHandler<Command, Results<PaymentDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;
        private readonly IAccountingPeriodGuard _periodGuard;
        private readonly IAcctgTransReversalService _reversal;
        private readonly IPaymentApplicationService _paymentApplicationService;

        public Handler(DataContext context, ILogger<Handler> logger, IAccountingPeriodGuard periodGuard,
            IAcctgTransReversalService reversal, IPaymentApplicationService paymentApplicationService)
        {
            _context = context;
            _logger = logger;
            _periodGuard = periodGuard;
            _reversal = reversal;
            _paymentApplicationService = paymentApplicationService;
        }

        public async Task<Results<PaymentDto>> Handle(Command request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.PaymentId))
            {
                return Results<PaymentDto>.Failure("Payment ID is required", "INVALID_INPUT");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.PaymentId == request.PaymentId, ct);

                if (payment == null)
                {
                    return Results<PaymentDto>.Failure("Payment not found", "PAYMENT_NOT_FOUND");
                }

                if (payment.StatusId is "PMNT_VOID" or "PMNT_CANCELLED" or "PMNT_CONFIRMED")
                {
                    return Results<PaymentDto>.Failure(
                        $"Cannot reset payment in status '{payment.StatusId}'",
                        "INVALID_PAYMENT_STATUS"
                    );
                }

                // Closed-period control on the posted rows about to be reversed.
                await _periodGuard.EnsureOpenForPaymentsAsync(new[] { request.PaymentId }, ct);

                var stamp = DateTime.UtcNow;
                var reason = string.IsNullOrWhiteSpace(request.Reason)
                    ? $"Reset payment {request.PaymentId} to draft"
                    : request.Reason.Trim();

                // 1. Detach from invoices and other payments. RemovePaymentApplication returns a
                //    fully paid invoice to INVOICE_READY, which the old ExecuteDelete never did.
                var applicationIds = await _context.PaymentApplications
                    .Where(pa => pa.PaymentId == request.PaymentId || pa.ToPaymentId == request.PaymentId)
                    .Select(pa => pa.PaymentApplicationId)
                    .ToListAsync(ct);

                foreach (var applicationId in applicationIds)
                {
                    var removed = await _paymentApplicationService.RemovePaymentApplication(applicationId);
                    if (!removed.IsSuccess)
                    {
                        await transaction.RollbackAsync(ct);
                        return Results<PaymentDto>.Failure(removed.ErrorMessage, "APPLICATION_REMOVE_FAILED");
                    }
                }

                // 2. Reverse, never delete. Unposted rows (none in practice) are removed as drafts.
                var reversible = await _reversal.FindReversibleForPaymentAsync(request.PaymentId, ct);
                var reversed = await _reversal.ReverseManyAsync(reversible,
                    new ReverseAcctgTransOptions { Reason = reason }, ct);
                if (!reversed.IsSuccess)
                {
                    await transaction.RollbackAsync(ct);
                    return Results<PaymentDto>.Failure(reversed.ErrorMessage, "REVERSAL_FAILED");
                }

                var unposted = await _context.AcctgTrans
                    .Include(t => t.AcctgTransEntries)
                    .Where(t => t.PaymentId == request.PaymentId && t.IsPosted != "Y")
                    .ToListAsync(ct);
                foreach (var draft in unposted)
                {
                    _context.AcctgTransEntries.RemoveRange(draft.AcctgTransEntries);
                    _context.AcctgTrans.Remove(draft);
                }

                // 3. Back to draft. Status is set directly: SetPaymentStatus refuses SENT/RECEIVED -> NOT_PAID
                //    by design and points here.
                payment.StatusId = "PMNT_NOT_PAID";
                // The override GL account is part of what the user entered, not of the posting, so
                // it survives a reset. Clearing it (as the old handler did) made the re-send fall
                // back to the payment type's default account: O18087 went to 150000 instead of the
                // employee's custody account 100073 on 2026-09-15.
                payment.LastUpdatedStamp = stamp;
                payment.LastUpdatedTxStamp = stamp;

                if (!string.IsNullOrEmpty(payment.PaymentPreferenceId))
                {
                    var preference = await _context.OrderPaymentPreferences
                        .FirstOrDefaultAsync(opp => opp.OrderPaymentPreferenceId == payment.PaymentPreferenceId, ct);
                    if (preference != null)
                    {
                        preference.StatusId = "PMNT_NOT_PAID";
                        preference.LastModifiedDate = stamp;
                    }
                }

                await _context.SaveChangesAsync(ct);

                var fromPartyName = await _context.Parties
                    .Where(p => p.PartyId == payment.PartyIdFrom)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(ct);

                var toPartyName = await _context.Parties
                    .Where(p => p.PartyId == payment.PartyIdTo)
                    .Select(p => p.Description)
                    .FirstOrDefaultAsync(ct);

                var paymentDto = new PaymentDto
                {
                    PaymentId = payment.PaymentId,
                    StatusId = payment.StatusId,
                    StatusDescription = "Not Paid",
                    Amount = payment.Amount,
                    EffectiveDate = payment.EffectiveDate,
                    Comments = payment.Comments,
                    PaymentMethodId = payment.PaymentMethodId,
                    PaymentTypeId = payment.PaymentTypeId,
                    PartyIdFrom = payment.PartyIdFrom,
                    PartyIdFromName = fromPartyName,
                    PartyIdTo = payment.PartyIdTo,
                    PartyIdToName = toPartyName,
                    OverrideGlAccountId = payment.OverrideGlAccountId,
                    ChequeNumber = payment.ChequeNumber,
                    ChequeDate = payment.ChequeDate,
                    PaymentRefNum = payment.PaymentRefNum,
                    CostCenterId = payment.CostCenterId,
                    IsBankTransfer = payment.IsBankTransfer,
                };

                await transaction.CommitAsync(ct);

                _logger.LogInformation(
                    "Payment {PaymentId} reset to draft: {Reversed} transactions reversed, {Apps} applications removed",
                    request.PaymentId, reversed.ResultData.Count(r => r.ReversalAcctgTransId != null), applicationIds.Count);

                return Results<PaymentDto>.Success(paymentDto);
            }
            catch (ClosedAccountingPeriodException ex)
            {
                await transaction.RollbackAsync(ct);
                return Results<PaymentDto>.Failure(ex.Message, "PERIOD_CLOSED");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogWarning(ex, "Concurrency error while resetting payment {PaymentId}", request.PaymentId);
                return Results<PaymentDto>.Failure(
                    "The payment was modified by another user. Please refresh and try again.",
                    "CONCURRENCY_ERROR"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Unexpected error resetting payment {PaymentId}", request.PaymentId);
                return Results<PaymentDto>.Failure(
                    $"An unexpected error occurred while resetting the payment: {ex.GetBaseException().Message}",
                    "UNEXPECTED_ERROR"
                );
            }
        }
    }
}
