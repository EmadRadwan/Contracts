using MediatR;
using Persistence;

namespace Application.Catalog.ProductTypes;

public class ListProductTypes
{
    public class Query : IRequest<Result<List<ProductTypeDto>>>
    {
        public string Language { get; set; }
    }


    public class Handler : IRequestHandler<Query, Result<List<ProductTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ProductTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var allowedTypes = new List<string> { "FINISHED_GOOD", "RAW_MATERIAL", "WIP" };
            var language = request.Language;
            var productTypes = _context.ProductTypes
                .Where(x => allowedTypes.Contains(x.ProductTypeId))
                .Select(x => new ProductTypeDto
                {
                    ProductTypeId = x.ProductTypeId,
                    Description = language == "ar" ? x.DescriptionArabic : x.Description
                })
                .ToList();


            return Result<List<ProductTypeDto>>.Success(productTypes);
        }
    }
}