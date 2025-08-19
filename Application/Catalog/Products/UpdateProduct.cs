using Application.Interfaces;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Microsoft.Extensions.Logging;

namespace Application.Catalog.Products
{
    public class UpdateProduct
    {
        public class Command : IRequest<Result<ProductDto>>
        {
            public ProductDto ProductDto { get; set; }
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
            private readonly IUserAccessor _userAccessor;
            private readonly ILogger<Handler> _logger;

            public Handler(DataContext context, IUserAccessor userAccessor, ILogger<Handler> logger)
            {
                _context = context;
                _userAccessor = userAccessor;
                _logger = logger;
            }

            public async Task<Result<ProductDto>> Handle(Command request, CancellationToken cancellationToken)
            {
                if (request.ProductDto == null || string.IsNullOrEmpty(request.ProductDto.ProductId))
                {
                    return Result<ProductDto>.Failure("Product ID is required.");
                }

                var user = await _context.Users.FirstOrDefaultAsync(
                    x => x.UserName == _userAccessor.GetUsername(), cancellationToken);
                if (user == null)
                {
                    return Result<ProductDto>.Failure("User not found.");
                }

                var product = await _context.Products
                    .Include(p => p.ProductType)
                    .Include(p => p.PrimaryProductCategory)
                    .FirstOrDefaultAsync(p => p.ProductId == request.ProductDto.ProductId, cancellationToken);
                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {request.ProductDto.ProductId} not found.");
                    return Result<ProductDto>.Failure($"Product with ID {request.ProductDto.ProductId} not found.");
                }
                

                // REFACTOR: Begin transaction
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var stamp = DateTime.Now;

                    // REFACTOR: Update product fields
                    product.ProductName = request.ProductDto.ProductName ?? product.ProductName;
                    product.ProductTypeId = request.ProductDto.ProductTypeId ?? product.ProductTypeId;
                    product.QuantityUomId = request.ProductDto.QuantityUomId ?? product.QuantityUomId;
                    product.Comments = request.ProductDto.Comments;
                    product.PrimaryProductCategoryId = request.ProductDto.PrimaryProductCategoryId ?? product.PrimaryProductCategoryId;
                    product.IsVirtual = request.ProductDto.IsVirtual ?? product.IsVirtual;
                    product.IsVariant = request.ProductDto.IsVariant ?? product.IsVariant;
                    product.QuantityIncluded = request.ProductDto.QuantityIncluded ?? product.QuantityIncluded;
                    product.PiecesIncluded = (int?)(request.ProductDto.PiecesIncluded ?? product.PiecesIncluded);
                    product.Description = request.ProductDto.Description ?? product.Description;
                    product.IntroductionDate = request.ProductDto.IntroductionDate ?? product.IntroductionDate;
                    product.OriginalImageUrl = request.ProductDto.OriginalImageUrl ?? product.OriginalImageUrl;
                    product.LastUpdatedStamp = stamp;

                    var activeFeatureAppls = await _context.ProductFeatureAppls
                        .Include(pfa => pfa.ProductFeature)
                        .Where(pfa => pfa.ProductId == product.ProductId &&
                                      (pfa.ProductFeature.ProductFeatureTypeId == "COLOR" ||
                                      pfa.ProductFeature.ProductFeatureTypeId == "SIZE" ||
                                       pfa.ProductFeature.ProductFeatureTypeId == "TRADEMARK_NAME") &&
                                      (pfa.ThruDate == null || pfa.ThruDate > stamp))
                        .ToListAsync(cancellationToken);
                    
                    if (activeFeatureAppls.Any())
                    {
                        // REFACTOR: Log and delete active records instead of expiring
                        _logger.LogInformation($"Found {activeFeatureAppls.Count} active ProductFeatureAppls for product {product.ProductId}. Deleting all.");
                        _context.ProductFeatureAppls.RemoveRange(activeFeatureAppls);
                        foreach (var appl in activeFeatureAppls)
                        {
                            _logger.LogInformation($"Deleted ProductFeatureAppl {appl.ProductFeatureId} for product {product.ProductId}.");
                        }
                    }
                    
                    // REFACTOR: Add new ProductFeatureAppl for ProductColorId
                    if (!string.IsNullOrEmpty(request.ProductDto.ProductColorId))
                    {
                        var colorFeatureAppl = new ProductFeatureAppl
                        {
                            ProductId = product.ProductId,
                            ProductFeatureId = request.ProductDto.ProductColorId,
                            FromDate = stamp,
                            ThruDate = null
                        };
                        _context.ProductFeatureAppls.Add(colorFeatureAppl);
                        _logger.LogInformation($"Added new color appl {colorFeatureAppl.ProductFeatureId} for product {product.ProductId}.");
                    }
                    
                    if (!string.IsNullOrEmpty(request.ProductDto.ProductSizeId))
                    {
                        var sizeFeatureAppl = new ProductFeatureAppl
                        {
                            ProductId = product.ProductId,
                            ProductFeatureId = request.ProductDto.ProductSizeId,
                            FromDate = stamp,
                            ThruDate = null
                        };
                        _context.ProductFeatureAppls.Add(sizeFeatureAppl);
                        _logger.LogInformation($"Added new color appl {sizeFeatureAppl.ProductFeatureId} for product {product.ProductId}.");
                    }

                    // REFACTOR: Add new ProductFeatureAppl for ProductTrademarkId
                    if (!string.IsNullOrEmpty(request.ProductDto.ProductTrademarkId))
                    {
                        var trademarkFeatureAppl = new ProductFeatureAppl
                        {
                            ProductId = product.ProductId,
                            ProductFeatureId = request.ProductDto.ProductTrademarkId,
                            FromDate = stamp,
                            ThruDate = null
                        };
                        _context.ProductFeatureAppls.Add(trademarkFeatureAppl);
                        _logger.LogInformation($"Added new trademark appl {trademarkFeatureAppl.ProductFeatureId} for product {product.ProductId}.");
                    }
                    
                    var activeAssociations = await _context.ProductAssocs
                        .Where(pa => pa.ProductId == product.ProductId &&
                                     pa.ProductAssocTypeId == "PRODUCT_VARIANT" &&
                                     (pa.ThruDate == null || pa.ThruDate > stamp))
                        .ToListAsync(cancellationToken);
                    if (activeAssociations.Any())
                    {
                        _context.ProductAssocs.RemoveRange(activeAssociations);
                    }

                    if (!string.IsNullOrEmpty(request.ProductDto.ModelProductId) &&
                        request.ProductDto.IsVirtual == "N" && request.ProductDto.IsVariant == "Y")
                    {
                        var productAssociation = new ProductAssoc
                        {
                            ProductId = product.ProductId,
                            ProductIdTo = request.ProductDto.ModelProductId,
                            ProductAssocTypeId = "PRODUCT_VARIANT",
                            FromDate = stamp,
                            ThruDate = null
                        };
                        _context.ProductAssocs.Add(productAssociation);
                    }


                    // REFACTOR: Save changes
                    var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                    if (!result)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        _logger.LogWarning($"Failed to update product {product.ProductId}.");
                        return Result<ProductDto>.Failure("Failed to update product.");
                    }

                    // REFACTOR: Commit transaction
                    await transaction.CommitAsync(cancellationToken);
                    

                    return Result<ProductDto>.Success(request.ProductDto);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, $"Failed to update product {request.ProductDto.ProductId}.");
                    return Result<ProductDto>.Failure($"Failed to update product: {ex.Message}");
                }
            }
        }
    }
}