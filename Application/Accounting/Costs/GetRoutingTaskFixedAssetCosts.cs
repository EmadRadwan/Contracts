using MediatR;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Application.Shipments.Costs
{
    public class GetRoutingTaskFixedAssetCosts
    {
        public class Query : IRequest<Result<List<FixedAssetStdCostsDto>>>
        {
            public string? ProductId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<FixedAssetStdCostsDto>>>
        {
            private readonly DataContext _context;

            public Handler(DataContext context)
            {
                _context = context;
            }

            public async Task<Result<List<FixedAssetStdCostsDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                // Step 1: Identify the main routing via WorkEffortGoodStandards
                var routingGS = await _context.WorkEffortGoodStandards
                    .Where(w => w.ProductId == request.ProductId && w.WorkEffortGoodStdTypeId == "ROU_PROD_TEMPLATE")
                    .Where(w => w.FromDate <= DateTime.UtcNow && (w.ThruDate == null || w.ThruDate >= DateTime.UtcNow))
                    .FirstOrDefaultAsync(cancellationToken);

                if (routingGS == null)
                {
                    return Result<List<FixedAssetStdCostsDto>>.Success(new List<FixedAssetStdCostsDto>());
                }

                // Step 2: Get tasks associated with the identified routing via WorkEffortAssocs
                var routingTasks = await _context.WorkEffortAssocs
                    .Where(w => w.WorkEffortIdFrom == routingGS.WorkEffortId &&
                                w.WorkEffortAssocTypeId == "ROUTING_COMPONENT")
                    .Where(w => w.FromDate <= DateTime.UtcNow && (w.ThruDate == null || w.ThruDate >= DateTime.UtcNow))
                    .OrderBy(w => w.SequenceNum)
                    .Select(w => w.WorkEffortIdTo)
                    .ToListAsync(cancellationToken);

                if (routingTasks == null || !routingTasks.Any())
                {
                    return Result<List<FixedAssetStdCostsDto>>.Success(new List<FixedAssetStdCostsDto>());
                }

                var fixedAssetStdCosts = new List<FixedAssetStdCostsDto>();

                // Step 3: For each task, retrieve FixedAsset and its associated FixedAssetStdCosts
                foreach (var taskId in routingTasks)
                {
                    var task = await _context.WorkEfforts.FindAsync(taskId);
                    if (task?.FixedAssetId != null)
                    {
                        var fixedAsset = await _context.FixedAssets.FindAsync(task.FixedAssetId);

                        if (fixedAsset != null)
                        {
                            var fixedAssetCosts = await _context.FixedAssetStdCosts
                                .Where(c => c.FixedAssetId == fixedAsset.FixedAssetId &&
                                            (c.FromDate <= DateTime.UtcNow && (c.ThruDate == null || c.ThruDate > DateTime.UtcNow)))
                                .ToListAsync(cancellationToken);

                            foreach (var cost in fixedAssetCosts)
                            {
                                fixedAssetStdCosts.Add(new FixedAssetStdCostsDto
                                {
                                    FixedAssetId = fixedAsset.FixedAssetId,
                                    FixedAssetStdCostTypeId = cost.FixedAssetStdCostTypeId,
                                    AmountUomId = cost.AmountUomId,
                                    FromDate = cost.FromDate,
                                    ThruDate = cost.ThruDate,
                                    Amount = (decimal)cost.Amount
                                });
                            }
                        }
                    }
                }

                return Result<List<FixedAssetStdCostsDto>>.Success(fixedAssetStdCosts);
            }
        }
    }
    
}
