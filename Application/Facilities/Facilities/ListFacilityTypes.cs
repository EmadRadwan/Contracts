using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Facilities.Facilities;

public class ListFacilityTypes
{
    public class Query : IRequest<Result<List<FacilityType>>>
    {
    }


    public class Handler : IRequestHandler<Query, Result<List<FacilityType>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<FacilityType>>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var orderAdjustmentTypes = await _context.FacilityTypes
                .ToListAsync();

            return Result<List<FacilityType>>.Success(orderAdjustmentTypes);
        }
    }
}