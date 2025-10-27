using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetSimplePurchaseProductsLov
{
    public class ProductsEnvelope
    {
        public List<ProductLovDto> Products { get; set; }
        public int ProductCount { get; set; }
    }

    public class Query : IRequest<Result<ProductsEnvelope>>
    {
        public ProductLovParams? Params { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ProductsEnvelope>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string GetLocalizedDescription(string language, string defaultDescription, string? arabicDescription)
        {
            return language == "ar" ? arabicDescription ?? defaultDescription : defaultDescription;
        }

        public async Task<Result<ProductsEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                // Purpose: Prevents null reference exceptions
                if (request?.Params == null)
                {
                    _logger.LogWarning("Invalid request: Params is null");
                    return Result<ProductsEnvelope>.Failure("Invalid request parameters.");
                }

                var language = request.Language ?? "en";
                // Purpose: Ensures consistent behavior if Language is null

                // REFACTOR: Simplified query to remove SupplierProducts, ProductFeatures, and color-related joins
                // Why: Removes dependencies on supplier-specific data and color features while maintaining core product information
                var tempQuery = (from prd in _context.Products
                    where prd.ProductTypeId != "SERVICE" &&
                          (string.IsNullOrEmpty(request.Params.SearchTerm) ||
                           prd.ProductName.Contains(request.Params.SearchTerm))
                    select new
                    {
                        ProductId = prd.ProductId,
                        BaseProductName = prd.ProductName,
                    }).AsQueryable();

                // Fetch results
                var tempResults = await tempQuery
                    .OrderBy(p => p.BaseProductName)
                    .Skip(request.Params.Skip)
                    .Take(request.Params.PageSize)
                    .ToListAsync(cancellationToken);

                // REFACTOR: Simplified mapping to ProductLovDto, removing color-related fields
                // Why: Removes ProductFeatureId and ColorDescription to align with requirements
                var products = tempResults.Select(r => new ProductLovDto
                {
                    ProductId = r.ProductId,
                    ProductName = r.BaseProductName,
                }).ToList();

                var productEnvelop = new ProductsEnvelope
                {
                    Products = products,
                    ProductCount = await tempQuery.CountAsync(cancellationToken)
                    // Purpose: Ensures async consistency
                };

                // REFACTOR: Updated logging to remove color variant and SupplierId references
                // Why: Aligns logging with simplified query structure
                _logger.LogInformation(
                    "Retrieved {ProductCount} product entries for purchase order, Language {Language}",
                    productEnvelop.ProductCount,
                    language);

                return Result<ProductsEnvelope>.Success(productEnvelop);
            }
            catch (Exception ex)
            {
                // REFACTOR: Updated error logging to remove SupplierId reference
                // Why: SupplierId is no longer part of the query parameters
                _logger.LogError(ex,
                    "Error retrieving purchase products, Language {Language}",
                    request?.Language);
                return Result<ProductsEnvelope>.Failure("Failed to retrieve purchase products.");
            }
        }
    }
}