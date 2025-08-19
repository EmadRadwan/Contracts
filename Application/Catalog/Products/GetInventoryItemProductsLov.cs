using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetInventoryItemProductsLov
{
    public class ProductsEnvelope
    {
        public List<ProductLovDto>? Products { get; set; }
        public int ProductCount { get; set; }
    }

    public class Query : IRequest<Result<ProductsEnvelope>>
    {
        public ProductLovParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ProductsEnvelope>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<ProductsEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = (from prd in _context.Products
                where (prd.IsVirtual == "N"
                       && request.Params.SearchTerm == null) ||
                      prd.ProductName!.Contains(request.Params.SearchTerm)
                select new ProductLovDto
                {
                    ProductId = prd.ProductId,
                    ProductName = prd.ProductName!
                }).AsQueryable();


            var products = await query
                .Skip(request.Params!.Skip)
                .Take(request.Params.PageSize)
                .ToListAsync();

            var productEnvelop = new ProductsEnvelope
            {
                Products = products,
                ProductCount = query.Count()
            };


            return Result<ProductsEnvelope>.Success(productEnvelop);
        }
    }
}