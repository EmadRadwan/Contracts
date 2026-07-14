using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Manufacturing;

public class GetInventoryItemsWithEdits
{
    public class Query : IRequest<Result<List<BomInventoryItemDto>>>
    {
        public string WorkEffortId { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<BomInventoryItemDto>>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<List<BomInventoryItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                // REFACTOR: Find the production run (WorkEffortParentId) for the given task
                // Purpose: Identify the PROD_ORDER_HEADER associated with the input task
                // Benefit: Allows finding all tasks in the production run
                var task = await _context.WorkEfforts
                    .Where(we => we.WorkEffortId == request.WorkEffortId && we.WorkEffortTypeId == "PROD_ORDER_TASK")
                    .Select(we => new { we.WorkEffortParentId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (task == null || string.IsNullOrEmpty(task.WorkEffortParentId))
                {
                    _logger.LogWarning("Task not found or not a PROD_ORDER_TASK for WorkEffortId: {WorkEffortId}", request.WorkEffortId);
                    return Result<List<BomInventoryItemDto>>.Failure("Task not found or not associated with a production run");
                }

                var productionRunId = task.WorkEffortParentId;

                // REFACTOR: Validate production run existence
                // Purpose: Ensure the parent is a valid PROD_ORDER_HEADER
                // Benefit: Prevents invalid queries and improves error handling
                var productionRunExists = await _context.WorkEfforts
                    .AnyAsync(we => we.WorkEffortId == productionRunId && we.WorkEffortTypeId == "PROD_ORDER_HEADER", cancellationToken);
                if (!productionRunExists)
                {
                    _logger.LogWarning("Production run not found for WorkEffortParentId: {ProductionRunId}", productionRunId);
                    return Result<List<BomInventoryItemDto>>.Failure("Production run not found");
                }

                // REFACTOR: Find the first task's WorkEffortId using WorkEffortAssocs SequenceNum
                // Purpose: Identify the first PROD_ORDER_TASK by joining with WorkEffortAssocs to get SequenceNum
                // Benefit: Ensures correct task ordering for BOM retrieval (e.g., 10030)
                var firstTask = await _context.WorkEfforts
                    .Where(we => we.WorkEffortParentId == productionRunId && we.WorkEffortTypeId == "PROD_ORDER_TASK")
                    .Join(_context.WorkEffortAssocs
                        .Where(wa => wa.WorkEffortAssocTypeId == "WORK_EFF_TEMPLATE"),
                        we => we.WorkEffortId,
                        wa => wa.WorkEffortIdTo,
                        (we, wa) => new { we.WorkEffortId, wa.SequenceNum })
                    .OrderBy(x => x.SequenceNum ?? int.MaxValue) // Use int.MaxValue for null SequenceNum
                    .ThenBy(x => x.WorkEffortId) // Fallback to WorkEffortId
                    .Select(x => new { x.WorkEffortId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (firstTask == null)
                {
                    // REFACTOR: Fallback to WorkEfforts without SequenceNum if no WorkEffortAssocs match
                    // Purpose: Handle cases where WorkEffortAssocs may not exist for all tasks
                    // Benefit: Increases robustness for incomplete data
                    firstTask = await _context.WorkEfforts
                        .Where(we => we.WorkEffortParentId == productionRunId && we.WorkEffortTypeId == "PROD_ORDER_TASK")
                        .OrderBy(we => we.CreatedDate ?? DateTime.MaxValue)
                        .ThenBy(we => we.WorkEffortId)
                        .Select(we => new { we.WorkEffortId })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (firstTask == null)
                    {
                        _logger.LogWarning("No tasks found for production run: {ProductionRunId}", productionRunId);
                        return Result<List<BomInventoryItemDto>>.Failure("No tasks found for the production run");
                    }
                }

                var workEffortId = firstTask.WorkEffortId;
                _logger.LogInformation(
                    "Identified first task WorkEffortId: {WorkEffortId} for ProductionRunId: {ProductionRunId} from input WorkEffortId: {InputWorkEffortId}",
                    workEffortId, productionRunId, request.WorkEffortId);

                // REFACTOR: Verify first task exists
                // Purpose: Ensure the identified task is valid
                // Benefit: Adds robustness against data inconsistencies
                var workEffortExists = await _context.WorkEfforts
                    .AnyAsync(we => we.WorkEffortId == workEffortId, cancellationToken);
                if (!workEffortExists)
                {
                    _logger.LogWarning("WorkEffort not found for WorkEffortId: {WorkEffortId}", workEffortId);
                    return Result<List<BomInventoryItemDto>>.Failure("First task not found");
                }

                // REFACTOR: Fetch BOM components for the first task
                // Purpose: Retrieve BOM data using the first task's WorkEffortId
                // Benefit: Maintains existing logic for consistency
                var components = await _context.WorkEffortGoodStandards
                    .Where(w => w.WorkEffortId == workEffortId && w.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED")
                    .Join(_context.InventoryItems,
                        w => w.ProductId,
                        i => i.ProductId,
                        (w, i) => new { w.ProductId, w.EstimatedQuantity, i.InventoryItemId, i.AvailableToPromiseTotal })
                    .Join(_context.Products,
                        x => x.ProductId,
                        p => p.ProductId,
                        (x, p) => new { x.ProductId, x.EstimatedQuantity, x.InventoryItemId, x.AvailableToPromiseTotal, p.ProductName })
                    .GroupJoin(_context.InventoryItemFeatures,
                        x => x.InventoryItemId,
                        iif => iif.InventoryItemId,
                        (x, iifs) => new { x.ProductId, x.EstimatedQuantity, x.InventoryItemId, x.AvailableToPromiseTotal, x.ProductName, Features = iifs })
                    .SelectMany(x => x.Features.DefaultIfEmpty(),
                        (x, iif) => new { x.ProductId, x.EstimatedQuantity, x.InventoryItemId, x.AvailableToPromiseTotal, x.ProductName, ProductFeatureId = iif != null ? iif.ProductFeatureId : null })
                    .GroupJoin(_context.ProductFeatures,
                        x => x.ProductFeatureId,
                        pf => pf.ProductFeatureId,
                        (x, pfs) => new { x.ProductId, x.EstimatedQuantity, x.InventoryItemId, x.AvailableToPromiseTotal, x.ProductName, x.ProductFeatureId, ProductFeatures = pfs })
                    .SelectMany(x => x.ProductFeatures.DefaultIfEmpty(),
                        (x, pf) => new BomInventoryItemDto
                        {
                            ProductId = x.ProductId,
                            EstimatedQuantity = (decimal)x.EstimatedQuantity,
                            InventoryItemId = x.InventoryItemId,
                            AvailableToPromiseTotal = (decimal)x.AvailableToPromiseTotal,
                            ProductFeatureId = x.ProductFeatureId,
                            ColorDescription = pf != null ? (request.Language == "ar" ? pf.DescriptionArabic : pf.Description) : "No Color",
                            ProductName = x.ProductName ?? "No Name"
                        })
                    .Where(x => x.AvailableToPromiseTotal > 0)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully retrieved {Count} BOM inventory items for ProductionRunId: {ProductionRunId}, WorkEffortId: {WorkEffortId}",
                    components.Count, productionRunId, workEffortId);

                return Result<List<BomInventoryItemDto>>.Success(components);
            }
            catch (Exception ex)
            {
                // REFACTOR: Handle and log exceptions
                // Purpose: Ensure robust error handling for data retrieval
                // Benefit: Provides clear error messages for debugging
                _logger.LogError(ex, "Error retrieving BOM inventory items for WorkEffortId: {WorkEffortId}", request.WorkEffortId);
                return Result<List<BomInventoryItemDto>>.Failure($"Error retrieving BOM inventory items: {ex.Message}");
            }
        }
    }

    public class BomInventoryItemDto
    {
        public string ProductId { get; set; }
        public decimal EstimatedQuantity { get; set; }
        public string InventoryItemId { get; set; }
        public decimal AvailableToPromiseTotal { get; set; }
        public string ProductFeatureId { get; set; }
        public string ColorDescription { get; set; }
        public string ProductName { get; set; }
    }
}