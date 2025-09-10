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

    // REFACTOR: Add ProductType to DTO
    // Purpose: Include product type information in the response
    // Context: Added new property to return whether product is RAW_MATERIAL or SERVICE
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
                // REFACTOR: Validate input
                // Purpose: Prevent null reference exceptions
                // Context: Unchanged from previous
                if (request?.Params == null)
                {
                    _logger.LogWarning("Invalid request: Params is null");
                    return Result<ProductsEnvelope>.Failure("Invalid request parameters.");
                }

                // REFACTOR: Filter by new certificate types
                // Purpose: Map the five new certificate types to appropriate ProductTypeId values (RAW_MATERIAL, SERVICE)
                // Context: Replaces PROCUREMENTS and CONTRACTING checks with logic for SUPPLY_PROCUREMENT_CERTIFICATE, WORKMANSHIP_CONTRACTING_CERTIFICATE, etc., aligning with the provided sheet
                var validProductTypes = request.Params.CertificateType switch
                {
                    "SUPPLY_PROCUREMENT_CERTIFICATE" => new[] { "RAW_MATERIAL" },
                    "EXTERNAL_SUPPLY_SALE_CERTIFICATE" => new[] { "RAW_MATERIAL" },
                    "WORKMANSHIP_CONTRACTING_CERTIFICATE" => new[] { "SERVICE" },
                    "CONTRACTOR_PURCHASE_CERTIFICATE" => new[] { "RAW_MATERIAL" },
                    "COMPANY_SUPPLY_SALE_CERTIFICATE" => new[] { "RAW_MATERIAL"},
                    _ => null
                };

                if (validProductTypes == null)
                {
                    _logger.LogWarning("Invalid certificate type: {CertificateType}", request.Params.CertificateType);
                    return Result<ProductsEnvelope>.Failure("Invalid certificate type.");
                }

                // REFACTOR: Query products
                // Purpose: Fetch ProductId, ProductName, and ProductType, filter by product types and search term
                // Context: Unchanged from previous, ensures compatibility with new certificate types
                var query = _context.Products
                    .Where(p => validProductTypes.Contains(p.ProductTypeId))
                    .AsQueryable();

                if (!string.IsNullOrEmpty(request.Params.SearchTerm))
                {
                    query = query.Where(p => p.ProductName.Contains(request.Params.SearchTerm));
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