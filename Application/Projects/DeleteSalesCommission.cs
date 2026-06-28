using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Projects;

public class DeleteSalesCommission
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

            await using var transaction = await _context.Database.BeginTransactionAsync(ct);

            try
            {
                // 1. If approved, clean up generated payments and accounting entries first
                if (commission.StatusId == "COMMISSION_APPROVED")
                {
                    var payments = await _context.Payments
                        .Where(p => p.SalesRequestId == commission.SalesRequestId
                                    && p.PaymentTypeId == "COMMISSION_PAYMENT")
                        .ToListAsync(ct);

                    if (payments.Any())
                    {
                        var paymentIds = payments.Select(p => p.PaymentId).ToList();

                        var finAccountTrans = await _context.FinAccountTrans
                            .Where(fat => fat.PaymentId != null && paymentIds.Contains(fat.PaymentId))
                            .ToListAsync(ct);

                        foreach (var fat in finAccountTrans)
                            fat.PaymentId = null;

                        _context.FinAccountTrans.RemoveRange(finAccountTrans);

                        var acctgTransList = await _context.AcctgTrans
                            .Include(t => t.AcctgTransEntries)
                            .Where(t => t.PaymentId != null && paymentIds.Contains(t.PaymentId))
                            .ToListAsync(ct);

                        foreach (var tran in acctgTransList)
                            _context.AcctgTransEntries.RemoveRange(tran.AcctgTransEntries);

                        _context.AcctgTrans.RemoveRange(acctgTransList);
                        _context.Payments.RemoveRange(payments);
                    }
                }

                // 2. Hard delete the commission record
                _context.SalesCommissions.Remove(commission);

                var saved = await _context.SaveChangesAsync(ct) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(ct);
                    return Result<Unit>.Failure("Failed to delete commission");
                }

                await transaction.CommitAsync(ct);
                return Result<Unit>.Success(Unit.Value);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                return Result<Unit>.Failure($"Failed to delete commission: {ex.Message}");
            }
        }
    }
}
