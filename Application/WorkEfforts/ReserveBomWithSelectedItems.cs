using Application.Catalog.Products.Services.Inventory;
using Application.Manufacturing;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

public class ReserveBomWithSelectedItems
{
    public class Command : IRequest<Result<ReserveBomWithSelectedItemsResult>>
    {
        public ReserveBomWithSelectedItemsParams ReserveBomParams { get; set; } = null!;
    }

    public class ReserveBomWithSelectedItemsParams
    {
        public string WorkEffortId { get; set; } = null!;
        public List<BomReservationItem> Items { get; set; } = null!;
        public bool IsAdditionalMaterials { get; set; } = false; // REFACTOR: Add flag for additional materials
    }

    public class BomReservationItem
    {
        public string InventoryItemId { get; set; } = null!;
        public string ProductId { get; set; } = null!;
        public decimal Quantity { get; set; }
    }

    public class ReserveBomWithSelectedItemsResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Command, Result<ReserveBomWithSelectedItemsResult>>
    {
        private readonly DataContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IProductionRunService _productionRunService;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, IProductionRunService productionRunService, IInventoryService inventoryService, ILogger<Handler> logger)
        {
            _context = context;
            _productionRunService = productionRunService;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        public async Task<Result<ReserveBomWithSelectedItemsResult>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Validate inputs to ensure WorkEffortId and Items are not null/empty
            // Purpose: Improve robustness by checking for invalid requests
            // Benefit: Prevents processing with incomplete data
            if (string.IsNullOrEmpty(request.ReserveBomParams.WorkEffortId) || request.ReserveBomParams.Items == null || !request.ReserveBomParams.Items.Any())
                return Result<ReserveBomWithSelectedItemsResult>.Failure("Invalid request: WorkEffortId or Items cannot be empty.");

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                // REFACTOR: Find the production run (WorkEffortParentId) for the given task
                // Purpose: Identify the PROD_ORDER_HEADER associated with the input task
                // Benefit: Allows finding the first task in the production run
                var task = await _context.WorkEfforts
                    .Where(we => we.WorkEffortId == request.ReserveBomParams.WorkEffortId && we.WorkEffortTypeId == "PROD_ORDER_TASK")
                    .Select(we => new { we.WorkEffortParentId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (task == null || string.IsNullOrEmpty(task.WorkEffortParentId))
                {
                    _logger.LogWarning("Task not found or not a PROD_ORDER_TASK for WorkEffortId: {WorkEffortId}", request.ReserveBomParams.WorkEffortId);
                    return Result<ReserveBomWithSelectedItemsResult>.Failure("Task not found or not associated with a production run");
                }

                var productionRunId = task.WorkEffortParentId;

                // REFACTOR: Validate production run existence
                // Purpose: Ensure the parent is a valid PROD_ORDER_HEADER
                // Benefit: Prevents invalid operations
                var productionRunExists = await _context.WorkEfforts
                    .AnyAsync(we => we.WorkEffortId == productionRunId && we.WorkEffortTypeId == "PROD_ORDER_HEADER", cancellationToken);

                if (!productionRunExists)
                {
                    _logger.LogWarning("Production run not found for WorkEffortParentId: {ProductionRunId}", productionRunId);
                    return Result<ReserveBomWithSelectedItemsResult>.Failure("Production run not found");
                }

                // REFACTOR: Find the first task's WorkEffortId using WorkEffortAssocs SequenceNum
                // Purpose: Identify the first PROD_ORDER_TASK for BOM reservations
                // Benefit: Ensures reservations are made against the correct task
                var firstTask = await _context.WorkEfforts
                    .Where(we => we.WorkEffortParentId == productionRunId && we.WorkEffortTypeId == "PROD_ORDER_TASK")
                    .Join(
                        _context.WorkEffortAssocs
                            .Where(wa => wa.WorkEffortAssocTypeId == "WORK_EFF_TEMPLATE"),
                        we => we.WorkEffortId,
                        wa => wa.WorkEffortIdTo,
                        (we, wa) => new { we.WorkEffortId, wa.SequenceNum }
                    )
                    .OrderBy(x => x.SequenceNum ?? int.MaxValue)
                    .ThenBy(x => x.WorkEffortId)
                    .Select(x => new { x.WorkEffortId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (firstTask == null)
                {
                    // REFACTOR: Fallback to WorkEfforts without SequenceNum
                    // Purpose: Handle cases where WorkEffortAssocs may not exist
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
                        return Result<ReserveBomWithSelectedItemsResult>.Failure("No tasks found for the production run");
                    }
                }

                var workEffortId = firstTask.WorkEffortId;
                _logger.LogInformation(
                    "Identified first task WorkEffortId: {WorkEffortId} for ProductionRunId: {ProductionRunId} from input WorkEffortId: {InputWorkEffortId}",
                    workEffortId, productionRunId, request.ReserveBomParams.WorkEffortId);

                // REFACTOR: Verify first task exists and is in a valid state
                // Purpose: Ensure the identified task is valid for reservations
                // Benefit: Prevents reservations against invalid tasks
                var workEffort = await _context.WorkEfforts.FindAsync(new object[] { workEffortId }, cancellationToken);
                if (workEffort == null)
                    return Result<ReserveBomWithSelectedItemsResult>.Failure($"WorkEffort {workEffortId} not found.");

                if (workEffort.CurrentStatusId == "PRUN_CANCELLED" || workEffort.CurrentStatusId == "PRUN_CLOSED")
                    return Result<ReserveBomWithSelectedItemsResult>.Failure($"Cannot reserve for a canceled/closed production run {workEffortId}.");

                // REFACTOR: Validate BOM components only for full BOM reservations
                // Purpose: Skip BOM validation for additional materials, as WorkEffortGoodStandards may not be in WEGS_CREATED state
                // Benefit: Allows additional material reservations after initial BOM issuance
                List<WorkEffortGoodStandard> bomComponents = null!;
                if (!request.ReserveBomParams.IsAdditionalMaterials)
                {
                    bomComponents = await _context.WorkEffortGoodStandards
                        .Where(wgs => wgs.WorkEffortId == workEffortId && wgs.StatusId == "WEGS_CREATED" && wgs.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED")
                        .ToListAsync(cancellationToken);
                }

                foreach (var item in request.ReserveBomParams.Items)
                {
                    // REFACTOR: Validate InventoryItem exists, is physical, and has sufficient ATP
                    // Purpose: Ensure valid inventory items for reservation
                    // Benefit: Maintains data integrity
                    var inventoryItem = await _context.InventoryItems
                        .Where(ii => ii.InventoryItemId == item.InventoryItemId && ii.ProductId == item.ProductId)
                        .Select(ii => new { ii.AvailableToPromiseTotal, ii.ProductId, ii.StatusId })
                        .FirstOrDefaultAsync(cancellationToken);

                    if (inventoryItem == null)
                        return Result<ReserveBomWithSelectedItemsResult>.Failure($"InventoryItem {item.InventoryItemId} not found for Product {item.ProductId}.");

                    if (inventoryItem.StatusId == "INV_NS_DEFECTIVE" || inventoryItem.StatusId == "INV_DEFECTIVE")
                        return Result<ReserveBomWithSelectedItemsResult>.Failure($"InventoryItem {item.InventoryItemId} is defective.");

                    if (inventoryItem.AvailableToPromiseTotal < item.Quantity)
                        return Result<ReserveBomWithSelectedItemsResult>.Failure(
                            $"Insufficient ATP for InventoryItem {item.InventoryItemId}: {inventoryItem.AvailableToPromiseTotal} available, {item.Quantity} requested.");

                    // REFACTOR: Validate BOM component only for full BOM reservations
                    // Purpose: Skip BOM check for additional materials to allow reservations post-issuance
                    // Benefit: Enables flexible additional material reservations
                    if (!request.ReserveBomParams.IsAdditionalMaterials)
                    {
                        var bomComponent = bomComponents.FirstOrDefault(bc => bc.ProductId == item.ProductId && (decimal)bc.EstimatedQuantity >= item.Quantity);
                        if (bomComponent == null)
                            return Result<ReserveBomWithSelectedItemsResult>.Failure(
                                $"Invalid reservation: Product {item.ProductId} not required or quantity exceeds BOM needs.");
                    }

                    // REFACTOR: Create reservation record using selected InventoryItemId
                    // Purpose: Record the reservation against the first task
                    // Benefit: Aligns with existing reservation logic
                    var reservation = new WorkEffortInventoryRes
                    {
                        WorkEffortInvResId = Guid.NewGuid().ToString(),
                        WorkEffortId = workEffortId,
                        InventoryItemId = item.InventoryItemId,
                        ReserveOrderEnumId = "INVRO_MANUAL",
                        Quantity = item.Quantity,
                        QuantityNotAvailable = 0,
                        ReservedDatetime = DateTime.UtcNow,
                        CreatedDatetime = DateTime.UtcNow,
                        PromisedDatetime = DateTime.UtcNow.AddDays(2),
                        Priority = 1,
                        LastUpdatedStamp = DateTime.UtcNow,
                        CreatedStamp = DateTime.UtcNow
                    };
                    _context.WorkEffortInventoryRes.Add(reservation);

                    // REFACTOR: Update ATP to reflect reservation
                    // Purpose: Adjust inventory availability using existing service
                    // Benefit: Maintains consistency with inventory management
                    var createDetailParam = new CreateInventoryItemDetailParam
                    {
                        InventoryItemId = item.InventoryItemId,
                        AvailableToPromiseDiff = -item.Quantity,
                        QuantityOnHandDiff = 0m,
                        WorkEffortId = workEffortId,
                        Description = "Manual reservation for production run"
                    };
                    await _inventoryService.CreateInventoryItemDetail(createDetailParam);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return Result<ReserveBomWithSelectedItemsResult>.Success(new ReserveBomWithSelectedItemsResult
                {
                    Success = true,
                    Message = "BOM items reserved successfully."
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error reserving BOM items for WorkEffortId: {WorkEffortId}", request.ReserveBomParams.WorkEffortId);
                return Result<ReserveBomWithSelectedItemsResult>.Failure($"Error reserving components: {ex.Message}");
            }
        }
    }
}