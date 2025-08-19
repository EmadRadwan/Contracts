using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Facilities.FacilityLocations;

public class GetFacilityLocationsLov
{
    public class Query : IRequest<Result<List<FacilityLocationDto>>>
    {

    }

    public class Handler : IRequestHandler<Query, Result<List<FacilityLocationDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }
        public async Task<Result<List<FacilityLocationDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = from floc in _context.FacilityLocations
                    join fac in _context.Facilities on floc.FacilityId equals fac.FacilityId
                    join enumerations in _context.Enumerations on floc.LocationTypeEnumId equals enumerations.EnumId
                    select new FacilityLocationDto
                    {
                        FacilityId = floc.FacilityId,
                        FacilityName = fac.FacilityName,
                        LocationSeqId = floc.LocationSeqId,
                        LocationTypeEnumId = floc.LocationTypeEnumId,
                        LocationTypeEnumDescription = enumerations.Description,
                        AreaId = floc.AreaId,
                        AisleId = floc.AisleId,
                        SectionId = floc.SectionId,
                        LevelId = floc.LevelId,
                        PositionId = floc.PositionId,
                    };
                    var result = await query.ToListAsync(cancellationToken);
            return Result<List<FacilityLocationDto>>.Success(result);
        }
    }
}