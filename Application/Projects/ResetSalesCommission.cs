using Application.Accounting.Services;
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
        private readonly IPaymentVoidService _voidService;

        public Handler(DataContext context, IPaymentVoidService voidService)
        {
            _context = context;
            _voidService = voidService;
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
                // Wipe the generated payments and everything they produced — including payments that
                // were already disbursed. The confirmation dialog states this outright, so a reset is
                // a deliberate discard of that history, not an accounting reversal.
                await CommissionPaymentCleanup.PurgeOrVoidAsync(_context, _voidService, commission.SalesRequestId, "Sales commission reset", ct);

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
