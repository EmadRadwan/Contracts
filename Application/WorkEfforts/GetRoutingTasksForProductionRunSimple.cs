using Application.Manufacturing;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.WorkEfforts
{
    public class GetRoutingTasksForProductionRunSimple
    {
        public class Query : IRequest<Result<List<ProductionRunRoutingTaskDto>>>
        {
            public string? ProductionRunId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<ProductionRunRoutingTaskDto>>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<Result<List<ProductionRunRoutingTaskDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                try
                {
                    // Validate the input parameter ProductionRunId
                    var productionRunId = request.ProductionRunId;

                    if (string.IsNullOrEmpty(productionRunId))
                    {
                        return Result<List<ProductionRunRoutingTaskDto>>.Failure("ProductionRunId is required.");
                    }

                    // Fetch production run routing tasks without any extra checks
                    var productionRunRoutingTasks = await _context.WorkEfforts
                        .Where(x => x.WorkEffortParentId == productionRunId && x.WorkEffortTypeId == "PROD_ORDER_TASK")
                        .Include(x => x.CurrentStatus)
                        .Include(x => x.FixedAsset)
                        .OrderBy(x => x.Priority)
                        .ToListAsync(cancellationToken);

                    // Process and map each routing task to the DTO
                    var taskDtos = productionRunRoutingTasks.Select(task => new ProductionRunRoutingTaskDto
                    {
                        WorkEffortId = task.WorkEffortId,
                        SequenceNum = task.Priority,
                        WorkEffortParentId = task.WorkEffortParentId,
                        WorkEffortName = task.WorkEffortName + " [" + task.WorkEffortId + "]",
                        Description = task.Description,
                        QuantityToProduce = task.QuantityToProduce,
                        CurrentStatusId = task.CurrentStatusId,
                        CurrentStatusDescription = task.CurrentStatus?.Description, // Ensure CurrentStatus is not null
                        EstimatedStartDate = task.EstimatedStartDate,
                        EstimatedCompletionDate = task.EstimatedCompletionDate,
                        EstimatedSetupMillis = task.EstimatedSetupMillis,
                        EstimatedMilliSeconds = task.EstimatedMilliSeconds,
                        ActualMilliSeconds = task.ActualMilliSeconds,
                        ActualSetupMillis = task.ActualSetupMillis,
                        FixedAssetId = task.FixedAssetId,
                        FixedAssetName = task.FixedAsset?.FixedAssetName // Ensure FixedAsset is not null
                    }).ToList();

                    // Return the list of routing tasks
                    return Result<List<ProductionRunRoutingTaskDto>>.Success(taskDtos);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while fetching routing tasks for production run.");
                    return Result<List<ProductionRunRoutingTaskDto>>.Failure("An error occurred while processing your request.");
                }
            }
        }
    }
}
