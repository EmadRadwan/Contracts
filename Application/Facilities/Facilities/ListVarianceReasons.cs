using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Facilities.Facilities;

public class ListVarianceReasons
{
    public class Query : IRequest<Result<List<VarianceReason>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<VarianceReason>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<VarianceReason>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var orderAdjustmentTypes = await _context.VarianceReasons
                .ToListAsync(cancellationToken: cancellationToken);

            return Result<List<VarianceReason>>.Success(orderAdjustmentTypes);
        }
    }
}