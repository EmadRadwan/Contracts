using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Facilities.Facilities;

public class ListFacilities
{
    public class Query : IRequest<Result<List<FacilityDto>>>
    {
        public FacilityParams? Params { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<FacilityDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<FacilityDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var language = request.Language;

            var query = _context.Facilities
                .Select(x => new FacilityDto
                {
                    FacilityId = x.FacilityId,
                    FacilityTypeId = x.FacilityTypeId,
                    FacilityTypeDescription = x.FacilityType!.Description,
                    FacilityName = language == "ar" ? x.FacilityNameArabic : x.FacilityName,
                    Description = x.Description
                })
                .AsQueryable();

            if (request.Params!.FacilityTypeId != "empty" && request.Params.FacilityTypeId != null)
                query = query.Where(x => x.FacilityTypeId == request.Params.FacilityTypeId);

            if (request.Params.FacilityName != "empty" && request.Params.FacilityName != null)
                query = query.Where(x => x.FacilityName!.Contains(request.Params.FacilityName));

            var queryString = query.ToQueryString();

            var result = query.ToListAsync();
            return Result<List<FacilityDto>>.Success(await result);
        }
    }
}