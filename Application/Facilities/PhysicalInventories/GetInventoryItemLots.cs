using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Facilities.FacilityInventories
{
    public class GetInventoryItemLots
    {
        public class Query : MediatR.IRequest<Result<List<LotDto>>>
        {
            public string ProductId { get; set; }
            public string WorkEffortId { get; set; }
        }

        public class Handler : MediatR.IRequestHandler<Query, Result<List<LotDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<LotDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var lots = await (
                        from weia in _context.WorkEffortInventoryAssigns
                        join ii in _context.InventoryItems on weia.InventoryItemId equals ii.InventoryItemId
                        where weia.WorkEffortId == request.WorkEffortId
                              && ii.ProductId == request.ProductId
                              && ii.LotId != null
                        select new LotDto { LotId = ii.LotId }
                    )
                    .Distinct()
                    .ToListAsync(cancellationToken);

                return Result<List<LotDto>>.Success(lots);
            }
        }

        public class LotDto
        {
            public string LotId { get; set; }
        }
    }
}