using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.UomTypes;

public class List
{
    public class Query : IRequest<Result<List<UomType>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<UomType>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<UomType>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.UomTypes
                .OrderBy(x => x.Description)
                .AsQueryable();


            var uomTypes = await query
                .ToListAsync();

            return Result<List<UomType>>.Success(uomTypes);
        }
    }
}