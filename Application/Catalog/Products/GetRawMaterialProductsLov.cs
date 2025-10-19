using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetRawMaterialProductsLov
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

                // REFACTOR: Hardcode RAW_MATERIAL product type
                // Purpose: Directly filter for RAW_MATERIAL products
                // Context: Removes certificateType dependency
                var query = _context.Products
                    .Where(p => p.ProductTypeId == "RAW_MATERIAL")
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