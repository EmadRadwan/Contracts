using System.Reflection;
using Application.Accounting.Services;
using Application.Common;
using Application.Core;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.WorkEfforts;

public interface IWorkEffortService
{
    Task<string> CreateWorkEffort(WorkEffort workEffort);

    void CreateWorkEffortGoodStandard(WorkEffortGoodStandard workEffortGoodStandard);


    Task<WorkEffort> UpdateWorkEffort(string workEffortId, Dictionary<string, object> parameters);
    Task<OperationResult> CreateWorkEffortInventoryProduced(string workEffortId, string inventoryItemId);
}

public class WorkEffortService : IWorkEffortService
{
    private readonly DataContext _context;
    private readonly ILogger _logger;
    private readonly IUtilityService _utilityService;
    private readonly IGeneralLedgerService _generalLedgerService;

    public WorkEffortService(DataContext context, ILogger<WorkEffortService> logger, IUtilityService utilityService,
        IGeneralLedgerService generalLedgerService)
    {
        _context = context;
        _logger = logger;
        _utilityService = utilityService;
        _generalLedgerService = generalLedgerService;
    }

    public async Task<string> CreateWorkEffort(WorkEffort workEffort)
    {
        var stamp = DateTime.UtcNow;

        // get next work effort id
        var newWorkEffortId = await _utilityService.GetNextSequence("WorkEffort");

        workEffort.WorkEffortId = newWorkEffortId;
        workEffort.LastStatusUpdate = stamp;
        workEffort.LastModifiedDate = stamp;
        workEffort.CreatedDate = stamp;
        workEffort.CreatedStamp = stamp;
        workEffort.LastUpdatedStamp = stamp;
        workEffort.RevisionNumber = 1;


        // Add WorkEffort entity to the context
        await _context.WorkEfforts.AddAsync(new WorkEffort
        {
            WorkEffortId = workEffort.WorkEffortId,
            WorkEffortTypeId = workEffort.WorkEffortTypeId,
            WorkEffortName = workEffort.WorkEffortName,
            EstimatedStartDate = workEffort.EstimatedStartDate,
            FacilityId = workEffort.FacilityId,
            Description = workEffort.Description,
            QuantityToProduce = workEffort.QuantityToProduce,
            WorkEffortPurposeTypeId = workEffort.WorkEffortPurposeTypeId,
            FixedAssetId = workEffort.FixedAssetId,
            EstimatedSetupMillis = workEffort.EstimatedSetupMillis,
            EstimatedMilliSeconds = workEffort.EstimatedMilliSeconds,
            EstimateCalcMethod = workEffort.EstimateCalcMethod,
            ReservPersons = workEffort.ReservPersons,
            CurrentStatusId = workEffort.CurrentStatusId,
            LastStatusUpdate = workEffort.LastStatusUpdate,
            LastModifiedDate = workEffort.LastModifiedDate,
            CreatedDate = workEffort.CreatedDate,
            CreatedStamp = workEffort.CreatedStamp,
            LastUpdatedStamp = workEffort.LastUpdatedStamp,
            RevisionNumber = workEffort.RevisionNumber
        });

        // Create new WorkEffortStatus entity
        var workEffortStatus = new WorkEffortStatus
        {
            WorkEffortId = workEffort.WorkEffortId,
            StatusId = workEffort.CurrentStatusId,
            StatusDatetime = stamp,
            CreatedStamp = stamp,
            LastUpdatedStamp = stamp
        };
        _context.WorkEffortStatuses.Add(workEffortStatus);

        return workEffort.WorkEffortId;
    }

    public void CreateWorkEffortGoodStandard(WorkEffortGoodStandard workEffortGoodStandard)
    {
        // Add WorkEffortGoodStandard entity to the context
        _context.WorkEffortGoodStandards.Add(workEffortGoodStandard);
    }

    private bool PropertiesAreEqual(WorkEffort current, WorkEffort original)
    {
        return current.CurrentStatusId == original.CurrentStatusId &&
               current.LastStatusUpdate == original.LastStatusUpdate &&
               current.LastModifiedDate == original.LastModifiedDate &&
               current.LastModifiedByUserLogin == original.LastModifiedByUserLogin &&
               current.RevisionNumber == original.RevisionNumber;
        // Add comparisons for other relevant fields...
    }

    public async Task<OperationResult> CreateWorkEffortInventoryProduced(string workEffortId, string inventoryItemId)
    {
        var result = new OperationResult();

        try
        {
            if (string.IsNullOrEmpty(inventoryItemId))
            {
                result.ErrorMessage = "InventoryItemId is required.";
                return result;
            }

            if (string.IsNullOrEmpty(workEffortId))
            {
                result.ErrorMessage = "WorkEffortId is required.";
                return result;
            }

            var newEntity = new WorkEffortInventoryProduced
            {
                WorkEffortId = workEffortId,
                InventoryItemId = inventoryItemId,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow,
                // Include other non-primary key attributes here
            };

            _context.WorkEffortInventoryProduceds.Add(newEntity);

            await _generalLedgerService.CreateAcctgTransForWorkEffortInventoryProduced(workEffortId, inventoryItemId);

            result.Success = true;
            return result;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<WorkEffort> UpdateWorkEffort(string workEffortId, Dictionary<string, object> parameters)
    {
        try
        {
            // Step 1: Fetch the WorkEffort entity
            var workEffort = await _context.WorkEfforts.FindAsync(workEffortId);
            if (workEffort == null)
            {
                throw new Exception("WorkEffort not found");
            }

            var nowTimestamp = DateTime.UtcNow;

            // Step 2: Handle status change if necessary
            if (parameters.ContainsKey("currentStatusId"))
            {
                string currentStatusId = parameters["currentStatusId"].ToString();
                if (!string.IsNullOrEmpty(currentStatusId) && currentStatusId != workEffort.CurrentStatusId)
                {
                    if (!string.IsNullOrEmpty(workEffort.CurrentStatusId))
                    {
                        // Validate status change
                        var isValidChange = await _context.StatusValidChanges.AnyAsync(svc =>
                            svc.StatusId == workEffort.CurrentStatusId && svc.StatusIdTo == currentStatusId);

                        if (!isValidChange)
                        {
                            _logger.LogError(
                                $"The status change from {workEffort.CurrentStatusId} to {currentStatusId} is not valid");
                            throw new Exception("The status change is not valid");
                        }
                    }

                    workEffort.CurrentStatusId = currentStatusId;
                    workEffort.LastStatusUpdate = nowTimestamp;
                    var newWorkEffortStatus = new WorkEffortStatus
                    {
                        WorkEffortId = workEffort.WorkEffortId,
                        StatusId = currentStatusId,
                        Reason = parameters.ContainsKey("reason") ? parameters["reason"].ToString() : null,
                        StatusDatetime = nowTimestamp,
                        CreatedStamp = nowTimestamp,
                        LastUpdatedStamp = nowTimestamp,
                    };

                    await _context.WorkEffortStatuses.AddAsync(newWorkEffortStatus);
                }
            }

            // Update other fields dynamically with case-insensitive property lookup
            foreach (var param in parameters)
            {
                var property = workEffort.GetType().GetProperty(param.Key,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(workEffort, param.Value);
                }
            }

            // Check if any changes were made
            if (_context.Entry(workEffort).State == EntityState.Modified)
            {
                workEffort.LastModifiedDate = nowTimestamp;
                workEffort.LastUpdatedStamp = nowTimestamp;
                workEffort.RevisionNumber = (workEffort.RevisionNumber ?? 0) + 1;
            }

            return workEffort;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating WorkEffort: {ex.Message}");
            throw;
        }
    }
}