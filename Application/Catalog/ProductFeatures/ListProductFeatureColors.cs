using AutoMapper;
using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Catalog.ProductFeatures;

public class ListProductFeatureColors
{
    public class Query : IRequest<Result<List<ProductFeatureColorDto>>>
    {
        public string Language { get; set; }
    }

    public class ProductFeatureColorDto
    {
        public string ProductColorId { get; set; }
        public string Description { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<ProductFeatureColorDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductFeatureColorDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;

            var colors = await _context.ProductFeatures
                .Where(pf => pf.ProductFeatureTypeId == "COLOR")
                .Select(pf => new ProductFeatureColorDto
                {
                    ProductColorId = pf.ProductFeatureId,
                    Description = language == "ar" ? pf.DescriptionArabic : pf.Description
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProductFeatureColorDto>>.Success(colors);
        }
    }
}