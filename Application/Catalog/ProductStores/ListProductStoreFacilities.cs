using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductStores;

public class ListProductStoreFacilities
{
    public class Query : IRequest<Result<List<ProductStoreFacilityDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<ProductStoreFacilityDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;


        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<List<ProductStoreFacilityDto>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var productStoreFacilities = await (from psf in _context.ProductStoreFacilities
                join f in _context.Facilities on psf.FacilityId equals f.FacilityId
                select new ProductStoreFacilityDto
                {
                    DestinationFacilityId = f.FacilityId,
                    FacilityName = f.FacilityName
                }).ToListAsync(cancellationToken);

            return Result<List<ProductStoreFacilityDto>>.Success(productStoreFacilities);
        }
    }
}