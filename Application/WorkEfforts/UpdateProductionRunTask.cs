using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Manufacturing;
using Application.WorkEfforts;
using Persistence;

namespace Application.WorkEfforts
{
    public class UpdateProductionRunTask
    {
        public class Command : IRequest<Result<UpdateProductionRunTaskResult>>
        {
            public UpdateProductionRunTaskContext UpdateProductionRunTaskContext { get; set; }
        }

        public class Handler : IRequestHandler<Command, Result<UpdateProductionRunTaskResult>>
        {
            private readonly IProductionRunService _productionRunService;
            private readonly DataContext _context;

            public Handler(IProductionRunService productionRunService, DataContext context)
            {
                _productionRunService = productionRunService;
                _context = context;
            }

            public async Task<Result<UpdateProductionRunTaskResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Call the service method to declare and produce the production run
                    var result = await _productionRunService.UpdateProductionRunTask(
                        request.UpdateProductionRunTaskContext
                    );

                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    // Return success response with the result
                    return Result<UpdateProductionRunTaskResult>.Success(result);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    // Handle exceptions and return failure response with an appropriate error message
                    var errorMessage = $"Error updating routing task: {ex.Message}";
                    return Result<UpdateProductionRunTaskResult>.Failure(errorMessage);
                }
            }
        }
    }
}
