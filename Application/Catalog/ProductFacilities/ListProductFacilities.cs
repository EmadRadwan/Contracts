using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.ProductFacilities;

public class ListProductFacilities
{
    public class Query : IRequest<Result<List<ProductFacilityDto>>>
    {
        public string ProductId { get; set; }
    }


    public class Handler : IRequestHandler<Query, Result<List<ProductFacilityDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;


        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductFacilityDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = _context.ProductFacilities
                .Where(z => z.ProductId == request.ProductId)
                .ProjectTo<ProductFacilityDto>(_mapper.ConfigurationProvider)
                .AsQueryable();

            var queryString = query.ToQueryString();

            var productFacilities = await query
                .ToListAsync();

            return Result<List<ProductFacilityDto>>.Success(productFacilities);
        }
    }
}