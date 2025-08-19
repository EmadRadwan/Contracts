using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Catalog.Products
{
    public class GetPhysicalInventoryProductsLov
    {
        public class PhysicalInventoryProductsEnvelope
        {
            public List<PhysicalInventoryProductLovDto> Products { get; set; }
            public int ProductCount { get; set; }
        }

        public class Query : IRequest<Result<PhysicalInventoryProductsEnvelope>>
        {
            public ProductLovParams? Params { get; set; }
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<PhysicalInventoryProductsEnvelope>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<PhysicalInventoryProductsEnvelope>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var productParams = request.Params ?? new ProductLovParams();
                var language = request.Language;
                var query = from prd in _context.Products
                    where (prd.ProductTypeId == "FINISHED_GOOD" || prd.ProductTypeId == "RAW_MATERIAL") &&
                          (productParams.SearchTerm == null || prd.ProductName.Contains(productParams.SearchTerm))
                    select new PhysicalInventoryProductLovDto
                    {
                        ProductId = prd.ProductId,
                        ProductName = prd.ProductName
                    };

                var distinctQuery = query.Distinct().AsQueryable();

                var products = await distinctQuery
                    .Skip(productParams.Skip)
                    .Take(productParams.PageSize)
                    .ToListAsync(cancellationToken);

                var productCount = await distinctQuery.CountAsync(cancellationToken);

                var productEnvelop = new PhysicalInventoryProductsEnvelope
                {
                    Products = products,
                    ProductCount = productCount
                };

                return Result<PhysicalInventoryProductsEnvelope>.Success(productEnvelop);
            }
        }
    }
}