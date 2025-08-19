using Application.Manufacturing;
using MediatR;
using Persistence;

namespace Application.WorkEfforts
{
    public class QuickChangeProductionRunStatus
    {
        public class Command : IRequest<Result<QuickChangeProductionRunStatusResult>>
        {
            public string ProductionRunId { get; set; }
            public string StatusId { get; set; }
            public string StartAllTasks { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<QuickChangeProductionRunStatusResult>>
        {
            private readonly DataContext _context;
            private readonly IProductionRunService _productionRunService;

            public Handler(IProductionRunService productionRunService, DataContext context)
            {
                _productionRunService = productionRunService;
                _context = context;
            }

            public async Task<Result<QuickChangeProductionRunStatusResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Call the service method to quickly change the production run status,
                    // which internally will perform the necessary steps (e.g. starting tasks,
                    // running tasks, producing items) based on the status and flags provided.
                    var result = await _productionRunService.QuickChangeProductionRunStatus(
                        request.ProductionRunId,
                        request.StatusId,
                        request.StartAllTasks
                    );

                    // Save any changes in the context (if applicable).
                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return Result<QuickChangeProductionRunStatusResult>.Success(result);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    // If any errors occur, capture the exception message in the failure result.
                    return Result<QuickChangeProductionRunStatusResult>.Failure(
                        $"Error quick changing production run status: {ex.Message}");
                }
            }
        }
    }
}
