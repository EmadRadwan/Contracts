using MediatR;
using Persistence;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Application.WorkEfforts
{
    public class GetRoutingTasksForProductionRun
    {
        public class Query : IRequest<Result<ProductionRunDetailsDto>>
        {
            public string? ProductionRunId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<ProductionRunDetailsDto>>
        {
            private readonly DataContext _context;
            private readonly ILogger<Handler> _logger;
            private readonly Serilog.ILogger loggerForTransaction;


            public Handler(DataContext context, ILogger<Handler> logger)
            {
                _context = context;
                _logger = logger;

                loggerForTransaction = Log.ForContext("Transaction", "Get Routing Tasks");
            }

            public async Task<Result<ProductionRunDetailsDto>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                try
                {
                    // Validate the input parameter ProductionRunId
                    var productionRunId = request.ProductionRunId;

                    if (string.IsNullOrEmpty(productionRunId))
                    {
                        return Result<ProductionRunDetailsDto>.Failure("ProductionRunId is required.");
                    }

                    // Retrieve the production run entity from the database
                    // This is analogous to the initial setup in the Groovy script where the production run context is established.
                    var productionRun =
                        await _context.WorkEfforts.FindAsync(new object[] { productionRunId }, cancellationToken);

                    if (productionRun == null)
                    {
                        return Result<ProductionRunDetailsDto>.Failure("Production run not found.");
                    }

                    // Initialize the DTO that will be returned with detailed production run information.
                    // This mirrors the context setup in the Groovy script.
                    var productionRunDto = new ProductionRunDetailsDto
                    {
                        Priority = productionRun.Priority,
                        WorkEffortId = productionRun.WorkEffortId,
                        ProductionRunId = productionRun.WorkEffortId,
                        QuantityToProduce = (double)(productionRun.QuantityToProduce ?? 0),
                        QuantityProduced = (double)(productionRun.QuantityProduced ?? 0),
                        QuantityRejected = (double)(productionRun.QuantityRejected ?? 0),
                        EstimatedCompletionDate = productionRun.EstimatedCompletionDate,
                        ProductionRunName = productionRun.WorkEffortName,
                        Description = productionRun.Description,
                        EstimatedStartDate = productionRun.EstimatedStartDate,
                        ActualStartDate = productionRun.ActualStartDate,
                        ActualCompletionDate = productionRun.ActualCompletionDate,
                        CurrentStatusId = productionRun.CurrentStatusId,
                        FacilityId = productionRun.FacilityId,
                        OrderItems = new List<WorkOrderItemFulfillment>(),
                        InventoryItems = new List<WorkEffortInventoryProduced>(),
                        ProductionRunRoutingTasks = new List<ProductionRunRoutingTaskDto>(),
                        ProductionRunComponents = new List<ProductionRunComponentDto>()
                    };


                    // Fetch the product that is produced by the production run
                    // This aligns with how the Groovy script fetches the product using `WorkEffortGoodStandard` with `PRUN_PROD_DELIV`.
                    var productProduced = await _context.WorkEffortGoodStandards
                        .AsNoTracking()
                        .Where(w => w.WorkEffortId == productionRunId && w.WorkEffortGoodStdTypeId == "PRUN_PROD_DELIV")
                        .Select(w => w.Product)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (productProduced != null)
                    {
                        productionRunDto.ProductId = productProduced.ProductId;
                        productionRunDto.ProductName =
                            productProduced.ProductName; // Assuming ProductName is a property of Product
                    }

                    // Determine if the production run can declare and produce inventory
                    // This step reflects the logic in the Groovy script that checks if the last task allows for declaration and production.
                    var lastTask = await _context.WorkEfforts
                        .AsNoTracking()
                        .Where(x => x.WorkEffortParentId == productionRunId && x.WorkEffortTypeId == "PROD_ORDER_TASK")
                        .OrderByDescending(x => x.Priority)
                        .FirstOrDefaultAsync(cancellationToken);

                    productionRunDto.CanDeclareAndProduce = (lastTask != null &&
                                                             (lastTask.CurrentStatusId == "PRUN_RUNNING" ||
                                                              lastTask.CurrentStatusId == "PRUN_COMPLETED"))
                        ? "Y"
                        : "N";

                    loggerForTransaction.Information(
                        "Last Task & CanDeclareAndProduce: Last task {LastTaskId} status: {StatusId}, CanDeclareAndProduce: {CanDeclareAndProduce}",
                        lastTask?.WorkEffortId, lastTask?.CurrentStatusId, productionRunDto.CanDeclareAndProduce);


                    // Fetch production run routing tasks
                    // This matches the logic in the Groovy script for retrieving and processing routing tasks.
                    var productionRunRoutingTasks = await _context.WorkEfforts
                        .Where(x => x.WorkEffortParentId == productionRunId && x.WorkEffortTypeId == "PROD_ORDER_TASK")
                        .Include(x => x.CurrentStatus) // Ensure CurrentStatus is included
                        .Include(x => x.FixedAsset) // Ensure FixedAsset is included
                        .AsNoTracking()
                        .OrderBy(x => x.Priority)
                        .ToListAsync(cancellationToken);

                    bool bomIssuedForFirstTask = false;
                    bool allPreviousTasksCompleted = true;


                    // Process each routing task, setting task-specific flags and adding task details to the DTO.
                    foreach (var task in productionRunRoutingTasks)
                    {
                        var isFirstTask = task.Priority == productionRunRoutingTasks.Min(t => t.Priority);
                        loggerForTransaction.Information("isFirstTask {isFirstTask}", isFirstTask);

                        var isFinalTask = task.Priority == productionRunRoutingTasks.Max(t => t.Priority);
                        loggerForTransaction.Information("isFinalTask {isFinalTask}", isFinalTask);


                        // Map the routing task to the DTO, including status, priority, and flags for start, issue, and complete tasks.
                        var taskDto = new ProductionRunRoutingTaskDto
                        {
                            WorkEffortId = task.WorkEffortId,
                            Priority = productionRun.Priority,
                            ActualStartDate = productionRun.ActualStartDate,
                            ActualCompletionDate = productionRun.ActualCompletionDate,
                            WorkEffortParentId = task.WorkEffortParentId,
                            WorkEffortName = task.WorkEffortName + " [" + task.WorkEffortId + "]",
                            Description = task.Description,
                            QuantityToProduce = task.QuantityToProduce,
                            CurrentStatusId = task.CurrentStatusId,
                            CurrentStatusDescription =
                                task.CurrentStatus?.Description, // Ensure CurrentStatus is not null
                            EstimatedStartDate = task.EstimatedStartDate,
                            EstimatedCompletionDate = task.EstimatedCompletionDate,
                            EstimatedSetupMillis = task.EstimatedSetupMillis,
                            EstimatedMilliSeconds = task.EstimatedMilliSeconds,
                            ActualMilliSeconds = task.ActualMilliSeconds,
                            ActualSetupMillis = task.ActualSetupMillis,
                            FixedAssetId = task.FixedAssetId,
                            FixedAssetName = task.FixedAsset?.FixedAssetName, // Ensure FixedAsset is not null
                            SequenceNum = task.Priority,
                            IsFinalTask = isFinalTask,
                            QuantityProduced = task.QuantityProduced,
                            QuantityRejected = task.QuantityRejected,
                            CanDeclareAndProduce = productionRunDto.CanDeclareAndProduce,
                            LastLotId = productionRunDto.LastLotId,
                            CanStartTask = false, // Default; logic will determine if it can start
                            CanCompleteTask = false, // Default; logic will determine if it can complete
                            CanDeclareTask = false, // Default; logic will determine if it can declare
                            CanProduce = "N", // Default for non-producing tasks
                            BomReservationInProgress = false
                        };

                        // First task logic for BOM issuance
                        if (isFirstTask)
                        {
                            var bomIssued = await _context.WorkEffortGoodStandards
                                .AsNoTracking()
                                .Where(w => w.WorkEffortId == task.WorkEffortId &&
                                            w.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED" &&
                                            w.StatusId != "WEGS_CREATED")
                                .AnyAsync(cancellationToken);

                            bomIssuedForFirstTask = bomIssued;
                            
                            // ==========================
                            // BOM: Reservation Check
                            // ==========================
                            // If no BOM is physically issued yet, but we DO have lines in
                            // WorkEffortInventoryRes, that means we "reserved" but not "issued."
                            var hasRes = await _context.WorkEffortInventoryRes
                                .AsNoTracking()
                                .Where(r => r.WorkEffortId == task.WorkEffortId && r.Quantity > 0)
                                .AnyAsync(cancellationToken);

                            // If we found reservations in progress (no BOM physically issued yet),
                            // then set BomReservationInProgress.
                            // Typically, we do this check only for the first task, but you can adapt for multiple tasks if needed
                            if (isFirstTask && !bomIssued && hasRes)
                            {
                                taskDto.BomReservationInProgress = true;
                            }

                            // If BOM physically issued and not running/completed => can start
                            if (bomIssued && task.CurrentStatusId != "PRUN_RUNNING" && task.CurrentStatusId != "PRUN_COMPLETED")
                            {
                                taskDto.CanStartTask = true;
                            }

                            // If the task is running -> can complete
                            if (task.CurrentStatusId == "PRUN_RUNNING")
                            {
                                taskDto.CanCompleteTask = true;
                                taskDto.CanDeclareTask  = true;
                            }

                            // If we found a reservation in progress => we won't let them start
                            // because the BOM isn't physically issued yet.
                            if (taskDto.BomReservationInProgress)
                            {
                                taskDto.CanStartTask = false;
                            }
                        }
                        else
                        {
                            // Check previous tasks using productionRunRoutingTasks
                            var previousTasksCompleted = productionRunRoutingTasks
                                .Where(x => x.Priority < task.Priority) // Only previous tasks
                                .Select(x => x.CurrentStatusId == "PRUN_COMPLETED") // Check completion status
                                .ToList();

                            // Determine if all previous tasks are completed
                            allPreviousTasksCompleted = previousTasksCompleted.All(x => x);

                            // Non-first task logic: check if all previous tasks are completed and BOM is issued
                            if (allPreviousTasksCompleted && bomIssuedForFirstTask &&
                                task.CurrentStatusId != "PRUN_RUNNING" && task.CurrentStatusId != "PRUN_COMPLETED")
                            {
                                taskDto.CanStartTask = true;
                            }

                            // If task is running, allow it to complete
                            if (task.CurrentStatusId == "PRUN_RUNNING")
                            {
                                taskDto.CanCompleteTask = true;
                                taskDto.CanDeclareTask = true;
                            }

                            // If task is not completed, mark allPreviousTasksCompleted as false
                            if (task.CurrentStatusId != "PRUN_COMPLETED")
                            {
                                allPreviousTasksCompleted = false;
                            }
                        }


                        // Additional logic for completion and declaration checks
                        if (task.CurrentStatusId == "PRUN_COMPLETED")
                        {
                            //taskDto.CanDeclareTask = true;
                            var byProducts = await _context.WorkEffortGoodStandards
                                .AsNoTracking()
                                .Where(w => w.WorkEffortId == task.WorkEffortId &&
                                            w.WorkEffortGoodStdTypeId == "PRUN_PROD_DELIV")
                                .ToListAsync(cancellationToken);

                            if (byProducts.Count > 0)
                            {
                                taskDto.CanProduce = "Y";
                            }
                        }

                        // Add the processed task DTO to the main production run DTO
                        productionRunDto.ProductionRunRoutingTasks.Add(taskDto);
                    }

                    var allTasksCompleted = productionRunRoutingTasks
                        .All(x => x.CurrentStatusId == "PRUN_COMPLETED");
                    productionRunDto.CanDeclareAndProduce = (allTasksCompleted) ? "Y" : "N";

                    // Return the fully populated DTO
                    return Result<ProductionRunDetailsDto>.Success(productionRunDto);
                }
                catch (Exception ex)
                {
                    // Log any exceptions that occur during processing, ensuring that errors are captured and reported.
                    _logger.LogError(ex, "An error occurred while getting routing tasks for production run.");
                    return Result<ProductionRunDetailsDto>.Failure("An error occurred while processing your request.");
                }
            }
        }
    }
}