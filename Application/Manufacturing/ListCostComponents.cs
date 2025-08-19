using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Manufacturing;

public class ListCostComponents
{
    public class Query : IRequest<IQueryable<CostComponentRecord>>
    {
        public ODataQueryOptions<CostComponentRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<CostComponentRecord>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<IQueryable<CostComponentRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = from costComponent in _context.CostComponents
                join product in _context.Products
                    on costComponent.ProductId equals product.ProductId
                join costComponentType in _context.CostComponentTypes
                    on costComponent.CostComponentTypeId equals costComponentType.CostComponentTypeId
                select new CostComponentRecord
                {
                    CostComponentCalcId = costComponent.CostComponentCalcId,
                    CostComponentTypeId = costComponent.CostComponentTypeId,
                    ProductId = costComponent.ProductId,
                    ProductFeatureId = costComponent.ProductFeatureId,
                    PartyId = costComponent.PartyId,
                    GeoId = costComponent.GeoId,
                    WorkEffortId = costComponent.WorkEffortId,
                    FixedAssetId = costComponent.FixedAssetId,
                    CostComponentId = costComponent.CostComponentId,
                    ProductName = product.ProductName,
                    CostComponentTypeDescription = costComponentType.Description,
                    FromDate = costComponent.FromDate,
                    ThruDate = costComponent.ThruDate,
                    Cost = costComponent.Cost,
                    CostUomId = costComponent.CostUomId,
                };


            return query;
        }
    }
}