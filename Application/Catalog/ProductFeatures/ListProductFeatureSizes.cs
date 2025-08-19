using MediatR;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace Application.Catalog.ProductFeatures;

public class ListProductFeatureSizes
{
    public class Query : IRequest<Result<List<ProductFeatureSizesDto>>>
    {
        public string Language { get; set; }
    }

    public class ProductFeatureSizesDto
    {
        public string ProductSizeId { get; set; }
        public string Description { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<ProductFeatureSizesDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ProductFeatureSizesDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;

            var sizes = await _context.ProductFeatures
                .Where(pf => pf.ProductFeatureTypeId == "SIZE")
                .Select(pf => new ProductFeatureSizesDto
                {
                    ProductSizeId = pf.ProductFeatureId,
                    Description = language == "ar" ? pf.DescriptionArabic : pf.Description
                })
                .ToListAsync(cancellationToken);

            // REFACTOR: Added empty record for dropdown clear option
            // Purpose: Allows users to deselect size in UI dropdowns
            sizes.Insert(0, new ProductFeatureSizesDto
            {
                ProductSizeId = "",
                Description = ""
            });

            return Result<List<ProductFeatureSizesDto>>.Success(sizes);
        }
    }
}