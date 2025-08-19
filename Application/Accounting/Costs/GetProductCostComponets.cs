using Application.Manufacturing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Microsoft.Extensions.Logging;

namespace Application.Accounting.Costs
{
    public class GetProductCostComponents
    {
        public class Query : IRequest<Result<ProductCostComponentsTotalsDto>>
        {
            public string? ProductId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ProductCostComponentsTotalsDto>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<Result<ProductCostComponentsTotalsDto>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                try
                {
                    // 1) Fetch the main cost components list
                    _logger.LogInformation("Fetching cost components for ProductId: {ProductId}", request.ProductId);
                    var costComponents = await (
                        from costComponent in _context.CostComponents
                        join product in _context.Products
                            on costComponent.ProductId equals product.ProductId
                        join costComponentType in _context.CostComponentTypes
                            on costComponent.CostComponentTypeId equals costComponentType.CostComponentTypeId
                        where costComponent.ProductId == request.ProductId
                        select new CostComponentDto
                        {
                            CostComponentCalcId = costComponent.CostComponentCalcId,
                            CostComponentTypeId = costComponent.CostComponentTypeId,
                            ProductId = costComponent.ProductId,
                            ProductFeatureId = costComponent.ProductFeatureId,
                            PartyId = costComponent.PartyId,
                            GeoId = costComponent.GeoId,
                            WorkEffortId = costComponent.WorkEffortId,
                            FixedAssetId = costComponent.FixedAssetId,
                            CostComponentId = costComponent.CostComponentId,
                            ProductName = product.ProductName,
                            CostComponentTypeDescription = costComponentType.Description,
                            FromDate = costComponent.FromDate,
                            ThruDate = costComponent.ThruDate,
                            Cost = costComponent.Cost,
                            CostUomId = costComponent.CostUomId
                        }
                    ).ToListAsync(cancellationToken);

                    // Initialize counters
                    var fohCount = 0;
                    var laborCount = 0;
                    var materialCount = 0;

                    // 2) Fetch routing for FOH and Labor
                    var filterDate = DateTime.UtcNow;
                    var routingGS = await _context.WorkEffortGoodStandards
                        .Where(w => w.ProductId == request.ProductId && w.WorkEffortGoodStdTypeId == "ROU_PROD_TEMPLATE")
                        .Where(w => w.FromDate <= filterDate && (w.ThruDate == null || w.ThruDate >= filterDate))
                        .FirstOrDefaultAsync(cancellationToken);

                    if (routingGS != null)
                    {
                        // 3) Fetch tasks
                        var tasks = await _context.WorkEffortAssocs
                            .Where(w => w.WorkEffortIdFrom == routingGS.WorkEffortId &&
                                        w.WorkEffortAssocTypeId == "ROUTING_COMPONENT")
                            .Where(w => w.FromDate <= filterDate && (w.ThruDate == null || w.ThruDate >= filterDate))
                            .OrderBy(w => w.SequenceNum)
                            .ToListAsync(cancellationToken);

                        if (tasks.Any())
                        {
                            var taskIds = tasks.Select(t => t.WorkEffortIdTo).Distinct().ToList();

                            // 4) Count FOH cost calculations (non-labor)
                            fohCount = await _context.WorkEffortCostCalcs
                                .Where(w => taskIds.Contains(w.WorkEffortId)
                                            && w.FromDate <= filterDate
                                            && (w.ThruDate == null || w.ThruDate >= filterDate)
                                            && w.CostComponentTypeId != "LABOR_COST")
                                .CountAsync(cancellationToken);

                            // 5) Count Labor cost calculations
                            laborCount = await _context.WorkEffortCostCalcs
                                .Where(w => taskIds.Contains(w.WorkEffortId)
                                            && w.FromDate <= filterDate
                                            && (w.ThruDate == null || w.ThruDate >= filterDate)
                                            && w.CostComponentTypeId == "LABOR_COST")
                                .CountAsync(cancellationToken);
                        }
                    }

                    // 6) Count Material cost components
                    var matFilterDate = DateTime.UtcNow;
                    var componentQuery =
                        from pa in _context.ProductAssocs
                        join p in _context.Products on pa.ProductIdTo equals p.ProductId
                        join c in _context.CostComponents on pa.ProductIdTo equals c.ProductId
                        join cct in _context.CostComponentTypes on c.CostComponentTypeId equals cct.CostComponentTypeId
                        where pa.ProductId == request.ProductId
                              && pa.ProductAssocTypeId == "MANUF_COMPONENT"
                              && pa.FromDate <= matFilterDate
                              && (pa.ThruDate == null || pa.ThruDate >= matFilterDate)
                              && c.CostComponentTypeId.StartsWith("EST_STD_MAT")
                              && c.FromDate <= matFilterDate
                              && (c.ThruDate == null || c.ThruDate >= matFilterDate)
                        select c.CostComponentId;

                    materialCount = await componentQuery.CountAsync(cancellationToken);

                    // 7) Prepare the result
                    var totalsDto = new ProductCostComponentsTotalsDto
                    {
                        CostComponents = costComponents,
                        DirectLaborCount = laborCount,
                        FohCostCount = fohCount,
                        MaterialCostCount = materialCount
                    };

                    _logger.LogInformation("Returning ProductCostComponentsTotalsDto for ProductId: {ProductId} with FOH: {FohCount}, Labor: {LaborCount}, Material: {MaterialCount}",
                        request.ProductId, fohCount, laborCount, materialCount);

                    return Result<ProductCostComponentsTotalsDto>.Success(totalsDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching product cost components for ProductId: {ProductId}", request.ProductId);
                    return Result<ProductCostComponentsTotalsDto>.Failure("Error fetching product cost components");
                }
            }
        }
    }
}