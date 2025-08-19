using MediatR;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Catalog.Products.Services.Cost;
using Microsoft.EntityFrameworkCore;

namespace Application.WorkEfforts
{
    public class GetProductRoutings
    {
        public class Query : IRequest<Result<List<RoutingWorkEffortDto>>>
        {
            public string? ProductId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<RoutingWorkEffortDto>>>
        {
            private readonly DataContext _context;
            private readonly ICostService _costService;

            public Handler(DataContext context, ICostService costService)
            {
                _context = context;
                _costService = costService;
            }

            public async Task<Result<List<RoutingWorkEffortDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Get the product routings
                var productRoutings = await _context.WorkEffortGoodStandards
                    .Where(wegs => wegs.ProductId == request.ProductId)  // Filter by ProductId in WorkEffortGoodStandards
                    .Join(_context.WorkEfforts,  // Join with WorkEfforts
                        wegs => wegs.WorkEffortId,  // Foreign key in WorkEffortGoodStandards
                        we => we.WorkEffortId,  // Primary key in WorkEfforts
                        (wegs, we) => new { wegs, we })  // Anonymous type to hold both entities
                    .Where(joined => joined.we.WorkEffortTypeId == "Routing")  // Filter by WorkEffortTypeId in WorkEfforts
                    .Select(joined => new RoutingWorkEffortDto  // Projection with join
                    {
                        RoutingId = joined.we.WorkEffortId,
                        RoutingName = joined.we.WorkEffortName,
                    })
                    .ToListAsync(cancellationToken);

                return Result<List<RoutingWorkEffortDto>>.Success(productRoutings);
            }
        }
    }
}
