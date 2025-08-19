using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Manufacturing;

public class ListBOMProductComponentsQuery
{
    public class Query : IRequest<IQueryable<BOMProductComponentRecord>>
    {
        public ODataQueryOptions<BOMProductComponentRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<BOMProductComponentRecord>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<IQueryable<BOMProductComponentRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = _context.ProductAssocs
                .Join(_context.Products,
                    pa => pa.ProductId,
                    p => p.ProductId,
                    (pa, p) => new { pa, p })
                .Join(_context.Products,
                    x => x.pa.ProductIdTo,
                    pTo => pTo.ProductId,
                    (x, pTo) => new { x, pTo })
                .Where(x => x.x.p.ProductTypeId == "FINISHED_GOOD" && x.x.pa.ProductAssocTypeId == "MANUF_COMPONENT")
                .Select(x => new BOMProductComponentRecord
                {
                    ProductId = x.x.p.ProductId,
                    QuantityUOMId = x.x.p.QuantityUomId,
                    QuantityUOMDescription = x.pTo.QuantityUom.Description,
                    ProductName = x.x.p.ProductName,
                    ProductDescription = x.x.p.Description,
                    ProductIdTo = x.pTo.ProductId,
                    ProductNameTo = x.pTo.ProductName,
                    ProductDescriptionTo = x.pTo.Description,
                    ProductAssocTypeId = x.x.pa.ProductAssocTypeId,
                    FromDate = x.x.pa.FromDate,
                    ThruDate = x.x.pa.ThruDate,
                    SequenceNum = x.x.pa.SequenceNum,
                    Reason = x.x.pa.Reason,
                    Quantity = x.x.pa.Quantity,
                    ScrapFactor = x.x.pa.ScrapFactor,
                    Instruction = x.x.pa.Instruction,
                    RoutingWorkEffortId = x.x.pa.RoutingWorkEffortId,
                    EstimateCalcMethod = x.x.pa.EstimateCalcMethod,
                    RecurrenceInfoId = x.x.pa.RecurrenceInfoId
                });


            return query;
        }
    }
}