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
    public class GetFinishedProductsLov
    {
        public class FinishedProductsEnvelope
        {
            public List<FinishedProductLovDto> Products { get; set; }
            public int ProductCount { get; set; }
        }

        public class Query : IRequest<Result<FinishedProductsEnvelope>>
        {
            public ProductLovParams? Params { get; set; }
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<FinishedProductsEnvelope>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<FinishedProductsEnvelope>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var productParams = request.Params ?? new ProductLovParams();
                var language = request.Language;

                var query = from prd in _context.Products
                    join assoc in _context.ProductAssocs on prd.ProductId equals assoc.ProductId
                    where (prd.ProductTypeId == "WIP") ||
                          assoc.ProductAssocTypeId == "MANUF_COMPONENT" &&
                          (productParams.SearchTerm == null || 
                            language == "en" && prd.ProductName.Contains(productParams.SearchTerm) ||
                            language == "tr" && prd.ProductNameTurkish.Contains(productParams.SearchTerm) ||
                            language == "ar" && prd.ProductNameArabic.Contains(productParams.SearchTerm)
                          )
                    select new FinishedProductLovDto
                    {
                        ProductId = prd.ProductId,
                        ProductName = language == "ar" ? prd.ProductNameArabic : language == "tr" ? prd.ProductNameTurkish : prd.ProductName
                    };

                var distinctQuery = query.Distinct().AsQueryable();

                var products = await distinctQuery
                    .Skip(productParams.Skip)
                    .Take(productParams.PageSize)
                    .ToListAsync(cancellationToken);

                var productCount = await distinctQuery.CountAsync(cancellationToken);

                var productEnvelop = new FinishedProductsEnvelope
                {
                    Products = products,
                    ProductCount = productCount
                };

                return Result<FinishedProductsEnvelope>.Success(productEnvelop);
            }
        }
    }
}