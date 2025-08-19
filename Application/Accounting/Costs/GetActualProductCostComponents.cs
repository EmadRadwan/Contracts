using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Accounting.Costs
{
    public class GetActualProductCostComponents
    {
        public class Query : IRequest<Result<ActualProductCostComponentsTotalsDto>>
        {
            public string ProductionRunId { get; set; }
        }

        public class ActualProductCostComponentsTotalsDto
        {
            public List<CostComponentDto> CostComponents { get; set; }
            public int FohCostCount { get; set; }
            public int DirectLaborCount { get; set; }
            public int MaterialCostCount { get; set; }
        }

        public class CostComponentDto
        {
            public string CostComponentCalcId { get; set; }
            public string CostComponentTypeId { get; set; }
            public string CostComponentTypeDescription { get; set; }
            public string ProductId { get; set; }
            public string ProductFeatureId { get; set; }
            public string PartyId { get; set; }
            public string GeoId { get; set; }
            public string WorkEffortId { get; set; }
            public string WorkEffortName { get; set; }
            public string FixedAssetId { get; set; }
            public string CostComponentId { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ThruDate { get; set; }
            public decimal Cost { get; set; }
            public string CostUomId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ActualProductCostComponentsTotalsDto>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;
            }

            public async Task<Result<ActualProductCostComponentsTotalsDto>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                try
                {
                    _logger.LogInformation("Fetching actual cost components for WorkEffortId: {WorkEffortId}",
                        request.ProductionRunId);

                    // Deduce ProductId from WorkEffort (production run)
                    // Deduce ProductId from WorkEffortGoodStandards
                    var productId = await _context.WorkEffortGoodStandards
                        .Where(wegs => wegs.WorkEffortId == request.ProductionRunId &&
                                       wegs.WorkEffortGoodStdTypeId == "PRUN_PROD_DELIV")
                        .Select(wegs => wegs.ProductId)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (string.IsNullOrEmpty(productId))
                    {
                        _logger.LogWarning("No ProductId found for WorkEffortId: {ProductionRunId}",
                            request.ProductionRunId);
                        return Result<ActualProductCostComponentsTotalsDto>.Failure(
                            "Production run not associated with a product");
                    }

                    // Fetch configured counts (standard totals)
                    var filterDate = DateTime.UtcNow;

                    // Step 1: Get routing
                    var routingGS = await _context.WorkEffortGoodStandards
                        .Where(w => w.ProductId == productId && w.WorkEffortGoodStdTypeId == "ROU_PROD_TEMPLATE")
                        .Where(w => w.FromDate <= filterDate && (w.ThruDate == null || w.ThruDate >= filterDate))
                        .FirstOrDefaultAsync(cancellationToken);

                    int fohCount = 0;
                    int laborCount = 0;

                    if (routingGS != null)
                    {
                        // Step 2: Fetch tasks
                        var tasks = await _context.WorkEffortAssocs
                            .Where(w => w.WorkEffortIdFrom == routingGS.WorkEffortId &&
                                        w.WorkEffortAssocTypeId == "ROUTING_COMPONENT")
                            .Where(w => w.FromDate <= filterDate && (w.ThruDate == null || w.ThruDate >= filterDate))
                            .OrderBy(w => w.SequenceNum)
                            .Select(w => w.WorkEffortIdTo)
                            .ToListAsync(cancellationToken);

                        // Step 3: Count FOH and Labor from WorkEffortCostCalcs
                        fohCount = await _context.WorkEffortCostCalcs
                            .Where(wecc => tasks.Contains(wecc.WorkEffortId) &&
                                           wecc.CostComponentTypeId != "LABOR_COST")
                            .CountAsync(cancellationToken);

                        laborCount = await _context.WorkEffortCostCalcs
                            .Where(wecc => tasks.Contains(wecc.WorkEffortId) &&
                                           wecc.CostComponentTypeId == "LABOR_COST")
                            .CountAsync(cancellationToken);
                    }

                    // Step 4: Count Material from ProductAssocs and CostComponents
                    var materialCount = await (from pa in _context.ProductAssocs
                            join cc in _context.CostComponents
                                on pa.ProductIdTo equals cc.ProductId
                            where pa.ProductId == productId &&
                                  pa.ProductAssocTypeId == "MANUF_COMPONENT" &&
                                  cc.CostComponentTypeId.StartsWith("EST_STD_MAT")
                            select cc)
                        .CountAsync(cancellationToken);

                    // Fetch actual cost components
                    // Task-level costs
                    var taskInfoList = await (from task in _context.WorkEfforts
                        where task.WorkEffortParentId == request.ProductionRunId &&
                              task.WorkEffortTypeId == "PROD_ORDER_TASK"
                        orderby task.WorkEffortId
                        select new
                        {
                            Task = task,
                            TaskCosts = (from cc in _context.CostComponents
                                join ccType in _context.CostComponentTypes
                                    on cc.CostComponentTypeId equals ccType.CostComponentTypeId
                                where cc.WorkEffortId == task.WorkEffortId
                                select new CostComponentDto
                                {
                                    CostComponentCalcId = cc.CostComponentCalcId,
                                    CostComponentTypeId = cc.CostComponentTypeId,
                                    CostComponentTypeDescription = ccType.Description,
                                    ProductId = cc.ProductId,
                                    ProductFeatureId = cc.ProductFeatureId,
                                    PartyId = cc.PartyId,
                                    WorkEffortId = cc.WorkEffortId,
                                    WorkEffortName = task.WorkEffortName,
                                    FixedAssetId = cc.FixedAssetId,
                                    CostComponentId = cc.CostComponentId,
                                    FromDate = cc.FromDate,
                                    ThruDate = cc.ThruDate,
                                    Cost = (decimal)cc.Cost,
                                    CostUomId = cc.CostUomId
                                }).ToList()
                        }).ToListAsync(cancellationToken);

                    // Production run-level costs
                    var productionRunCosts = await (from cc in _context.CostComponents
                        join ccType in _context.CostComponentTypes
                            on cc.CostComponentTypeId equals ccType.CostComponentTypeId
                        where cc.WorkEffortId == request.ProductionRunId
                        select new CostComponentDto
                        {
                            CostComponentCalcId = cc.CostComponentCalcId,
                            CostComponentTypeId = cc.CostComponentTypeId,
                            CostComponentTypeDescription = ccType.Description,
                            ProductId = cc.ProductId,
                            ProductFeatureId = cc.ProductFeatureId,
                            PartyId = cc.PartyId,
                            WorkEffortId = cc.WorkEffortId,
                            WorkEffortName = null,
                            FixedAssetId = cc.FixedAssetId,
                            CostComponentId = cc.CostComponentId,
                            FromDate = cc.FromDate,
                            ThruDate = cc.ThruDate,
                            Cost = (decimal)cc.Cost,
                            CostUomId = cc.CostUomId
                        }).ToListAsync(cancellationToken);

                    var combinedResults = taskInfoList
                        .SelectMany(t => t.TaskCosts)
                        .Concat(productionRunCosts)
                        .ToList();

                    var resultDto = new ActualProductCostComponentsTotalsDto
                    {
                        CostComponents = combinedResults,
                        FohCostCount = fohCount,
                        DirectLaborCount = laborCount,
                        MaterialCostCount = materialCount
                    };

                    _logger.LogInformation(
                        "Returning ActualProductCostComponentsTotalsDto for WorkEffortId: {WorkEffortId} with FOH: {FohCount}, Labor: {LaborCount}, Material: {MaterialCount}",
                        request.ProductionRunId, fohCount, laborCount, materialCount);

                    return Result<ActualProductCostComponentsTotalsDto>.Success(resultDto);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching actual cost components for WorkEffortId: {ProductionRunId}",
                        request.ProductionRunId);
                    return Result<ActualProductCostComponentsTotalsDto>.Failure(
                        "Error fetching actual cost components");
                }
            }
        }
    }
}