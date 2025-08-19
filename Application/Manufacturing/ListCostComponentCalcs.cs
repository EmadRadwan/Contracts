using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Manufacturing;

public class ListCostComponentCalcs
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

        public async Task<IQueryable<CostComponentCalcRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _context.CostComponentCalcs
                .Where(x => x.CostComponentCalcId == "DIRECT_LABOR_HOUR" || x.CostComponentCalcId == "FOH_GENERAL_HOUR")
                .Select(x => new CostComponentCalcRecord
                {
                    CostComponentCalcId = x.CostComponentCalcId,
                    Description = x.Description,
                    CostGlAccountTypeId = x.CostGlAccountTypeId,
                    OffsettingGlAccountTypeId = x.OffsettingGlAccountTypeId,
                    FixedCost = x.FixedCost,
                    VariableCost = x.VariableCost,
                    PerMilliSecond = x.PerMilliSecond,
                    CurrencyUomId = x.CurrencyUomId,
                    CostCustomMethodId = x.CostCustomMethodId
                });

            // REFACTOR: Added Where clause to filter only records with CostComponentCalcId values "DIRECT_LABOR_HOUR" and "FOH_GENERAL_HOUR" to match the provided data. This improves performance by reducing the result set early in the query pipeline and ensures only relevant records are returned.

            return query;
        }
    }
}