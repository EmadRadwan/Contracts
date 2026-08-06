using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class ResetSalesCommission
{
    public class Command : IRequest<Result<Unit>>
    {
        public string SalesCommissionId { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<Unit>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<Unit>> Handle(Command request, CancellationToken ct)
        {
            var commission = await _context.SalesCommissions
                .FirstOrDefaultAsync(x => x.SalesCommissionId == request.SalesCommissionId, ct);

            if (commission == null)
                return Result<Unit>.Failure("Commission record not found");

            if (commission.StatusId != "COMMISSION_APPROVED")
                return Result<Unit>.Failure("Only approved commissions can be reset");

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // 1. Find commission payments (identified by PaymentTypeId since Payment has no SalesCommissionId)
                var payments = await _context.Payments
                    .Where(p => p.SalesRequestId == commission.SalesRequestId
                                && p.PaymentTypeId == "COMMISSION_PAYMENT")
                    .ToListAsync(ct);

                // Guard: reset hard-deletes these payments (and their FinAccountTrans / AcctgTrans).
                // That is only safe while a payment is still unpaid or was cancelled/voided. Once a
                // commission payment has actually been disbursed (PMNT_SENT / PMNT_CONFIRMED) or
                // received, deleting it would erase real financial history instead of reversing it,
                // so block the reset and require the payment to be voided/cancelled first.
                var deletableStatuses = new[] { "PMNT_NOT_PAID", "PMNT_CANCELLED", "PMNT_VOID", "PMNT_VOIDED" };
                var blockingPayment = payments.FirstOrDefault(p => !deletableStatuses.Contains(p.StatusId));
                if (blockingPayment != null)
                    return Result<Unit>.Failure(
                        $"لا يمكن إعادة تعيين العمولة — دفعة العمولة {blockingPayment.PaymentId} تم صرفها بالفعل (الحالة {blockingPayment.StatusId}). يجب إلغاء/إبطال الدفعة أولاً.");

                if (payments.Any())
                {
                    var paymentIds = payments.Select(p => p.PaymentId).ToList();

                    // Delete FinAccountTrans linked to these payments; null FK first to break the cycle
                    var finAccountTrans = await _context.FinAccountTrans
                        .Where(fat => fat.PaymentId != null && paymentIds.Contains(fat.PaymentId))
                        .ToListAsync(ct);

                    foreach (var fat in finAccountTrans)
                        fat.PaymentId = null;

                    _context.FinAccountTrans.RemoveRange(finAccountTrans);

                    // Delete AcctgTrans and entries created when payments were sent
                    var acctgTransList = await _context.AcctgTrans
                        .Include(t => t.AcctgTransEntries)
                        .Where(t => t.PaymentId != null && paymentIds.Contains(t.PaymentId))
                        .ToListAsync(ct);

                    foreach (var tran in acctgTransList)
                        _context.AcctgTransEntries.RemoveRange(tran.AcctgTransEntries);

                    _context.AcctgTrans.RemoveRange(acctgTransList);
                    _context.Payments.RemoveRange(payments);
                }

                // 2. Reset commission status back to pending
                commission.StatusId = "COMMISSION_PENDING";
                commission.LastUpdatedStamp = DateTime.UtcNow;

                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<Unit>.Failure("Failed to reset commission");
                }

                await transaction.CommitAsync(ct);
                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<Unit>.Failure($"Failed to reset commission: {ex.Message}");
            }
        }
    }
}
