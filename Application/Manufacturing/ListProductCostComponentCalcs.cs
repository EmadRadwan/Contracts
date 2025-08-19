using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Manufacturing;

public class ListProductCostComponentCalcs
{
    public class Query : IRequest<IQueryable<CostComponentCalcRecord>>
    {
        public ODataQueryOptions<CostComponentCalcRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<CostComponentCalcRecord>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<IQueryable<CostComponentCalcRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = _context.CostComponentCalcs
                .Join(_context.ProductCostComponentCalcs,
                    costComponentCalc => costComponentCalc.CostComponentCalcId,
                    productCostComponentCalc => productCostComponentCalc.CostComponentCalcId,
                    (costComponentCalc, productCostComponentCalc) => new { costComponentCalc, productCostComponentCalc })
                .Join(_context.CostComponentTypes,
                    combined => combined.productCostComponentCalc.CostComponentTypeId,
                    costComponentType => costComponentType.CostComponentTypeId,
                    (combined, costComponentType) => new { combined.costComponentCalc, combined.productCostComponentCalc, costComponentType })
                .Select(x => new CostComponentCalcRecord
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
                });


            return query;
        }
    }
}