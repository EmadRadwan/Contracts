using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Geos;

public class ListCountry
{
    public class Query : IRequest<Result<List<Geo>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<Geo>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<Geo>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.Geos
                .Where(w => w.GeoTypeId == "COUNTRY")
                .OrderBy(x => x.GeoName)
                .AsQueryable();

            var geos = await query
                .ToListAsync();


            return Result<List<Geo>>.Success(geos);
        }
    }
}