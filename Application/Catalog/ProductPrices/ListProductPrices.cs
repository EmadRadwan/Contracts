using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductPrices;

public class ListProductPrices
{
    public class Query : IRequest<Result<List<ProductPriceDto>>>
    {
        public string ProductId { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<ProductPriceDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;


        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductPriceDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.ProductPrices
                .Where(z => z.ProductId == request.ProductId)
                .OrderBy(x => x.ProductId)
                .Join(_context.Uoms,
                        pp => pp.CurrencyUomId,
                        u => u.UomId,
                        (pp, u) => new { pp, u })
                    
                .Select(component => new ProductPriceDto
                {
                    ProductId = component.pp.ProductId,
                    ProductPriceTypeId = component.pp.ProductPriceTypeId,
                    ProductPriceTypeDescription = request.Language == "ar" ? component.pp.ProductPriceType.DescriptionArabic : component.pp.ProductPriceType.Description,
                    Price = component.pp.Price,
                    FromDate = component.pp.FromDate,
                    ThruDate = component.pp.ThruDate,
                    CurrencyUomId = component.u.UomId,
                    CurrencyUomDescription = request.Language == "ar" && component.u.DescriptionArabic != null ? component.u.DescriptionArabic : request.Language == "tr" ? component.u.DescriptionTurkish : component.u.Description
                })
                .AsQueryable().AsNoTracking();

            var queryString = query.ToQueryString();

            var result = query.ToListAsync();
            return Result<List<ProductPriceDto>>.Success(await result);
        }
    }
}