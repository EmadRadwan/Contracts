using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductCategories;

public class List
{
    public class Query : IRequest<Result<List<ProductCategoryDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<ProductCategoryDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;


        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductCategoryDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = _context.ProductCategories
                .Select(r => new ProductCategoryDto
                {
                    PrimaryProductCategoryId = r.ProductCategoryId,
                    ProductCategoryId = r.ProductCategoryId,
                    Description = r.Description
                });


            var productCategories = await query
                .ToListAsync(cancellationToken);

            return Result<List<ProductCategoryDto>>.Success(productCategories);
        }
    }
}