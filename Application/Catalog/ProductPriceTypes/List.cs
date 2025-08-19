using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.ProductPriceTypes;

public class List
{
    public class Query : IRequest<Result<List<ProductPriceType>>>
    {
        public string Language { get; set; }
    }


    public class Handler : IRequestHandler<Query, Result<List<ProductPriceType>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ProductPriceType>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.ProductPriceTypes
                .OrderBy(x => x.Description)
                .Select(ppt => new ProductPriceType
                {
                    ProductPriceTypeId = ppt.ProductPriceTypeId,
                    Description = request.Language == "ar" ? ppt.DescriptionArabic : ppt.Description
                })
                .AsQueryable();


            var productPriceTypes = await query
                .ToListAsync();

            return Result<List<ProductPriceType>>.Success(productPriceTypes);
        }
    }
}