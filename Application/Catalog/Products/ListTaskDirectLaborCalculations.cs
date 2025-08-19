using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products
{
    public class ListTaskDirectLaborCalculations
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
                            .Include(w => w.WorkEffortIdToNavigation)    // Eagerly load WorkEffort related to WorkEffortIdTo
                            .Where(w => w.WorkEffortIdFrom == routingGS.WorkEffortId && w.WorkEffortAssocTypeId == "ROUTING_COMPONENT")
                            .Where(w => w.FromDate <= filterDate && (w.ThruDate == null || w.ThruDate >= filterDate))
                            .OrderBy(w => w.SequenceNum)
                            .ToListAsync(cancellationToken);

                        // For each task, get the latest record from WorkEffortCostCalcs and link it to CostComponentCalcs
                        foreach (var task in tasks)
                        {
                            var latestCostCalc = await _context.WorkEffortCostCalcs
                                .Include(w => w.CostComponentType)
                                .Where(w => w.WorkEffortId == task.WorkEffortIdTo 
                                && w.FromDate <= filterDate &&
                                (w.ThruDate == null || w.ThruDate >= filterDate)
                                && w.CostComponentTypeId == "LABOR_COST")
                                .OrderByDescending(w => w.FromDate)
                                .FirstOrDefaultAsync(cancellationToken);

                            if (latestCostCalc != null)
                            {
                                var costComponentCalc = await _context.CostComponentCalcs
                                    .Where(c => c.CostComponentCalcId == latestCostCalc.CostComponentCalcId)
                                    .FirstOrDefaultAsync(cancellationToken);

                                if (costComponentCalc != null)
                                {
                                    // Map to DTO
                                    var CostComponentCalcDto = new CostComponentCalcDto
                                    {
                                        WorkEffortId = task.WorkEffortIdTo,
                                        WorkEffortName = task.WorkEffortIdToNavigation.WorkEffortName,
                                        CostComponentTypeId = latestCostCalc.CostComponentTypeId,
                                        CostComponentTypeDescription = latestCostCalc.CostComponentType.Description,
                                        ProductId = request.ProductId,
                                        Description = costComponentCalc.Description,
                                        VariableCost = (decimal)costComponentCalc.VariableCost,
                                        PermilliSecond = (decimal)costComponentCalc.PerMilliSecond,
                                        CurrencyUomId = costComponentCalc.CurrencyUomId
                                    };

                                    result.Add(CostComponentCalcDto);
                                }
                            }
                        }
                    }

                    // Return the calculated result
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
