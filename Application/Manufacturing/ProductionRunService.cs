using System.Runtime.CompilerServices;
using API.Middleware;
using Application.Accounting.Services;
using Application.Catalog.Products;
using Application.Catalog.Products.Services.Cost;
using Application.Catalog.Products.Services.Inventory;
using Application.Core;
using Application.Facilities;
using Application.Interfaces;
using Application.WorkEfforts;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence;
using Serilog;
using InvalidOperationException = System.InvalidOperationException;

namespace Application.Manufacturing;

public interface IProductionRunService
{
    Task<CreateProductionRunResponse> CreateProductionRun(
        string productId,
        DateTime startDate,
        decimal prQuantity,
        string facilityId,
        string routingId = null,
        string workEffortName = null,
        string description = null);

    Task<UpdateProductionRunResponse> UpdateProductionRun(
        string productionRunId,
        decimal? quantity,
        DateTime? estimatedStartDate,
        string workEffortName,
        string description,
        string facilityId);

    Task<ChangeProductionRunStatusResult> ChangeProductionRunStatus(
        string productionRunId,
        string statusId);

    Task<QuickChangeProductionRunStatusResult> QuickChangeProductionRunStatus(
        string productionRunId,
        string statusId,
        string startAllTasks);

    Task<ChangeProductionRunTaskStatusResult> ChangeProductionRunTaskStatus(
        string productionRunId,
        string taskId,
        string statusId,
        bool? issueAllComponents);

    Task<Results<IssueProductionRunTaskResult>> IssueProductionRunTask(
        string workEffortId,
        string reserveOrderEnumId = null,
        string failIfItemsAreNotAvailable = "Y",
        string failIfItemsAreNotOnHand = "Y");

    Task<DeclareAndProduceProductionRunResult> ProductionRunDeclareAndProduce(
        DeclareAndProduceProductionRunParams declareAndProduceProductionRunParams);

    Task<UpdateProductionRunTaskResult> UpdateProductionRunTask(UpdateProductionRunTaskContext context);

    Task<List<WorkEffortGoodStandardDto>> ListIssueProductionRunDeclComponents(string productionRunId, string language);
    Task<List<ProductionRunComponentDto>> GetProductionRunComponentsForReturn(string productionRunId);

    Task ReserveProductionRunTask(string workEffortId, string requireInventory = "Y");

    Task IssueProductionRunReservations(
        string workEffortId,
        bool failIfNotEnoughQoh = true,
        string? reasonEnumId = null,
        string? description = null);

    Task<AssignPartyToWorkEffortResult> AssignPartyToWorkEffort(
        string workEffortId,
        string partyId,
        string roleTypeId,
        DateTime? fromDate,
        string statusId);

    Task<ProductionRunTaskReturnMaterialResult> ProductionRunTaskReturnMaterial(
        string workEffortId, string productId, decimal quantity, string lotId = null, string uomId = null);
}

public class ProductionRunService : IProductionRunService
{
    private readonly ITechDataService _techDataService;
    private readonly DataContext _context;
    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly IRoutingService _routingService;
    private readonly IWorkEffortService _workEffortService;
    private readonly ICostService _costService;
    private readonly IUtilityService _utilityService;
    private readonly IInventoryService _inventoryService;
    private readonly IAcctgMiscService _acctgMiscService;
    private readonly IGeneralLedgerService _generalLedgerService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUserAccessor _userAccessor;
    private readonly IFacilityService _facilityService;
    private readonly Serilog.ILogger loggerForTransaction;

    public ProductionRunService(DataContext context, ILogger<ProductionRunService> logger,
        IRoutingService routingService,
        IWorkEffortService workEffortService, ICostService costService, ITechDataService techDataService,
        IUtilityService utilityService, IInventoryService inventoryService, IAcctgMiscService acctgMiscService,
        IGeneralLedgerService generalLedgerService, IServiceProvider serviceProvider, IUserAccessor userAccessor,
        IFacilityService facilityService
    )
    {
        _context = context;
        _logger = logger;
        _techDataService = techDataService;
        _routingService = routingService;
        _workEffortService = workEffortService;
        _costService = costService;
        _utilityService = utilityService;
        _inventoryService = inventoryService;
        _acctgMiscService = acctgMiscService;
        _generalLedgerService = generalLedgerService;
        _serviceProvider = serviceProvider;
        _userAccessor = userAccessor;
        _facilityService = facilityService;

        loggerForTransaction = Log.ForContext("Transaction", "create production run");
    }

    public async Task<CreateProductionRunResponse> CreateProductionRun(
        string productId,
        DateTime startDate,
        decimal prQuantity,
        string facilityId,
        string routingId = null,
        string workEffortName = null,
        string description = null)
    {
        try
        {
            loggerForTransaction.Information(
                "Starting the production run creation for product {ProductId} with quantity {Quantity} at facility {FacilityId}",
                productId, prQuantity, facilityId);

            var response = new CreateProductionRunResponse();
            response.EstimatedStartDate = startDate;

            loggerForTransaction.Information("Fetching product details for ProductId: {ProductId}", productId);
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                loggerForTransaction.Warning("Product with ID {ProductId} does not exist", productId);
                throw new Exception("Product does not exist");
            }

            loggerForTransaction.Information(
                "Fetching routing details for ProductId: {ProductId} with RoutingId: {RoutingId}", productId,
                routingId);
            var routingOutMap = await _routingService.GetProductRouting(productId, routingId, startDate);
            if (routingOutMap == null || routingOutMap.Routing == null)
            {
                loggerForTransaction.Warning("Routing for ProductId {ProductId} does not exist", productId);
                throw new Exception("Routing does not exist");
            }

            var routingTasks = routingOutMap.Tasks;
            if (routingTasks == null || !routingTasks.Any())
            {
                loggerForTransaction.Warning("No routing tasks found for ProductId {ProductId}", productId);
                throw new Exception("Routing tasks do not exist");
            }

            loggerForTransaction.Information(
                "Fetching manufacturing components for ProductId: {ProductId} and quantity {Quantity}", productId,
                prQuantity);
            var components =
                await _routingService.GetManufacturingComponents(productId, prQuantity, null, startDate.ToString(),
                    false);

            loggerForTransaction.Information("Creating production run header for ProductId {ProductId}", productId);
            workEffortName = workEffortName ??
                             $"{product.ProductName ?? product.ProductId}-{routingTasks.First().WorkEffortIdFromNavigation.WorkEffortName}";
            var productionRunId = await _workEffortService.CreateWorkEffort(new WorkEffort
            {
                WorkEffortTypeId = "PROD_ORDER_HEADER",
                WorkEffortPurposeTypeId = "WEPT_PRODUCTION_RUN",
                CurrentStatusId = "PRUN_CREATED",
                WorkEffortName = workEffortName,
                Description = description,
                FacilityId = facilityId,
                EstimatedStartDate = startDate,
                QuantityToProduce = prQuantity
            });
            loggerForTransaction.Information("Production run header created with ProductionRunId {ProductionRunId}",
                productionRunId);

            loggerForTransaction.Information(
                "Creating WorkEffortGoodStandard record for produced goods of ProductId {ProductId}", productId);
            _workEffortService.CreateWorkEffortGoodStandard(new WorkEffortGoodStandard
            {
                WorkEffortId = productionRunId,
                ProductId = productId,
                WorkEffortGoodStdTypeId = "PRUN_PROD_DELIV",
                StatusId = "WEGS_CREATED",
                EstimatedQuantity = (double?)prQuantity,
                FromDate = startDate,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            });

            loggerForTransaction.Information("Creating production run tasks for ProductionRunId {ProductionRunId}",
                productionRunId);
            var first = true;
            foreach (var routingTaskAssoc in routingOutMap.Tasks)
            {
                var fromDate = routingTaskAssoc.FromDate;
                var thruDate = routingTaskAssoc.ThruDate;
                bool isValueActive = (!thruDate.HasValue || thruDate > startDate) &&
                                     (!fromDate.HasValue || fromDate <= startDate);

                if (isValueActive)
                {
                    loggerForTransaction.Information("Creating production run task from RoutingTaskId {RoutingTaskId}",
                        routingTaskAssoc.WorkEffortIdTo);

                    var routingTask =
                        await _context.WorkEfforts.FindAsync(routingTaskAssoc.WorkEffortIdTo);

                    var totalTime = await _costService.GetEstimatedTaskTime(routingTask.WorkEffortId);
                    var endDate = await _techDataService.AddForward(
                        await _techDataService.GetTechDataCalendar(routingTask),
                        startDate, totalTime.EstimatedTaskTime);

                    var taskId = await _utilityService.GetNextSequence("WorkEffort");
                    var productionRunTask = new WorkEffort
                    {
                        WorkEffortId = taskId,
                        Priority = routingTaskAssoc.SequenceNum,
                        WorkEffortPurposeTypeId = "WEPT_PRODUCTION_RUN",
                        WorkEffortName = routingTask.WorkEffortName,
                        Description = routingTask.Description,
                        FixedAssetId = routingTask.FixedAssetId,
                        WorkEffortTypeId = "PROD_ORDER_TASK",
                        CurrentStatusId = "PRUN_CREATED",
                        WorkEffortParentId = productionRunId,
                        FacilityId = facilityId,
                        ReservPersons = routingTask.ReservPersons,
                        EstimatedStartDate = startDate,
                        EstimatedCompletionDate = endDate,
                        EstimatedSetupMillis = routingTask.EstimatedSetupMillis,
                        EstimatedMilliSeconds = routingTask.EstimatedMilliSeconds,
                        QuantityToProduce = prQuantity,
                        LastStatusUpdate = DateTime.UtcNow,
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow
                    };
                    _context.WorkEfforts.Add(productionRunTask);

                    var productionRunTaskId = productionRunTask.WorkEffortId;

                    var workEffortAssoc = new WorkEffortAssoc
                    {
                        WorkEffortIdFrom = routingTask.WorkEffortId,
                        WorkEffortIdTo = productionRunTaskId,
                        WorkEffortAssocTypeId = "WORK_EFF_TEMPLATE",
                        SequenceNum = routingTaskAssoc.SequenceNum,
                        FromDate = startDate,
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow
                    };
                    _context.WorkEffortAssocs.Add(workEffortAssoc);

                    await CloneWorkEffortPartyAssignments(routingTask.WorkEffortId, productionRunTaskId);
                    await CloneWorkEffortCostCalcs(routingTask.WorkEffortId, productionRunTaskId);

                    foreach (var node in components.Components)
                    {
                        if ((node.RoutingWorkEffortId == null && first) ||
                            (node.RoutingWorkEffortId == routingTask.WorkEffortId))
                        {
                            loggerForTransaction.Information("Adding component {ComponentId} to task {TaskId}",
                                node.ProductIdTo, productionRunTaskId);

                            var productionRunGoodStandard = new WorkEffortGoodStandard
                            {
                                WorkEffortId = productionRunTaskId,
                                ProductId = node.ProductIdTo,
                                WorkEffortGoodStdTypeId = "PRUNT_PROD_NEEDED",
                                StatusId = "WEGS_CREATED",
                                FromDate = node.FromDate,
                                EstimatedQuantity = (double?)node.Quantity * (double?)prQuantity,
                                LastUpdatedStamp = DateTime.UtcNow,
                                CreatedStamp = DateTime.UtcNow
                            };
                            _context.WorkEffortGoodStandards.Add(productionRunGoodStandard);
                        }
                    }

                    first = false;
                    startDate = endDate;
                }
            }

            loggerForTransaction.Information(
                "Updating production run with estimated completion date: {EstimatedCompletionDate}", startDate);
            await _workEffortService.UpdateWorkEffort(productionRunId, new Dictionary<string, object>
            {
                { "currentStatusId", "PRUN_CREATED" },
                { "estimatedCompletionDate", startDate }
            });

            response.ProductionRunId = productionRunId;
            response.ProductId = new ProductLovDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName
            };
            response.ProductName = product.ProductName;
            response.FacilityName = (await _context.Facilities.FindAsync(facilityId))
                ?.FacilityName;
            response.CurrentStatusDescription = "Created";
            response.EstimatedCompletionDate = startDate;

            loggerForTransaction.Information("Production run created successfully with ID {ProductionRunId}",
                productionRunId);
            return response;
        }
        catch (Exception ex)
        {
            loggerForTransaction.Error(ex,
                "An error occurred while creating the production run for product {ProductId}", productId);
            throw;
        }
    }


    public async Task<UpdateProductionRunResponse> UpdateProductionRun(
        string productionRunId,
        decimal? quantity,
        DateTime? estimatedStartDate,
        string workEffortName,
        string description,
        string facilityId)
    {
        // Check if productionRunId is not empty
        if (!string.IsNullOrEmpty(productionRunId))
        {
            try
            {
                // Fetch the production run by its ID (WorkEffort)
                var productionRun = await _context.WorkEfforts.FindAsync(productionRunId);

                // If production run exists, proceed with the update
                if (productionRun != null)
                {
                    // Ensure the current status is either PRUN_CREATED or PRUN_SCHEDULED
                    if (productionRun.CurrentStatusId != "PRUN_CREATED" &&
                        productionRun.CurrentStatusId != "PRUN_SCHEDULED")
                    {
                        throw new InvalidOperationException("ManufacturingProductionRunPrinted");
                    }

                    // Fetch the WorkEffortGoodStandard for this production run to manage quantity
                    var productionRunProduct = await _context.WorkEffortGoodStandards
                        .FirstOrDefaultAsync(w =>
                            w.WorkEffortId == productionRunId && w.WorkEffortGoodStdTypeId == "PRUN_PROD_DELIV");

                    if (productionRunProduct == null)
                    {
                        throw new InvalidOperationException("WorkEffortGoodStandard not found for the production run");
                    }

                    // Check if the completion date needs to be updated
                    bool updateCompletionDate = false;

                    // Update the quantity if it is provided and differs from the current value in WorkEffortGoodStandard
                    if (quantity.HasValue && quantity.Value != (decimal)productionRunProduct.EstimatedQuantity)
                    {
                        // Call SetQuantityAsync to update the quantity and components
                        await SetQuantity(quantity.Value, productionRunId);

                        productionRunProduct.EstimatedQuantity = (double?)quantity.Value;
                        _context.WorkEffortGoodStandards.Update(productionRunProduct);
                        updateCompletionDate = true; // Set the flag if quantity is updated
                    }

                    // Update the estimated start date if it is provided and differs from the current value
                    if (estimatedStartDate.HasValue && estimatedStartDate.Value != productionRun.EstimatedStartDate)
                    {
                        productionRun.EstimatedStartDate = estimatedStartDate.Value;
                        updateCompletionDate = true; // Set the flag if start date is updated
                    }

                    // Update the production run name if provided
                    if (!string.IsNullOrEmpty(workEffortName))
                    {
                        productionRun.WorkEffortName = workEffortName;
                    }

                    // Update the description if provided
                    if (!string.IsNullOrEmpty(description))
                    {
                        productionRun.Description = description;
                    }

                    // Update the facility ID if provided
                    if (!string.IsNullOrEmpty(facilityId))
                    {
                        productionRun.FacilityId = facilityId;
                    }

                    // Recalculate the estimated completion date if needed
                    if (updateCompletionDate)
                    {
                        var newEstimatedCompletionDate = await RecalculateEstimatedCompletionDate(
                            (decimal)productionRunProduct.EstimatedQuantity,
                            productionRun.EstimatedStartDate ?? DateTime.MinValue,
                            productionRunId);

                        productionRun.EstimatedCompletionDate = newEstimatedCompletionDate;
                    }


                    _context.WorkEfforts.Update(productionRun);

                    // If the estimated completion date needs to be updated and the status is PRUN_SCHEDULED
                    if (updateCompletionDate && productionRun.CurrentStatusId == "PRUN_SCHEDULED")
                    {
                        // Call SetEstimatedDeliveryDatesAsync to update the delivery dates
                        await SetEstimatedDeliveryDates();
                    }

                    // Return the updated production run details in the response object
                    return new UpdateProductionRunResponse
                    {
                        ProductionRunId = productionRun.WorkEffortId,
                        CurrentStatusDescription = productionRun.CurrentStatusId,
                        EstimatedCompletionDate = productionRun.EstimatedCompletionDate ?? DateTime.MinValue,
                        EstimatedStartDate = productionRun.EstimatedStartDate ?? DateTime.MinValue,
                        Quantity = (decimal)quantity
                    };
                }
                else
                {
                    // Log an error if the production run was not found
                    _logger.LogError($"No productionRun found for productionRunId = {productionRunId}");
                    throw new InvalidOperationException("ManufacturingProductionRunNotUpdated");
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions that occur
                _logger.LogError(ex, $"Error occurred while updating production run with ID = {productionRunId}");
                throw new InvalidOperationException("ManufacturingProductionRunNotUpdated");
            }
        }

        // Log an error if productionRunId is empty and throw an exception
        _logger.LogError("Service updateProductionRun called with an empty productionRunId");
        throw new ArgumentException("ManufacturingProductionRunNotUpdated");
    }

    public async Task CloneWorkEffortPartyAssignments(string routingTaskId, string productionRunTaskId)
    {
        try
        {
            // Fetch and filter work effort party assignments for the given routingTaskId
            var workEffortPartyAssignments = await _context.WorkEffortPartyAssignments
                .Where(wepa => wepa.WorkEffortId == routingTaskId &&
                               (wepa.FromDate == null || wepa.FromDate <= DateTime.UtcNow) &&
                               (wepa.ThruDate == null || wepa.ThruDate > DateTime.UtcNow))
                .ToListAsync();

            if (workEffortPartyAssignments != null)
            {
                foreach (var workEffortPartyAssignment in workEffortPartyAssignments)
                {
                    try
                    {
                        var result = await AssignPartyToWorkEffort(
                            productionRunTaskId,
                            workEffortPartyAssignment.PartyId,
                            workEffortPartyAssignment.RoleTypeId,
                            workEffortPartyAssignment.FromDate,
                            workEffortPartyAssignment.StatusId);

                        if (result.IsError)
                        {
                            _logger.LogError($"Error assigning party to work effort: {result.ErrorMessage}");
                        }
                        else
                        {
                            _logger.LogInformation(
                                $"ProductionRunPartyAssignment for party: {workEffortPartyAssignment.PartyId} created");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Problem calling the AssignPartyToWorkEffort method");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching or filtering work effort party assignments");
        }
    }

    public async Task<AssignPartyToWorkEffortResult> AssignPartyToWorkEffort(
        string workEffortId,
        string partyId,
        string roleTypeId,
        DateTime? fromDate,
        string statusId)
    {
        try
        {
            // Check if the requested party assignment already exists
            var existingAssignment = await _context.WorkEffortPartyAssignments
                .FirstOrDefaultAsync(assignment =>
                    assignment.WorkEffortId == workEffortId &&
                    assignment.PartyId == partyId &&
                    assignment.RoleTypeId == roleTypeId);

            if (existingAssignment != null)
            {
                // Handle error scenario where assignment already exists
                return new AssignPartyToWorkEffortResult
                {
                    IsError = true,
                    ErrorMessage = "WorkEffortPartyAssignmentError: Assignment already exists."
                };
            }

            // Create a new WorkEffortPartyAssignment entity
            var newAssignment = new WorkEffortPartyAssignment
            {
                WorkEffortId = workEffortId,
                PartyId = partyId,
                RoleTypeId = roleTypeId,
                FromDate = fromDate ?? DateTime.UtcNow, // Use current time if FromDate is not provided
                StatusId = statusId,
                CreatedStamp = DateTime.Now, // Timestamps for audit purposes
                CreatedTxStamp = DateTime.Now
            };

            _context.WorkEffortPartyAssignments.Add(newAssignment);


            return new AssignPartyToWorkEffortResult
            {
                IsError = false,
                AssignedFromDate = newAssignment.FromDate // Return assigned FromDate for confirmation
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning party to work effort");
            return new AssignPartyToWorkEffortResult
            {
                IsError = true,
                ErrorMessage = ex.Message // Return exception message as error
            };
        }
    }

    public async Task CloneWorkEffortCostCalcs(string routingTaskId, string productionRunTaskId)
    {
        try
        {
            // Step 1: Fetch cost calculations related to the routingTaskId
            var workEffortCostCalcs = await _context.WorkEffortCostCalcs
                .Where(c => c.WorkEffortId == routingTaskId &&
                            (c.ThruDate == null || c.ThruDate > DateTime.UtcNow) &&
                            (c.FromDate == null || c.FromDate <= DateTime.UtcNow))
                .ToListAsync();

            // Step 2: Iterate and create new cost calculations for productionRunTaskId
            foreach (var costCalc in workEffortCostCalcs)
            {
                var newCostCalc = new WorkEffortCostCalc
                {
                    WorkEffortId = productionRunTaskId,
                    CostComponentTypeId = costCalc.CostComponentTypeId,
                    CostComponentCalcId = costCalc.CostComponentCalcId,
                    FromDate = costCalc.FromDate,
                    ThruDate = costCalc.ThruDate,
                    CreatedStamp = DateTime.UtcNow,
                    LastUpdatedStamp = DateTime.UtcNow
                };

                _context.WorkEffortCostCalcs.Add(newCostCalc);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error cloning WorkEffortCostCalcs: {ex.Message}");
        }
    }

    /// <summary>
    /// Quick moves a ProductionRun to the passed in status, performing all the needed tasks along the way.
    /// Aligns closely with the OFBiz version.
    /// </summary>
    /// <param name="productionRunId">The identifier of the production run.</param>
    /// <param name="statusId">The target status.</param>
    /// <param name="startAllTasks">Flag ("Y" to start all tasks) for running tasks.</param>
    /// <returns>A result object with the outcome and additional details.</returns>
    public async Task<QuickChangeProductionRunStatusResult> QuickChangeProductionRunStatus(
        string productionRunId,
        string statusId,
        string startAllTasks)
    {
        // Initialize the result object.
        var result = new QuickChangeProductionRunStatusResult { Success = true };

        // Fetch the production run entity.
        var productionRun = await _context.WorkEfforts.FindAsync(productionRunId);
        // Special Remark: In OFBiz, this validation isn’t done; here we explicitly validate the entity.
        if (productionRun == null)
        {
            return new QuickChangeProductionRunStatusResult
            {
                Success = false,
                ErrorMessage = "Production run not found."
            };
        }

        try
        {
            // Step 1: For specific statuses, first update to "PRUN_DOC_PRINTED".
            if (statusId == "PRUN_DOC_PRINTED" ||
                statusId == "PRUN_RUNNING" ||
                statusId == "PRUN_COMPLETED" ||
                statusId == "PRUN_CLOSED")
            {
                // Special Remark: Changing status to "PRUN_DOC_PRINTED" as per OFBiz logic.
                var changeToDocPrintedResult = await ChangeProductionRunStatus(productionRunId, "PRUN_DOC_PRINTED");
                if (!changeToDocPrintedResult.Success)
                {
                    return new QuickChangeProductionRunStatusResult
                    {
                        Success = false,
                        ErrorMessage = "Error changing production run status to DOC_PRINTED."
                    };
                }
            }

            // Step 2: If status is "PRUN_RUNNING" and startAllTasks flag is "Y", then start all tasks.
            if (statusId == "PRUN_RUNNING" && startAllTasks == "Y")
            {
                // Special Remark: Initiating quick start of all production run tasks.
                var startTasksResult = await QuickStartAllProductionRunTasks(productionRunId);
                if (!startTasksResult.Success)
                {
                    return new QuickChangeProductionRunStatusResult
                    {
                        Success = false,
                        ErrorMessage = "Error starting all production run tasks."
                    };
                }
            }

            // Step 3: If status is "PRUN_COMPLETED" or "PRUN_CLOSED", run all tasks.
            if (statusId == "PRUN_COMPLETED" || statusId == "PRUN_CLOSED")
            {
                // Special Remark: Running all production run tasks for COMPLETED or CLOSED status.
                var runAllTasksResult = await QuickRunAllProductionRunTasks(productionRunId);
                if (!runAllTasksResult.Success)
                {
                    return new QuickChangeProductionRunStatusResult
                    {
                        Success = false,
                        ErrorMessage = "Error running all production run tasks."
                    };
                }
            }

            // Step 4: Additional processing if status is "PRUN_CLOSED".
            if (statusId == "PRUN_CLOSED")
            {
                // Special Remark: For CLOSED status, produce items (move to warehouse) and then finalize status.
                var produceResult = await ProductionRunProduce(productionRun.WorkEffortId, null);
                if (!produceResult.Success)
                {
                    return new QuickChangeProductionRunStatusResult
                    {
                        Success = false,
                        ErrorMessage = "Error producing items for production run."
                    };
                }

                // Final update to "PRUN_CLOSED".
                var finalCloseStatusResult = await ChangeProductionRunStatus(productionRunId, "PRUN_CLOSED");
                if (!finalCloseStatusResult.Success)
                {
                    return new QuickChangeProductionRunStatusResult
                    {
                        Success = false,
                        ErrorMessage = "Error changing production run status to CLOSED."
                    };
                }
            }
            else
            {
                // For statuses other than CLOSED, perform a final status update.
                // Special Remark: Using productionRun.WorkEffortId for final status update aligns with the OFBiz flow.
                var finalChangeStatusResult = await ChangeProductionRunStatus(productionRun.WorkEffortId, statusId);
                if (!finalChangeStatusResult.Success)
                {
                    return new QuickChangeProductionRunStatusResult
                    {
                        Success = false,
                        ErrorMessage = "Error changing production run status."
                    };
                }
            }
        }
        catch (Exception ex)
        {
            // Special Remark: Exception handling mirrors the OFBiz try-catch block.
            // Here, you might log the error using your preferred logging framework.
            return new QuickChangeProductionRunStatusResult
            {
                Success = false,
                ErrorMessage = "Manufacturing production run status not changed."
            };
        }

        // Set additional output details.
        result.CurrentStatusId = statusId;
        result.CurrentStatusDescription = statusId switch
        {
            "PRUN_CREATED" => "Created",
            "PRUN_SCHEDULED" => "Scheduled",
            "PRUN_DOC_PRINTED" => "Confirmed",
            "PRUN_RUNNING" => "Running",
            "PRUN_COMPLETED" => "Completed",
            "PRUN_CLOSED" => "Closed",
            _ => "Unknown"
        };

        result.EstimatedStartDate = productionRun.EstimatedStartDate ?? DateTime.MinValue;
        result.ActualStartDate = productionRun.ActualStartDate ?? DateTime.MinValue;
        result.EstimatedCompletionDate = productionRun.EstimatedCompletionDate ?? DateTime.MinValue;
        result.ActualCompletionDate = productionRun.ActualCompletionDate ?? DateTime.MinValue;

        return result;
    }


    public async Task<ChangeProductionRunStatusResult> ChangeProductionRunStatus(
        string productionRunId,
        string statusId)
    {
        var result = new ChangeProductionRunStatusResult();

        // Fetch the production run entity.
        var productionRun = await _context.WorkEfforts.FindAsync(productionRunId);

        // Validate the input entity.
        if (productionRun == null)
        {
            return new ChangeProductionRunStatusResult
            {
                Success = false,
                ErrorMessage = "Production run not found.",
            };
        }

        var currentStatusId = productionRun.CurrentStatusId;

        // If the current status is the same as the new status, return the current status.
        if (currentStatusId == statusId)
        {
            result.Data["newStatusId"] = currentStatusId;
            result.Message = "Production run status unchanged.";
            result.CurrentStatusDescription = currentStatusId switch
            {
                "PRUN_CREATED" => "Created",
                "PRUN_SCHEDULED" => "Scheduled",
                "PRUN_DOC_PRINTED" => "Confirmed",
                "PRUN_RUNNING" => "Running",
                "PRUN_COMPLETED" => "Completed",
                "PRUN_CLOSED" => "Closed",
                _ => "Unknown"
            };
            return result;
        }

        try
        {
            // PRUN_CREATED --> PRUN_SCHEDULED
            if (currentStatusId == "PRUN_CREATED" && statusId == "PRUN_SCHEDULED")
            {
                await _workEffortService.UpdateWorkEffort(productionRunId, new Dictionary<string, object>
                {
                    { "currentStatusId", statusId }
                });
                await UpdateProductionRunTasksStatus(productionRunId, statusId);
                result.Data["newStatusId"] = statusId;
                result.Message = "Production run status changed.";
                result.CurrentStatusDescription = "Scheduled";
                return result;
            }

            // PRUN_CREATED or PRUN_SCHEDULED --> PRUN_DOC_PRINTED
            if ((currentStatusId == "PRUN_CREATED" || currentStatusId == "PRUN_SCHEDULED") &&
                (statusId == null || statusId == "PRUN_DOC_PRINTED"))
            {
                await _workEffortService.UpdateWorkEffort(productionRunId, new Dictionary<string, object>
                {
                    { "currentStatusId", "PRUN_DOC_PRINTED" }
                });
                await UpdateProductionRunTasksStatus(productionRunId, "PRUN_DOC_PRINTED");
                result.Data["newStatusId"] = "PRUN_DOC_PRINTED";
                result.Message = "Production run status changed.";
                result.CurrentStatusDescription = "Confirmed";
                return result;
            }

            // PRUN_DOC_PRINTED --> PRUN_RUNNING
            // This should be called only when the first task is started.
            if (currentStatusId == "PRUN_DOC_PRINTED" && (statusId == null || statusId == "PRUN_RUNNING"))
            {
                // First check if there are production runs with precedence not still completed.
                try
                {
                    var mandatoryWorkEfforts = await _context.WorkEffortAssocs
                        .Where(wea =>
                            wea.WorkEffortIdTo == productionRunId && wea.WorkEffortAssocTypeId == "WORK_EFF_PRECEDENCY")
                        .ToListAsync();

                    foreach (var mandatoryWorkEffortAssoc in mandatoryWorkEfforts)
                    {
                        var mandatoryWorkEffortId = mandatoryWorkEffortAssoc.WorkEffortIdFrom;
                        var mandatoryWorkEffort = await _context.WorkEfforts.FindAsync(mandatoryWorkEffortId);
                        var mandatoryWorkEffortStatus = mandatoryWorkEffort.CurrentStatusId;

                        if (!(mandatoryWorkEffortStatus == "PRUN_COMPLETED" ||
                              mandatoryWorkEffortStatus == "PRUN_RUNNING" ||
                              mandatoryWorkEffortStatus == "PRUN_CLOSED"))
                        {
                            return new ChangeProductionRunStatusResult
                            {
                                Success = false,
                                ErrorMessage = "Mandatory production run not completed.",
                                CurrentStatusDescription = "Confirmed",
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new ChangeProductionRunStatusResult
                    {
                        Success = false,
                        ErrorMessage = "Error querying mandatory work efforts.",
                        CurrentStatusDescription = "Confirmed",
                    };
                }

                await _workEffortService.UpdateWorkEffort(productionRunId, new Dictionary<string, object>
                {
                    { "currentStatusId", "PRUN_RUNNING" },
                    { "actualStartDate", DateTime.UtcNow }
                });
                result.Data["newStatusId"] = "PRUN_RUNNING";
                result.Message = "Production run status changed.";
                result.CurrentStatusDescription = "Running";
                return result;
            }

            // PRUN_RUNNING --> PRUN_COMPLETED
            // This should be called only when the last task is completed.
            if (currentStatusId == "PRUN_RUNNING" && (statusId == null || statusId == "PRUN_COMPLETED"))
            {
                await _workEffortService.UpdateWorkEffort(productionRunId, new Dictionary<string, object>
                {
                    { "currentStatusId", "PRUN_COMPLETED" },
                    { "actualCompletionDate", DateTime.UtcNow }
                });
                result.Data["newStatusId"] = "PRUN_COMPLETED";
                result.Message = "Production run status changed.";
                result.CurrentStatusDescription = "Completed";
                return result;
            }

            // PRUN_COMPLETED --> PRUN_CLOSED
            if (currentStatusId == "PRUN_COMPLETED" && (statusId == null || statusId == "PRUN_CLOSED"))
            {
                await _workEffortService.UpdateWorkEffort(productionRunId, new Dictionary<string, object>
                {
                    { "currentStatusId", "PRUN_CLOSED" }
                });
                await UpdateProductionRunTasksStatus(productionRunId, "PRUN_CLOSED");
                result.Data["newStatusId"] = "PRUN_CLOSED";
                result.Message = "Production run status changed.";
                result.CurrentStatusDescription = "Closed";
                return result;
            }

            result.Data["newStatusId"] = currentStatusId;
            result.Message = "Production run status unchanged.";
            result.CurrentStatusDescription = currentStatusId switch
            {
                "PRUN_CREATED" => "Created",
                "PRUN_SCHEDULED" => "Scheduled",
                "PRUN_DOC_PRINTED" => "Confirmed",
                "PRUN_RUNNING" => "Running",
                "PRUN_COMPLETED" => "Completed",
                "PRUN_CLOSED" => "Closed",
                _ => "Unknown"
            };
        }
        catch (Exception ex)
        {
            return new ChangeProductionRunStatusResult
            {
                Success = false,
                ErrorMessage = "Manufacturing production run status not changed.",
            };
        }

        return result;
    }

    private async Task UpdateProductionRunTasksStatus(string productionRunId, string statusId)
    {
        // Fetch tasks related to the production run.
        var tasks = await _context.WorkEfforts.Where(task => task.WorkEffortParentId == productionRunId).ToListAsync();

        // Update each task's status.
        foreach (var task in tasks)
        {
            await _workEffortService.UpdateWorkEffort(task.WorkEffortId, new Dictionary<string, object>
            {
                { "currentStatusId", statusId }
            });
        }
    }


    public async Task<QuickStartAllProductionRunTasksResult> QuickStartAllProductionRunTasks(string productionRunId)
    {
        var result = new QuickStartAllProductionRunTasksResult();

        var productionRun = await _context.WorkEfforts.FindAsync(productionRunId);

        if (productionRun == null)
        {
            return new QuickStartAllProductionRunTasksResult
            {
                Success = false,
                ErrorMessage = "Production run does not exist."
            };
        }

        var tasks = await _context.WorkEfforts
            .Where(we => we.WorkEffortParentId == productionRunId)
            .ToListAsync();

        foreach (var task in tasks)
        {
            var currentStatus = task.CurrentStatusId;
            if (currentStatus == "PRUN_CREATED" || currentStatus == "PRUN_SCHEDULED" ||
                currentStatus == "PRUN_DOC_PRINTED")
            {
                try
                {
                    var quickStartAllProductionRunTasksResult =
                        await ChangeProductionRunTaskStatus(productionRunId, task.WorkEffortId, "PRUN_RUNNING", false);
                    if (!string.IsNullOrEmpty(quickStartAllProductionRunTasksResult.ErrorMessage))
                    {
                        return new QuickStartAllProductionRunTasksResult
                        {
                            Success = false,
                            ErrorMessage = quickStartAllProductionRunTasksResult.ErrorMessage
                        };
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception details
                    // Log.Error(ex, "Problem calling the ChangeProductionRunTaskStatus service");
                    return new QuickStartAllProductionRunTasksResult
                    {
                        Success = false,
                        ErrorMessage = $"Production run status not changed due to an error: {ex.Message}"
                    };
                }
            }
        }

        // Indicate success
        result.Success = true;
        return result;
    }

    public async Task<List<WorkEffort>> GetProductionRunRoutingTasks(string productionRunId)
    {
        // Check if the production run exists
        var productionRun = await _context.WorkEfforts.FindAsync(productionRunId);
        if (productionRun == null)
        {
            // Optionally, you can throw an exception or return an empty list
            throw new Exception("Production run does not exist");
        }

        // Fetch related tasks
        return await _context.WorkEfforts
            .Where(we => we.WorkEffortParentId == productionRunId && we.WorkEffortTypeId == "PROD_ORDER_TASK")
            .OrderBy(we => we.Priority)
            .ToListAsync();
    }

    public async Task<ChangeProductionRunTaskStatusResult> ChangeProductionRunTaskStatus(
        string productionRunId,
        string taskId,
        string statusId,
        bool? issueAllComponents)
    {
        var response = new ChangeProductionRunTaskStatusResult();

        try
        {
            // Fetch the production run by its primary key
            var productionRun = await _context.WorkEfforts.FindAsync(productionRunId);

            // Check if production run exists
            if (productionRun == null)
            {
                response.ErrorMessage = "Manufacturing Production Run does not exist.";
                return response;
            }

            // Fetch all routing tasks related to the production run, ordered by priority
            var tasks = await GetProductionRunRoutingTasks(productionRunId);

            // Locate the specific task we need to change status for
            var theTask = tasks.FirstOrDefault(t => t.WorkEffortId == taskId);

            // Check if the task exists
            if (theTask == null)
            {
                response.ErrorMessage = "Manufacturing Production Run Task does not exist.";
                return response;
            }

            var currentStatusId = theTask.CurrentStatusId;
            var oldStatusId = theTask.CurrentStatusId;

            // If the status hasn't changed, return the current status
            if (statusId != null && currentStatusId == statusId)
            {
                response.OldStatusId = oldStatusId;
                response.NewStatusId = currentStatusId;
                response.SuccessMessage = "Manufacturing Production Run Task status is unchanged.";
                response.CurrentStatusDescription = currentStatusId switch
                {
                    "PRUN_CREATED" => "Created",
                    "PRUN_SCHEDULED" => "Scheduled",
                    "PRUN_DOC_PRINTED" => "Confirmed",
                    "PRUN_RUNNING" => "Running",
                    "PRUN_COMPLETED" => "Completed",
                    "PRUN_CLOSED" => "Closed",
                    _ => "Unknown"
                };

                return response;
            }

            // Check if all prerequisite tasks are completed or running
            var allPrecTaskCompletedOrRunning = tasks
                .Where(t => t.WorkEffortId != taskId && t.Priority < theTask.Priority)
                .All(t => t.CurrentStatusId == "PRUN_COMPLETED" || t.CurrentStatusId == "PRUN_RUNNING");

            // Check if all tasks are completed
            var allTaskCompleted = tasks
                .Where(t => t.WorkEffortId != taskId)
                .All(t => t.CurrentStatusId == "PRUN_COMPLETED");

            // Handle status changes for starting a task
            if ((currentStatusId == "PRUN_CREATED" || currentStatusId == "PRUN_SCHEDULED" ||
                 currentStatusId == "PRUN_DOC_PRINTED") &&
                (statusId == null || statusId == "PRUN_RUNNING"))
            {
                if (!allPrecTaskCompletedOrRunning)
                {
                    response.ErrorMessage = "Cannot start task. Previous tasks are not completed.";
                    return response;
                }

                if (productionRun.CurrentStatusId == "PRUN_CREATED")
                {
                    response.ErrorMessage = "Cannot start task. Documents not printed.";
                    return response;
                }

                // Update the task status
                await _workEffortService.UpdateWorkEffort(taskId, new Dictionary<string, object>
                {
                    { "currentStatusId", "PRUN_RUNNING" }
                });

                // Update main production run status to 'PRUN_RUNNING' if it's not already running
                if (productionRun.CurrentStatusId != "PRUN_RUNNING")
                {
                    await ChangeProductionRunStatus(productionRunId, "PRUN_RUNNING");

                    response.MainProductionRunStartDate =
                        null; // Indicate that the calling method should fetch the updated values
                    response.MainProductionRunStatus = "PRUN_RUNNING";
                }

                response.OldStatusId = oldStatusId;
                response.NewStatusId = "PRUN_RUNNING";
                response.SuccessMessage = "Task status changed to PRUN_RUNNING.";
                response.CurrentStatusDescription = "Running";

                return response;
            }

            // Handle status changes for completing a task
            if (currentStatusId == "PRUN_RUNNING" && (statusId == null || statusId == "PRUN_COMPLETED"))
            {
                if (issueAllComponents == true)
                {
                    var inventoryAssigned = await _context.WorkEffortInventoryAssigns
                        .Where(wia => wia.WorkEffortId == taskId)
                        .ToListAsync();

                    if (!inventoryAssigned.Any())
                    {
                        await IssueProductionRunTask(taskId);
                    }
                }

                // Update the task status
                await _workEffortService.UpdateWorkEffort(taskId, new Dictionary<string, object>
                {
                    { "currentStatusId", "PRUN_COMPLETED" }
                });

                // Ensure default values for null quantities

                decimal quantityToProduce = theTask.QuantityToProduce ?? 0m;
                decimal quantityProduced = theTask.QuantityProduced ?? 0m;
                decimal quantityRejected = theTask.QuantityRejected ?? 0m;

                // Calculate the total quantity produced and rejected
                decimal totalQuantity = quantityProduced + quantityRejected;
                decimal diffQuantity = quantityToProduce - totalQuantity;

                // If the difference is greater than zero, adjust quantityProduced
                if (diffQuantity > 0)
                {
                    quantityProduced += diffQuantity;
                }

                // Set the adjusted quantityProduced
                theTask.QuantityProduced = quantityProduced;

                // Set actual setup millis if not already set
                if (theTask.ActualSetupMillis == null)
                {
                    theTask.ActualSetupMillis = theTask.EstimatedSetupMillis;

                    await _workEffortService.UpdateWorkEffort(taskId, new Dictionary<string, object>
                    {
                        { "ActualSetupMillis", theTask.ActualSetupMillis }
                    });
                }

                // Calculate actual milliseconds if not already set
                if (theTask.ActualMilliSeconds == null && theTask.EstimatedMilliSeconds != null)
                {
                    theTask.ActualMilliSeconds = (double?)quantityProduced * (double?)theTask.EstimatedMilliSeconds;

                    await _workEffortService.UpdateWorkEffort(taskId, new Dictionary<string, object>
                    {
                        { "ActualMilliSeconds", theTask.ActualMilliSeconds }
                    });
                }


                await CreateProductionRunTaskCosts(taskId);

                if (allTaskCompleted)
                {
                    await ChangeProductionRunStatus(productionRunId, "PRUN_COMPLETED");

                    response.MainProductionRunStartDate =
                        null; // Indicate that the calling method should fetch the updated values
                    response.MainProductionRunStatus = "PRUN_COMPLETED";

                    var facility = await _context.Facilities.FindAsync(productionRun.FacilityId);
                    var ownerPartyId = facility?.OwnerPartyId;

                    var partyAccountingPreference = await _acctgMiscService.GetPartyAccountingPreferences(ownerPartyId);

                    if (partyAccountingPreference == null)
                    {
                        response.ErrorMessage = "Unable to find costs for the production run.";
                        response.CurrentStatusDescription = "Completed";

                        return response;
                    }

                    GetProductionRunCostResult getProductionRunCostResult = await GetProductionRunCost(productionRunId);
                    var totalCost = getProductionRunCostResult.TotalCost;
                    var productProduced = await GetProductProduced(productionRun.WorkEffortId);

                    var productCostComponentCalcs = await _context.ProductCostComponentCalcs
                        .Where(pccc => pccc.ProductId == productProduced.ProductId)
                        .OrderBy(pccc => pccc.SequenceNum)
                        .ToListAsync();

                    foreach (var productCostComponentCalc in productCostComponentCalcs)
                    {
                        var costComponentCalc = await _context.CostComponentCalcs
                            .FindAsync(productCostComponentCalc.CostComponentCalcId);

                        var customMethod = await _context.CustomMethods
                            .FindAsync(costComponentCalc.CostCustomMethodId);

                        if (customMethod == null)
                        {
                            continue;
                        }

                        // Construct parameters
                        var customMethodParameters = new CustomMethodParameters
                        {
                            ProductCostComponentCalc = productCostComponentCalc,
                            CostComponentCalc = costComponentCalc,
                            CurrencyUomId = partyAccountingPreference.BaseCurrencyUomId,
                            CostComponentTypePrefix = "EST",
                            BaseCost = totalCost
                        };

                        // Dynamically invoke the custom method
                        //var methodInfo = typeof(CustomMethodsService).GetMethod(customMethod.CustomMethodName);
                        /*var productCostAdjustment =
                            (decimal)methodInfo.Invoke(null, new object[] { customMethodParameters });
                            */

                        // Inject CustomMethodsService through DI
                        var customMethodsService = _serviceProvider.GetRequiredService<CustomMethodsService>();

                        // Call the method directly without using reflection
                        decimal productCostAdjustment =
                            await customMethodsService.ProductCostPercentageFormula(customMethodParameters);


                        totalCost += productCostAdjustment;

                        var newCostComponent = new CostComponent
                        {
                            CostComponentId = Guid.NewGuid().ToString(),
                            WorkEffortId = productionRunId,
                            CostComponentCalcId = costComponentCalc.CostComponentCalcId,
                            CostComponentTypeId = "ACTUAL_" + productCostComponentCalc.CostComponentTypeId,
                            CostUomId = partyAccountingPreference.BaseCurrencyUomId,
                            Cost = productCostAdjustment,
                            CreatedStamp = DateTime.UtcNow,
                            LastUpdatedStamp = DateTime.UtcNow
                        };

                        await _costService.CreateCostComponent(newCostComponent);
                    }
                }

                response.OldStatusId = oldStatusId;
                response.NewStatusId = "PRUN_COMPLETED";
                response.SuccessMessage = "Task status changed to PRUN_COMPLETED.";
                response.CurrentStatusDescription = "Completed";

                return response;
            }

            response.OldStatusId = oldStatusId;
            response.NewStatusId = currentStatusId;
            response.SuccessMessage = "Task status changed.";
            response.CurrentStatusDescription = currentStatusId switch
            {
                "PRUN_CREATED" => "Created",
                "PRUN_SCHEDULED" => "Scheduled",
                "PRUN_DOC_PRINTED" => "Confirmed",
                "PRUN_RUNNING" => "Running",
                "PRUN_COMPLETED" => "Completed",
                "PRUN_CLOSED" => "Closed",
                _ => "Unknown"
            };

            return response;
        }
        catch (Exception ex)
        {
            // Log the exception
            response.ErrorMessage = $"An error occurred: {ex.Message}";
            return response;
        }
    }


    //  Issues the Inventory for a Production Run Task.
    // Note that this skips the normal inventory reservation process.
    public async Task<Results<IssueProductionRunTaskResult>> IssueProductionRunTask(
        string workEffortId,
        string reserveOrderEnumId = null,
        string failIfItemsAreNotAvailable = "Y",
        string failIfItemsAreNotOnHand = "Y")
    {
        var result = new IssueProductionRunTaskResult();

        try
        {
            // Fetch the WorkEffort entity by its ID.
            var workEffort = await _context.WorkEfforts.FindAsync(workEffortId);

            if (workEffort == null)
            {
                return Results<IssueProductionRunTaskResult>.Failure(
                    $"WorkEffort with ID {workEffortId} not found.",
                    "WORK_EFFORT_NOT_FOUND");
            }

            if (workEffort.CurrentStatusId == "PRUN_CANCELLED")
            {
                // REFACTOR: Return structured failure for invalid status
                return Results<IssueProductionRunTaskResult>.Failure(
                    "Cannot issue inventory for a cancelled production run task.",
                    "INVALID_WORK_EFFORT_STATUS");
            }

            // Check if the current status is not 'PRUN_CANCELLED'.
            if (workEffort.CurrentStatusId != "PRUN_CANCELLED")
            {
                // Fetch components based on the lookupComponents criteria.
                var components = await _context.WorkEffortGoodStandards
                    .Where(wgs => wgs.WorkEffortId == workEffortId
                                  && wgs.StatusId == "WEGS_CREATED"
                                  && wgs.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED"
                                  && wgs.FromDate <= DateTime.UtcNow &&
                                  (wgs.ThruDate == null || wgs.ThruDate >= DateTime.UtcNow))
                    .ToListAsync();

                // Iterate through each component and call the service to issue inventory.
                foreach (var component in components)
                {
                    if (!string.IsNullOrEmpty(component.ProductId))
                    {
                        var issuances = await _context.WorkEffortInventoryAssigns
                            .Where(wia => wia.WorkEffortId == workEffortId)
                            .ToListAsync();

                        var totalIssuance = issuances.Sum(i => i.Quantity);

                        double quantity = (double)component.EstimatedQuantity;

                        if (totalIssuance != (double?)0.00m)
                        {
                            quantity = (double)(component.EstimatedQuantity - totalIssuance);
                        }

                        var product = await _context.Products
                            .Where(p => p.ProductId == component.ProductId)
                            .Select(p => new { p.ProductName })
                            .FirstOrDefaultAsync();
                        var productName = product?.ProductName ?? $"Unknown Product (ID: {component.ProductId})";


                        var componentResult = await IssueProductionRunTaskComponent(
                            workEffortId,
                            component.ProductId,
                            fromDate: component.FromDate,
                            quantity: (decimal?)quantity,
                            failIfItemsAreNotAvailable: failIfItemsAreNotAvailable,
                            failIfItemsAreNotOnHand: failIfItemsAreNotOnHand,
                            reserveOrderEnumId: reserveOrderEnumId,
                            lotId: null, // Provide default values for the other parameters
                            locationSeqId: null,
                            secondaryLocationSeqId: null,
                            reasonEnumId: null,
                            description: "BOM Part"
                        );

                        if (componentResult.QuantityMissing > 0)
                        {
                            result.InsufficientItems.Add(new InsufficientItem
                            {
                                ProductName = productName,
                                QuantityMissing = componentResult.QuantityMissing
                            });
                        }
                    }
                }

                if (result.InsufficientItems.Any())
                {
                    var errorMessage = "Insufficient inventory for the following items: " +
                                       string.Join(", ", result.InsufficientItems
                                           .Select(i => $"Product: {i.ProductName}, Missing: {i.QuantityMissing}"));
                    return Results<IssueProductionRunTaskResult>.Failure(errorMessage, "INSUFFICIENT_INVENTORY");
                }

                // Log the completion of inventory issuance.
                _logger.LogInformation($"Issued inventory for workEffortId {workEffort.WorkEffortId}.");
                return Results<IssueProductionRunTaskResult>.Success(result);
            }

            return Results<IssueProductionRunTaskResult>.Success(result);
        }
        catch (Exception ex)
        {
            return Results<IssueProductionRunTaskResult>.Failure(
                ex.Message,
                ex.Message.Contains("Production run is cancelled or closed")
                    ? "INVALID_PRODUCTION_RUN_STATUS"
                    : "DEFAULT");
        }
    }

    private async Task<(decimal QuantityMissing, WorkEffortGoodStandard WorkEffortGoodStandard)>
        IssueProductionRunTaskComponent(
            string workEffortId, string productId, DateTime? fromDate,
            decimal? quantity, string failIfItemsAreNotAvailable = "Y",
            string failIfItemsAreNotOnHand = "Y", string reserveOrderEnumId = null,
            string lotId = null, string locationSeqId = null, string secondaryLocationSeqId = null,
            string reasonEnumId = null, string description = null)
    {
        WorkEffortGoodStandard workEffortGoodStandard = null;
        try
        {
            // Fetch the WorkEffort entity.
            var workEffort = await _context.WorkEfforts.FindAsync(workEffortId);
            if (workEffort == null)
            {
                throw new Exception("WorkEffort not found.");
            }

            // Fetch the ProductionRun (parent WorkEffort).
            var productionRun = await _context.WorkEfforts.FindAsync(workEffort.WorkEffortParentId);
            if (productionRun == null)
            {
                throw new Exception("ProductionRun not found.");
            }

            // Check if the production run is either canceled or closed.
            if (productionRun.CurrentStatusId == "PRUN_CANCELLED" || productionRun.CurrentStatusId == "PRUN_CLOSED")
            {
                throw new Exception("ManufacturingAddProdCompInCompCanStatusError");
            }


            // Check if fromDate is not provided.
            if (fromDate == null)
            {
                // Use the provided productId and quantity parameters directly.
                productId = productId;
                quantity = quantity ?? 0.0M;
            }
            else
            {
                // If fromDate is provided, attempt to fetch the specific WorkEffortGoodStandard entry.
                workEffortGoodStandard = await _context.WorkEffortGoodStandards
                    .FirstOrDefaultAsync(wegs =>
                        wegs.WorkEffortId == workEffortId && wegs.ProductId == productId &&
                        wegs.FromDate == fromDate && wegs.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED");

                // Set the quantity based on the found record, if any.
                quantity = quantity ?? (decimal?)workEffortGoodStandard?.EstimatedQuantity;

                // If no WorkEffortGoodStandard is found, create a new entry.
                if (workEffortGoodStandard == null)
                {
                    var createWorkEffortGoodStdMap = new WorkEffortGoodStandard
                    {
                        WorkEffortId = workEffortId,
                        ProductId = productId,
                        WorkEffortGoodStdTypeId = "PRUNT_PROD_NEEDED",
                        FromDate = fromDate.Value,
                        EstimatedQuantity = (double?)quantity.Value,
                        StatusId = "WEGS_CREATED"
                    };
                    _context.WorkEffortGoodStandards.Add(createWorkEffortGoodStdMap);

                    // If the task is completed, ensure the productId is set.
                    if (workEffort.CurrentStatusId == "PRUN_COMPLETED")
                    {
                        productId = productId;
                    }
                }
            }
            //<!-- kind of like the inventory reservation routine,
            //find InventoryItems to issue from, but instead of doing the reservation just create an issuance and an inventory item detail for the change -->

            // Proceed if productId is set.
            if (productId != null)
            {
                DateTime nowTimestamp = DateTime.UtcNow;
                var orderByExpression = GetOrderByExpression(reserveOrderEnumId);

                var query = _context.InventoryItems
                    .Where(ii => ii.ProductId == productId && ii.FacilityId == workEffort.FacilityId);

                // Adjust query based on lotId.
                if (!string.IsNullOrEmpty(lotId))
                {
                    failIfItemsAreNotAvailable = "Y";
                    query = query.Where(ii => ii.LotId == lotId);
                }

                // Adjust query based on locationSeqId.
                if (!string.IsNullOrEmpty(locationSeqId))
                {
                    query = query.Where(ii => ii.LocationSeqId == locationSeqId);
                }

                // Get the primary list of inventory items.
                var primaryInventoryItemList = await orderByExpression(query).ToListAsync();
                var inventoryItemList = new List<InventoryItem>(primaryInventoryItemList);

                // Include secondary location inventory items if applicable.
                if (!string.IsNullOrEmpty(locationSeqId) && !string.IsNullOrEmpty(secondaryLocationSeqId))
                {
                    var secondaryQuery = _context.InventoryItems
                        .Where(ii =>
                            ii.ProductId == productId && ii.FacilityId == workEffort.FacilityId &&
                            ii.LocationSeqId == secondaryLocationSeqId);

                    var secondaryInventoryItemList = await orderByExpression(secondaryQuery).ToListAsync();
                    inventoryItemList.AddRange(secondaryInventoryItemList);
                }

                decimal quantityNotIssued = quantity.Value;
                bool useReservedItems = false;

                // Iterate through the inventory items and issue components.
                foreach (var inventoryItem in inventoryItemList)
                {
                    quantityNotIssued = await IssueProductionRunTaskComponentInline(inventoryItem, quantityNotIssued,
                        workEffortId,
                        productId, nowTimestamp, failIfItemsAreNotAvailable, failIfItemsAreNotOnHand, useReservedItems,
                        reasonEnumId, description);

                    // If quantityNotIssued is reduced to 0, break out of the loop.
                    if (quantityNotIssued == 0)
                    {
                        break;
                    }
                }

                // Check if some quantity is still not issued, and use reserved items if necessary.
                if (failIfItemsAreNotAvailable != "Y" && quantityNotIssued > 0)
                {
                    useReservedItems = true;
                    foreach (var inventoryItem in inventoryItemList)
                    {
                        if (quantityNotIssued > 0)
                        {
                            quantityNotIssued = await IssueProductionRunTaskComponentInline(inventoryItem,
                                quantityNotIssued, workEffortId,
                                productId, nowTimestamp, failIfItemsAreNotAvailable, failIfItemsAreNotOnHand,
                                useReservedItems, reasonEnumId, description);

                            // If quantityNotIssued is reduced to 0, break out of the loop.
                            if (quantityNotIssued == 0)
                            {
                                break;
                            }
                        }
                    }
                }

                // If there's still quantity not issued, handle it.
                if (quantityNotIssued != 0)
                {
                    if (failIfItemsAreNotAvailable == "Y" || string.IsNullOrEmpty(failIfItemsAreNotOnHand))
                    {
                        return (quantityNotIssued, workEffortGoodStandard);
                    }

                    var lastNonSerInventoryItem =
                        inventoryItemList.LastOrDefault(ii => ii.InventoryItemTypeId == "NON_SERIAL_INV_ITEM");

                    if (lastNonSerInventoryItem != null)
                    {
                        var assignMap = new WorkEffortInventoryAssign
                        {
                            WorkEffortId = workEffortId,
                            InventoryItemId = lastNonSerInventoryItem.InventoryItemId,
                            Quantity = (double?)quantityNotIssued
                        };
                        _context.WorkEffortInventoryAssigns.Add(assignMap);

                        await _inventoryService.BalanceInventoryItems(lastNonSerInventoryItem.InventoryItemId,
                            lastNonSerInventoryItem.FacilityId);
                    }
                    else
                    {
                        var newInventoryItem = new InventoryItem
                        {
                            ProductId = productId,
                            FacilityId = workEffort.FacilityId,
                            InventoryItemTypeId = "NON_SERIAL_INV_ITEM"
                        };
                        _context.InventoryItems.Add(newInventoryItem);

                        var assignMap = new WorkEffortInventoryAssign
                        {
                            WorkEffortId = workEffortId,
                            InventoryItemId = newInventoryItem.InventoryItemId,
                            Quantity = (double?)quantityNotIssued
                        };
                        _context.WorkEffortInventoryAssigns.Add(assignMap);

                        var createInventoryItemDetail = new InventoryItemDetail
                        {
                            InventoryItemId = newInventoryItem.InventoryItemId,
                            QuantityOnHandDiff = -quantityNotIssued,
                            ReasonEnumId = reasonEnumId,
                            Description = description
                        };
                        _context.InventoryItemDetails.Add(createInventoryItemDetail);
                    }

                    quantityNotIssued = 0;
                }

                // Update WorkEffortGoodStandard status if needed.
                if (workEffortGoodStandard != null)
                {
                    // Get the list of tracked WorkEffortInventoryAssign entities
                    var trackedEntities = _context.ChangeTracker.Entries<WorkEffortInventoryAssign>()
                        .Where(entry => entry.State == EntityState.Added || entry.State == EntityState.Modified)
                        .Select(entry => entry.Entity)
                        .ToList();

                    // Compute totalIssuance based on the tracked entities
                    var totalIssuance = trackedEntities
                        .Where(weia =>
                            weia.WorkEffortId == workEffortGoodStandard.WorkEffortId &&
                            weia.InventoryItem.ProductId == workEffortGoodStandard.ProductId
                        )
                        .Sum(weia => weia.Quantity ?? 0);


                    if (workEffortGoodStandard.EstimatedQuantity <= totalIssuance)
                    {
                        workEffortGoodStandard.StatusId = "WEGS_COMPLETED";
                        _context.WorkEffortGoodStandards.Update(workEffortGoodStandard);
                    }
                }
            }

            return (0, workEffortGoodStandard);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error issuing production run task component for workEffortId {workEffortId}, productId {productId}: {ex.Message}");
            throw;
        }
    }


    private async Task<decimal> IssueProductionRunTaskComponentInline(
        InventoryItem inventoryItem,
        decimal quantityNotIssued,
        string workEffortId,
        string productId,
        DateTime nowTimestamp,
        string failIfItemsAreNotAvailable = "N",
        string failIfItemsAreNotOnHand = "N",
        bool useReservedItems = false,
        string reasonEnumId = null,
        string description = null)
    {
        try
        {
            if (quantityNotIssued > 0)
            {
                // Check if the inventory item is serialized and available
                if (inventoryItem.InventoryItemTypeId == "SERIALIZED_INV_ITEM" &&
                    inventoryItem.StatusId == "INV_AVAILABLE")
                {
                    // Change status on inventoryItem to "INV_DELIVERED"
                    inventoryItem.StatusId = "INV_DELIVERED";
                    _context.InventoryItems.Update(inventoryItem);

                    // Create WorkEffortInventoryAssign record
                    var issuanceCreateMap = new WorkEffortInventoryAssign
                    {
                        WorkEffortId = workEffortId,
                        InventoryItemId = inventoryItem.InventoryItemId,
                        Quantity = 1
                    };
                    await AssignInventoryToWorkEffort(issuanceCreateMap);

                    // Reduce the quantity not issued by 1
                    quantityNotIssued -= 1;
                }

                // Check if the inventory item is non-serialized and available
                if ((string.IsNullOrEmpty(inventoryItem.StatusId) || inventoryItem.StatusId == "INV_AVAILABLE") &&
                    inventoryItem.InventoryItemTypeId == "NON_SERIAL_INV_ITEM")
                {
                    decimal inventoryItemQuantity;

                    // Determine the quantity to deduct based on availability or total quantity on hand
                    if (failIfItemsAreNotAvailable == "Y")
                    {
                        inventoryItemQuantity = (decimal)inventoryItem.QuantityOnHandTotal;
                    }
                    else
                    {
                        inventoryItemQuantity = (decimal)inventoryItem.AvailableToPromiseTotal;
                    }

                    if (inventoryItemQuantity > 0)
                    {
                        decimal deductAmount = quantityNotIssued > inventoryItemQuantity
                            ? inventoryItemQuantity
                            : quantityNotIssued;

                        // Create WorkEffortInventoryAssign record
                        var issuanceCreateMap = new WorkEffortInventoryAssign
                        {
                            WorkEffortId = workEffortId,
                            InventoryItemId = inventoryItem.InventoryItemId,
                            Quantity = (double?)deductAmount
                        };
                        await AssignInventoryToWorkEffort(issuanceCreateMap);

                        // Add an InventoryItemDetail
                        var createDetailMap = new CreateInventoryItemDetailParam()
                        {
                            InventoryItemId = inventoryItem.InventoryItemId,
                            WorkEffortId = workEffortId,
                            AvailableToPromiseDiff = -deductAmount,
                            QuantityOnHandDiff = -deductAmount,
                            ReasonEnumId = reasonEnumId,
                            Description = description
                        };
                        await _inventoryService.CreateInventoryItemDetail(createDetailMap);

                        // Reduce the quantity not issued by the deducted amount
                        quantityNotIssued -= deductAmount;

                        // Balance inventory items
                        await _inventoryService.BalanceInventoryItems(inventoryItem.InventoryItemId,
                            inventoryItem.FacilityId);
                    }
                }
            }

            return quantityNotIssued;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error issuing production run task component inline for InventoryItem {inventoryItem.InventoryItemId}, WorkEffortId {workEffortId}, ProductId {productId}: {ex.Message}");
            throw; // Optionally rethrow the exception if you want it to propagate further.
        }
    }

    public async Task AssignInventoryToWorkEffort(WorkEffortInventoryAssign input)
    {
        // Check if WorkEffortId is provided (mandatory field)
        if (string.IsNullOrEmpty(input.WorkEffortId))
        {
            throw new ArgumentException("WorkEffortId is required.");
        }

        // Check if InventoryItemId is provided (mandatory field)
        if (string.IsNullOrEmpty(input.InventoryItemId))
        {
            throw new ArgumentException("InventoryItemId is required.");
        }

        try
        {
            // Check if an existing record exists for the given WorkEffortId and InventoryItemId
            var existingRecord = await _context.WorkEffortInventoryAssigns
                .FindAsync(input.WorkEffortId, input.InventoryItemId);

            if (existingRecord != null)
            {
                // Update the quantity if the record exists
                existingRecord.Quantity += input.Quantity;
                _context.WorkEffortInventoryAssigns.Update(existingRecord);
            }
            else
            {
                // Create a new record if it doesn't exist
                var newRecord = new WorkEffortInventoryAssign
                {
                    WorkEffortId = input.WorkEffortId,
                    InventoryItemId = input.InventoryItemId,
                    Quantity = input.Quantity,
                    CreatedStamp = DateTime.UtcNow,
                    LastUpdatedStamp = DateTime.UtcNow
                };

                await _context.WorkEffortInventoryAssigns.AddAsync(newRecord);
            }

            // Call the CreateAcctgTransForWorkEffortIssuance method after successful assignment
            var acctgTransId =
                await _generalLedgerService.CreateAcctgTransForWorkEffortIssuance(input.WorkEffortId,
                    input.InventoryItemId);
        }
        catch (Exception ex)
        {
            // Log the exception and rethrow it
            _logger.LogError(
                $"Error assigning inventory to work effort for WorkEffortId {input.WorkEffortId} and InventoryItemId {input.InventoryItemId}: {ex.Message}");
            throw;
        }
    }

    public async Task<QuickRunAllProductionRunTasksResult> QuickRunAllProductionRunTasks(string productionRunId)
    {
        var result = new QuickRunAllProductionRunTasksResult { Success = true };

        try
        {
            // Retrieve tasks for the given production run ID
            var tasks = _context.WorkEfforts
                .Where(task => task.WorkEffortParentId == productionRunId)
                .ToList();

            // Iterate over each task and process
            foreach (var task in tasks)
            {
                string taskId = task.WorkEffortId;

                // Call method to run the production run task
                var serviceResult = await QuickRunProductionRunTask(productionRunId, taskId);

                // Check if service result indicates an error
                if (!string.IsNullOrEmpty(serviceResult.ErrorMessage))
                {
                    result.Success = false;
                    result.ErrorMessage = serviceResult.ErrorMessage;
                    break; // Exit loop on first error
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception occurred: {ex.Message}");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    private async Task<QuickRunProductionRunTaskResult> QuickRunProductionRunTask(string productionRunId, string taskId)
    {
        // Local logging helper that prints the taskId and the caller line number.
        void Log(string message, ConsoleColor color, [CallerLineNumber] int lineNumber = 0)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[TaskID: {taskId}] [Line {lineNumber}] {message}");
            Console.ResetColor();
        }

        try
        {
            Log($"[START] QuickRunProductionRunTask for ProductionRunId: {productionRunId}, TaskId: {taskId}",
                ConsoleColor.Cyan);

            /*// Log start of method execution
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(
                $"[START] QuickRunProductionRunTask for ProductionRunId: {productionRunId}, TaskId: {taskId}");
            Console.ResetColor();
            */

            // Initialize the success result
            var result = new QuickRunProductionRunTaskResult { Success = true };

            // Fetch task details from the database
            /*Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Fetching task details from the database...");
            Console.ResetColor();*/
            Log("Fetching task details from the database...", ConsoleColor.Blue);

            var task = await _context.WorkEfforts.FindAsync(taskId);
            if (task == null)
            {
                Log("Error: Task not found.", ConsoleColor.Red);
                return new QuickRunProductionRunTaskResult
                {
                    Success = false,
                    ErrorMessage = "Task not found."
                };
            }

            // Log initial task status
            string currentStatusId = task.CurrentStatusId;
            string prevStatusId = "";
            Log($"Initial task status: {currentStatusId}", ConsoleColor.Yellow);


            // Loop until the task status is "PRUN_COMPLETED"
            while (currentStatusId != "PRUN_COMPLETED")
            {
                Log($"Current task status: {currentStatusId}", ConsoleColor.Magenta);


                // Determine next status based on the current status
                string nextStatus;
                if (currentStatusId == "PRUN_CREATED" || currentStatusId == "PRUN_SCHEDULED" ||
                    currentStatusId == "PRUN_DOC_PRINTED")
                {
                    nextStatus = "PRUN_RUNNING";
                }
                else if (currentStatusId == "PRUN_RUNNING")
                {
                    nextStatus = "PRUN_COMPLETED";
                }
                else
                {
                    Log($"Error: Unexpected task status encountered: {currentStatusId}", ConsoleColor.Red);

                    return new QuickRunProductionRunTaskResult
                    {
                        Success = false,
                        ErrorMessage = $"Unexpected task status: {currentStatusId}"
                    };
                }

                Log($"Attempting to change status from {currentStatusId} to {nextStatus}", ConsoleColor.Green);


                // Call service to change task status (with issueAllComponents set to true)
                var serviceResult = await ChangeProductionRunTaskStatus(productionRunId, taskId, nextStatus, true);

                // Check for errors returned by the service
                if (!string.IsNullOrEmpty(serviceResult.ErrorMessage))
                {
                    Log($"Service error: {serviceResult.ErrorMessage}", ConsoleColor.Red);

                    return new QuickRunProductionRunTaskResult
                    {
                        Success = false,
                        ErrorMessage = serviceResult.ErrorMessage
                    };
                }

                // Log details of the service result
                if (!string.IsNullOrEmpty(serviceResult.SuccessMessage))
                {
                    Log($"Service message: {serviceResult.SuccessMessage}", ConsoleColor.Green);
                }

                Log($"Service new status: {serviceResult.NewStatusId}", ConsoleColor.Yellow);


                // Handle "unchanged" status scenario
                if (!string.IsNullOrEmpty(serviceResult.SuccessMessage) &&
                    serviceResult.SuccessMessage.Contains("unchanged"))
                {
                    if (string.IsNullOrEmpty(prevStatusId))
                    {
                        prevStatusId = serviceResult.NewStatusId;
                        currentStatusId = serviceResult.NewStatusId;
                        Log(
                            $"Status unchanged on first iteration. Setting both previous and current status to {serviceResult.NewStatusId}",
                            ConsoleColor.Blue);
                    }
                    else if (serviceResult.NewStatusId == prevStatusId)
                    {
                        Log($"Error: Detected stagnation. Task status '{prevStatusId}' is not progressing.",
                            ConsoleColor.Red);

                        return new QuickRunProductionRunTaskResult
                        {
                            Success = false,
                            ErrorMessage = $"Task status '{prevStatusId}' is not progressing."
                        };
                    }
                    else
                    {
                        prevStatusId = serviceResult.NewStatusId;
                        currentStatusId = serviceResult.NewStatusId;
                        Log($"Status updated despite 'unchanged' message. New status is {serviceResult.NewStatusId}",
                            ConsoleColor.Blue);
                    }
                }
                else
                {
                    // Update current status based on service result
                    currentStatusId = serviceResult.NewStatusId;
                    if (!string.IsNullOrEmpty(prevStatusId) && currentStatusId == prevStatusId)
                    {
                        Log($"Error: Status stagnation detected. Task status '{prevStatusId}' remains unchanged.",
                            ConsoleColor.Red);

                        return new QuickRunProductionRunTaskResult
                        {
                            Success = false,
                            ErrorMessage = $"Task status '{prevStatusId}' is not progressing."
                        };
                    }

                    prevStatusId = currentStatusId;
                    Log($"Status updated to: {currentStatusId}", ConsoleColor.Yellow);
                }
            }

            Log("Task has reached PRUN_COMPLETED. Operation successful.", ConsoleColor.Cyan);


            return result;
        }
        catch (Exception ex)
        {
            Log($"Exception occurred: {ex.Message}", ConsoleColor.Red);

            return new QuickRunProductionRunTaskResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<CreateProductionRunTaskCostsResult> CreateProductionRunTaskCosts(string productionRunTaskId)
    {
        try
        {
            var workEffort =
                await _utilityService.FindLocalOrDatabaseAsync<WorkEffort>(productionRunTaskId);

            if (workEffort == null)
            {
                return new CreateProductionRunTaskCostsResult
                {
                    Success = false,
                    ErrorMessage = "Production run task not found."
                };
            }

            // Calculate the actual total milliseconds from setup and runtime
            double actualTotalMilliSeconds = (workEffort.ActualSetupMillis ?? 0) + (workEffort.ActualMilliSeconds ?? 0);

            // Get the routing task associated with the work effort
            var routingTaskAssoc = await _context.WorkEffortAssocs
                .FirstOrDefaultAsync(assoc =>
                    assoc.WorkEffortIdTo == productionRunTaskId && assoc.WorkEffortAssocTypeId == "WORK_EFF_TEMPLATE" &&
                    assoc.FromDate <= DateTime.UtcNow && (assoc.ThruDate == null || assoc.ThruDate >= DateTime.UtcNow));

            WorkEffort routingTask = null;
            if (routingTaskAssoc != null)
            {
                routingTask = await _context.WorkEfforts.FindAsync(routingTaskAssoc.WorkEffortIdFrom);
            }

            // Retrieve cost calculations for the work effort
            var workEffortCostCalcs = await _context.WorkEffortCostCalcs
                .Where(calc => calc.WorkEffortId == productionRunTaskId &&
                               calc.FromDate <= DateTime.UtcNow &&
                               (calc.ThruDate == null || calc.ThruDate >= DateTime.UtcNow))
                .ToListAsync();

            foreach (var workEffortCostCalc in workEffortCostCalcs)
            {
                var costComponentCalc = await _context.CostComponentCalcs
                    .FindAsync(workEffortCostCalc.CostComponentCalcId);

                var customMethod = await _context.CustomMethods
                    .FindAsync(costComponentCalc.CostCustomMethodId);

                if (customMethod == null || string.IsNullOrEmpty(customMethod.CustomMethodName))
                {
                    // Compute the total time
                    double totalTime = actualTotalMilliSeconds;
                    if (costComponentCalc.PerMilliSecond.HasValue && costComponentCalc.PerMilliSecond.Value != 0)
                    {
                        // perMilliSecond represents how many milliseconds of task time are equivalent to 1 unit of cost.
                        totalTime /= (double)costComponentCalc.PerMilliSecond.Value;
                    }

                    // Compute the cost
                    var fixedCost = costComponentCalc.FixedCost ?? 0;
                    var variableCost = costComponentCalc.VariableCost ?? 0;
                    var totalCost = fixedCost + variableCost * (decimal)totalTime;

                    // Store the cost
                    var costComponent = new CostComponent
                    {
                        CostComponentId = Guid.NewGuid().ToString(),
                        WorkEffortId = productionRunTaskId,
                        CostComponentCalcId = costComponentCalc.CostComponentCalcId,
                        CostComponentTypeId = "ACT_" + workEffortCostCalc.CostComponentTypeId,
                        CostUomId = costComponentCalc.CurrencyUomId,
                        Cost = totalCost,
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow
                    };
                    await _costService.CreateCostComponent(costComponent);
                }
                else
                {
                    // Use the custom method to compute the costs
                    // Note: Custom method invocation is application-specific and may require reflection or other means
                }
            }

            // Calculate costs associated with fixed assets
            var fixedAsset = await _context.FixedAssets.FindAsync(workEffort.FixedAssetId);
            if (fixedAsset != null && routingTask != null)
            {
                fixedAsset = await _context.FixedAssets.FindAsync(routingTask.FixedAssetId);
            }


            if (fixedAsset != null)
            {
                var setupCosts = await _context.FixedAssetStdCosts
                    .Where(fasc =>
                        fasc.FixedAssetId == fixedAsset.FixedAssetId && fasc.FixedAssetStdCostTypeId == "SETUP_COST"
                                                                     && fasc.FromDate <= DateTime.UtcNow &&
                                                                     (fasc.ThruDate == null ||
                                                                      fasc.ThruDate >= DateTime.UtcNow))
                    .ToListAsync();

                var usageCosts = await _context.FixedAssetStdCosts
                    .Where(fasc =>
                        fasc.FixedAssetId == fixedAsset.FixedAssetId && fasc.FixedAssetStdCostTypeId == "USAGE_COST"
                                                                     && fasc.FromDate <= DateTime.UtcNow &&
                                                                     (fasc.ThruDate == null ||
                                                                      fasc.ThruDate >= DateTime.UtcNow))
                    .ToListAsync();

                var setupCost = setupCosts.FirstOrDefault();
                var usageCost = usageCosts.FirstOrDefault();

                if (setupCost != null || usageCost != null)
                {
                    var currencyUomId = setupCost != null ? setupCost.AmountUomId : usageCost.AmountUomId;
                    var setupCostAmount =
                        (setupCost != null ? setupCost.Amount * (decimal)workEffort.ActualSetupMillis : 0);
                    var usageCostAmount =
                        (usageCost != null ? usageCost.Amount * (decimal)workEffort.ActualMilliSeconds : 0);

                    var fixedAssetCost = (setupCostAmount + usageCostAmount) / 3600000;

                    var costComponent = new CostComponent
                    {
                        CostComponentId = Guid.NewGuid().ToString(),
                        WorkEffortId = productionRunTaskId,
                        CostComponentTypeId = "MANUFACTURING_OVERHEAD",
                        CostUomId = currencyUomId,
                        Cost = fixedAssetCost,
                        FixedAssetId = fixedAsset.FixedAssetId,
                        CreatedStamp = DateTime.UtcNow,
                        LastUpdatedStamp = DateTime.UtcNow
                    };

                    await _costService.CreateCostComponent(costComponent);
                }
            }

            // Calculate material costs
            var materialsCostByCurrency = new Dictionary<string, decimal>();

            var inventoryConsumed = _context.WorkEffortInventoryAssigns
                .Where(weia => weia.WorkEffortId == productionRunTaskId)
                .Include(weia => weia.InventoryItem) // Ensure InventoryItem is included in the query
                .ToList();


            foreach (var inventory in inventoryConsumed)
            {
                var quantity = inventory.Quantity ?? 0;
                var unitCost = inventory.InventoryItem.UnitCost ?? 0;

                if (quantity == 0 || unitCost == 0)
                {
                    continue;
                }

                var currencyUomId = inventory.InventoryItem.CurrencyUomId;
                if (!materialsCostByCurrency.ContainsKey(currencyUomId))
                {
                    materialsCostByCurrency[currencyUomId] = 0;
                }

                materialsCostByCurrency[currencyUomId] += unitCost * (decimal)quantity;
            }

            foreach (var kvp in materialsCostByCurrency)
            {
                var costComponent = new CostComponent
                {
                    CostComponentId = Guid.NewGuid().ToString(),
                    WorkEffortId = productionRunTaskId,
                    CostComponentTypeId = "ACT_MAT_COST",
                    CostUomId = kvp.Key,
                    Cost = kvp.Value,
                    CreatedStamp = DateTime.UtcNow,
                    LastUpdatedStamp = DateTime.UtcNow
                };
                await _costService.CreateCostComponent(costComponent);
            }

            return new CreateProductionRunTaskCostsResult { Success = true };
        }
        catch (Exception ex)
        {
            // Log the exception
            // Logger.LogError(ex.Message, ex);

            return new CreateProductionRunTaskCostsResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<ProductionRunProduceResult> ProductionRunProduce(
        string workEffortId,
        string facilityId,
        string? productId = null,
        decimal? dischargeQty = null, // This is how many final goods to produce, or partial usage
        string? quantityUomId = null,
        string inventoryItemTypeId = "NON_SERIAL_INV_ITEM",
        string? lotId = null,
        string? locationSeqId = null,
        bool? createLotIfNeeded = true,
        bool? autoCreateLot = false
    )
    {
        var result = new ProductionRunProduceResult();
        var inventoryItemIds = new List<string>();
        result.InventoryItemIds = inventoryItemIds;

        try
        {
            // 1) Load Production Run
            var productionRun = await _context.WorkEfforts.FindAsync(workEffortId);
            if (productionRun == null)
            {
                result.Errors.Add($"Production run {workEffortId} not found.");
                return result;
            }

            // 2) Identify "run product" from WorkEffortGoodStd
            var wipProduct = await GetProductProduced(workEffortId);
            if (wipProduct == null)
            {
                result.Errors.Add($"No WIP (or run) product found for run {workEffortId}.");
                return result;
            }

            // 3) If the user supplied a productId different from the WIP,
            //    we assume partial discharge => producing that real product
            Product finalProduct;
            if (!string.IsNullOrEmpty(productId) && productId != wipProduct.ProductId)
            {
                finalProduct = await _context.Products.FindAsync(productId);
                if (finalProduct == null)
                {
                    result.Errors.Add($"Product {productId} not found for partial discharge.");
                    return result;
                }
            }
            else
            {
                // If productId == wipProduct.ProductId or is null => produce the WIP product itself
                finalProduct = wipProduct;
            }

            // 4) Determine how much quantity we are producing for the final product.
            //    - In a partial discharge scenario, 'dischargeQty' might be the number of plates, or
            //      already something computed by the caller.
            //    - If not provided, default to the leftover from last routing task, etc. (classic scenario).
            if (!dischargeQty.HasValue)
            {
                // classic fallback logic
                var tasks = _context.WorkEfforts
                    .Where(t => t.WorkEffortParentId == workEffortId)
                    .ToList();
                var lastTask = tasks.OrderByDescending(t => t.Priority).FirstOrDefault();
                if (lastTask == null)
                {
                    result.Errors.Add("No routing tasks found for production run.");
                    return result;
                }

                var quantityProduced = productionRun.QuantityProduced ?? 0;
                var quantityDeclared = lastTask.QuantityProduced ?? 0;
                var maxQuantity = quantityDeclared - quantityProduced;
                if (maxQuantity <= 0)
                {
                    return result; // no new production
                }

                dischargeQty = maxQuantity;
            }

            // 5) Handle lot creation if needed
            if (lotId == null && autoCreateLot == true)
            {
                createLotIfNeeded = true;
            }

            try
            {
                if (string.IsNullOrEmpty(lotId) && createLotIfNeeded == true)
                {
                    var lotResult = _inventoryService.CreateLot(
                        lotId: null,
                        quantity: dischargeQty.Value,
                        creationDate: DateTime.UtcNow
                    );
                    if (lotResult == null)
                    {
                        result.Errors.Add("Lot creation failed.");
                        return result;
                    }

                    lotId = lotResult.LotId;
                }
                else if (!string.IsNullOrEmpty(lotId))
                {
                    var existingLot = _context.Lots.FirstOrDefault(l => l.LotId == lotId);
                    if (existingLot == null && createLotIfNeeded == false)
                    {
                        result.Errors.Add($"Lot '{lotId}' not found and createLotIfNeeded is false.");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Lot creation error: {ex.Message}");
                return result;
            }

            // 6) Determine which facility
            var usedFacilityId = !string.IsNullOrEmpty(facilityId)
                ? facilityId
                : productionRun.FacilityId;
            var usedFacility = await _context.Facilities.FindAsync(usedFacilityId);
            if (usedFacility == null)
            {
                result.Errors.Add($"Facility {usedFacilityId} not found.");
                return result;
            }

            // 7) Figure out the total “actual cost” from cost components associated with this run
            decimal totalCost = 0m;
            try
            {
                // Sum cost on the run itself
                var runCost = _context.CostComponents
                    .Where(cc => cc.WorkEffortId == productionRun.WorkEffortId)
                    .Sum(cc => cc.Cost);

                // Sum cost from child tasks
                var tasks = _context.WorkEfforts
                    .Where(t => t.WorkEffortParentId == productionRun.WorkEffortId)
                    .ToList();

                var tasksCost = _context.CostComponents
                    .Where(cc => tasks.Select(t => t.WorkEffortId).Contains(cc.WorkEffortId))
                    .Sum(cc => cc.Cost);

                totalCost = (decimal)(runCost + tasksCost);
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error summing costComponents for run {workEffortId}: {ex.Message}");
                return result;
            }

            // 8) If product is WIP => we handle classic or partial usage
            //    We want to see how much of the container’s capacity we have used so far
            //    in order to do a pro-rated cost for the new discharge.
            decimal unitCost = 0;
            decimal usedPortionCost = 0;

            // Check if wipProduct is truly WIP
            if (wipProduct.ProductTypeId == "WIP")
            {
                // The total container capacity is wipProduct.QuantityIncluded
                decimal totalCapacity = wipProduct.QuantityIncluded ?? 0;
                if (totalCapacity <= 0)
                {
                    result.Errors.Add($"WIP product {wipProduct.ProductId} has no valid QuantityIncluded.");
                    return result;
                }

                // Production run’s “QuantityProduced” => how many kg (or L, or units) have been used so far
                //   including the chunk we just declared.
                decimal usedSoFar = productionRun.QuantityProduced ?? 0;

                // We can ratio out the cost:
                // totalCost is for the entire container’s run
                // if usedSoFar / totalCapacity = X%
                // => totalCost * X% = cost for that used portion
                // => but we only want the incremental chunk that was just declared, i.e. `dischargeQtyPart`
                //    if user just declared 4.64 kg usage, that’s “the newly used portion”.
                // So:
                //   costSoFar = ( usedSoFar / totalCapacity ) * totalCost
                //   costPreviously = ( (usedSoFar - dischargeQtyPart) / totalCapacity ) * totalCost
                //   costIncrement = costSoFar - costPreviously

                decimal dischargeQtyPart = 0m; // how many kg we just used
                // If the caller is storing “the kg used” directly as quantity in productionRun, we can:
                //   “oldUsed = quantityProducedBeforeThisCall”
                //   “usedSoFar = new quantityProduced”
                //   “dischargeQtyPart = usedSoFar - oldUsed”

                // But we might not have “oldUsed” directly in this function
                // => If we trust dischargeQty is exactly the new chunk used in the container:
                dischargeQtyPart = (decimal)dischargeQty;

                // cost for "usedSoFar" portion:
                decimal costSoFar = (usedSoFar / totalCapacity) * (decimal)totalCost;

                // cost for previous portion (before we added dischargeQtyPart)
                decimal prevUsed = usedSoFar - dischargeQtyPart;
                decimal costPrevious = (prevUsed / totalCapacity) * (decimal)totalCost;

                usedPortionCost = costSoFar - costPrevious; // The incremental cost for the newly used chunk

                // Meanwhile, finalProduct might be the same WIP or a different product
                // If finalProduct == wipProduct => we’re simply “finalizing” more of the WIP container
                // => the inventory unit cost for that chunk = usedPortionCost / dischargeQtyPart
                if (dischargeQtyPart != 0)
                {
                    unitCost = usedPortionCost / dischargeQtyPart;
                }
                else
                {
                    // if dischargeQtyPart=0 => no usage
                    unitCost = 0;
                }
            }
            else
            {
                // If product is NOT WIP => we do simpler logic
                // e.g. totalCost / total items
                if (dischargeQty > 0)
                {
                    unitCost = totalCost / dischargeQty.Value;
                }
            }

            // 9) Create Inventory Items for finalProduct
            try
            {
                if (inventoryItemTypeId == "SERIALIZED_INV_ITEM")
                {
                    // loop up to dischargeQty as “pieces”
                    int itemCount = (int)Math.Floor(dischargeQty.Value);
                    for (int i = 0; i < itemCount; i++)
                    {
                        var newItem = new InventoryItem
                        {
                            ProductId = finalProduct.ProductId,
                            InventoryItemTypeId = "SERIALIZED_INV_ITEM",
                            FacilityId = usedFacilityId,
                            StatusId = "INV_AVAILABLE",
                            LotId = lotId,
                            LocationSeqId = locationSeqId,
                            UomId = quantityUomId,
                            UnitCost = unitCost,
                            DatetimeReceived = DateTime.Now,
                            DatetimeManufactured = DateTime.Now,
                            Comments = $"Created by production run {workEffortId}",
                            CreatedStamp = DateTime.UtcNow,
                            LastUpdatedStamp = DateTime.UtcNow
                        };
                        _context.InventoryItems.Add(newItem);
                        inventoryItemIds.Add(newItem.InventoryItemId);

                        var detail = new InventoryItemDetail
                        {
                            InventoryItemId = newItem.InventoryItemId,
                            WorkEffortId = productionRun.WorkEffortId,
                            AvailableToPromiseDiff = 1,
                            QuantityOnHandDiff = 1,
                            CreatedStamp = DateTime.UtcNow,
                            LastUpdatedStamp = DateTime.UtcNow
                        };
                        _context.InventoryItemDetails.Add(detail);

                        var produced = new WorkEffortInventoryProduced
                        {
                            WorkEffortId = productionRun.WorkEffortId,
                            InventoryItemId = newItem.InventoryItemId,
                            CreatedStamp = DateTime.UtcNow,
                            LastUpdatedStamp = DateTime.UtcNow
                        };
                        _context.WorkEffortInventoryProduceds.Add(produced);

                        await _inventoryService.BalanceInventoryItems(newItem.InventoryItemId, usedFacilityId);
                    }
                }
                else
                {
                    // Single record with total dischargeQty
                    var invParam = new CreateInventoryItemParam
                    {
                        ProductId = finalProduct.ProductId,
                        InventoryItemTypeId = "NON_SERIAL_INV_ITEM",
                        FacilityId = usedFacilityId,
                        StatusId = "INV_AVAILABLE",
                        LotId = lotId,
                        IsReturned = "N",
                        LocationSeqId = locationSeqId,
                        UomId = quantityUomId,
                        UnitCost = unitCost,
                        Comments = $"Created by production run {workEffortId}",
                        DateTimeReceived = DateTime.UtcNow,
                        DateTimeManufactured = DateTime.UtcNow,
                        QuantityOnHand = dischargeQty
                    };

                    var newInventoryItemId = await _inventoryService.CreateInventoryItem(invParam);
                    inventoryItemIds.Add(newInventoryItemId);

                    var detailParam = new CreateInventoryItemDetailParam
                    {
                        InventoryItemId = newInventoryItemId,
                        WorkEffortId = productionRun.WorkEffortId,
                        AvailableToPromiseDiff = dischargeQty,
                        QuantityOnHandDiff = dischargeQty
                    };
                    await _inventoryService.CreateInventoryItemDetail(detailParam);

                    await _workEffortService.CreateWorkEffortInventoryProduced(workEffortId, newInventoryItemId);
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error creating inventory: {ex.Message}");
                return result;
            }

            // 10) Final: done. We assume the caller already updated productionRun.QuantityProduced
            //     with the partial usage for a WIP container scenario. Or in a classic scenario,
            //     we increment the run’s quantity produced here if needed.

            // In partial discharge, we might do nothing. In a full scenario, we might do:
            // productionRun.QuantityProduced += dischargeQty;

            // But let's show how:
            productionRun.QuantityProduced = (productionRun.QuantityProduced ?? 0) + 0; // or +dischargeQty if you want
            try
            {
                await _workEffortService.UpdateWorkEffort(productionRun.WorkEffortId, new Dictionary<string, object>
                {
                    { "QuantityProduced", productionRun.QuantityProduced },
                    { "ActualCompletionDate", DateTime.UtcNow }
                });
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error updating run: {ex.Message}");
                return result;
            }

            result.Success = true;
            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Error producing run: {ex.Message}");
            return result;
        }
    }


    public async Task<GetProductionRunCostResult> GetProductionRunCost(string workEffortId)
    {
        try
        {
            // Initialize the total cost to zero
            decimal? totalCost = 0;

            // Retrieve all tasks associated with the given production run (workEffortId)
            var tasks = await _context.WorkEfforts
                .Where(we => we.WorkEffortParentId == workEffortId)
                .OrderBy(we => we.WorkEffortId)
                .ToListAsync();

            // Retrieve the cost of the production run header
            var productionRunHeaderCost = await GetWorkEffortCosts(workEffortId); // Changed: added await
            if (!productionRunHeaderCost.Success)
            {
                return new GetProductionRunCostResult
                {
                    Success = false,
                    ErrorMessage = productionRunHeaderCost.ErrorMessage
                };
            }

            totalCost += productionRunHeaderCost.TotalCost;

            // Retrieve and sum the costs of all associated tasks
            foreach (var task in tasks)
            {
                var taskCost = await GetWorkEffortCosts(task.WorkEffortId); // Changed: added await
                if (!taskCost.Success)
                {
                    return new GetProductionRunCostResult
                    {
                        Success = false,
                        ErrorMessage = taskCost.ErrorMessage
                    };
                }

                totalCost += taskCost.TotalCost;
            }

            // Return the total cost
            return new GetProductionRunCostResult
            {
                Success = true,
                TotalCost = totalCost
            };
        }
        catch (Exception exc)
        {
            // Log the exception (logging mechanism not shown here)
            // Logger.LogError(exc.Message, exc);

            return new GetProductionRunCostResult
            {
                Success = false,
                ErrorMessage = "Unable to find costs for production run " + workEffortId + ": " + exc.Message
            };
        }
    }

    public async Task<GetWorkEffortCostsResult> GetWorkEffortCosts(string workEffortId)
    {
        try
        {
            // Retrieve the work effort by ID
            var workEffort = await _context.WorkEfforts
                .FindAsync(workEffortId);

            // If the work effort does not exist, return an error
            if (workEffort == null)
            {
                return new GetWorkEffortCostsResult
                {
                    Success = false,
                    ErrorMessage = $"Work effort {workEffortId} does not exist."
                };
            }


            /*var costComponents = await _utilityService.FindLocalOrDatabaseListAsync<CostComponent>(
                query => query.Where(cc => cc.FromDate <= DateTime.Now && (cc.ThruDate == null || cc.ThruDate >= DateTime.Now)),
                workEffortId);
                */


            // Retrieve all valid cost components associated with the work effort
            var costComponents = await _context.CostComponents
                .Where(cc => cc.WorkEffortId == workEffortId)
                .Where(cc => cc.FromDate <= DateTime.Now && (cc.ThruDate == null || cc.ThruDate >= DateTime.Now))
                .ToListAsync();

            // Initialize total costs to zero
            decimal? totalCost = 0;
            decimal? totalCostNoMaterials = 0;

            // Sum the costs of all cost components and separate material costs
            foreach (var costComponent in costComponents)
            {
                totalCost += costComponent.Cost;
                if (costComponent.CostComponentTypeId != "ACTUAL_MAT_COST")
                {
                    totalCostNoMaterials += costComponent.Cost;
                }
            }

            // Return the result with the calculated total costs and cost components
            return new GetWorkEffortCostsResult
            {
                Success = true,
                CostComponents = costComponents,
                TotalCost = totalCost,
                TotalCostNoMaterials = totalCostNoMaterials
            };
        }
        catch (Exception exc)
        {
            // Log the exception (logging mechanism not shown here)
            // Logger.LogError(exc.Message, exc);

            return new GetWorkEffortCostsResult
            {
                Success = false,
                ErrorMessage = $"Unable to find costs for work effort {workEffortId}: {exc.Message}"
            };
        }
    }

    private Func<IQueryable<InventoryItem>, IOrderedQueryable<InventoryItem>> GetOrderByExpression(
        string reserveOrderEnumId)
    {
        return reserveOrderEnumId switch
        {
            "INVRO_FIFO_EXP" => query => query.OrderBy(ii => ii.ExpireDate),
            "INVRO_LIFO_EXP" => query => query.OrderByDescending(ii => ii.ExpireDate),
            "INVRO_LIFO_REC" => query => query.OrderByDescending(ii => ii.DatetimeReceived),
            _ => query => query.OrderBy(ii => ii.DatetimeReceived)
        };
    }

    public async Task<ProductionRunTaskProduceResult> ProductionRunTaskProduce(string workEffortId,
        string productId,
        decimal quantity,
        string facilityId = null, string locationSeqId = null, decimal? unitCost = null,
        string currencyUomId = null,
        string inventoryItemTypeId = null, string lotId = null, string isReturned = "N", string uomId = null)
    {
        var result = new ProductionRunTaskProduceResult();
        var inventoryItemIds = new List<string>();

        inventoryItemTypeId ??= "NON_SERIAL_INV_ITEM";

        if (facilityId == null)
        {
            var productionRun = await _context.WorkEfforts.FindAsync(workEffortId);
            facilityId = productionRun?.FacilityId;
        }

        if (inventoryItemTypeId == "SERIALIZED_INV_ITEM")
        {
            try
            {
                int numOfItems = (int)quantity;
                for (int i = 0; i < numOfItems; i++)
                {
                    var serviceContext = new CreateInventoryItemParam
                    {
                        ProductId = productId,
                        InventoryItemTypeId = "SERIALIZED_INV_ITEM",
                        StatusId = "INV_AVAILABLE",
                        FacilityId = facilityId,
                        DateTimeReceived = DateTime.UtcNow,
                        DateTimeManufactured = DateTime.UtcNow,
                        Comments = $"Created by production run task {workEffortId}",
                        UnitCost = unitCost,
                        CurrencyUomId = currencyUomId,
                        LotId = lotId,
                        UomId = uomId,
                        IsReturned = isReturned
                    };

                    var inventoryItemId = await _inventoryService.CreateInventoryItem(serviceContext);


                    var detailParam = new CreateInventoryItemDetailParam
                    {
                        InventoryItemId = inventoryItemId,
                        WorkEffortId = workEffortId,
                        AvailableToPromiseDiff = 1,
                        QuantityOnHandDiff = 1,
                    };

                    await _inventoryService.CreateInventoryItemDetail(detailParam);

                    _workEffortService.CreateWorkEffortInventoryProduced(workEffortId, inventoryItemId);

                    inventoryItemIds.Add(inventoryItemId);

                    //TODO: refactor
                    await _inventoryService.BalanceInventoryItems(productId, facilityId);
                }
            }
            catch (Exception exc)
            {
                result.ErrorMessage = exc.Message;
                return result;
            }
        }
        else
        {
            try
            {
                var serviceContext = new CreateInventoryItemParam
                {
                    ProductId = productId,
                    InventoryItemTypeId = "NON_SERIAL_INV_ITEM",
                    StatusId = "INV_AVAILABLE",
                    FacilityId = facilityId,
                    DateTimeReceived = DateTime.UtcNow,
                    DateTimeManufactured = DateTime.UtcNow,
                    Comments = $"Created by production run task {workEffortId}",
                    UnitCost = unitCost,
                    CurrencyUomId = currencyUomId,
                    LotId = lotId,
                    UomId = uomId,
                    IsReturned = isReturned
                };

                var inventoryItemId = await _inventoryService.CreateInventoryItem(serviceContext);


                var detailParam = new CreateInventoryItemDetailParam
                {
                    InventoryItemId = inventoryItemId,
                    WorkEffortId = workEffortId,
                    AvailableToPromiseDiff = quantity,
                    QuantityOnHandDiff = quantity,
                };

                await _inventoryService.CreateInventoryItemDetail(detailParam);

                _workEffortService.CreateWorkEffortInventoryProduced(workEffortId, inventoryItemId);

                inventoryItemIds.Add(inventoryItemId);

                //TODO: refactor
                await _inventoryService.BalanceInventoryItems(productId, facilityId);
            }
            catch (Exception exc)
            {
                result.ErrorMessage = exc.Message;
                return result;
            }
        }

        result.InventoryItemIds = inventoryItemIds;
        result.HasError = false;
        return result;
    }

    public async Task<UpdateProductionRunTaskResult> UpdateProductionRunTask(UpdateProductionRunTaskContext context)
    {
        var result = new UpdateProductionRunTaskResult { Success = true };

        // Extracting mandatory input fields
        var productionRunId = context.ProductionRunId;
        var workEffortId = context.ProductionRunTaskId;

        // Extracting optional input fields with default values if they are null
        var fromDate = context.FromDate ?? DateTime.Now;
        var toDate = context.ToDate ?? DateTime.Now;
        var addQuantityProduced = context.AddQuantityProduced ?? 0m;
        var addQuantityRejected = context.AddQuantityRejected ?? 0m;
        var addSetupTime = context.AddSetupTime ?? 0m;
        var addTaskTime = context.AddTaskTime ?? 0m;
        var comments = context.Comments ?? "";
        var issueRequiredComponents = context.IssueRequiredComponents ?? false;
        var componentsLocations = context.ComponentLocations;

        // Extracting and handling partyId
        var partyId = context.PartyId;
        if (string.IsNullOrEmpty(partyId))
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername());
            //partyId = user.partyId // Default to user login's partyId if partyId is not provided
        }

        // Check if the production run exists
        var productionRun = await _context.WorkEfforts.FindAsync(productionRunId);
        if (productionRun == null)
        {
            return new UpdateProductionRunTaskResult
            {
                Success = false,
                ErrorMessage = $"Production run with ID {productionRunId} does not exist."
            };
        }

        // Fetch the associated tasks and validate the specific task
        var tasks = await _context.WorkEfforts
            .Where(task => task.WorkEffortParentId == productionRunId && task.WorkEffortTypeId == "PROD_ORDER_TASK")
            .ToListAsync();

        var theTask = tasks.FirstOrDefault(t => t.WorkEffortId == workEffortId);
        if (theTask == null)
        {
            return new UpdateProductionRunTaskResult
            {
                Success = false,
                ErrorMessage = $"Production run task with ID {workEffortId} does not exist."
            };
        }

        // Ensure the task is in the correct status
        string currentStatusId = theTask.CurrentStatusId;
        if (currentStatusId != "PRUN_RUNNING")
        {
            return new UpdateProductionRunTaskResult
            {
                Success = false,
                ErrorMessage = "The production run task is not currently running."
            };
        }

        // Handling quantities
        decimal quantityProduced = theTask.QuantityProduced ?? 0m;
        decimal quantityRejected = theTask.QuantityRejected ?? 0m;
        decimal totalQuantityProduced = quantityProduced + addQuantityProduced;
        decimal totalQuantityRejected = quantityRejected + addQuantityRejected;

        // Handle component issuance if required
        if (issueRequiredComponents && addQuantityProduced > 0)
        {
            decimal quantityToProduce = theTask.QuantityToProduce ?? 0m;
            if (quantityToProduce > 0)
            {
                try
                {
                    var components = await _context.WorkEffortGoodStandards
                        .Where(c => c.WorkEffortId == workEffortId)
                        .ToListAsync();

                    foreach (var component in components)
                    {
                        decimal totalRequiredMaterialQuantity = decimal.Divide(
                            (decimal)(component.EstimatedQuantity.Value * (double)totalQuantityProduced),
                            quantityToProduce);

                        var issuances = await (
                            from weia in _context.WorkEffortInventoryAssigns
                            join ii in _context.InventoryItems on weia.InventoryItemId equals ii.InventoryItemId
                            where weia.WorkEffortId == workEffortId && ii.ProductId == component.ProductId
                            select weia.Quantity
                        ).ToListAsync();

                        double? totalIssuedNullable = issuances.Sum(i => i ?? 0.0);
                        double totalIssued = (totalIssuedNullable ?? 0.0);
                        double requiredQuantity = (double)(totalRequiredMaterialQuantity - (decimal)totalIssued);

                        if (requiredQuantity > 0)
                        {
                            string locationSeqId = null;
                            string secondaryLocationSeqId = null;
                            string failIfItemsAreNotAvailable = null;


                            // Call IssueProductionRunTaskComponent function
                            await IssueProductionRunTaskComponent(
                                workEffortId: workEffortId,
                                productId: component.ProductId,
                                fromDate: component.FromDate,
                                quantity: (decimal?)requiredQuantity,
                                failIfItemsAreNotAvailable: failIfItemsAreNotAvailable,
                                locationSeqId: locationSeqId,
                                secondaryLocationSeqId: secondaryLocationSeqId
                            );
                        }
                    }
                }
                catch (Exception e)
                {
                    return new UpdateProductionRunTaskResult
                    {
                        Success = false,
                        ErrorMessage = "Problem occurred during component issuance."
                    };
                }
            }
        }

        // Store the updated task (work effort), incorporating the logic from the Java class
        try
        {
            // Build parameters for UpdateWorkEffort
            var parameters = new Dictionary<string, object>
            {
                { "QuantityProduced", totalQuantityProduced },
                { "QuantityRejected", totalQuantityRejected }
            };

            if (addTaskTime > 0)
            {
                double actualMilliSeconds = theTask.ActualMilliSeconds ?? 0.0;
                parameters["ActualMilliSeconds"] = actualMilliSeconds + (double)addTaskTime;
            }

            if (addSetupTime > 0)
            {
                double actualSetupMillis = theTask.ActualSetupMillis ?? 0.0;
                parameters["ActualSetupMillis"] = actualSetupMillis + (double)addSetupTime;
            }

            // Update the work effort (task) using UpdateWorkEffort function
            await _workEffortService.UpdateWorkEffort(workEffortId, parameters);
        }
        catch (Exception exc)
        {
            return new UpdateProductionRunTaskResult
            {
                Success = false,
                ErrorMessage = exc.Message
            };
        }

        return result;
    }

    public async Task<Product> GetProductProduced(string productionRunId)
    {
        try
        {
            // Find the production run by ID
            var productionRun = await _context.WorkEfforts.FindAsync(productionRunId);

            // Check if the production run exists
            if (productionRun != null)
            {
                // Fetch the related WorkEffortGoodStandard entries for the production run with type "PRUN_PROD_DELIV"
                var productionRunProducts = await _context.WorkEffortGoodStandards
                    .Where(wegs =>
                        wegs.WorkEffortId == productionRunId && wegs.WorkEffortGoodStdTypeId == "PRUN_PROD_DELIV")
                    .ToListAsync();

                // Get the first production run product entry
                var productionRunProduct = productionRunProducts.FirstOrDefault();

                // Check if a production run product entry exists
                if (productionRunProduct != null)
                {
                    // Find the product by the ProductId in the production run product entry
                    var productProduced = await _context.Products.FindAsync(productionRunProduct.ProductId);

                    // Check if the product exists
                    if (productProduced != null)
                    {
                        // Return the found product
                        return productProduced;
                    }
                }
            }

            // Return null if no product is found
            return null;
        }
        catch (Exception ex)
        {
            // Log the exception and rethrow it
            _logger.LogError($"Error getting product produced for productionRunId {productionRunId}: {ex.Message}");
            throw;
        }
    }

    public async Task<DeclareAndProduceProductionRunResult> ProductionRunDeclareAndProduce(
        DeclareAndProduceProductionRunParams declareAndProduceProductionRunParams)
    {
        try
        {
            // (1) Retrieve the production run
            var productionRun = await _context.WorkEfforts
                .FindAsync(declareAndProduceProductionRunParams.WorkEffortId);

            if (productionRun == null)
            {
                return DeclareAndProduceProductionRunResult.CreateFailureResult("Production run not found.");
            }

            // (2) Identify which product the run is supposed to produce
            var productProduced = await GetProductProduced(productionRun.WorkEffortId);
            if (productProduced == null)
            {
                return DeclareAndProduceProductionRunResult.CreateFailureResult(
                    "Product produced not found (WorkEffortGoodStandard is missing?).");
            }


            if (productProduced.ProductTypeId == "WIP")
            {
                // We must do partial or full discharge from this subassembly.
                // If 'ProductId' is empty => maybe user wants to finalize the subassembly itself
                if (!string.IsNullOrEmpty(declareAndProduceProductionRunParams.ProductId))
                {
                    /// partial discharge from the WIP container to the real product
                    return await HandlePartialDischargeOfWIPRelatedProduct(
                        productionRun,
                        productProduced,
                        declareAndProduceProductionRunParams.ProductId, // real finished product ID
                        declareAndProduceProductionRunParams
                            .FacilityId, // facility where the real product is produced
                        declareAndProduceProductionRunParams.Quantity,
                        declareAndProduceProductionRunParams.InventoryItemTypeId,
                        declareAndProduceProductionRunParams.LotId,
                        declareAndProduceProductionRunParams.LocationSeqId,
                        declareAndProduceProductionRunParams.CreateLotIfNeeded ?? true,
                        declareAndProduceProductionRunParams.AutoCreateLot ?? true,
                        declareAndProduceProductionRunParams.ComponentsLocationMap
                    );
                }
            }

            // (3) Continue with existing code
            //     This is your original logic if the product is NOT WIP
            //     OR if it is WIP but we decided to let the function handle finalization.
            //     We'll keep the code below mostly intact.
            // -----------------------------------------------------------

            // Retrieve the production run routing tasks separately
            var productionRunRoutingTasks = await _context.WorkEfforts
                .Where(task => task.WorkEffortParentId == productionRun.WorkEffortId)
                .ToListAsync();

            var quantityProduced = productionRun.QuantityProduced ?? 0;
            var quantityToProduce = productionRun.QuantityToProduce ?? 0;
            var minimumQuantityProducedByTask = quantityProduced + declareAndProduceProductionRunParams.Quantity;

            // Check if the minimum quantity produced by task exceeds the quantity to produce
            if (minimumQuantityProducedByTask > quantityToProduce)
            {
                return DeclareAndProduceProductionRunResult.CreateFailureResult(
                    "Quantity produced exceeds the quantity to produce.");
            }

            // Update tasks associated with the production run
            foreach (var task in productionRunRoutingTasks)
            {
                if (task.CurrentStatusId == "PRUN_RUNNING")
                {
                    var quantityDeclared = task.QuantityProduced ?? 0;
                    if (minimumQuantityProducedByTask > quantityDeclared)
                    {
                        var addQuantityProduced = minimumQuantityProducedByTask - quantityDeclared;

                        // Create the UpdateProductionRunTaskContext object
                        var updateTaskContext = new UpdateProductionRunTaskContext
                        {
                            ProductionRunId = productionRun.WorkEffortId,
                            ProductionRunTaskId = task.WorkEffortId,
                            AddQuantityProduced = addQuantityProduced,
                            IssueRequiredComponents = true,
                            ComponentLocations = declareAndProduceProductionRunParams.ComponentsLocationMap
                        };

                        // Call to update the production run task status and quantities
                        var updateTaskResult = await UpdateProductionRunTask(updateTaskContext);

                        if (!updateTaskResult.Success)
                        {
                            return DeclareAndProduceProductionRunResult.CreateFailureResult(updateTaskResult
                                .ErrorMessage);
                        }
                    }
                }
            }

            // Call the ProductionRunProduce service or logic to create inventory
            var produceResult = await ProductionRunProduce(
                declareAndProduceProductionRunParams.WorkEffortId,
                declareAndProduceProductionRunParams.FacilityId, null,
                declareAndProduceProductionRunParams.Quantity,
                null,
                declareAndProduceProductionRunParams.InventoryItemTypeId,
                declareAndProduceProductionRunParams.LotId,
                declareAndProduceProductionRunParams.LocationSeqId,
                declareAndProduceProductionRunParams.CreateLotIfNeeded ?? false,
                declareAndProduceProductionRunParams.AutoCreateLot ?? false
            );

            if (!produceResult.Success)
            {
                return DeclareAndProduceProductionRunResult.CreateFailureResult(
                    produceResult.Errors.FirstOrDefault());
            }

            // Return success result
            return DeclareAndProduceProductionRunResult.CreateSuccessResult(produceResult.InventoryItemIds,
                (decimal)declareAndProduceProductionRunParams.Quantity);
        }
        catch (Exception ex)
        {
            // Handle any exceptions that occur during the operation
            return DeclareAndProduceProductionRunResult.CreateFailureResult(
                $"Error during production run declaration and produce: {ex.Message}");
        }
    }

    private async Task<DeclareAndProduceProductionRunResult> HandlePartialDischargeOfWIPRelatedProduct(
        WorkEffort wipProductionRun,
        Product wipProduct,
        string realProductId,
        string facilityId, // user-supplied facility for the real product
        decimal? declareQuantity,
        string? inventoryItemTypeId,
        string? lotId,
        string? locationSeqId,
        bool createLotIfNeeded,
        bool autoCreateLot,
        List<ComponentLocation>? componentsLocationMap
    )
    {
        if (!declareQuantity.HasValue)
        {
            return DeclareAndProduceProductionRunResult.CreateFailureResult(
                "Quantity to produce (pieces) is required for partial discharge.");
        }

        // 1) Find BOM usage from realProduct => wipProduct
        var usageRecord = await _context.ProductAssocs
            .Where(pa => pa.ProductId == realProductId
                         && pa.ProductIdTo == wipProduct.ProductId
                         && pa.ProductAssocTypeId == "MANUF_COMPONENT")
            .FirstOrDefaultAsync();

        if (usageRecord == null)
        {
            return DeclareAndProduceProductionRunResult.CreateFailureResult(
                $"No BOM linking {realProductId} => {wipProduct.ProductId} found.");
        }

        decimal usagePerPiece = (decimal)usageRecord.Quantity;
        decimal totalNeeded = usagePerPiece * declareQuantity.Value;

        // 2) Check if the WIP run has enough left
        var quantityProduced = wipProductionRun.QuantityProduced ?? 0;

        // Use wipProduct.QuantityIncluded for full capacity
        var totalCapacity = wipProduct.QuantityIncluded ?? 0;
        if (totalCapacity <= 0)
        {
            return DeclareAndProduceProductionRunResult.CreateFailureResult(
                "WIP product capacity (QuantityIncluded) is not set or invalid.");
        }

        var wipLeft = totalCapacity - quantityProduced;

        if (totalNeeded > wipLeft)
        {
            return DeclareAndProduceProductionRunResult.CreateFailureResult(
                $"Not enough WIP left. Needed {totalNeeded}, have {wipLeft}.");
        }

        // 3) "Consume" that subassembly => increment wipProductionRun.QuantityProduced
        wipProductionRun.QuantityProduced = quantityProduced + totalNeeded;

        // 4) Produce the real product
        var produceResult = await ProductionRunProduce(
            wipProductionRun.WorkEffortId,
            facilityId,
            realProductId,
            declareQuantity.Value,
            null,
            inventoryItemTypeId ?? "NON_SERIAL_INV_ITEM",
            lotId,
            locationSeqId,
            createLotIfNeeded,
            autoCreateLot
        );

        if (!produceResult.Success)
        {
            return DeclareAndProduceProductionRunResult.CreateFailureResult(
                produceResult.Errors.FirstOrDefault());
        }

        // Return success
        return DeclareAndProduceProductionRunResult.CreateSuccessResult(
            produceResult.InventoryItemIds,
            declareQuantity.Value
        );
    }


    private async Task<DateTime?> RecalculateEstimatedCompletionDate(
        decimal quantity, DateTime estimatedStartDate, string productionRunId)
    {
        // Fetch the routing tasks associated with the production run
        var routingTasks = await _context.WorkEfforts
            .Where(rt => rt.WorkEffortParentId == productionRunId && rt.WorkEffortTypeId == "PROD_ORDER_TASK")
            .OrderBy(rt => rt.Priority)
            .ToListAsync();

        DateTime? estimatedCompletionDate = estimatedStartDate;

        // Calculate the estimated completion date based on the routing tasks
        foreach (var task in routingTasks)
        {
            TaskTimeResult estimatedTaskTime =
                await _costService.GetEstimatedTaskTime(task.WorkEffortId, null, null, quantity: quantity);
            estimatedCompletionDate = estimatedCompletionDate?.AddMilliseconds(estimatedTaskTime.EstimatedTaskTime);

            // Update the task's estimated start and completion dates
            task.EstimatedStartDate = estimatedStartDate;
            task.EstimatedCompletionDate = estimatedCompletionDate;

            _context.WorkEfforts.Update(task);
            estimatedStartDate = estimatedCompletionDate.Value; // Set the start date of the next task
        }

        return estimatedCompletionDate;
    }

    private async Task SetEstimatedDeliveryDates()
    {
        var now = DateTime.UtcNow;
        var products = new List<ProductEstimate>();

        try
        {
            // Fetch work efforts related to production runs with WorkEffort and WorkEffortGoodStandard
            var productionRuns = await (from we in _context.WorkEfforts
                join wegs in _context.WorkEffortGoodStandards
                    on we.WorkEffortId equals wegs.WorkEffortId
                where wegs.WorkEffortGoodStdTypeId == "PRUN_PROD_DELIV"
                      && wegs.StatusId == "WEGS_CREATED"
                      && we.WorkEffortTypeId == "PROD_ORDER_HEADER"
                select new
                {
                    we.WorkEffortId,
                    we.WorkEffortTypeId,
                    wegs.ProductId,
                    wegs.WorkEffortGoodStdTypeId,
                    wegs.EstimatedQuantity,
                    we.EstimatedCompletionDate,
                    we.CurrentStatusId,
                    we.QuantityToProduce,
                    we.QuantityProduced
                }).ToListAsync();


            foreach (var productionRun in productionRuns)
            {
                if (productionRun.CurrentStatusId == "PRUN_CLOSED" ||
                    productionRun.CurrentStatusId == "PRUN_CREATED")
                    continue;

                var qtyToProduce = productionRun.QuantityToProduce ?? 0;
                var qtyProduced = productionRun.QuantityProduced ?? 0;

                if (qtyProduced >= qtyToProduce)
                    continue;

                var qtyDiff = qtyToProduce - qtyProduced;
                var productId = productionRun.ProductId;
                var estimatedShipDate = productionRun.EstimatedCompletionDate ?? now;

                var productEstimate = products.FirstOrDefault(p => p.ProductId == productId);

                if (productEstimate == null)
                {
                    productEstimate = new ProductEstimate
                    {
                        ProductId = productId,
                        DateEstimates = new List<DateEstimate>()
                    };
                    products.Add(productEstimate);
                }

                var dateEstimate =
                    productEstimate.DateEstimates.FirstOrDefault(d => d.EstimatedShipDate == estimatedShipDate);

                if (dateEstimate == null)
                {
                    dateEstimate = new DateEstimate
                    {
                        EstimatedShipDate = estimatedShipDate,
                        RemainingQty = 0,
                        Reservations = new List<OrderItemShipGroup>()
                    };
                    productEstimate.DateEstimates.Add(dateEstimate);
                }

                dateEstimate.RemainingQty += qtyDiff;
            }

            // Fetch approved purchase orders with OrderHeader and OrderItem
            var purchaseOrders = await _context.OrderHeaders
                .Join(
                    _context.OrderItems,
                    oh => oh.OrderId, // Key from OrderHeader
                    oi => oi.OrderId, // Key from OrderItem
                    (oh, oi) => new
                    {
                        oh.OrderId,
                        oh.OrderDate,
                        OrderStatusId = oh.StatusId, // Alias for orderStatusId
                        oh.GrandTotal,
                        oh.ProductStoreId,
                        oh.OrderTypeId,
                        oi.OrderItemSeqId,
                        oi.ProductId,
                        oi.Quantity,
                        oi.CancelQuantity,
                        oi.UnitPrice,
                        oi.UnitListPrice,
                        oi.ItemDescription,
                        ItemStatusId = oi.StatusId, // Alias for itemStatusId
                        oi.EstimatedShipDate,
                        oi.EstimatedDeliveryDate,
                        oi.ShipBeforeDate,
                        oi.ShipAfterDate,
                        oi.OrderItemTypeId
                    }
                )
                .Where(result => result.OrderTypeId == "PURCHASE_ORDER" && result.ItemStatusId == "ITEM_APPROVED")
                .OrderBy(result => result.OrderId)
                .ToListAsync();

            foreach (var order in purchaseOrders)
            {
                var productId = order.ProductId;
                var orderQuantity = order.Quantity ?? 0;
                var estimatedShipDate = order.EstimatedDeliveryDate ?? now;

                var productEstimate = products.FirstOrDefault(p => p.ProductId == productId);

                if (productEstimate == null)
                {
                    productEstimate = new ProductEstimate
                    {
                        ProductId = productId,
                        DateEstimates = new List<DateEstimate>()
                    };
                    products.Add(productEstimate);
                }

                var dateEstimate =
                    productEstimate.DateEstimates.FirstOrDefault(d => d.EstimatedShipDate == estimatedShipDate);

                if (dateEstimate == null)
                {
                    dateEstimate = new DateEstimate
                    {
                        EstimatedShipDate = estimatedShipDate,
                        RemainingQty = 0,
                        Reservations = new List<OrderItemShipGroup>()
                    };
                    productEstimate.DateEstimates.Add(dateEstimate);
                }

                dateEstimate.RemainingQty += orderQuantity;
            }

            // Fetch backorders with OrderItem, OrderItemShipGrpInvRes, and InventoryItem
            var backorders = await _context.OrderItems
                .Join(
                    _context.OrderItemShipGrpInvRes, // Join with OrderItemShipGrpInvRes
                    oi => new { oi.OrderId, oi.OrderItemSeqId },
                    oisgir => new { oisgir.OrderId, oisgir.OrderItemSeqId },
                    (oi, oisgir) => new
                    {
                        oi.OrderId,
                        oi.ProductId,
                        oi.OrderItemSeqId,
                        oi.ItemDescription,
                        oi.ShipAfterDate,
                        oi.ShipBeforeDate,
                        oi.EstimatedShipDate,
                        oi.EstimatedDeliveryDate,
                        oisgir.QuantityNotAvailable, // Select allowed fields
                        oi.OrderItemTypeId,
                        oisgir.InventoryItemId,
                        oisgir.ShipGroupSeqId
                    }
                )
                .Join(
                    _context.InventoryItems, // Join with InventoryItem
                    result => result.InventoryItemId,
                    ii => ii.InventoryItemId,
                    (result, ii) => new
                    {
                        result.OrderId,
                        result.ProductId,
                        result.OrderItemSeqId,
                        result.ItemDescription,
                        result.ShipAfterDate,
                        result.ShipBeforeDate,
                        result.EstimatedShipDate,
                        result.EstimatedDeliveryDate,
                        result.QuantityNotAvailable,
                        result.OrderItemTypeId,
                        result.InventoryItemId,
                        result.ShipGroupSeqId,
                        ii.SerialNumber // Only select allowed fields from InventoryItem (exclude productId, statusId, comments)
                    }
                )
                .Where(result => result.QuantityNotAvailable != null && result.QuantityNotAvailable > 0)
                .OrderBy(result => result.ShipBeforeDate)
                .ToListAsync();

            foreach (var backorder in backorders)
            {
                var productId = backorder.ProductId;
                var quantityNotAvailable = backorder.QuantityNotAvailable ?? 0;

                // Fetch the required by date from OrderItemShipGroup (ShipByDate)
                var orderItemShipGroup = await _context.OrderItemShipGroups
                    .FindAsync(backorder.OrderId, backorder.ShipGroupSeqId);

                var requiredByDate = orderItemShipGroup?.ShipByDate ?? now;

                // Find the product in the products list
                var productEstimate = products.FirstOrDefault(p => p.ProductId == productId);
                if (productEstimate == null)
                    continue;

                // Find all the date estimates for the product that are before or equal to the requiredByDate
                var subsetEstimates = productEstimate.DateEstimates
                    .Where(d => d.EstimatedShipDate <= requiredByDate)
                    .OrderBy(d => d.EstimatedShipDate)
                    .ToList();

                foreach (var dateEstimate in subsetEstimates)
                {
                    var remainingQty = dateEstimate.RemainingQty;

                    if (remainingQty >= quantityNotAvailable)
                    {
                        // Subtract the quantity and update the reservation
                        dateEstimate.RemainingQty -= quantityNotAvailable;

                        // Fetch the actual entity from the database to update PromisedDatetime
                        var orderItemShipGrpInvRes = await _context.OrderItemShipGrpInvRes
                            .FindAsync(backorder.OrderId, backorder.ShipGroupSeqId, backorder.OrderItemSeqId,
                                backorder.InventoryItemId);

                        if (orderItemShipGrpInvRes != null)
                        {
                            orderItemShipGrpInvRes.PromisedDatetime = dateEstimate.EstimatedShipDate;
                            _context.OrderItemShipGrpInvRes.Update(orderItemShipGrpInvRes);
                        }

                        dateEstimate.Reservations
                            .Add(orderItemShipGroup); // Add this backorder to the reservation list
                        break; // Stop once the quantity is fulfilled
                    }
                    else
                    {
                        // Deduct the remaining quantity and move to the next available date
                        quantityNotAvailable -= remainingQty;
                        dateEstimate.RemainingQty = 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while setting estimated delivery dates");
            throw new InvalidOperationException("Error while setting estimated delivery dates");
        }
    }

    public async Task SetQuantity(decimal newQuantity, string productionRunId)
    {
        // Fetch the current production run product to determine the previous quantity
        var productionRunProduct = await _context.WorkEffortGoodStandards
            .FirstOrDefaultAsync(w =>
                w.WorkEffortId == productionRunId && w.WorkEffortGoodStdTypeId == "PRUN_PROD_DELIV");

        if (productionRunProduct == null)
        {
            throw new InvalidOperationException("WorkEffortGoodStandard not found for the production run");
        }

        // Store the previous quantity for component recalculations
        var previousQuantity = (decimal)productionRunProduct.EstimatedQuantity;

        // Update the WorkEffortGoodStandard estimated quantity
        productionRunProduct.EstimatedQuantity = (double?)newQuantity;
        _context.WorkEffortGoodStandards.Update(productionRunProduct);

        // **Update the WorkEffort's QuantityToProduce field as well**
        var productionRun = await _context.WorkEfforts.FindAsync(productionRunId);
        if (productionRun != null)
        {
            productionRun.QuantityToProduce = newQuantity;
            _context.WorkEfforts.Update(productionRun);
        }

        // Fetch the BOM components associated with the production run
        var productionRunComponents = await GetProductionRunComponents(productionRunId);

        // If components are found, recalculate the quantities
        if (productionRunComponents != null && productionRunComponents.Any())
        {
            foreach (var component in productionRunComponents)
            {
                var componentQuantity = component.EstimatedQuantity ?? 0;

                // Adjust the component quantity by scaling it to the new production quantity
                component.EstimatedQuantity =
                    (double?)(componentQuantity / (double)previousQuantity * (double)newQuantity);

                // Update the BOM component in the database
                _context.WorkEffortGoodStandards.Update(component);
            }
        }
    }

    public async Task<List<WorkEffortGoodStandard>> GetProductionRunComponents(string productionRunId)
    {
        try
        {
            // Check if the production run exists (equivalent to `exist()` in Java)
            var productionRunExists = await _context.WorkEfforts
                .AnyAsync(we => we.WorkEffortId == productionRunId);
            if (!productionRunExists)
            {
                return null; // Or handle this case as per your application logic
            }

            // Fetch routing tasks associated with the production run
            var productionRunRoutingTasks = await _context.WorkEfforts
                .Where(we => we.WorkEffortParentId == productionRunId && we.WorkEffortTypeId == "PROD_ORDER_TASK")
                .OrderBy(we => we.Priority)
                .ToListAsync();

            var productionRunComponents = new List<WorkEffortGoodStandard>();

            if (productionRunRoutingTasks != null && productionRunRoutingTasks.Any())
            {
                // Iterate over the routing tasks and get their associated components (BOM)
                foreach (var routingTask in productionRunRoutingTasks)
                {
                    var components = await _context.WorkEffortGoodStandards
                        .Where(w => w.WorkEffortId == routingTask.WorkEffortId &&
                                    w.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED")
                        .ToListAsync();

                    // Add the found components to the list
                    productionRunComponents.AddRange(components);
                }
            }

            return productionRunComponents;
        }
        catch (Exception ex) // Replace with a more specific exception type if needed
        {
            // Log the error (implement logging as per your logging system)
            Console.WriteLine($"Error retrieving production run components: {ex.Message}");
            throw; // Re-throw or handle the exception as per your application logic
        }
    }

    public async Task<List<WorkEffortGoodStandardDto>> ListIssueProductionRunDeclComponents(string productionRunId,
        string language)
    {
        try
        {
            // Check if the production run exists
            var productionRunExists = await _context.WorkEfforts
                .AnyAsync(we => we.WorkEffortId == productionRunId);
            if (!productionRunExists)
            {
                return null;
            }

            // Fetch routing tasks associated with the production run
            var productionRunRoutingTasks = await _context.WorkEfforts
                .Where(we => we.WorkEffortParentId == productionRunId && we.WorkEffortTypeId == "PROD_ORDER_TASK")
                .OrderBy(we => we.Priority)
                .ToListAsync();

            var productionRunComponents = new List<WorkEffortGoodStandardDto>();

            if (productionRunRoutingTasks.Any())
            {
                foreach (var routingTask in productionRunRoutingTasks)
                {
                    // Use GroupBy to ensure unique products and avoid duplicates
                    var components = await (from w in _context.WorkEffortGoodStandards
                        join p in _context.Products on w.ProductId equals p.ProductId
                        join u in _context.Uoms on p.QuantityUomId equals u.UomId
                        join l in _context.ProductFacilityLocations on new
                                { w.ProductId, FacilityId = routingTask.FacilityId }
                            equals new { l.ProductId, l.FacilityId } into locationJoin
                        from l in locationJoin.DefaultIfEmpty()
                        join s in _context.ProductFacilityLocations on new
                                { w.ProductId, FacilityId = routingTask.FacilityId }
                            equals new { s.ProductId, s.FacilityId } into secondaryLocationJoin
                        from s in secondaryLocationJoin.DefaultIfEmpty()
                        join i in _context.InventoryItems on w.ProductId equals i.ProductId into inventoryJoin
                        from i in inventoryJoin.DefaultIfEmpty()
                        where w.WorkEffortId == routingTask.WorkEffortId &&
                              w.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED"
                        // Group by product to avoid duplicates
                        group new { w, p, u, l, s, i } by new
                        {
                            w.WorkEffortId,
                            w.ProductId,
                            p.ProductName,
                            u.Description,
                            u.DescriptionArabic,
                            u.DescriptionTurkish,
                            w.WorkEffortGoodStdTypeId,
                            w.FromDate,
                            w.ThruDate,
                            w.StatusId,
                            w.EstimatedQuantity,
                            w.EstimatedCost,
                            PrimaryLocationSeqId = l.LocationSeqId, // Renamed to avoid ambiguity
                            SecondaryLocationSeqId = s.LocationSeqId, // Renamed to avoid ambiguity
                            i.LotId
                        }
                        into grouped
                        select new WorkEffortGoodStandardDto
                        {
                            WorkEffortId = grouped.Key.WorkEffortId,
                            ProductId = grouped.Key.ProductId,
                            ProductName = grouped.Key.ProductName,
                            ProductQuantityUom =
                                language == "ar"
                                    ? grouped.Key.DescriptionArabic
                                    : grouped.Key.Description, // UOM description
                            WorkEffortGoodStdTypeId = grouped.Key.WorkEffortGoodStdTypeId,
                            FromDate = grouped.Key.FromDate,
                            ThruDate = grouped.Key.ThruDate,
                            StatusId = grouped.Key.StatusId,
                            EstimatedQuantity = grouped.Key.EstimatedQuantity,
                            EstimatedCost = grouped.Key.EstimatedCost,

                            // Location and lot fields
                            LocationSeqId = grouped.Key.PrimaryLocationSeqId,
                            SecondaryLocationSeqId = grouped.Key.SecondaryLocationSeqId,
                            LotId = grouped.Key.LotId,

                            // Initialize issued and returned quantities
                            IssuedQuantity = 0,
                            ReturnedQuantity = 0
                        }).ToListAsync();

                    // Calculate issued and returned quantities using WorkEffortInventoryAssign and InventoryItem
                    foreach (var component in components)
                    {
                        // WorkEffortInventoryAssign alias: assign
                        var issuances = await (from assign in _context.WorkEffortInventoryAssigns
                                join invItem in _context.InventoryItems on assign.InventoryItemId equals invItem
                                    .InventoryItemId
                                where assign.WorkEffortId == component.WorkEffortId &&
                                      invItem.ProductId == component.ProductId
                                select new { assign.Quantity })
                            .ToListAsync();

                        component.IssuedQuantity = issuances.Sum(i => i.Quantity ?? 0);

                        // WorkEffortInventoryProduced alias: produced
                        var returns = await (from produced in _context.WorkEffortInventoryProduceds
                                join invItem in _context.InventoryItems on produced.InventoryItemId equals invItem
                                    .InventoryItemId
                                where produced.WorkEffortId == component.WorkEffortId &&
                                      invItem.ProductId == component.ProductId
                                select new { produced.InventoryItemId })
                            .ToListAsync();

                        foreach (var returnedItem in returns)
                        {
                            var returnDetail = await _context.InventoryItemDetails
                                .Where(i => i.InventoryItemId == returnedItem.InventoryItemId)
                                .OrderBy(i => i.InventoryItemDetailSeqId)
                                .FirstOrDefaultAsync();

                            if (returnDetail != null && returnDetail.QuantityOnHandDiff.HasValue)
                            {
                                component.ReturnedQuantity += (double)returnDetail.QuantityOnHandDiff.Value;
                            }
                        }
                    }

                    productionRunComponents.AddRange(components);
                }
            }

            return productionRunComponents;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving production run components: {ex.Message}");
            throw;
        }
    }


    public async Task<ProductionRunTaskReturnMaterialResult> ProductionRunTaskReturnMaterial(
        string workEffortId, string productId, decimal quantity, string lotId = null, string uomId = null)
    {
        var result = new ProductionRunTaskReturnMaterialResult();

        // Early check: if quantity is zero, simply return success.
        if (quantity == 0)
        {
            result.HasError = false;
            return result;
        }

        try
        {
            // Fetch total issued quantity for the task and product from WorkEffortInventoryAssign and InventoryItem
            var totalIssued = await (from weia in _context.WorkEffortInventoryAssigns
                join ii in _context.InventoryItems on weia.InventoryItemId equals ii.InventoryItemId
                where weia.WorkEffortId == workEffortId && ii.ProductId == productId
                select weia.Quantity ?? 0).SumAsync();

            // Fetch total returned quantity for the task and product from WorkEffortInventoryProduced and InventoryItemDetail
            var totalReturned = await (from weip in _context.WorkEffortInventoryProduceds
                join iid in _context.InventoryItemDetails on weip.InventoryItemId equals iid.InventoryItemId
                join ii in _context.InventoryItems on weip.InventoryItemId equals ii.InventoryItemId
                where weip.WorkEffortId == workEffortId && ii.ProductId == productId
                select iid.QuantityOnHandDiff ?? 0).SumAsync();

            // Check if the return quantity exceeds the available issued quantity
            if (quantity > (decimal)(totalIssued - (double)totalReturned))
            {
                result.HasError = true;
                result.ErrorMessage =
                    $"Cannot return more than issued. Available: {totalIssued - (double)totalReturned}, Requested: {quantity}";
                return result;
            }

            // Process the return (call productionRunTaskProduce with the isReturned flag set)
            var produceResult = await ProductionRunTaskProduce(
                workEffortId, productId, quantity, lotId: lotId, isReturned: "Y", uomId: uomId);

            if (produceResult.HasError)
            {
                result.HasError = true;
                result.ErrorMessage = produceResult.ErrorMessage;
                return result;
            }

            result.InventoryItemIds = produceResult.InventoryItemIds;
            result.HasError = false;
        }
        catch (Exception ex)
        {
            // Log the exception (optional)
            // _logger.LogError(ex, "Error occurred in ProductionRunTaskReturnMaterial");

            result.HasError = true;
            result.ErrorMessage = $"An error occurred while processing the return: {ex.Message}";
        }

        return result;
    }


    public async Task<List<ProductionRunComponentDto>> GetProductionRunComponentsForReturn(string productionRunId)
    {
        var productionRunComponents = new List<ProductionRunComponentDto>();

        // Fetch the production run data
        var productionRun = await _context.WorkEfforts.FindAsync(productionRunId);
        if (productionRun == null) return productionRunComponents;

        // Fetch routing tasks
        var routingTasks = await _context.WorkEfforts
            .Where(we => we.WorkEffortParentId == productionRunId && we.WorkEffortTypeId == "PROD_ORDER_TASK")
            .OrderBy(we => we.Priority)
            .ToListAsync();

        // Fetch components for each routing task
        foreach (var routingTask in routingTasks)
        {
            var components = await (from w in _context.WorkEffortGoodStandards
                join p in _context.Products on w.ProductId equals p.ProductId
                join we in _context.WorkEfforts on w.WorkEffortId equals we.WorkEffortId
                where w.WorkEffortId == routingTask.WorkEffortId &&
                      w.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED"
                select new ProductionRunComponentDto
                {
                    ProductId = w.ProductId,
                    ProductName = p.ProductName,
                    EstimatedQuantity = (double)w.EstimatedQuantity,
                    WorkEffortId = w.WorkEffortId,
                    WorkEffortName = we.WorkEffortName,
                    FromDate = w.FromDate,
                    LotId = null,
                    IssuedQuantity = (double)_context.WorkEffortInventoryAssigns
                        .Where(weia => weia.WorkEffortId == w.WorkEffortId &&
                                       weia.InventoryItem.ProductId == w.ProductId)
                        .Sum(weia => weia.Quantity ?? 0),
                    ReturnedQuantity = (double)(from wep in _context.WorkEffortInventoryProduceds
                        join iid in _context.InventoryItemDetails on wep.InventoryItemId equals iid.InventoryItemId
                        where wep.WorkEffortId == w.WorkEffortId &&
                              iid.InventoryItem.ProductId == w.ProductId
                        select iid.QuantityOnHandDiff ?? 0).Sum()
                }).ToListAsync();

            // Only include components with IssuedQuantity > 0
            productionRunComponents.AddRange(components.Where(c => c.IssuedQuantity > 0));
        }

        return productionRunComponents;
    }

    public async Task ReserveProductionRunTask(string workEffortId, string requireInventory = "Y")
    {
        try
        {
            // 1) Fetch WorkEffort
            var workEffort = await _context.WorkEfforts.FindAsync(workEffortId);
            if (workEffort == null)
                throw new Exception($"WorkEffort {workEffortId} not found.");

            // 2) Validate status
            if (workEffort.CurrentStatusId == "PRUN_CANCELLED" || workEffort.CurrentStatusId == "PRUN_CLOSED")
                throw new Exception($"Cannot reserve for a canceled/closed production run {workEffortId}.");

            // 3) Get BOM components
            var bomComponents = await _context.WorkEffortGoodStandards
                .Where(wgs => wgs.WorkEffortId == workEffortId
                              && wgs.StatusId == "WEGS_CREATED"
                              && wgs.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED")
                .ToListAsync();

            if (!bomComponents.Any())
            {
                _logger.LogInformation($"No BOM components found for WorkEffort {workEffortId}.");
                return;
            }

            // 4) For each BOM component, reserve 
            foreach (var comp in bomComponents)
            {
                if (string.IsNullOrEmpty(comp.ProductId))
                    continue;

                decimal neededQty = (decimal)comp.EstimatedQuantity;
                if (neededQty <= 0)
                    continue;

                // Reserve
                var notReserved = await ReserveProductionRunInventory(
                    workEffortId: workEffortId,
                    productId: comp.ProductId,
                    quantity: neededQty,
                    reserveOrderEnumId: "INVRO_FIFO_REC",
                    requireInventory: requireInventory
                );


                // Optional: If you want to log or do partial reservations, do it here
                var actuallyReserved = neededQty - notReserved;
                _logger.LogInformation($"For WorkEffort {workEffortId}, Product {comp.ProductId}, " +
                                       $"requested {neededQty}, reserved {actuallyReserved}.");
            }

            _logger.LogInformation($"Completed reservation for WorkEffort {workEffortId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in ReserveProductionRunTask for WorkEffortId {workEffortId}: {ex.Message}");
            throw; // or handle gracefully
        }
    }

    private async Task<decimal> ReserveProductionRunInventory(
        string workEffortId,
        string productId,
        decimal quantity,
        string reserveOrderEnumId,
        DateTime? reservedDatetime = null,
        string? requireInventory = "Y",
        string? containerId = null,
        string? lotId = null,
        long? sequenceId = null,
        int? priority = 1)
    {
        try
        {
            // 1) Validate Inputs
            if (string.IsNullOrEmpty(workEffortId)) throw new ArgumentException("WorkEffortId is required.");
            if (string.IsNullOrEmpty(productId)) throw new ArgumentException("ProductId is required.");
            if (quantity <= 0) throw new ArgumentException("Quantity must be > 0.");
            if (string.IsNullOrEmpty(reserveOrderEnumId)) reserveOrderEnumId = "INVRO_FIFO_REC";
            if (reservedDatetime == null) reservedDatetime = DateTime.UtcNow;

            // 2) Check if product is physical
            var product = await _context.Products
                .Select(p => new { p.ProductId, p.ProductTypeId })
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null) throw new Exception("Product not found.");

            var productType = await _context.ProductTypes
                .FindAsync(product.ProductTypeId);
            if (productType == null) throw new Exception("ProductType not found.");

            if (productType.IsPhysical == "N")
            {
                // Non-physical product => no actual inventory to reserve
                return 0;
            }

            // We'll track how much we fail to reserve
            decimal quantityNotReserved = quantity;

            // 3) Build base InventoryItem query for items that can be reserved
            var baseQuery = _context.InventoryItems
                .Where(ii => ii.ProductId == productId
                             && ii.AvailableToPromiseTotal > 0
                             && ii.StatusId != "INV_NS_DEFECTIVE"
                             && ii.StatusId != "INV_DEFECTIVE");

            // Filter by container, lot if specified
            if (!string.IsNullOrEmpty(containerId))
            {
                baseQuery = baseQuery.Where(ii => ii.ContainerId == containerId);
            }

            if (!string.IsNullOrEmpty(lotId))
            {
                baseQuery = baseQuery.Where(ii => ii.LotId == lotId);
            }

            // 4) Order the inventory items based on the reserveOrderEnumId
            // Similar to your existing logic
            switch (reserveOrderEnumId)
            {
                case "INVRO_GUNIT_COST":
                    baseQuery = baseQuery.OrderByDescending(ii => ii.UnitCost);
                    break;
                case "INVRO_LUNIT_COST":
                    baseQuery = baseQuery.OrderBy(ii => ii.UnitCost);
                    break;
                case "INVRO_FIFO_EXP":
                    baseQuery = baseQuery.OrderBy(ii => ii.ExpireDate);
                    break;
                case "INVRO_LIFO_EXP":
                    baseQuery = baseQuery.OrderByDescending(ii => ii.ExpireDate);
                    break;
                case "INVRO_LIFO_REC":
                    baseQuery = baseQuery.OrderByDescending(ii => ii.DatetimeReceived);
                    break;
                default:
                    baseQuery = baseQuery.OrderBy(ii => ii.DatetimeReceived);
                    break;
            }

            // We'll find pick-locations first, then bulk, then no location
            // You could unify this logic to one routine if you prefer
            // for clarity, we do it in steps like your existing code

            // Helper function to get items by location type
            async Task<List<InventoryItem>> GetItemsByLocationType(string locationType)
            {
                // locationType might be "FLT_PICKLOC" or "FLT_BULK"
                var list = await (
                    from ii in baseQuery
                    join fl in _context.FacilityLocations on ii.LocationSeqId equals fl.LocationSeqId into flJoin
                    from fl in flJoin.DefaultIfEmpty()
                    where fl.LocationTypeEnumId == locationType
                    select ii
                ).ToListAsync();

                return list;
            }

            // 5) Try to reserve from pick-locations first
            quantityNotReserved = await ReserveFromLocationType("FLT_PICKLOC",
                workEffortId, productId, quantityNotReserved,
                reserveOrderEnumId, (DateTime)reservedDatetime, sequenceId, priority);

            // 6) If still needed, reserve from bulk
            if (quantityNotReserved > 0)
            {
                quantityNotReserved = await ReserveFromLocationType("FLT_BULK",
                    workEffortId, productId, quantityNotReserved,
                    reserveOrderEnumId, (DateTime)reservedDatetime, sequenceId, priority);
            }

            // 7) If still needed, reserve from items with no location
            if (quantityNotReserved > 0)
            {
                quantityNotReserved = await ReserveFromNoLocation(
                    workEffortId, productId, quantityNotReserved,
                    reserveOrderEnumId, (DateTime)reservedDatetime, sequenceId, priority);
            }

            // 8) Handle leftover if we still have unreserved quantity
            if (quantityNotReserved > 0)
            {
                if (requireInventory == "Y")
                {
                    // Not enough inventory => throw
                    throw new InsufficientInventoryException(productId, quantity, quantityNotReserved);
                }
                // else partial reservation is allowed
                // Optional: you could create a special "backorder" row or
                // negative ATP line. For simplicity we do nothing or
                // just return how many we couldn't reserve.
            }

            return quantityNotReserved; // 0 means fully reserved
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in ReserveProductionRunInventory: {ex.Message}");
            throw;
        }
    }

    private async Task<decimal> ReserveFromLocationType(
        string locationTypeEnumId,
        string workEffortId,
        string productId,
        decimal quantityNotReserved,
        string reserveOrderEnumId,
        DateTime reservedDatetime,
        long? sequenceId,
        int? priority)
    {
        if (quantityNotReserved <= 0) return 0;

        // Find the relevant inventory items
        var items = await (
            from ii in _context.InventoryItems
            join fl in _context.FacilityLocations on ii.LocationSeqId equals fl.LocationSeqId into flJoin
            from fl in flJoin.DefaultIfEmpty()
            where fl.LocationTypeEnumId == locationTypeEnumId
                  && ii.ProductId == productId
                  && ii.AvailableToPromiseTotal > 0
                  && ii.StatusId != "INV_NS_DEFECTIVE"
                  && ii.StatusId != "INV_DEFECTIVE"
            select ii
        ).ToListAsync();

        foreach (var inventoryItem in items)
        {
            if (quantityNotReserved <= 0) break;

            quantityNotReserved = await ReserveForInventoryItemInline(
                inventoryItem,
                workEffortId,
                quantityNotReserved,
                reserveOrderEnumId,
                reservedDatetime,
                sequenceId,
                priority
            );
        }

        return quantityNotReserved;
    }

    private async Task<decimal> ReserveFromNoLocation(
        string workEffortId,
        string productId,
        decimal quantityNotReserved,
        string reserveOrderEnumId,
        DateTime reservedDatetime,
        long? sequenceId,
        int? priority)
    {
        if (quantityNotReserved <= 0) return 0;

        var items = await _context.InventoryItems
            .Where(ii => ii.ProductId == productId
                         && string.IsNullOrEmpty(ii.LocationSeqId)
                         && ii.AvailableToPromiseTotal > 0
                         && ii.StatusId != "INV_NS_DEFECTIVE"
                         && ii.StatusId != "INV_DEFECTIVE")
            .ToListAsync();

        foreach (var inventoryItem in items)
        {
            if (quantityNotReserved <= 0) break;

            quantityNotReserved = await ReserveForInventoryItemInline(
                inventoryItem,
                workEffortId,
                quantityNotReserved,
                reserveOrderEnumId,
                reservedDatetime,
                sequenceId,
                priority
            );
        }

        return quantityNotReserved;
    }


    private async Task<decimal> ReserveForInventoryItemInline(
        InventoryItem inventoryItem,
        string workEffortId,
        decimal quantityNotReserved,
        string reserveOrderEnumId,
        DateTime reservedDatetime,
        long? sequenceId,
        int? priority)
    {
        if (quantityNotReserved <= 0) return 0;

        decimal availableToPromise = inventoryItem.AvailableToPromiseTotal ?? 0m;
        if (availableToPromise <= 0) return quantityNotReserved;

        // Determine how much we can reserve from this item
        decimal reserveAmount = Math.Min(quantityNotReserved, availableToPromise);

        // 1) Decrease the item’s ATP by reserveAmount
        // (We do NOT decrease QuantityOnHandTotal for a reservation)
        var createDetailParam = new CreateInventoryItemDetailParam
        {
            InventoryItemId = inventoryItem.InventoryItemId,
            AvailableToPromiseDiff = -reserveAmount,
            QuantityOnHandDiff = 0m, // we're not issuing, only reserving
            WorkEffortId = workEffortId,
            ReasonEnumId = null,
            Description = "Reservation for production run"
        };
        await _inventoryService.CreateInventoryItemDetail(createDetailParam);

        // 2) Insert/Update the WorkEffortInventoryRes record 
        //    (the "staging" record that says we reserved X from this item)
        var newRes = new WorkEffortInventoryRes
        {
            WorkEffortInvResId = Guid.NewGuid().ToString(),
            WorkEffortId = workEffortId,
            InventoryItemId = inventoryItem.InventoryItemId,
            ReserveOrderEnumId = reserveOrderEnumId,
            Quantity = reserveAmount,
            QuantityNotAvailable = 0, // or if partial, you might set leftover here
            ReservedDatetime = reservedDatetime,
            CreatedDatetime = DateTime.UtcNow,
            PromisedDatetime = reservedDatetime.AddDays(2), // Example logic
            Priority = priority,
            LastUpdatedStamp = DateTime.UtcNow,
            CreatedStamp = DateTime.UtcNow
        };

        _context.WorkEffortInventoryRes.Add(newRes);

        // 3) Adjust local leftover
        quantityNotReserved -= reserveAmount;

        return quantityNotReserved;
    }

    public async Task IssueProductionRunReservations(
        string workEffortId,
        bool failIfNotEnoughQoh = true,
        string? reasonEnumId = null,
        string? description = null)
    {
        try
        {
            // 1) Validate WorkEffort
            var workEffort = await _context.WorkEfforts.FindAsync(workEffortId);
            if (workEffort == null)
                throw new Exception($"WorkEffort {workEffortId} not found.");

            // Optionally check if WorkEffort is not canceled or closed
            if (workEffort.CurrentStatusId == "PRUN_CANCELLED" || workEffort.CurrentStatusId == "PRUN_CLOSED")
                throw new Exception($"WorkEffort {workEffortId} is canceled or closed; cannot issue reservations.");

            // 2) Fetch all reservation lines for this WorkEffort
            var reservationLines = await _context.WorkEffortInventoryRes
                .Where(r => r.WorkEffortId == workEffortId && r.Quantity > 0)
                .ToListAsync();

            if (!reservationLines.Any())
            {
                _logger.LogInformation($"No reservations found for WorkEffort {workEffortId}.");
                return;
            }

            // 3) Issue each reservation line
            foreach (var line in reservationLines)
            {
                // Defensive checks
                var quantityToIssue = line.Quantity ?? 0m;
                if (quantityToIssue <= 0) continue; // skip

                if (string.IsNullOrEmpty(line.InventoryItemId))
                    throw new Exception($"Reservation line {line.WorkEffortInvResId} has no InventoryItemId.");

                // Retrieve the InventoryItem
                var inventoryItem = await _context.InventoryItems.FindAsync(line.InventoryItemId);
                if (inventoryItem == null)
                    throw new Exception($"InventoryItem {line.InventoryItemId} not found.");

                // 4) Check QOH or available ATP
                // Because we are now physically issuing, we typically reduce QOH
                // If failIfNotEnoughQoh==true, throw if QOH is insufficient
                var onHand = inventoryItem.QuantityOnHandTotal ?? 0m;
                if (onHand < quantityToIssue && failIfNotEnoughQoh)
                {
                    throw new Exception(
                        $"Not enough on-hand quantity for InventoryItem {inventoryItem.InventoryItemId}. " +
                        $"Wanted to issue {quantityToIssue}, only {onHand} available.");
                }

                // 5) Create a WorkEffortInventoryAssign record (like direct issuance)
                var assignMap = new WorkEffortInventoryAssign
                {
                    WorkEffortId = line.WorkEffortId,
                    InventoryItemId = line.InventoryItemId,
                    Quantity = (double)quantityToIssue
                };
                await AssignInventoryToWorkEffort(assignMap);

                // 6) Create an InventoryItemDetail to reduce QOH
                // This physically issues the items
                var detailParam = new CreateInventoryItemDetailParam
                {
                    InventoryItemId = inventoryItem.InventoryItemId,
                    WorkEffortId = workEffortId,
                    QuantityOnHandDiff = -quantityToIssue,
                    AvailableToPromiseDiff = 0m, // ATP was reduced at reservation time
                    ReasonEnumId = reasonEnumId,
                    Description = description
                };
                await _inventoryService.CreateInventoryItemDetail(detailParam);

                // 7) Balance the InventoryItem after the QOH change
                await _inventoryService.BalanceInventoryItems(inventoryItem.InventoryItemId, inventoryItem.FacilityId);

                // 8) Mark the reservation line as fulfilled
                // You can remove it or zero out the quantity
                _context.WorkEffortInventoryRes.Remove(line);
                // OR: line.Quantity = 0; _context.WorkEffortInventoryRes.Update(line);
            }

            _logger.LogInformation($"Successfully issued reservations for WorkEffort {workEffortId}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                $"Error issuing reservations for WorkEffort {workEffortId}: {ex.Message}");
            throw;
        }
    }


// Helper methods below ...
}

// Double checked functions that started from QuickChangeProductionRunStatus
// QuickChangeProductionRunStatus
// ChangeProductionRunStatus
// QuickStartAllProductionRunTasks
// UpdateWorkEffort
// ChangeProductionRunTaskStatus
// GetProductionRunCost
// GetWorkEffortCosts
// GetProductionRunRoutingTasks
// CloneWorkEffortCostCalc
// CloneWorkEffortPartyAssignment
// IssueProductionRunTas
// IssueProductionRunTaskComponent
// GetProductProduced
// IssueProductionRunTaskComponentInline
// AssignInventoryToWorkEffort