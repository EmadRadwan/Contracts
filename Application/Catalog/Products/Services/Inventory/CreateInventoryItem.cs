using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.Products.Services.Inventory;

public class CreateInventoryItem
{
    // REFACTOR: Defined a DTO to match the React form fields, ensuring type safety
    // and a clear contract between the frontend and backend.
    public class CreateInventoryItemDto
    {
        public string ProductId { get; set; } = null!;
        public string? LotId { get; set; }
        public string? ContainerId { get; set; }
        public string FacilityId { get; set; } = null!;
        public string? LocationSeqId { get; set; } = null!;
        public string? CurrencyUomId { get; set; }
        public decimal? UnitCost { get; set; }
        public DateTime? DatetimeReceived { get; set; }
        public DateTime? ExpireDate { get; set; }
        public string? ColorFeatureId { get; set; }
        public string? SizeFeatureId { get; set; }
    }

    // REFACTOR: Created a MediatR command to encapsulate the request, following the
    // CQRS pattern used in CreateProduct for consistency.
    public class Command : IRequest<Result<string>>
    {
        public CreateInventoryItemDto? InventoryItemDto { get; set; }
    }

    // REFACTOR: Added FluentValidation to enforce required fields, aligning with the
    // React form’s requiredValidator and ensuring data integrity.
    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.InventoryItemDto!.ProductId).NotEmpty().WithMessage("Product ID is required");
            RuleFor(x => x.InventoryItemDto!.FacilityId).NotEmpty().WithMessage("Facility ID is required");
        }
    }

    // REFACTOR: Implemented a MediatR handler to process the command, orchestrate the
    // service call, and manage transactions, mirroring CreateProduct’s structure.
    public class Handler : IRequestHandler<Command, Result<string>>
    {
        private readonly DataContext _context;
        private readonly IInventoryService _inventoryService;
        private readonly IUserAccessor _userAccessor;

        // REFACTOR: Injected dependencies (DataContext, InventoryService, IUserAccessor)
        // via constructor to enable DI, testability, and decoupling.
        public Handler(DataContext context, IInventoryService inventoryService, IUserAccessor userAccessor)
        {
            _context = context;
            _inventoryService = inventoryService;
            _userAccessor = userAccessor;
        }

        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Validated user existence, consistent with CreateProduct, to ensure
            // only authenticated users can create inventory items.
            var user = await _context.Users.FirstOrDefaultAsync(
                x => x.UserName == _userAccessor.GetUsername(), cancellationToken);
            if (user == null)
            {
                return Result<string>.Failure("User not found");
            }

            // REFACTOR: Used transaction to ensure data consistency, as the InventoryService
            // may modify multiple entities (e.g., InventoryItem, Lot).
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // REFACTOR: Mapped DTO to CreateInventoryItemParam, setting only form-provided
                // fields to avoid overpopulating unused properties.
                var param = new CreateInventoryItemParam
                {
                    ProductId = request.InventoryItemDto!.ProductId,
                    LotId = request.InventoryItemDto.LotId,
                    FacilityId = request.InventoryItemDto.FacilityId,
                    LocationSeqId = request.InventoryItemDto.LocationSeqId,
                    CurrencyUomId = request.InventoryItemDto.CurrencyUomId,
                    UnitCost = request.InventoryItemDto.UnitCost,
                    DateTimeReceived = request.InventoryItemDto.DatetimeReceived,
                    ContainerId = request.InventoryItemDto.ContainerId,
                    IsReturned = "N" // Default to "N" to match Groovy/OFBiz behavior
                };

                // REFACTOR: Called InventoryService’s CreateInventoryItem, reusing existing
                // business logic and keeping the handler focused on orchestration.
                var inventoryItemId = await _inventoryService.CreateInventoryItem(param);

                if (!string.IsNullOrEmpty(request.InventoryItemDto!.ColorFeatureId))
                {
                    var colorFeature = await _context.ProductFeatures
                        .FirstOrDefaultAsync(x => x.ProductFeatureId == request.InventoryItemDto.ColorFeatureId,
                            cancellationToken);
                    if (colorFeature != null)
                    {
                        var feature = new InventoryItemFeature
                        {
                            InventoryItemId = inventoryItemId,
                            ProductFeatureId = request.InventoryItemDto.ColorFeatureId,
                            ProductId = request.InventoryItemDto.ProductId,
                        };
                        _context.InventoryItemFeatures.Add(feature);
                    }
                    else
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<string>.Failure(
                            $"Invalid Color Feature ID: {request.InventoryItemDto.ColorFeatureId}");
                    }
                }

                if (!string.IsNullOrEmpty(request.InventoryItemDto.SizeFeatureId))
                {
                    var sizeFeature = await _context.ProductFeatures
                        .FirstOrDefaultAsync(x => x.ProductFeatureId == request.InventoryItemDto.SizeFeatureId,
                            cancellationToken);
                    if (sizeFeature != null)
                    {
                        var feature = new InventoryItemFeature
                        {
                            InventoryItemId = inventoryItemId,
                            ProductFeatureId = request.InventoryItemDto.SizeFeatureId,
                            ProductId = request.InventoryItemDto.ProductId,
                        };
                        _context.InventoryItemFeatures.Add(feature);
                    }
                    else
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return Result<string>.Failure(
                            $"Invalid Size Feature ID: {request.InventoryItemDto.SizeFeatureId}");
                    }
                }

                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<string>.Failure($"Failed to create inventory item");
                }

                await transaction.CommitAsync(cancellationToken);

                return Result<string>.Success(inventoryItemId);
            }
            catch (Exception ex)
            {
                // REFACTOR: Rolled back transaction on error and returned a failure result
                // with the error message, consistent with CreateProduct’s error handling.
                await transaction.RollbackAsync(cancellationToken);
                return Result<string>.Failure($"Failed to create inventory item: {ex.Message}");
            }
        }
    }
}