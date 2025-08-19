using MediatR;
using Application.Manufacturing;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.WorkEfforts;

public class ReserveProductionRunTask
{
    public class Command : IRequest<Result<ReserveProductionRunTaskResult>>
    {
        public ReserveProductionRunTaskParams ReserveProductionRunTaskParams { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<ReserveProductionRunTaskResult>>
    {
        private readonly IProductionRunService _productionRunService;
        private readonly DataContext _context;

        public Handler(IProductionRunService productionRunService, DataContext context)
        {
            _productionRunService = productionRunService;
            _context = context;
        }

        public async Task<Result<ReserveProductionRunTaskResult>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // 1) Call your service method to reserve production run inventory
                await _productionRunService.ReserveProductionRunTask(
                    request.ReserveProductionRunTaskParams.WorkEffortId,
                    request.ReserveProductionRunTaskParams.RequireInventory
                );
                
                // 2) Save changes
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                // 3) Return success response
                var response = new ReserveProductionRunTaskResult
                {
                    Message = "Reservation completed successfully."
                };
                return Result<ReserveProductionRunTaskResult>.Success(response);
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await transaction.RollbackAsync(cancellationToken);

                // Return failure response
                return Result<ReserveProductionRunTaskResult>.Failure($"Error reserving components: {ex.Message}");
            }
        }
    }
}