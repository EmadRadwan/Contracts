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
                // Context: Unchanged
                if (request?.Params == null)
                {
                    _logger.LogWarning("Invalid request: Params is null");
                    return Result<ProductsEnvelope>.Failure("Invalid request parameters.");
                }

                // REFACTOR: Filter by certificate type
                // Purpose: Select RAW_MATERIAL for PROCUREMENT_CERTIFICATE, SERVICE_PRODUCT for CONTRACTING_CERTIFICATE
                // Context: Added dynamic ProductTypeId filter based on CertificateType
                var productTypeId = request.Params.CertificateType == "PROCUREMENT_CERTIFICATE"
                    ? "RAW_MATERIAL"
                    : request.Params.CertificateType == "CONTRACTING_CERTIFICATE"
                        ? "SERVICE_PRODUCT"
                        : null;

                if (productTypeId == null)
                {
                    _logger.LogWarning("Invalid certificate type: {CertificateType}", request.Params.CertificateType);
                    return Result<ProductsEnvelope>.Failure("Invalid certificate type.");
                }

                // REFACTOR: Query products
                // Purpose: Fetch ProductId and ProductName, filter by supplierId, searchTerm, and certificate type
                // Context: Updated to include ProductTypeId filter
                var query = _context.Products
                    .Where(p => p.ProductTypeId == productTypeId)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(request.Params.SupplierId))
                    query = query
                        .Join(_context.SupplierProducts,
                            p => p.ProductId,
                            sp => sp.ProductId,
                            (p, sp) => new { Product = p, SupplierProduct = sp })
                        .Where(x => x.SupplierProduct.PartyId == request.Params.SupplierId &&
                                    x.SupplierProduct.AvailableThruDate == null)
                        .Select(x => x.Product);

                if (!string.IsNullOrEmpty(request.Params.SearchTerm))
                    query = query.Where(p => p.ProductName.Contains(request.Params.SearchTerm));

                var total = await query.CountAsync(cancellationToken);

                var products = await query
                    .OrderBy(p => p.ProductName)
                    .Skip(request.Params.Skip)
                    .Take(request.Params.PageSize)
                    .Select(p => new ProductLovDto
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName
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