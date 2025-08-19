using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.CustomerRequestType;

public class List
{
    public class Query : IRequest<Result<List<CustRequestType>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<CustRequestType>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<CustRequestType>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.CustRequestTypes
                .OrderBy(x => x.Description)
                .AsQueryable();


            var CustRequestTypes = await query
                .ToListAsync();

            return Result<List<CustRequestType>>.Success(CustRequestTypes);
        }
    }
}