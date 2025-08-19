using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.ContactMechPurposeTypes;

public class List
{
    public class Query : IRequest<Result<List<ContactMechPurposeType>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<ContactMechPurposeType>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ContactMechPurposeType>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            string[] elementsToExclude = { "PRIMARY_PHONE", "PRIMARY_EMAIL", "GENERAL_LOCATION" };

            var query = _context.ContactMechPurposeTypes
                .Where(y => elementsToExclude.All(p2 => p2 != y.ContactMechPurposeTypeId))
                .OrderBy(x => x.Description)
                .AsQueryable();


            var contactMechPurposeTypes = await query
                .ToListAsync();

            return Result<List<ContactMechPurposeType>>.Success(contactMechPurposeTypes);
        }
    }
}