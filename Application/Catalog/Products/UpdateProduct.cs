using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class UpdateProduct
{
    public class Command : IRequest<Result<ProductDto2>>
    {
        public ProductDto2 ProductDto2 { get; set; }
    }
    

    public class Handler : IRequestHandler<Command, Result<ProductDto2>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<ProductDto2>> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.ProductDto2; // guaranteed non-null by validator

            // --------------------------------------------------------------------
            // 1. User validation
            // --------------------------------------------------------------------
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername(), cancellationToken);
            if (user == null)
                return Result<ProductDto2>.Failure("User not found");

            // --------------------------------------------------------------------
            // 2. Load existing product
            // --------------------------------------------------------------------
            var product = await _context.Products
                .Include(p => p.ProductType)
                .Include(p => p.PrimaryProductCategory)
                .FirstOrDefaultAsync(p => p.ProductId == dto.ProductId, cancellationToken);

            if (product == null)
            {
                _logger.LogWarning("Update failed: Product with ID {ProductId} not found.", dto.ProductId);
                return Result<ProductDto2>.Failure($"Product with ID {dto.ProductId} not found.");
            }

            // --------------------------------------------------------------------
            // 3. Detect type change: non-Apartment → APARTMENT → null out apartment fields
            // --------------------------------------------------------------------
            // REFACTOR: Clear apartment-specific fields when switching TO APARTMENT
            // Purpose: Prevent stale data from previous non-apartment product type
            // Why: Ensures clean state like CreateProduct; avoids invalid combinations
            bool isSwitchingToApartment = dto.ProductTypeId == "APARTMENT" && product.ProductTypeId != "APARTMENT";

            if (isSwitchingToApartment)
            {
                // List of fields that are ONLY valid for APARTMENT type
                var apartmentOnlyFields = new[]
                {
                    nameof(Product.ProjectId),
                    nameof(Product.FloorNumber),
                    nameof(Product.ApartmentSpaceM2),
                    nameof(Product.GardenSpaceM2),
                    nameof(Product.ApartmentPricePerM2),
                    nameof(Product.GardenPricePerM2),
                    nameof(Product.ApartmentStatusId),
                    nameof(Product.BuildingNumber)
                };

                var productType = typeof(Product);
                foreach (var field in apartmentOnlyFields)
                {
                    var prop = productType.GetProperty(field);
                    if (prop != null)
                        prop.SetValue(product, null);
                }

                _logger.LogInformation(
                    "Product {ProductId} changed from {OldType} to APARTMENT. Cleared {Count} apartment-only fields.",
                    dto.ProductId, product.ProductTypeId, apartmentOnlyFields.Length);
            }

            // --------------------------------------------------------------------
            // 4. Transaction scope
            // --------------------------------------------------------------------
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var now = DateTime.UtcNow; // REFACTOR: Use UTC for consistency across servers

                // ----------------------------------------------------------------
                // 5. Update core product fields
                // ----------------------------------------------------------------
                product.ProductName = dto.ProductName ?? product.ProductName;
                product.ProductTypeId = dto.ProductTypeId ?? product.ProductTypeId;
                product.QuantityUomId = dto.QuantityUomId ?? product.QuantityUomId;
                product.Comments = dto.Comments;
                product.PrimaryProductCategoryId = dto.PrimaryProductCategoryId ?? product.PrimaryProductCategoryId;

                // ----------------------------------------------------------------
                // 6. Update apartment-specific fields ONLY if type is APARTMENT
                // ----------------------------------------------------------------
                // REFACTOR: Conditional assignment – only apply when ProductTypeId == "APARTMENT"
                // Why: Prevents invalid data when type is not APARTMENT
                if (dto.ProductTypeId == "APARTMENT")
                {
                    product.ProjectId = !string.IsNullOrWhiteSpace(dto.ProjectId) ? dto.ProjectId : null;
                    product.FloorNumber = dto.FloorNumber;
                    product.ApartmentSpaceM2 = dto.ApartmentSpaceM2;
                    product.GardenSpaceM2 = dto.GardenSpaceM2;
                    product.ApartmentPricePerM2 = dto.ApartmentPricePerM2;
                    product.GardenPricePerM2 = dto.GardenPricePerM2;
                    product.ApartmentStatusId = !string.IsNullOrWhiteSpace(dto.ApartmentStatusId) ? dto.ApartmentStatusId : null;
                    product.BuildingNumber = dto.BuildingNumber;
                }

                product.LastUpdatedStamp = now;

                
                // ----------------------------------------------------------------
                // 9. Persist changes
                // ----------------------------------------------------------------
                var saved = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogWarning("Failed to save updates for product {ProductId}.", dto.ProductId);
                    return Result<ProductDto2>.Failure("Failed to update product");
                }

                await transaction.CommitAsync(cancellationToken);

                // ----------------------------------------------------------------
                // 10. Return enriched DTO (single query with joins)
                // ----------------------------------------------------------------
                // REFACTOR: Use identical projection as CreateProduct
                // Why: Avoid N+1, ensure consistency in returned data
                var productToReturn = await (
                        from p in _context.Products
                        join pt in _context.ProductTypes on p.ProductTypeId equals pt.ProductTypeId
                        join pc in _context.ProductCategories
                            on p.PrimaryProductCategoryId equals pc.ProductCategoryId into pcg
                        from pc in pcg.DefaultIfEmpty()
                        where p.ProductId == dto.ProductId
                        select new ProductDto2
                        {
                            ProductId = p.ProductId,
                            ProductName = p.ProductName,
                            ProductTypeId = p.ProductTypeId,
                            Comments = p.Comments,
                            ProductTypeDescription = pt.Description,
                            PrimaryProductCategoryId = p.PrimaryProductCategoryId,
                            PrimaryProductCategoryDescription = pc != null ? pc.Description : null,
                            ProjectId = p.ProjectId,
                            FloorNumber = p.FloorNumber,
                            ApartmentSpaceM2 = p.ApartmentSpaceM2,
                            GardenSpaceM2 = p.GardenSpaceM2,
                            ApartmentPricePerM2 = p.ApartmentPricePerM2,
                            GardenPricePerM2 = p.GardenPricePerM2,
                            ApartmentStatusId = p.ApartmentStatusId,
                            BuildingNumber = p.BuildingNumber,
                        })
                    .SingleOrDefaultAsync(cancellationToken);

                return productToReturn != null
                    ? Result<ProductDto2>.Success(productToReturn)
                    : Result<ProductDto2>.Failure("Failed to retrieve updated product");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Update failed for product {ProductId}", dto.ProductId);
                return Result<ProductDto2>.Failure($"Failed to update product: {ex.Message}");
            }
        }
    }
}