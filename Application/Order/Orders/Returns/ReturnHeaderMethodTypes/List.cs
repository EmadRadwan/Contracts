using Application.Order.ReturnHeaderMethodTypes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders.Returns.ReturnHeaderMethodTypes;

public class List
{
    public class Query : IRequest<Result<List<ReturnHeaderTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<ReturnHeaderTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ReturnHeaderTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.ReturnHeaderTypes
                .Select(x => new ReturnHeaderTypeDto
                {
                    Value = x.ReturnHeaderTypeId,
                    Label = x.Description
                })
                .OrderBy(x => x.Label)
                .AsQueryable();


            var returnHeaderTypeTypes = await query
                .ToListAsync();

            return Result<List<ReturnHeaderTypeDto>>.Success(returnHeaderTypeTypes);
        }
    }
}