using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;


namespace Application.Order.Orders
{
    public class ListReturnItemsStatusLov
    {
        public class Query : IRequest<Result<List<ReturnStatusDto>>>
        {
        }
        public class Handler : IRequestHandler<Query, Result<List<ReturnStatusDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<ReturnStatusDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = await _context.StatusItems
                    .Where(t => t.StatusTypeId == "INV_SERIALIZED_STTS")
                    .OrderBy(si => si.StatusId)
                    .ThenBy(si => si.Description)
                    .Select(t => new ReturnStatusDto
                    {
                        StatusId = t.StatusId,
                        Description = t.Description ?? "N/A"
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<ReturnStatusDto>>.Success(result);
            }
        }
    }
}