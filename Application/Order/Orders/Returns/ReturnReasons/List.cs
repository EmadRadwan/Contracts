using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders.Returns.ReturnReasons;

public class List
{
    public class Query : IRequest<Result<List<ReturnReasonDto>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<ReturnReasonDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<ReturnReasonDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.ReturnReasons
                .Select(x => new ReturnReasonDto
                {
                    ReturnReasonId = x.ReturnReasonId,
                    Description = x.Description
                })
                .OrderBy(x => x.Description)
                .AsQueryable();


            var returnReasons = await query
                .ToListAsync();

            return Result<List<ReturnReasonDto>>.Success(returnReasons);
        }
    }
}