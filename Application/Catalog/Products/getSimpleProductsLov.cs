using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetSimpleProductsLov
{
    public class ProductsEnvelope
    {
        public List<ProductLovDto> Products { get; set; }
        public int ProductCount { get; set; }
    }

    public class ProductLovDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string? PrimaryProductCategoryDescription { get; set; }
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

        public async Task<Result<ProductsEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            try
            {
                if (request?.Params == null)
                {
                    _logger.LogWarning("Invalid request: Params is null");
                    return Result<ProductsEnvelope>.Failure("Invalid request parameters.");
                }

                var validProductTypes = request.Params.CertificateType switch
                {
                    "SUPPLY_PROCUREMENT_CERTIFICATE" => new[] { "RAW_MATERIAL" },
                    "WORKMANSHIP_CONTRACTING_CERTIFICATE" => new[] { "SERVICE" },
                    "COMPANY_SUPPLY_SALE_CERTIFICATE" => new[] { "RAW_MATERIAL"},
                    _ => null
                };

                if (validProductTypes == null)
                {
                    _logger.LogWarning("Invalid certificate type: {CertificateType}", request.Params.CertificateType);
                    return Result<ProductsEnvelope>.Failure("Invalid certificate type.");
                }

                // REFACTOR: Update search logic
                // Purpose: Allow searching by both ProductId and ProductName
                // Context: Modified query to include ProductId in search conditions
                var language = request.Language;

                var query =
                    from p in _context.Products
                    where validProductTypes.Contains(p.ProductTypeId)
                    join pc in _context.ProductCategories on p.PrimaryProductCategoryId equals pc.ProductCategoryId
                        into pcGroup
                    from pc in pcGroup.DefaultIfEmpty() // LEFT JOIN for category
                    select new { p, pc };

                if (!string.IsNullOrEmpty(request.Params.SearchTerm))
                {
                    query = query.Where(x =>
                        x.p.ProductName.Contains(request.Params.SearchTerm) ||
                        x.p.ProductId.Contains(request.Params.SearchTerm));
                }

                var total = await query.CountAsync(cancellationToken);

                var products = await query
                    .OrderBy(x => x.p.ProductName)
                    .Skip(request.Params.Skip)
                    .Take(request.Params.PageSize)
                    .Select(x => new ProductLovDto
                    {
                        ProductId = x.p.ProductId,
                        ProductName = x.p.ProductName,
                        ProductType = x.p.ProductTypeId,
                        PrimaryProductCategoryDescription = x.pc != null
                            ? (language == "ar" ? x.pc.DescriptionArabic : x.pc.Description)
                            : null
                    })
                    .ToListAsync(cancellationToken);

                var productEnvelope = new ProductsEnvelope
                {
                    Products = products,
                    ProductCount = total
                };

                return Result<ProductsEnvelope>.Success(productEnvelope);
            }
            catch (Exception ex)
            {
                return Result<ProductsEnvelope>.Failure("Failed to retrieve products.");
            }
        }
    }
}