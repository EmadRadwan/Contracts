using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetFacilityProductsLov
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
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor, ILogger<Handler> logger)
        {
            _mapper = mapper;
            _context = context;
            _userAccessor = userAccessor;
            _logger = logger;
        }

        public async Task<Result<ProductsEnvelope>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = (from prd in _context.Products
                join prodFac in _context.ProductFacilities on prd.ProductId equals prodFac.ProductId
                join fac in _context.Facilities on prodFac.FacilityId equals fac.FacilityId
                where (fac.FacilityId == request.Params.FacilityId && request.Params.SearchTerm == null) ||
                      prd.ProductName.Contains(request.Params.SearchTerm)
                select new ProductLovDto
                {
                    ProductId = prd.ProductId,
                    ProductName = prd.ProductName
                }).AsQueryable();


            var products = await query
                .Skip(request.Params.Skip)
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