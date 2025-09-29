using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Projects
{
    public partial class ProjectService
    {
        public async Task<Results<IssueMaterialsForCertificateResult>> IssueMaterialsForCertificate(
            string workEffortId, 
            string failIfItemsAreNotAvailable = "Y", 
            string failIfItemsAreNotOnHand = "Y")
        {
            var result = new IssueMaterialsForCertificateResult();
            // REFACTOR: Added transaction management for atomic issuance and accounting;
            // ensures all-or-none issuance with PROJECTS_UNDER_DEVELOPMENT (143000) and INVENTORY_ACCOUNT (140000).
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var workEffort = await _context.WorkEfforts.FindAsync(workEffortId);
                if (workEffort == null)
                    return Results<IssueMaterialsForCertificateResult>.Failure(
                        $"WorkEffort with ID {workEffortId} not found.", "WORK_EFFORT_NOT_FOUND");

                if (workEffort.CurrentStatusId == "WEPR_COMPLETED" || workEffort.CurrentStatusId == "WEPR_CANCELLED")
                    return Results<IssueMaterialsForCertificateResult>.Failure(
                        "Cannot issue materials for a completed or cancelled certificate.", "INVALID_WORK_EFFORT_STATUS");

                var certificateItems = await _context.WorkEfforts
                    .Where(w => w.WorkEffortParentId == workEffortId 
                        && w.WorkEffortTypeId == "CERTIFICATE_ITEM" 
                        && w.CurrentStatusId == "WEPR_CREATED" 
                        && w.ProductId != null 
                        && w.Quantity > 0)
                    .ToListAsync();

                var insufficientItems = new List<InsufficientItem>();
                foreach (var item in certificateItems)
                {
                    var productId = item.ProductId;
                    var quantity = item.Quantity ?? 0;
                    var availableInventory = await _context.InventoryItems
                        .Where(ii => ii.ProductId == productId 
                            && ii.FacilityId == workEffort.FacilityId 
                            && (string.IsNullOrEmpty(ii.StatusId) || ii.StatusId == "INV_AVAILABLE"))
                        .SumAsync(ii => failIfItemsAreNotAvailable == "Y" ? ii.QuantityOnHandTotal : ii.AvailableToPromiseTotal) ?? 0;

                    if (availableInventory < quantity)
                    {
                        var product = await _context.Products
                            .Where(p => p.ProductId == productId)
                            .Select(p => new { p.ProductName })
                            .FirstOrDefaultAsync();
                        var productName = product?.ProductName ?? $"Unknown Product (ID: {productId})";
                        insufficientItems.Add(new InsufficientItem { ProductName = productName, QuantityMissing = quantity - availableInventory });
                    }
                }

                if (insufficientItems.Any())
                {
                    var errorMessage = "Insufficient inventory for the following items: " + 
                        string.Join(", ", insufficientItems.Select(i => $"Product: {i.ProductName}, Missing: {i.QuantityMissing}"));
                    return Results<IssueMaterialsForCertificateResult>.Failure(errorMessage, "INSUFFICIENT_INVENTORY");
                }

                foreach (var item in certificateItems)
                {
                    var productId = item.ProductId;
                    var quantity = item.Quantity ?? 0;
                    var inventoryItems = await _context.InventoryItems
                        .Where(ii => ii.ProductId == productId 
                            && ii.FacilityId == workEffort.FacilityId 
                            && ii.QuantityOnHandTotal > 0 
                            && (string.IsNullOrEmpty(ii.StatusId) || ii.StatusId == "INV_AVAILABLE"))
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
                        await _inventoryService.BalanceInventoryItems(inventoryItem.InventoryItemId);

                        // REFACTOR: Added accounting transaction for certificate issuance;
                        // uses CreateAcctgTransForCertificateIssuance with PROJECTS_UNDER_DEVELOPMENT and INVENTORY_ACCOUNT.
                        await CreateAcctgTransForCertificateIssuance(workEffortId, inventoryItem.InventoryItemId);

                        quantityNotIssued -= deductAmount;
                    }

                    if (quantityNotIssued > 0 && failIfItemsAreNotAvailable == "Y")
                    {
                        var product = await _context.Products
                            .Where(p => p.ProductId == productId)
                            .Select(p => new { p.ProductName })
                            .FirstOrDefaultAsync();
                        var productName = product?.ProductName ?? $"Unknown Product (ID: {productId})";
                        result.InsufficientItems.Add(new InsufficientItem { ProductName = productName, QuantityMissing = quantityNotIssued });
                    }
                }

                if (result.InsufficientItems.Any())
                {
                    var errorMessage = "Failed to issue all materials for the following items: " + 
                        string.Join(", ", result.InsufficientItems.Select(i => $"Product: {i.ProductName}, Missing: {i.QuantityMissing}"));
                    return Results<IssueMaterialsForCertificateResult>.Failure(errorMessage, "INSUFFICIENT_INVENTORY");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation($"Issued materials for certificate WorkEffortId {workEffortId}.");
                return Results<IssueMaterialsForCertificateResult>.Success(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error issuing materials for certificate WorkEffortId: {WorkEffortId}", workEffortId);
                return Results<IssueMaterialsForCertificateResult>.Failure(
                    ex.Message ?? "An unexpected error occurred while issuing materials for certificate.");
            }
        }
    }
}