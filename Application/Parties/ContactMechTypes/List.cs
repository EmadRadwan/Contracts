using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Parties.ContactMechTypes;

public class List
{
    public class Query : IRequest<Result<List<ContactMechType>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<ContactMechType>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ContactMechType>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.ContactMechTypes
                .OrderBy(x => x.Description)
                .AsQueryable();


            var contactMechTypes = await query
                .ToListAsync();

            return Result<List<ContactMechType>>.Success(contactMechTypes);
        }
    }
}