using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Manufacturing;

public class GetBomInventoryItems
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
                // REFACTOR: Validate WorkEffortId exists before querying.
                // Business: Ensures valid WorkEffortId to prevent unnecessary queries.
                // Technical: Reduces database load and improves error handling.
                var workEffortExists = await _context.WorkEfforts
                    .AnyAsync(we => we.WorkEffortId == request.WorkEffortId, cancellationToken);
                if (!workEffortExists)
                {
                    _logger.LogWarning("WorkEffort not found for WorkEffortId: {WorkEffortId}", request.WorkEffortId);
                    return Result<List<BomInventoryItemDto>>.Failure("WorkEffort not found");
                }

                // REFACTOR: Debug intermediate steps.
                // Purpose: Log record counts to identify where results drop to 0.
                // Benefit: Pinpoints query failure point.
                var step1 = await _context.WorkEffortGoodStandards
                    .Where(w => w.WorkEffortId == request.WorkEffortId && w.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED")
                    .ToListAsync(cancellationToken);
                _logger.LogInformation("Step 1: WorkEffortGoodStandards count: {Count}", step1.Count);

                var step2 = await _context.WorkEffortGoodStandards
                    .Where(w => w.WorkEffortId == request.WorkEffortId && w.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED")
                    .Join(_context.InventoryItems,
                        w => w.ProductId,
                        i => i.ProductId,
                        (w, i) => new { w.ProductId, w.EstimatedQuantity, i.InventoryItemId, i.AvailableToPromiseTotal })
                    .ToListAsync(cancellationToken);
                _logger.LogInformation("Step 2: After InventoryItems join count: {Count}", step2.Count);

                var components = await _context.WorkEffortGoodStandards
                    .Where(w => w.WorkEffortId == request.WorkEffortId && w.WorkEffortGoodStdTypeId == "PRUNT_PROD_NEEDED")
                    .Join(_context.InventoryItems,
                        w => w.ProductId,
                        i => i.ProductId,
                        (w, i) => new { w.ProductId, w.EstimatedQuantity, i.InventoryItemId, i.AvailableToPromiseTotal })
                    // REFACTOR: Join with Products table to include ProductName.
                    // Business: Provides meaningful product names for BOM components in the UI.
                    // Technical: Adds ProductName without Arabic localization.
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
                            // REFACTOR: Select ProductName directly.
                            // Business: Returns ProductName without localization, as ProductNameArabic is no longer used.
                            // Technical: Simplifies query by removing language-based selection for ProductName.
                            ProductName = x.ProductName ?? "No Name"
                        })
                    .Where(x => x.AvailableToPromiseTotal > 0)
                    .ToListAsync(cancellationToken);

                // REFACTOR: Log successful retrieval.
                // Business: Confirms successful fetch of BOM components.
                // Technical: Adds traceability for debugging and monitoring.
                _logger.LogInformation(
                    "Successfully retrieved {Count} BOM inventory items for WorkEffortId: {WorkEffortId}",
                    components.Count, request.WorkEffortId);

                return Result<List<BomInventoryItemDto>>.Success(components);
            }
            catch (Exception ex)
            {
                // REFACTOR: Handle and log exceptions.
                // Business: Ensures robust error handling for data retrieval.
                // Technical: Provides clear error messages for debugging.
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
        // REFACTOR: Add ProductName property to DTO.
        // Business: Allows frontend to display human-readable product names.
        // Technical: Stores ProductName from Products table without localization.
        public string ProductName { get; set; }
    }
}