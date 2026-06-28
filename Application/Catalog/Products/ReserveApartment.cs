using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class ReserveApartment
{
    public class Command : IRequest<Result<ProductDto2>>
    {
        public string ProductId { get; set; }
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
            // 1. User validation
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername(), cancellationToken);

            if (user == null)
                return Result<ProductDto2>.Failure("User not found");

            // 2. Load apartment and verify it exists
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == request.ProductId, cancellationToken);

            if (product == null)
            {
                _logger.LogWarning("ReserveApartment failed: Product {ProductId} not found.", request.ProductId);
                return Result<ProductDto2>.Failure($"Product with ID {request.ProductId} not found.");
            }

            // 3. Guard: must be an apartment with AVAILABLE status
            if (product.ProductTypeId != "APARTMENT")
                return Result<ProductDto2>.Failure("Product is not of type APARTMENT.");

            if (product.ApartmentStatusId != "APARTMENT_AVAILABLE")
            {
                _logger.LogWarning(
                    "ReserveApartment failed: Product {ProductId} has status {Status}, expected APARTMENT_AVAILABLE.",
                    request.ProductId, product.ApartmentStatusId);
                return Result<ProductDto2>.Failure(
                    $"Apartment cannot be reserved — current status is '{product.ApartmentStatusId}'.");
            }

            // 4. Flip status and persist
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                product.ApartmentStatusId = "APARTMENT_RESERVED";
                product.LastUpdatedStamp = DateTime.UtcNow;

                var saved = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogWarning("ReserveApartment: SaveChanges wrote nothing for product {ProductId}.", request.ProductId);
                    return Result<ProductDto2>.Failure("Failed to reserve apartment.");
                }

                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation("Apartment {ProductId} reserved successfully.", request.ProductId);

                // 5. Return enriched DTO (mirrors UpdateProduct projection)
                var productToReturn = await (
                        from p in _context.Products
                        join pt in _context.ProductTypes on p.ProductTypeId equals pt.ProductTypeId
                        join pc in _context.ProductCategories
                            on p.PrimaryProductCategoryId equals pc.ProductCategoryId into pcg
                        from pc in pcg.DefaultIfEmpty()
                        where p.ProductId == request.ProductId
                        select new ProductDto2
                        {
                            ProductId          = p.ProductId,
                            ProductName        = p.ProductName,
                            ProductTypeId      = p.ProductTypeId,
                            Comments           = p.Comments,
                            ProductTypeDescription          = pt.Description,
                            PrimaryProductCategoryId        = p.PrimaryProductCategoryId,
                            PrimaryProductCategoryDescription = pc != null ? pc.Description : null,
                            ProjectId              = p.ProjectId,
                            FloorNumber            = p.FloorNumber,
                            ApartmentSpaceM2       = p.ApartmentSpaceM2,
                            GardenSpaceM2          = p.GardenSpaceM2,
                            ApartmentPricePerM2    = p.ApartmentPricePerM2,
                            GardenPricePerM2       = p.GardenPricePerM2,
                            ApartmentStatusId      = p.ApartmentStatusId,
                            BuildingNumber         = p.BuildingNumber,
                        })
                    .SingleOrDefaultAsync(cancellationToken);

                return productToReturn != null
                    ? Result<ProductDto2>.Success(productToReturn)
                    : Result<ProductDto2>.Failure("Failed to retrieve updated apartment.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "ReserveApartment threw for product {ProductId}", request.ProductId);
                return Result<ProductDto2>.Failure($"Failed to reserve apartment: {ex.Message}");
            }
        }
    }
}