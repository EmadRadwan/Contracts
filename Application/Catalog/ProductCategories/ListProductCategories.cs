using Application.ProductCategories;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductCategories;

public class ListProductCategories
{
    public class Query : IRequest<Result<List<ProductCategoryMemberDto>>>
    {
        public string ProductId { get; set; }
    }


    public class Handler : IRequestHandler<Query, Result<List<ProductCategoryMemberDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;


        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductCategoryMemberDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = _context.ProductCategoryMembers
                .Where(z => z.ProductId == request.ProductId)
                .ProjectTo<ProductCategoryMemberDto>(_mapper.ConfigurationProvider)
                .AsQueryable();

            var queryString = query.ToQueryString();

            var productCategoryMembers = await query
                .ToListAsync();

            return Result<List<ProductCategoryMemberDto>>.Success(productCategoryMembers);
        }
    }
}