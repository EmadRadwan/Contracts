using Application.Interfaces;
using AutoMapper;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.Products;

public class CreateProduct
{
    public class Command : IRequest<Result<ProductDto>>
    {
        public ProductDto? ProductDto { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.ProductDto).SetValidator(new ProductValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<ProductDto>>
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

        public async Task<Result<ProductDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            // Validate user
            var user = await _context.Users.FirstOrDefaultAsync(x =>
                x.UserName == _userAccessor.GetUsername(), cancellationToken);
            if (user == null)
            {
                return Result<ProductDto>.Failure("User not found");
            }

            // Check if productId already exists
            var existingProduct = await _context.Products
                .AnyAsync(x => x.ProductId == request.ProductDto!.ProductId, cancellationToken);
            if (existingProduct)
            {
                return Result<ProductDto>.Failure("Product ID already exists");
            }

            var stamp = DateTime.Now;
            
            if (!string.IsNullOrEmpty(request.ProductDto!.ModelProductId) &&
                request.ProductDto.IsVirtual == "N" && request.ProductDto.IsVariant == "Y")
            {
                var modelProductExists = await _context.Products
                    .AnyAsync(x => x.ProductId == request.ProductDto.ModelProductId, cancellationToken);
                if (!modelProductExists)
                {
                    return Result<ProductDto>.Failure("Model Product ID does not exist");
                }
            }

            // Begin transaction
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create Product entity
                var product = new Product
                {
                    ProductId = request.ProductDto!.ProductId,
                    ProductName = request.ProductDto.ProductName,
                    ProductTypeId = request.ProductDto.ProductTypeId,
                    QuantityUomId = request.ProductDto.QuantityUomId,
                    Comments = request.ProductDto.Comments,
                    PrimaryProductCategoryId = request.ProductDto.PrimaryProductCategoryId,
                    IsVirtual = request.ProductDto.IsVirtual,
                    IsVariant = request.ProductDto.IsVariant,
                    QuantityIncluded = request.ProductDto.QuantityIncluded,
                    PiecesIncluded = (int?)request.ProductDto.PiecesIncluded,
                    CreatedDate = stamp,
                    LastUpdatedStamp = stamp
                };
                _context.Products.Add(product);

                // Add ProductFeatureAppls for productColorId and productTrademarkId
                if (!string.IsNullOrEmpty(request.ProductDto.ProductColorId))
                {
                    var colorFeatureAppl = new ProductFeatureAppl
                    {
                        ProductId = product.ProductId,
                        ProductFeatureId = request.ProductDto.ProductColorId,
                        FromDate = stamp,
                        ThruDate = null // Or set to a future date if applicable
                    };
                    _context.ProductFeatureAppls.Add(colorFeatureAppl);
                }
                
                if (!string.IsNullOrEmpty(request.ProductDto.ProductSizeId))
                {
                    var colorFeatureAppl = new ProductFeatureAppl
                    {
                        ProductId = product.ProductId,
                        ProductFeatureId = request.ProductDto.ProductSizeId,
                        FromDate = stamp,
                        ThruDate = null // Or set to a future date if applicable
                    };
                    _context.ProductFeatureAppls.Add(colorFeatureAppl);
                }

                if (!string.IsNullOrEmpty(request.ProductDto.ProductTrademarkId))
                {
                    var trademarkFeatureAppl = new ProductFeatureAppl
                    {
                        ProductId = product.ProductId,
                        ProductFeatureId = request.ProductDto.ProductTrademarkId,
                        FromDate = stamp,
                        ThruDate = null // Or set to a future date if applicable
                    };
                    _context.ProductFeatureAppls.Add(trademarkFeatureAppl);
                }
                
                
                // Save changes
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return Result<ProductDto>.Failure("Failed to create product");
                }

                // Commit transaction
                await transaction.CommitAsync(cancellationToken);

                // Fetch product details for response
                var productToReturn = await (from pr in _context.Products
                    join pt in _context.ProductTypes on pr.ProductTypeId equals pt.ProductTypeId
                    join pc in _context.ProductCategories on pr.PrimaryProductCategoryId equals pc.ProductCategoryId
                    where pr.ProductId == request.ProductDto.ProductId
                    select new ProductDto
                    {
                        ProductId = pr.ProductId,
                        ProductName = pr.ProductName,
                        ProductTypeId = pr.ProductTypeId,
                        Comments = pr.Comments,
                        QuantityIncluded = pr.QuantityIncluded,
                        PiecesIncluded = pr.PiecesIncluded,
                        ProductTypeDescription = pt.Description,
                        PrimaryProductCategoryId = pr.PrimaryProductCategoryId,
                        QuantityUomId = pr.QuantityUomId,
                        PrimaryProductCategoryDescription = pc.Description,
                        ProductColorId = request.ProductDto.ProductColorId,
                        ProductSizeId = request.ProductDto.ProductSizeId,
                        ProductTrademarkId = request.ProductDto.ProductTrademarkId
                    }).SingleOrDefaultAsync(cancellationToken);

                if (productToReturn == null)
                {
                    return Result<ProductDto>.Failure("Failed to retrieve created product");
                }

                return Result<ProductDto>.Success(productToReturn);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<ProductDto>.Failure($"Failed to create product: {ex.Message}");
            }
        }
    }
}