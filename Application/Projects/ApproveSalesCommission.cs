using Application.Core;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Projects;

public class ApproveSalesCommission
{
    public class Command : IRequest<Result<SalesCommissionDto>>
    {
        public string SalesCommissionId { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<SalesCommissionDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<SalesCommissionDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var commission = await _context.SalesCommissions
                .FirstOrDefaultAsync(x => x.SalesCommissionId == request.SalesCommissionId, cancellationToken);

            if (commission == null)
                return Result<SalesCommissionDto>.Failure("Commission record not found");

            if (commission.StatusId != "COMMISSION_PENDING")
                return Result<SalesCommissionDto>.Failure("Only pending commissions can be approved");

            commission.StatusId = "COMMISSION_APPROVED";
            commission.LastUpdatedStamp = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to approve commission {Id}", request.SalesCommissionId);
                return Result<SalesCommissionDto>.Failure("Failed to approve commission");
            }

            return Result<SalesCommissionDto>.Success(new SalesCommissionDto
            {
                SalesCommissionId = commission.SalesCommissionId,
                SalesRequestId = commission.SalesRequestId,
                StatusId = commission.StatusId
            });
        }
    }
}
