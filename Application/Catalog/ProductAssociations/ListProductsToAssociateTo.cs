using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductAssociations
{
    // REFACTOR: Define DTO to match OFBiz Product entity for dropdown
    public class ProductDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
    }

    public class ListProductsToAssociateTo
    {
        public class Query : IRequest<Result<List<ProductDto>>>
        {
            public string ExcludeProductId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<ProductDto>>>
        {
            private readonly DataContext _context;

            // REFACTOR: Inject DataContext to query OFBiz Product entity
            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<ProductDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // REFACTOR: Build dynamic query to exclude selectedProduct and apply filters
                var query = _context.Products.AsQueryable();

                // REFACTOR: Exclude selectedProduct to prevent self-association, critical for ProductAssociationsForm
                if (!string.IsNullOrEmpty(request.ExcludeProductId))
                {
                    query = query.Where(x => x.ProductId != request.ExcludeProductId);
                }


                // REFACTOR: Select ProductDto with localized productName, sorted for dropdown usability
                var products = await query
                    .Select(x => new ProductDto
                    {
                        ProductId = x.ProductId,
                        ProductName = x.ProductName,
                    })
                    .OrderBy(x => x.ProductName) // User-friendly sorting
                    .ToListAsync(cancellationToken);

                // REFACTOR: Return empty list for no results to align with React component's fallback
                return Result<List<ProductDto>>.Success(products);
            }
        }
    }
}