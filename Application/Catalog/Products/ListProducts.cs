using Application.Core;
using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class ListProducts
{
    public class Query : IRequest<Result<PagedList<ProductDto>>>
    {
        public ProductParams? Params { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<PagedList<ProductDto>>>
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

        public async Task<Result<PagedList<ProductDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.Products
                .OrderBy(x => x.ProductId)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .AsQueryable();


            var queryString = query.ToQueryString();

            /*var result = query.ToListAsync();
            return Result<PagedList<ProductDto>>.Success(await result);*/

            return Result<PagedList<ProductDto>>.Success(
                await PagedList<ProductDto>.ToPagedList(query, request.Params.PageNumber,
                    request.Params.PageSize)
            );
        }
    }
}