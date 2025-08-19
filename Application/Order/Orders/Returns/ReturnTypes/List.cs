using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders.Returns.ReturnTypes;

public class List
{
    public class Query : IRequest<Result<List<ReturnTypeDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<ReturnTypeDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ReturnTypeDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.ReturnTypes
                .Select(x => new ReturnTypeDto
                {
                    ReturnTypeId = x.ReturnTypeId,
                    Description = x.Description
                })
                .OrderBy(x => x.Description)
                .AsQueryable();


            var returnTypes = await query
                .ToListAsync();

            return Result<List<ReturnTypeDto>>.Success(returnTypes);
        }
    }
}