using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Shipments.Transactions;

public class ListAccountingTransactionTypes
{
    public class Query : IRequest<Result<List<AcctgTransTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<AcctgTransTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<AcctgTransTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.AcctgTransTypes
                .OrderBy(x => x.Description)
                .Select(x => new AcctgTransTypeDto
                {
                    AcctgTransTypeId = x.AcctgTransTypeId,
                    ParentTypeId = x.ParentTypeId,
                    Description = x.Description
                })
                .AsQueryable();


            var acctgTransTypes = await query
                .ToListAsync(cancellationToken);

            return Result<List<AcctgTransTypeDto>>.Success(acctgTransTypes);
        }
    }
}