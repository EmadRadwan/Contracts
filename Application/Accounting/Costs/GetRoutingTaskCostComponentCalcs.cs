using MediatR;
using Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Catalog.Products.Services.Cost;
using Application.Manufacturing;
using Microsoft.EntityFrameworkCore;

namespace Application.Shipments.Costs
{
    public class GetRoutingTaskCostComponentCalcs
    {
        public class Query : IRequest<Result<List<CostComponentCalcDto>>>
        {
            public string? ProductId { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<CostComponentCalcDto>>>
        {
            private readonly DataContext _context;
            private readonly ICostService _costService;

            public Handler(DataContext context, ICostService costService)
            {
                _context = context;
                _costService = costService;
            }

            public async Task<Result<List<CostComponentCalcDto>>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                // Step 1: Identify the main routing via WorkEffortGoodStandards
                var routingGS = await _context.WorkEffortGoodStandards
                    .Where(w => w.ProductId == request.ProductId && w.WorkEffortGoodStdTypeId == "ROU_PROD_TEMPLATE")
                    .Where(w => w.FromDate <= DateTime.UtcNow && (w.ThruDate == null || w.ThruDate >= DateTime.UtcNow))
                    .FirstOrDefaultAsync(cancellationToken);

                if (routingGS == null)
                {
                    return Result<List<CostComponentCalcDto>>.Success(new List<CostComponentCalcDto>());
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
                    return Result<List<CostComponentCalcDto>>.Success(new List<CostComponentCalcDto>());
                }

                // Step 3: Get cost component calcs associated with the routing tasks via WorkEffortCostCalcs
                var query = await _context.WorkEffortCostCalcs
                    .Where(weCostCalc => routingTasks.Contains(weCostCalc.WorkEffortId))
                    .Join(_context.CostComponentCalcs,
                        weCostCalc => weCostCalc.CostComponentCalcId,
                        costComponentCalc => costComponentCalc.CostComponentCalcId,
                        (weCostCalc, costComponentCalc) => new CostComponentCalcDto
                        {
                            CostComponentCalcId = costComponentCalc.CostComponentCalcId,
                            Description = costComponentCalc.Description,
                            CostGlAccountTypeId = costComponentCalc.CostGlAccountTypeId,
                            OffsettingGlAccountTypeId = costComponentCalc.OffsettingGlAccountTypeId,
                            FixedCost = costComponentCalc.FixedCost,
                            VariableCost = costComponentCalc.VariableCost,
                            PerMilliSecond = costComponentCalc.PerMilliSecond,
                            CurrencyUomId = costComponentCalc.CurrencyUomId,
                            CostCustomMethodId = costComponentCalc.CostCustomMethodId,
                            WorkEffortId = weCostCalc.WorkEffortId,
                            CostComponentTypeId = weCostCalc.CostComponentTypeId,
                            FromDate = weCostCalc.FromDate,
                            ThruDate = weCostCalc.ThruDate
                        })
                    .Where(cc => cc.FromDate <= DateTime.UtcNow && (cc.ThruDate == null || cc.ThruDate >= DateTime.UtcNow))
                    .ToListAsync(cancellationToken);

                return Result<List<CostComponentCalcDto>>.Success(query);
            }
        }
    }
}
