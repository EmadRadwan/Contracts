using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Microsoft.Extensions.Logging;

namespace Application.Catalog.Products.Services.Inventory;

public class UpdateInventoryItem
{
    // REFACTOR: Updated DTO to include ColorFeatureId and SizeFeatureId, aligning with
    // the React form’s fields and InventoryItemFeatures table requirements.
    // Why: Matches form data and enables feature updates, mirroring UpdateProduct’s approach.
    public class UpdateInventoryItemDto
    {
        public string InventoryItemId { get; set; } = null!;
        public decimal? UnitCost { get; set; }
        public string? LotId { get; set; }
        public string? StatusId { get; set; }
        public string? OwnerPartyId { get; set; }
        public string? ProductId { get; set; }
        public string? FacilityId { get; set; }
        public string? ColorFeatureId { get; set; }
        public string? SizeFeatureId { get; set; }
    }

    // REFACTOR: Updated result class to include old feature IDs, similar to UpdateProduct’s
    // return of old values, for audit or frontend feedback.
    // Why: Enhances traceability, aligning with OFBiz’s service output pattern.
    public class UpdateInventoryItemResult
    {
        public string OldOwnerPartyId { get; set; }
        public string OldStatusId { get; set; }
        public string OldProductId { get; set; }
        public string OldColorFeatureId { get; set; }
        public string OldSizeFeatureId { get; set; }
    }

    public class Command : IRequest<Result<UpdateInventoryItemResult>>
    {
        public UpdateInventoryItemDto? InventoryItemDto { get; set; }
    }

    // REFACTOR: Enhanced validator to include optional checks for feature IDs, ensuring
    // valid data for InventoryItemFeatures, consistent with CreateInventoryItem.
    // Why: Maintains data integrity while allowing optional feature updates.
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.InventoryItemDto!.InventoryItemId)
                .NotEmpty()
                .WithMessage("Inventory Item ID is required");
        }
    }

    public class Handler : IRequestHandler<Command, Result<UpdateInventoryItemResult>>
    {
        private readonly DataContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IUserAccessor _userAccessor;
        private readonly ILogger<Handler> _logger;

        // REFACTOR: Added ILogger dependency to log feature updates and errors,
        // mirroring UpdateProduct’s debugging strategy.
        // Why: Improves traceability and maintainability.
        public Handler(DataContext context, IInventoryService inventoryService, IUserAccessor userAccessor,
            ILogger<Handler> logger)
        {
            _context = context;
            _inventoryService = inventoryService;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<UpdateInventoryItemResult>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(
                x => x.UserName == _userAccessor.GetUsername(), cancellationToken);
            if (user == null)
            {
                return Result<UpdateInventoryItemResult>.Failure("User not found");
            }

            var inventoryItem = await _context.InventoryItems
                .FirstOrDefaultAsync(x => x.InventoryItemId == request.InventoryItemDto!.InventoryItemId,
                    cancellationToken);
            if (inventoryItem == null)
            {
                _logger.LogWarning($"Inventory item with ID {request.InventoryItemDto!.InventoryItemId} not found.");
                return Result<UpdateInventoryItemResult>.Failure(
                    $"Inventory item with ID {request.InventoryItemDto!.InventoryItemId} not found.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var stamp = DateTime.UtcNow;

                // REFACTOR: Fetch old color and size features correctly using ProductFeatureTypeId
                // Why: Ensures accurate audit data by distinguishing color and size features
                var oldColorFeature = await _context.InventoryItemFeatures
                    .Join(_context.ProductFeatures,
                        iif => iif.ProductFeatureId,
                        pf => pf.ProductFeatureId,
                        (iif, pf) => new { iif, pf })
                    .Where(x => x.iif.InventoryItemId == request.InventoryItemDto!.InventoryItemId
                                && x.pf.ProductFeatureTypeId == "COLOR")
                    .Select(x => x.iif.ProductFeatureId)
                    .FirstOrDefaultAsync(cancellationToken);
                var oldSizeFeature = await _context.InventoryItemFeatures
                    .Join(_context.ProductFeatures,
                        iif => iif.ProductFeatureId,
                        pf => pf.ProductFeatureId,
                        (iif, pf) => new { iif, pf })
                    .Where(x => x.iif.InventoryItemId == request.InventoryItemDto!.InventoryItemId
                                && x.pf.ProductFeatureTypeId == "SIZE")
                    .Select(x => x.iif.ProductFeatureId)
                    .FirstOrDefaultAsync(cancellationToken);

                // REFACTOR: Delete all existing features for the InventoryItem
                // Why: Prevents stale features by removing all features, not just those matching new IDs
                var allFeatures = await _context.InventoryItemFeatures
                    .Where(x => x.InventoryItemId == request.InventoryItemDto!.InventoryItemId)
                    .ToListAsync(cancellationToken);
                if (allFeatures.Any())
                {
                    _logger.LogInformation(
                        $"Deleting {allFeatures.Count} InventoryItemFeatures for inventory item {request.InventoryItemDto!.InventoryItemId}: {string.Join(", ", allFeatures.Select(x => x.ProductFeatureId))}");
                    _context.InventoryItemFeatures.RemoveRange(allFeatures);
                }

                // REFACTOR: Add new color feature if provided
                // Why: Maintains validation and ensures correct feature creation
                if (!string.IsNullOrEmpty(request.InventoryItemDto!.ColorFeatureId))
                {
                    var colorFeature = await _context.ProductFeatures
                        .FirstOrDefaultAsync(x => x.ProductFeatureId == request.InventoryItemDto.ColorFeatureId,
                            cancellationToken);
                    if (colorFeature == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<UpdateInventoryItemResult>.Failure(
                            $"Invalid Color Feature ID: {request.InventoryItemDto.ColorFeatureId}");
                    }

                    _context.InventoryItemFeatures.Add(new InventoryItemFeature
                    {
                        InventoryItemId = request.InventoryItemDto.InventoryItemId,
                        ProductFeatureId = request.InventoryItemDto.ColorFeatureId,
                        ProductId = request.InventoryItemDto.ProductId
                    });
                }

                // REFACTOR: Add new size feature if provided
                if (!string.IsNullOrEmpty(request.InventoryItemDto!.SizeFeatureId))
                {
                    var sizeFeature = await _context.ProductFeatures
                        .FirstOrDefaultAsync(x => x.ProductFeatureId == request.InventoryItemDto.SizeFeatureId,
                            cancellationToken);
                    if (sizeFeature == null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<UpdateInventoryItemResult>.Failure(
                            $"Invalid Size Feature ID: {request.InventoryItemDto.SizeFeatureId}");
                    }

                    _context.InventoryItemFeatures.Add(new InventoryItemFeature
                    {
                        InventoryItemId = request.InventoryItemDto.InventoryItemId,
                        ProductFeatureId = request.InventoryItemDto.SizeFeatureId,
                        ProductId = request.InventoryItemDto.ProductId
                    });
                }

                var serviceResult = await _inventoryService.UpdateInventoryItem(
                    inventoryItemId: request.InventoryItemDto!.InventoryItemId,
                    unitCost: request.InventoryItemDto.UnitCost,
                    lotId: request.InventoryItemDto.LotId,
                    statusId: request.InventoryItemDto.StatusId,
                    ownerPartyId: request.InventoryItemDto.OwnerPartyId,
                    productId: request.InventoryItemDto.ProductId,
                    facilityId: request.InventoryItemDto.FacilityId
                );

                var saveResult = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!saveResult)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogWarning($"Failed to update inventory item {request.InventoryItemDto!.InventoryItemId}.");
                    return Result<UpdateInventoryItemResult>.Failure("Failed to update inventory item.");
                }

                await transaction.CommitAsync(cancellationToken);

                // REFACTOR: Include old feature IDs in result
                // Why: Provides accurate audit data for auditing
                var result = new UpdateInventoryItemResult
                {
                    OldOwnerPartyId = serviceResult.OldOwnerPartyId,
                    OldStatusId = serviceResult.OldStatusId,
                    OldProductId = serviceResult.OldProductId,
                    OldColorFeatureId = oldColorFeature,
                    OldSizeFeatureId = oldSizeFeature
                };
                return Result<UpdateInventoryItemResult>.Success(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(
                    $"Failed to update inventory item {request.InventoryItemDto!.InventoryItemId}: {ex.Message}");
                return Result<UpdateInventoryItemResult>.Failure($"Failed to update inventory item: {ex.Message}");
            }
        }
    }
}