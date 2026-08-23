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
                // 1. If approved, clean up generated payments and every artifact they produced
                //    (ledger entries, bank transactions, reconciliation rows, attributes).
                //    Already-disbursed payments are included — the confirmation dialog says so.
                if (commission.StatusId == "COMMISSION_APPROVED")
                    await CommissionPaymentCleanup.PurgeAsync(_context, commission.SalesRequestId, ct);

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
