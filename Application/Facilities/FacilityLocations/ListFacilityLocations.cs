using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Facilities.FacilityLocations;

public class ListFacilityLocations
{
    public class Query : IRequest<IQueryable<FacilityLocationRecord>>
    {
        public ODataQueryOptions<FacilityLocationRecord> Options { get; set; }
        public string Language { get; set; }
    }
    public class Handler : IRequestHandler<Query, IQueryable<FacilityLocationRecord>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;


        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<IQueryable<FacilityLocationRecord>> Handle(Query request,
            CancellationToken cancellationToken)
            {
                var language = request.Language;
                var query = from floc in _context.FacilityLocations
                    join fac in _context.Facilities on floc.FacilityId equals fac.FacilityId
                    join enumerations in _context.Enumerations on floc.LocationTypeEnumId equals enumerations.EnumId
                    select new FacilityLocationRecord
                    {
                        FacilityId = floc.FacilityId,
                        FacilityName = language == "ar" ? fac.FacilityNameArabic : language == "tr" ? fac.FacilityNameTurkish : fac.FacilityName,
                        LocationSeqId = floc.LocationSeqId,
                        LocationTypeEnumId = floc.LocationTypeEnumId,
                        LocationTypeEnumDescription = enumerations.Description,
                        AreaId = floc.AreaId,
                        AisleId = floc.AisleId,
                        SectionId = floc.SectionId,
                        LevelId = floc.LevelId,
                        PositionId = floc.PositionId,
                    };
                return query;
            }

    }
}