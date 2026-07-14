using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Catalog.Products.Services.Inventory;
using Application.Core;
using Application.Manufacturing;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.WorkEfforts
{
    public class IssueProductionRunReservationsParams
    {
        public string WorkEffortId { get; set; } = null!;
        public bool FailIfNotEnoughQoh { get; set; } = true;
        public string? ReasonEnumId { get; set; }
        public string? Description { get; set; }
    }

    public class IssueProductionRunReservationsResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = null!;
        public List<InsufficientItem> InsufficientItems { get; set; } = new();
    }

    public class InsufficientItem
    {
        public string ProductName { get; set; } = null!;
        public decimal QuantityMissing { get; set; }
        public string InventoryItemId { get; set; } = null!;
    }

    public class IssueProductionRunReservations
    {
        public class Command : IRequest<Result<IssueProductionRunReservationsResult>>
        {
            public IssueProductionRunReservationsParams IssueParams { get; set; } = null!;
        }

        public class Handler : IRequestHandler<Command, Result<IssueProductionRunReservationsResult>>
        {
            private readonly DataContext _context;
            private readonly IProductionRunService _productionRunService;
            private readonly IInventoryService _inventoryService;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, IProductionRunService productionRunService, IInventoryService inventoryService, ILogger<Handler> logger)
            {
                _context = context;
                _productionRunService = productionRunService;
                _inventoryService = inventoryService;
                _logger = logger;
            }

            public async Task<Result<IssueProductionRunReservationsResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                var result = new IssueProductionRunReservationsResult();

                // REFACTOR: Validate input parameters
                // Purpose: Ensure WorkEffortId is not null or empty
                // Benefit: Prevents invalid requests from proceeding
                if (string.IsNullOrEmpty(request.IssueParams.WorkEffortId))
                    return Result<IssueProductionRunReservationsResult>.Failure("WorkEffortId cannot be null or empty.");

                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    // Validate WorkEffort existence and status
                    var workEffort = await _context.WorkEfforts.FindAsync(new object[] { request.IssueParams.WorkEffortId }, cancellationToken);
                    if (workEffort == null)
                        return Result<IssueProductionRunReservationsResult>.Failure(
                            $"WorkEffort {request.IssueParams.WorkEffortId} not found.");

                    if (workEffort.CurrentStatusId == "PRUN_CANCELLED" || workEffort.CurrentStatusId == "PRUN_CLOSED")
                        return Result<IssueProductionRunReservationsResult>.Failure(
                            $"WorkEffort {request.IssueParams.WorkEffortId} is cancelled or closed; cannot issue reservations.");

                    // REFACTOR: Fetch BOM components with relaxed validation for additional materials
                    // Purpose: Allow issuance if no BOM components are found
                    // Benefit: Supports issuing additional materials post-initial issuance
                    var bomComponents = await _context.WorkEffortGoodStandards
                        .Where(wgs =>
                            wgs.WorkEffortId == request.IssueParams.WorkEffortId &&
                            wgs.StatusId == "WEGS_CREATED" &&
                            wgs.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED" &&
                            wgs.FromDate <= DateTime.UtcNow &&
                            (wgs.ThruDate == null || wgs.ThruDate >= DateTime.UtcNow))
                        .ToListAsync(cancellationToken);

                    var isAdditionalMaterials = !bomComponents.Any();
                    if (isAdditionalMaterials)
                    {
                        _logger.LogWarning(
                            "No BOM components found for WorkEffortId: {WorkEffortId}. Treating as issuance for additional materials.",
                            request.IssueParams.WorkEffortId);
                    }

                    // Fetch reservation lines
                    var reservationLines = await _context.WorkEffortInventoryRes
                        .Where(r => r.WorkEffortId == request.IssueParams.WorkEffortId && r.Quantity > 0)
                        .Include(r => r.InventoryItem)
                        .ToListAsync(cancellationToken);

                    if (!reservationLines.Any())
                    {
                        _logger.LogInformation($"No reservations found for WorkEffort {request.IssueParams.WorkEffortId}.");
                        result.Success = true;
                        result.Message = "No reservations to issue.";
                        return Result<IssueProductionRunReservationsResult>.Success(result);
                    }

                    // REFACTOR: Accumulate issued quantities in memory
                    // Purpose: Track total issued per product locally
                    // Benefit: Ensures accurate status updates
                    var issuedQuantities = new Dictionary<string?, decimal>();

                    foreach (var line in reservationLines)
                    {
                        // REFACTOR: Validate InventoryItemId
                        // Purpose: Ensure reservation has valid InventoryItemId
                        // Benefit: Improves error traceability
                        if (string.IsNullOrEmpty(line.InventoryItemId))
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<IssueProductionRunReservationsResult>.Failure(
                                $"Reservation {line.WorkEffortInvResId} for WorkEffort {request.IssueParams.WorkEffortId} has no InventoryItemId.");
                        }

                        // REFACTOR: Skip BOM validation for additional materials
                        // Purpose: Allow issuance without BOM check if no components found
                        // Benefit: Supports additional material issuances
                        if (!isAdditionalMaterials)
                        {
                            var bomComponent = bomComponents.FirstOrDefault(bc => bc.ProductId == line.InventoryItem?.ProductId);
                            if (bomComponent == null)
                            {
                                await transaction.RollbackAsync(cancellationToken);
                                return Result<IssueProductionRunReservationsResult>.Failure(
                                    $"Reservation {line.WorkEffortInvResId} for InventoryItem {line.InventoryItemId} with Product {line.InventoryItem?.ProductId} does not match any BOM component.");
                            }
                        }

                        var quantityToIssue = line.Quantity ?? 0m;
                        if (quantityToIssue <= 0)
                            continue;

                        // Validate InventoryItem and QOH
                        var inventoryItem = await _context.InventoryItems
                            .FirstOrDefaultAsync(ii => ii.InventoryItemId == line.InventoryItemId, cancellationToken);

                        if (inventoryItem == null)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return Result<IssueProductionRunReservationsResult>.Failure(
                                $"InventoryItem {line.InventoryItemId} not found for Reservation {line.WorkEffortInvResId}.");
                        }

                        var onHand = inventoryItem.QuantityOnHandTotal ?? 0m;
                        if (onHand < quantityToIssue && request.IssueParams.FailIfNotEnoughQoh)
                        {
                            var product = await _context.Products
                                .Where(p => p.ProductId == inventoryItem.ProductId)
                                .Select(p => new { p.ProductName })
                                .FirstOrDefaultAsync(cancellationToken);

                            result.InsufficientItems.Add(new InsufficientItem
                            {
                                ProductName = product?.ProductName ?? $"Unknown Product (ID: {inventoryItem.ProductId})",
                                QuantityMissing = quantityToIssue - onHand,
                                InventoryItemId = inventoryItem.InventoryItemId
                            });
                            continue;
                        }

                        // Create WorkEffortInventoryAssign record
                        var assignMap = new WorkEffortInventoryAssign
                        {
                            WorkEffortId = line.WorkEffortId,
                            InventoryItemId = line.InventoryItemId,
                            Quantity = (double)quantityToIssue,
                            CreatedStamp = DateTime.UtcNow,
                            LastUpdatedStamp = DateTime.UtcNow
                        };
                        await _productionRunService.AssignInventoryToWorkEffort(assignMap);

                        // Create InventoryItemDetail to reduce QOH
                        var detailParam = new CreateInventoryItemDetailParam
                        {
                            InventoryItemId = inventoryItem.InventoryItemId,
                            WorkEffortId = request.IssueParams.WorkEffortId,
                            QuantityOnHandDiff = -quantityToIssue,
                            AvailableToPromiseDiff = 0m, // ATP was reduced during reservation
                            ReasonEnumId = request.IssueParams.ReasonEnumId,
                            Description = request.IssueParams.Description ?? "Issuance for reserved BOM component"
                        };
                        await _inventoryService.CreateInventoryItemDetail(detailParam);

                        // Balance inventory after issuance
                        await _inventoryService.BalanceInventoryItems(inventoryItem.InventoryItemId, inventoryItem.FacilityId);

                        // Remove reservation line
                        _context.WorkEffortInventoryRes.Remove(line);

                        // REFACTOR: Accumulate issued quantity for this product
                        // Purpose: Track issued quantities for BOM status updates
                        // Benefit: Ensures accurate completion checks
                        var productId = line.InventoryItem?.ProductId;
                        if (issuedQuantities.TryGetValue(productId, out var currentIssued))
                        {
                            issuedQuantities[productId] = currentIssued + quantityToIssue;
                        }
                        else
                        {
                            issuedQuantities[productId] = quantityToIssue;
                        }
                    }

                    // REFACTOR: Update BOM statuses only if components exist
                    // Purpose: Set WEGS_COMPLETED only for initial BOM issuances
                    // Benefit: Avoids invalid updates for additional materials
                    if (!isAdditionalMaterials)
                    {
                        foreach (var bomComponent in bomComponents)
                        {
                            if (issuedQuantities.TryGetValue(bomComponent.ProductId, out var totalIssued) &&
                                bomComponent.EstimatedQuantity <= (double?)totalIssued)
                            {
                                bomComponent.StatusId = "WEGS_COMPLETED"; // Entity is tracked
                            }
                        }
                    }

                    if (result.InsufficientItems.Any())
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        var errorMessage = "Insufficient inventory for the following items: " +
                                           string.Join(", ", result.InsufficientItems
                                               .Select(i => $"Product: {i.ProductName}, InventoryItem: {i.InventoryItemId}, Missing: {i.QuantityMissing}"));
                        return Result<IssueProductionRunReservationsResult>.Failure(errorMessage);
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    result.Success = true;
                    result.Message = "Reservations issued successfully.";
                    _logger.LogInformation($"Successfully issued reservations for WorkEffort {request.IssueParams.WorkEffortId}.");
                    return Result<IssueProductionRunReservationsResult>.Success(result);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError($"Error issuing reservations for WorkEffort {request.IssueParams.WorkEffortId}: {ex.Message}");
                    return Result<IssueProductionRunReservationsResult>.Failure($"Error issuing reservations: {ex.Message}");
                }
            }
        }
    }
}