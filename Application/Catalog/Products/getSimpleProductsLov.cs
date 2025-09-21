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
    }

    public class Query : IRequest<Result<ProductsEnvelope>>
    {
        public ProductLovParams? Params { get; set; }
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
                var query = _context.Products
                    .Where(p => validProductTypes.Contains(p.ProductTypeId))
                    .AsQueryable();

                if (!string.IsNullOrEmpty(request.Params.SearchTerm))
                {
                    query = query.Where(p => 
                        p.ProductName.Contains(request.Params.SearchTerm) || 
                        p.ProductId.Contains(request.Params.SearchTerm));
                }

                var total = await query.CountAsync(cancellationToken);

                var products = await query
                    .OrderBy(p => p.ProductName)
                    .Skip(request.Params.Skip)
                    .Take(request.Params.PageSize)
                    .Select(p => new ProductLovDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        ProductType = p.ProductTypeId
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