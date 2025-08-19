using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.PartyTypes;

public class List
{
    public class Query : IRequest<Result<List<PartyType>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<PartyType>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<PartyType>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.PartyTypes
                .OrderBy(x => x.Description)
                .AsQueryable();


            var partyTypes = await query
                .ToListAsync();

            return Result<List<PartyType>>.Success(partyTypes);
        }
    }
}