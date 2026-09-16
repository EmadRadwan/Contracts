using Application.Accounting.Payments;
using Application.Accounting.Services;
using Application.Core;
using Application.Projects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.SalesRequests;

/// <summary>
/// Removes an apartment sales request.
///
/// Step 3 of the auditor's soft-delete requirement (Sep 2026). Two outcomes:
///  - Nothing has reached the ledger (no posted transaction, every payment still a draft): the
///    request, its draft payments and its instalment schedule are physically deleted, as before.
///  - Anything has been posted or received: the request is kept and marked
///    SALES_REQUEST_CANCELLED. Sent or received payments are voided (kept, reversed, bank rows
///    cancelled); draft payments are purged; every posted transaction booked against the request
///    itself is reversed. Nothing posted is removed.
/// In both cases the apartment goes back to available.
/// </summary>
public class DeleteSalesRequest
{
    public class Command : IRequest<Result<Unit>>
    {
        public string SalesRequestId { get; set; } = null!;
        public string? Reason { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;
        private readonly IAccountingPeriodGuard _periodGuard;
        private readonly IPaymentVoidService _voidService;
        private readonly IAcctgTransReversalService _reversal;
        private readonly ILedgerHistoryService _ledgerHistory;

        private const string ApartmentAvailableStatusId = "APARTMENT_AVAILABLE";

        public Handler(DataContext context, IAccountingPeriodGuard periodGuard, IPaymentVoidService voidService,
            IAcctgTransReversalService reversal, ILedgerHistoryService ledgerHistory)
        {
            _context = context;
            _periodGuard = periodGuard;
            _voidService = voidService;
            _reversal = reversal;
            _ledgerHistory = ledgerHistory;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var salesRequestId = request.SalesRequestId;

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var sr = await _context.SalesRequests
                    .Include(s => s.Installments)
                    .FirstOrDefaultAsync(x => x.SalesRequestId == salesRequestId, ct);

                if (sr == null)
                    return Result<Unit>.Failure("Sales request not found");

                if (sr.StatusId == "SALES_REQUEST_CANCELLED")
                    return Result<Unit>.Failure("طلب المبيعات ملغى بالفعل. (Sales request is already cancelled.)");

                var apartment = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == sr.ProductId && p.ProductTypeId == "APARTMENT", ct);

                if (apartment == null)
                    return Result<Unit>.Failure("Associated apartment not found");

                await _periodGuard.EnsureOpenForSalesRequestsAsync(new[] { salesRequestId }, ct);

                var reason = string.IsNullOrWhiteSpace(request.Reason)
                    ? $"Sales request {salesRequestId} cancelled"
                    : request.Reason.Trim();

                var history = await _ledgerHistory.ForSalesRequestAsync(salesRequestId, ct);
                var anyMoneyMoved = await _context.Payments
                    .AnyAsync(p => p.SalesRequestId == salesRequestId
                                   && p.StatusId != "PMNT_NOT_PAID" && p.StatusId != "PMNT_VOID", ct);
                var mustKeep = history.PostedAcctgTransCount > 0 || anyMoneyMoved;

                // Commission payments: drafts purged, disbursed ones voided.
                await CommissionPaymentCleanup.PurgeOrVoidAsync(_context, _voidService, salesRequestId, reason, ct);

                var commissions = await _context.SalesCommissions
                    .Where(c => c.SalesRequestId == salesRequestId)
                    .ToListAsync(ct);
                if (commissions.Any())
                    _context.SalesCommissions.RemoveRange(commissions);

                // Customer payments: voided when they reached the ledger, purged while still drafts.
                var payments = await _context.Payments
                    .Where(p => p.SalesRequestId == salesRequestId
                                && p.PaymentTypeId != CommissionPaymentCleanup.CommissionPaymentTypeId)
                    .ToListAsync(ct);

                var drafts = new List<Domain.Payment>();
                foreach (var payment in payments)
                {
                    if (payment.StatusId == "PMNT_VOID") continue;

                    var paymentHistory = await _ledgerHistory.ForPaymentAsync(payment.PaymentId, ct);
                    if (payment.StatusId == "PMNT_NOT_PAID" && paymentHistory.PostedAcctgTransCount == 0)
                    {
                        drafts.Add(payment);
                        continue;
                    }

                    var voided = await _voidService.VoidPaymentAsync(payment.PaymentId, reason, ct);
                    if (!voided.IsSuccess)
                    {
                        await transaction.RollbackAsync(ct);
                        return Result<Unit>.Failure(voided.ErrorMessage);
                    }
                }
                await PaymentArtifactCleanup.PurgePaymentsAsync(_context, drafts, ct);

                // Postings booked against the request itself (sale recognition, maintenance deposit).
                var reversible = await _reversal.FindReversibleForSalesRequestAsync(salesRequestId, ct);
                var reversed = await _reversal.ReverseManyAsync(reversible,
                    new ReverseAcctgTransOptions { Reason = reason }, ct);
                if (!reversed.IsSuccess)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<Unit>.Failure(reversed.ErrorMessage);
                }

                var unpostedOwn = await _context.AcctgTrans
                    .Where(t => t.SalesRequestId == salesRequestId && t.IsPosted != "Y")
                    .Select(t => t.AcctgTransId)
                    .ToListAsync(ct);
                await PaymentArtifactCleanup.PurgeAcctgTransAsync(_context, unpostedOwn, ct);

                // The instalment schedule is a plan, not a financial record.
                if (sr.Installments.Any())
                    _context.SalesRequestInstallments.RemoveRange(sr.Installments);

                apartment.ApartmentStatusId = ApartmentAvailableStatusId;
                apartment.ReservedBySalesRequestId = null;

                if (mustKeep)
                {
                    sr.StatusId = "SALES_REQUEST_CANCELLED";
                    sr.LastUpdatedStamp = DateTime.UtcNow;
                }
                else
                {
                    _context.SalesRequests.Remove(sr);
                }

                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<Unit>.Failure("Failed to delete sales request");
                }

                await transaction.CommitAsync(ct);
                return Result<Unit>.Success(Unit.Value);
            }
            catch (ClosedAccountingPeriodException ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<Unit>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                var detail = ex.GetBaseException().Message;
                return Result<Unit>.Failure($"Failed to delete sales request: {detail}");
            }
        }
    }
}
