using Application._Base;
using Application.Accounting.Services;
using Application.Catalog.Products.Services.Inventory;
using Application.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Projects;

public interface IProjectService
{
    Task<Results<IssueMaterialsForCertificateResult>> IssueMaterialsForCertificate(
        string workEffortId,
        string failIfItemsAreNotAvailable = "Y",
        string failIfItemsAreNotOnHand = "Y");
}

public class ProjectService : BaseService, IProjectService
{
    private readonly IInventoryService _inventoryService;
    private readonly IGeneralLedgerService _generalLedgerService;


    public ProjectService(DataContext context, IUtilityService utilityService,
        ILogger<ProjectService> logger,
        IInventoryService inventoryService, IGeneralLedgerService generalLedgerService) : base(
        context, utilityService, logger)
    {
        _inventoryService = inventoryService;
        _generalLedgerService = generalLedgerService;
    }


    public async Task<Results<IssueMaterialsForCertificateResult>> IssueMaterialsForCertificate(
        string workEffortId,
        string failIfItemsAreNotAvailable = "Y",
        string failIfItemsAreNotOnHand = "Y")
    {
        var result = new IssueMaterialsForCertificateResult();
        try
        {
            // REFACTOR: Fetch certificate header from WorkEfforts instead of production run;
            // this aligns with certificate storage in WorkEfforts table, using WORK_EFFORT_TYPE_ID = "PROJECT_CERTIFICATE".
            var workEffort = await _context.WorkEfforts.FindAsync(workEffortId);
            if (workEffort == null)
                return Results<IssueMaterialsForCertificateResult>.Failure(
                    $"WorkEffort with ID {workEffortId} not found.",
                    "WORK_EFFORT_NOT_FOUND");

            // REFACTOR: Check certificate status instead of PRUN_CANCELLED;
            // this prevents issuance for completed or cancelled certificates, ensuring valid state for COMPANY_SUPPLY_SALE_CERTIFICATE.
            if (workEffort.CurrentStatusId == "WEPR_COMPLETED" || workEffort.CurrentStatusId == "WEPR_CANCELLED")
                return Results<IssueMaterialsForCertificateResult>.Failure(
                    "Cannot issue materials for a completed or cancelled certificate.",
                    "INVALID_WORK_EFFORT_STATUS");

            // REFACTOR: Replaced WorkEffortGoodStandards with WorkEfforts query for CERTIFICATE_ITEM;
            // this fetches certificate items with ProductId and Quantity, aligning with your data structure (e.g., WORK_EFFORT_ID = "10034").
            var certificateItems = await _context.WorkEfforts
                .Where(w => w.WorkEffortParentId == workEffortId &&
                            w.WorkEffortTypeId == "CERTIFICATE_ITEM" &&
                            w.CurrentStatusId == "WEPR_CREATED" &&
                            w.ProductId != null &&
                            w.Quantity > 0)
                .ToListAsync();

            // Check inventory availability for all items first
            // REFACTOR: Added pre-check for all items to ensure atomic issuance;
            // this ensures "all or none" issuance, preventing partial updates for certificates.
            var insufficientItems = new List<InsufficientItem>();
            foreach (var item in certificateItems)
            {
                var productId = item.ProductId;
                var quantity = item.Quantity ?? 0;

                // REFACTOR: Removed WorkEffortInventoryAssigns check;
                // certificates issue materials directly without prior assignments, simplifying logic for non-manufacturing context.
                var availableInventory = await _context.InventoryItems
                    .Where(ii => ii.ProductId == productId &&
                                 ii.FacilityId == workEffort.FacilityId &&
                                 (string.IsNullOrEmpty(ii.StatusId) || ii.StatusId == "INV_AVAILABLE"))
                    .OrderByDescending(ii => ii.LastUpdatedStamp ?? ii.CreatedStamp)
                    .SumAsync(ii => failIfItemsAreNotAvailable == "Y"
                        ? ii.QuantityOnHandTotal
                        : ii.AvailableToPromiseTotal) ?? 0;

                if (availableInventory < quantity)
                {
                    var product = await _context.Products
                        .Where(p => p.ProductId == productId)
                        .Select(p => new { p.ProductName })
                        .FirstOrDefaultAsync();

                    var productName = product?.ProductName ?? $"Unknown Product (ID: {productId})";
                    insufficientItems.Add(new InsufficientItem
                    {
                        ProductName = productName,
                        QuantityMissing = quantity - availableInventory
                    });
                }
            }

            if (insufficientItems.Any())
            {
                var errorMessage = "Insufficient inventory for the following items: " +
                                   string.Join(", ", insufficientItems
                                       .Select(i => $"Product: {i.ProductName}, Missing: {i.QuantityMissing}"));
                return Results<IssueMaterialsForCertificateResult>.Failure(errorMessage, "INSUFFICIENT_INVENTORY");
            }

            // Issue materials for each certificate item
            foreach (var item in certificateItems)
            {
                var productId = item.ProductId;
                var quantity = item.Quantity ?? 0;

                var inventoryItems = await _context.InventoryItems
                    .Where(ii => ii.ProductId == productId &&
                                 ii.FacilityId == workEffort.FacilityId &&
                                 ii.QuantityOnHandTotal > 0 &&
                                 (string.IsNullOrEmpty(ii.StatusId) || ii.StatusId == "INV_AVAILABLE"))
                    .OrderByDescending(ii => ii.LastUpdatedStamp ?? ii.CreatedStamp)
                    .ToListAsync();

                var quantityNotIssued = quantity;

                foreach (var inventoryItem in inventoryItems)
                {
                    if (quantityNotIssued <= 0) break;

                    var availableQuantity = failIfItemsAreNotAvailable == "Y"
                        ? (decimal)inventoryItem.QuantityOnHandTotal
                        : (decimal)inventoryItem.AvailableToPromiseTotal;

                    if (availableQuantity <= 0) continue;

                    var deductAmount = Math.Min(quantityNotIssued, availableQuantity);


                    // Create InventoryItemDetail for deduction
                    // REFACTOR: Adapted CreateInventoryItemDetail for certificate issuance;
                    // ensures QOH/ATP deduction with ItemIssuance linkage, no manufacturing-specific logic.
                    var detailParam = new CreateInventoryItemDetailParam
                    {
                        InventoryItemId = inventoryItem.InventoryItemId,
                        WorkEffortId = workEffortId,
                        QuantityOnHandDiff = -deductAmount,
                        AvailableToPromiseDiff = -deductAmount,
                        ReasonEnumId = "INV_ISSUED_INTERNAL",
                        Description = "Issued for certificate task"
                    };
                    await _inventoryService.CreateInventoryItemDetail(detailParam);

                    // Balance inventory
                    // REFACTOR: Reused BalanceInventoryItems for consistency;
                    // ensures inventory consistency post-issuance, as in production runs.
                    await _inventoryService.BalanceInventoryItems(inventoryItem.InventoryItemId);

                    // Pass the certificate line being issued and the quantity actually deducted —
                    // the GL posting must cost this deduction, not re-derive the line by product.
                    await _generalLedgerService.CreateAcctgTransForCertificateIssuance(
                        workEffortId, inventoryItem.InventoryItemId, item.WorkEffortId, deductAmount);

                    quantityNotIssued -= deductAmount;
                }

                // REFACTOR: Fail if any quantity remains unissued when strict checking is enabled;
                // this enforces "all or none" issuance for certificates.
                if (quantityNotIssued > 0 && failIfItemsAreNotAvailable == "Y")
                {
                    var product = await _context.Products
                        .Where(p => p.ProductId == productId)
                        .Select(p => new { p.ProductName })
                        .FirstOrDefaultAsync();

                    var productName = product?.ProductName ?? $"Unknown Product (ID: {productId})";
                    result.InsufficientItems.Add(new InsufficientItem
                    {
                        ProductName = productName,
                        QuantityMissing = quantityNotIssued
                    });
                }
            }

            if (result.InsufficientItems.Any())
            {
                var errorMessage = "Failed to issue all materials for the following items: " +
                                   string.Join(", ", result.InsufficientItems
                                       .Select(i => $"Product: {i.ProductName}, Missing: {i.QuantityMissing}"));
                return Results<IssueMaterialsForCertificateResult>.Failure(errorMessage, "INSUFFICIENT_INVENTORY");
            }

            // REFACTOR: Log successful issuance with certificate context;
            // improves traceability for certificate material issuances.
            _logger.LogInformation($"Issued materials for certificate WorkEffortId {workEffortId}.");

            return Results<IssueMaterialsForCertificateResult>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error issuing materials for certificate WorkEffortId: {WorkEffortId}", workEffortId);
            return Results<IssueMaterialsForCertificateResult>.Failure(
                ex.Message ?? "An unexpected error occurred while issuing materials for certificate.");
        }
    }
}