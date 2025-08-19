using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.Products;

public class GetFinishedProductsForWIP
{
    public class Query : IRequest<Result<List<FinishedProductLovDto>>>
    {
        public string ProductId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<FinishedProductLovDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<FinishedProductLovDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var products = await _context.Products
                .Join(_context.ProductAssocs,
                    p => p.ProductId,
                    pa => pa.ProductId,
                    (p, pa) => new { Product = p, ProductAssoc = pa })
                .Where(x => x.ProductAssoc.ProductIdTo == request.ProductId
                            && x.ProductAssoc.ProductAssocTypeId == "MANUF_COMPONENT")
                .Select(x => new FinishedProductLovDto
                {
                    ProductId = x.Product.ProductId,
                    ProductName = x.Product.ProductName,
                    Quantity = x.ProductAssoc.Quantity ?? 0
                })
                .ToListAsync(cancellationToken);
            
            return Result<List<FinishedProductLovDto>>.Success(products);
        }
    }
}