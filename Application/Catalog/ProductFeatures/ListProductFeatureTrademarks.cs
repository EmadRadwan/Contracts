using AutoMapper;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Catalog.ProductFeatures;

public class ListProductFeatureTrademarks
{
    public class Query : IRequest<Result<List<ProductFeatureTrademarksDto>>>
    {
        public string Language { get; set; }
    }

    public class ProductFeatureTrademarksDto
    {
        public string ProductTrademarkId { get; set; }
        public string Description { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<ProductFeatureTrademarksDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductFeatureTrademarksDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;

            var colors = await _context.ProductFeatures
                .Where(pf => pf.ProductFeatureTypeId == "TRADEMARK_NAME")
                .Select(pf => new ProductFeatureTrademarksDto
                {
                    ProductTrademarkId = pf.ProductFeatureId,
                    Description = language == "ar" ? pf.DescriptionArabic : pf.Description
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProductFeatureTrademarksDto>>.Success(colors);
        }
    }
}