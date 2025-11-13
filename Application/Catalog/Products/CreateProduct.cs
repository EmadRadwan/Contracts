using Application.Interfaces;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.Products;

public class CreateProduct
{
    public class Command : IRequest<Result<ProductDto2>>
    {
        public ProductDto2? ProductDto2 { get; set; }
    }


    public class Handler : IRequestHandler<Command, Result<ProductDto2>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor, IMapper mapper)
        {
            _userAccessor = userAccessor;
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<ProductDto2>> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.ProductDto2!; // guaranteed non-null by validator

            // --------------------------------------------------------------------
            // 1. User validation
            // --------------------------------------------------------------------
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername(), cancellationToken);
            if (user == null) return Result<ProductDto2>.Failure("User not found");

            // --------------------------------------------------------------------
            // 2. Duplicate ProductId check
            // --------------------------------------------------------------------
            if (await _context.Products.AnyAsync(p => p.ProductId == dto.ProductId, cancellationToken))
                return Result<ProductDto2>.Failure("Product ID already exists");

            var now = DateTime.UtcNow; // REFACTOR: Use UTC for consistency across servers


            // --------------------------------------------------------------------
            // 4. Transaction scope
            // --------------------------------------------------------------------
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                dto.PrimaryProductCategoryId =
                    dto.ProductTypeId == "APARTMENT" ? "APARTMENTS" : dto.PrimaryProductCategoryId;

                // ----------------------------------------------------------------
                // 5. Core Product entity
                // ----------------------------------------------------------------
                var product = new Product
                {
                    ProductId = dto.ProductId,
                    ProductName = dto.ProductName,
                    ProductTypeId = dto.ProductTypeId,
                    QuantityUomId = dto.QuantityUomId,
                    Comments = dto.Comments,
                    PrimaryProductCategoryId = dto.PrimaryProductCategoryId,
                    ProjectId = !string.IsNullOrWhiteSpace(dto.ProjectId)
                        ? dto.ProjectId
                        : null,
                    FloorNumber = dto.FloorNumber, // NEW
                    ApartmentSpaceM2 = dto.ApartmentSpaceM2, // NEW
                    GardenSpaceM2 = dto.GardenSpaceM2, // NEW
                    ApartmentPricePerM2 = dto.ApartmentPricePerM2, // NEW
                    GardenPricePerM2 = dto.GardenPricePerM2, // NEW
                    ApartmentStatusId = !string.IsNullOrWhiteSpace(dto.ApartmentStatusId)
                        ? dto.ApartmentStatusId
                        : null,
                    LandNumber = dto.LandNumber, // NEW
                    CreatedDate = now,
                    LastUpdatedStamp = now
                };
                _context.Products.Add(product);

                // ----------------------------------------------------------------
                // 7. Persist
                // ----------------------------------------------------------------
                var saved = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!saved)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<ProductDto2>.Failure("Failed to create product");
                }

                await transaction.CommitAsync(cancellationToken);

                // ----------------------------------------------------------------
                // 8. Return enriched DTO (join descriptions)
                // ----------------------------------------------------------------
                // REFACTOR: Single query with required joins; avoids N+1 and simplifies mapping
                var productToReturn = await (
                        from p in _context.Products
                        join pt in _context.ProductTypes
                            on p.ProductTypeId equals pt.ProductTypeId
                        join pc in _context.ProductCategories
                            on p.PrimaryProductCategoryId equals pc.ProductCategoryId
                            into pcGroup
                        from pc in pcGroup.DefaultIfEmpty() // <-- LEFT JOIN
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
                            LandNumber = p.LandNumber,
                        })
                    .SingleOrDefaultAsync(cancellationToken);

                return productToReturn != null
                    ? Result<ProductDto2>.Success(productToReturn)
                    : Result<ProductDto2>.Failure("Failed to retrieve created product");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<ProductDto2>.Failure($"Failed to create product: {ex.Message}");
            }
        }
    }
}