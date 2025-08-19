using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products
{
    public class ListTaskFOHCalculations
    {
        public class Query : IRequest<Result<List<CostComponentCalcDto>>>
        {
            public string ProductId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<CostComponentCalcDto>>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<Result<List<CostComponentCalcDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = new List<CostComponentCalcDto>();
                DateTime filterDate = DateTime.UtcNow; // Default to current date

                try
                {
                    // Fetch routing based on the productId
                    _logger.LogInformation("Starting to fetch routing for ProductId: {ProductId}", request.ProductId);
                    WorkEffortGoodStandard routingGS = await _context.WorkEffortGoodStandards
                        .Where(w => w.ProductId == request.ProductId && w.WorkEffortGoodStdTypeId == "ROU_PROD_TEMPLATE")
                        .Where(w => w.FromDate <= filterDate && (w.ThruDate == null || w.ThruDate >= filterDate))
                        .FirstOrDefaultAsync(cancellationToken);

                    if (routingGS != null)
                    {
                        // Fetch associated tasks
                        var tasks = await _context.WorkEffortAssocs
                            .Include(w => w.WorkEffortIdToNavigation)
                            .Where(w => w.WorkEffortIdFrom == routingGS.WorkEffortId && w.WorkEffortAssocTypeId == "ROUTING_COMPONENT")
                            .Where(w => w.FromDate <= filterDate && (w.ThruDate == null || w.ThruDate >= filterDate))
                            .OrderBy(w => w.SequenceNum)
                            .ToListAsync(cancellationToken);

                        // For each task, get all valid records from WorkEffortCostCalcs and link to CostComponentCalcs
                        foreach (var task in tasks)
                        {
                            var costCalcs = await _context.WorkEffortCostCalcs
                                .Include(w => w.CostComponentType)
                                .Where(w => w.WorkEffortId == task.WorkEffortIdTo 
                                    && w.FromDate <= filterDate 
                                    && (w.ThruDate == null || w.ThruDate >= filterDate)
                                    && w.CostComponentTypeId != "LABOR_COST")
                                .ToListAsync(cancellationToken);

                            foreach (var costCalc in costCalcs)
                            {
                                var costComponentCalc = await _context.CostComponentCalcs
                                    .Where(c => c.CostComponentCalcId == costCalc.CostComponentCalcId)
                                    .FirstOrDefaultAsync(cancellationToken);

                                if (costComponentCalc != null)
                                {
                                    // Map to DTO
                                    var costComponentCalcDto = new CostComponentCalcDto
                                    {
                                        WorkEffortId = task.WorkEffortIdTo,
                                        WorkEffortName = task.WorkEffortIdToNavigation?.WorkEffortName ?? "Unknown",
                                        CostComponentTypeId = costCalc.CostComponentTypeId,
                                        CostComponentTypeDescription = costCalc.CostComponentType?.Description ?? "Unknown",
                                        ProductId = request.ProductId,
                                        Description = costComponentCalc.Description,
                                        VariableCost = (decimal)costComponentCalc.VariableCost,
                                        PermilliSecond = (decimal)costComponentCalc.PerMilliSecond,
                                        CurrencyUomId = costComponentCalc.CurrencyUomId
                                    };

                                    result.Add(costComponentCalcDto);
                                }
                            }
                        }
                    }

                    // Return the calculated result
                    _logger.LogInformation("Returning {Count} cost calculation records for ProductId: {ProductId}", result.Count, request.ProductId);
                    return Result<List<CostComponentCalcDto>>.Success(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating product costs for ProductId: {ProductId}", request.ProductId);
                    return Result<List<CostComponentCalcDto>>.Failure("Error calculating product costs");
                }
            }
        }
    }
}