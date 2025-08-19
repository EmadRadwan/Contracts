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
    public class GetProductCostComponentCalcs
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
                var query = await _context.CostComponentCalcs
                    .Join(_context.ProductCostComponentCalcs,
                        costComponentCalc => costComponentCalc.CostComponentCalcId,
                        productCostComponentCalc => productCostComponentCalc.CostComponentCalcId,
                        (costComponentCalc, productCostComponentCalc) =>
                            new { costComponentCalc, productCostComponentCalc })
                    .Join(_context.CostComponentTypes,
                        combined => combined.productCostComponentCalc.CostComponentTypeId,
                        costComponentType => costComponentType.CostComponentTypeId,
                        (combined, costComponentType) => new
                            { combined.costComponentCalc, combined.productCostComponentCalc, costComponentType })
                    .Select(x => new CostComponentCalcDto
                    {
                        CostComponentCalcId = x.costComponentCalc.CostComponentCalcId,
                        Description = x.costComponentCalc.Description,
                        CostGlAccountTypeId = x.costComponentCalc.CostGlAccountTypeId,
                        OffsettingGlAccountTypeId = x.costComponentCalc.OffsettingGlAccountTypeId,
                        FixedCost = x.costComponentCalc.FixedCost,
                        VariableCost = x.costComponentCalc.VariableCost,
                        PerMilliSecond = x.costComponentCalc.PerMilliSecond,
                        CurrencyUomId = x.costComponentCalc.CurrencyUomId,
                        CostCustomMethodId = x.costComponentCalc.CostCustomMethodId,
                        ProductId = x.productCostComponentCalc.ProductId,
                        CostComponentTypeId = x.costComponentType.CostComponentTypeId,
                        CostComponentTypeDescription = x.costComponentType.Description,
                        FromDate = x.productCostComponentCalc.FromDate,
                        ThruDate = x.productCostComponentCalc.ThruDate
                    }).Where(cc => cc.ProductId == request.ProductId).ToListAsync(cancellationToken);


                return Result<List<CostComponentCalcDto>>.Success(query);
            }
        }
    }
}