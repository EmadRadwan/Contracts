using Application.Manufacturing;
using MediatR;
using Persistence;

namespace Application.WorkEfforts;

public class ChangeProductionRunStatus
{
    public class Command : IRequest<Result<ChangeProductionRunStatusResult>>
    {
        public ChangeProductionRunStatusDto ChangeProductionRunStatusDto { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<ChangeProductionRunStatusResult>>
    {
        private readonly DataContext _context;
        private readonly IProductionRunService _productionRunService;

        public Handler(IProductionRunService productionRunService, DataContext context)
        {
            _productionRunService = productionRunService;
            _context = context;
        }

        public async Task<Result<ChangeProductionRunStatusResult>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // Call the service method to change production run status

                var response = await _productionRunService.ChangeProductionRunStatus(
                    request.ChangeProductionRunStatusDto.ProductionRunId,
                    request.ChangeProductionRunStatusDto.StatusId
                );

                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return Result<ChangeProductionRunStatusResult>.Success(response);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                // Handle exceptions and return failure response
                return Result<ChangeProductionRunStatusResult>.Failure(
                    $"Error changing production run status: {ex.Message}");
            }
        }
    }
}