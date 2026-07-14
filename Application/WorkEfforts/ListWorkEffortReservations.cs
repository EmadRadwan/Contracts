using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.WorkEfforts;

public class ReservationItem
{
    public string WorkEffortInvResId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string InventoryItemId { get; set; } = null!;
    public string ColorDescription { get; set; } = null!;
    public decimal Quantity { get; set; }
}

public class ListWorkEffortReservations
{
    public class Query : IRequest<Result<List<ReservationItem>>>
    {
        public string WorkEffortId { get; set; } = null!;
        public string Language { get; set; } = "en";
    }

    public class Handler : IRequestHandler<Query, Result<List<ReservationItem>>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<List<ReservationItem>>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                // REFACTOR: Validate WorkEffortId exists
                // Purpose: Ensure valid WorkEffortId to prevent unnecessary queries
                // Benefit: Reduces database load and improves error handling
                if (string.IsNullOrEmpty(request.WorkEffortId))
                {
                    _logger.LogWarning("WorkEffortId is null or empty");
                    return Result<List<ReservationItem>>.Failure("WorkEffortId cannot be null or empty");
                }

                var workEffortExists = await _context.WorkEfforts
                    .AnyAsync(we => we.WorkEffortId == request.WorkEffortId, cancellationToken);
                if (!workEffortExists)
                {
                    _logger.LogWarning("WorkEffort not found for WorkEffortId: {WorkEffortId}", request.WorkEffortId);
                    return Result<List<ReservationItem>>.Failure("WorkEffort not found");
                }

                // REFACTOR: Log intermediate steps
                // Purpose: Debug query to identify where results drop
                // Benefit: Pinpoints issues in joins or filtering
                var step1 = await _context.WorkEffortInventoryRes
                    .Where(r => r.WorkEffortId == request.WorkEffortId && r.Quantity > 0 && r.InventoryItemId != null)
                    .ToListAsync(cancellationToken);
                _logger.LogInformation("Step 1: WorkEffortInventoryRes count: {Count}", step1.Count);

                // REFACTOR: Remove ProductFeatureAppls join, use ProductFeatures directly
                // Purpose: Fetch ColorDescription from ProductFeatures via InventoryItemFeatures
                // Benefit: Correctly retrieves color data without assuming ProductFeatureAppls relationship
                var reservations = await _context.WorkEffortInventoryRes
                    .Where(r => r.WorkEffortId == request.WorkEffortId && r.Quantity > 0 && r.InventoryItemId != null)
                    .Join(_context.InventoryItems,
                        r => r.InventoryItemId,
                        ii => ii.InventoryItemId,
                        (r, ii) => new { r.WorkEffortInvResId, Quantity = r.Quantity ?? 0m, ii.ProductId, ii.InventoryItemId })
                    .GroupJoin(_context.InventoryItemFeatures,
                        x => x.InventoryItemId,
                        iif => iif.InventoryItemId,
                        (x, iifs) => new { x.WorkEffortInvResId, x.Quantity, x.ProductId, x.InventoryItemId, InventoryItemFeatures = iifs })
                    .SelectMany(
                        x => x.InventoryItemFeatures.DefaultIfEmpty(),
                        (x, iif) => new { x.WorkEffortInvResId, x.Quantity, x.ProductId, x.InventoryItemId, ProductFeatureId = iif != null ? iif.ProductFeatureId : null })
                    .Join(_context.Products,
                        x => x.ProductId,
                        p => p.ProductId,
                        (x, p) => new { x.WorkEffortInvResId, x.Quantity, x.ProductId, x.InventoryItemId, x.ProductFeatureId, ProductName = p.ProductName ?? "No Name" })
                    .GroupJoin(_context.ProductFeatures,
                        x => x.ProductFeatureId,
                        pf => pf.ProductFeatureId,
                        (x, pfs) => new { x.WorkEffortInvResId, x.Quantity, x.ProductId, x.InventoryItemId, x.ProductFeatureId, x.ProductName, ProductFeatures = pfs })
                    .SelectMany(
                        x => x.ProductFeatures.DefaultIfEmpty(),
                        (x, pf) => new ReservationItem
                        {
                            WorkEffortInvResId = x.WorkEffortInvResId,
                            ProductId = x.ProductId,
                            ProductName = x.ProductName,
                            InventoryItemId = x.InventoryItemId,
                            ColorDescription = pf != null ? (request.Language == "ar" ? pf.DescriptionArabic ?? pf.Description : pf.Description) ?? "No Color" : "No Color",
                            Quantity = x.Quantity
                        })
                    .ToListAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully retrieved {Count} reservation items for WorkEffortId: {WorkEffortId}",
                    reservations.Count, request.WorkEffortId);

                return Result<List<ReservationItem>>.Success(reservations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reservation items for WorkEffortId: {WorkEffortId}", request.WorkEffortId);
                return Result<List<ReservationItem>>.Failure($"Error retrieving reservation items: {ex.Message}");
            }
        }
    }
}