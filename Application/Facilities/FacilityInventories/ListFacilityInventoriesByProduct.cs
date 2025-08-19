using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Facilities.FacilityInventories;

public class ListFacilityInventoriesByProduct
{
    public class Query : IRequest<IQueryable<FacilityInventoryRecord>>
    {
        public ODataQueryOptions<FacilityInventoryRecord> Options { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<FacilityInventoryRecord>>
    {
        private readonly DataContext _context;


        public Handler(DataContext context)
        {
            _context = context;
        }
        //todo: add product UOM

        public async Task<IQueryable<FacilityInventoryRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var query = from prd in _context.Products
                join prdf in _context.ProductFacilities on prd.ProductId equals prdf.ProductId
                join prc in _context.ProductPrices on prd.ProductId equals prc.ProductId
                join inv in _context.InventoryItems on prd.ProductId equals inv.ProductId
                join fac in _context.Facilities on inv.FacilityId equals fac.FacilityId
                group inv by new
                {
                    prd.ProductId,
                    prd.ProductName,
                    fac.FacilityName,
                    prc.Price,
                    prdf.MinimumStock,
                    prdf.ReorderQuantity
                }
                into grp
                select new FacilityInventoryRecord
                {
                    FacilityId = grp.Key.FacilityName,
                    ProductId = grp.Key.ProductId,
                    ProductName = grp.Key.ProductName,
                    QuantityOnHandTotal = grp.Sum(g => g.QuantityOnHandTotal),
                    AvailableToPromiseTotal = grp.Sum(g => g.AvailableToPromiseTotal),
                    DefaultPrice = grp.Key.Price,
                    OrderedQuantity = (from oi in _context.OrderItems
                        where oi.ProductId == grp.Key.ProductId && oi.StatusId != "Complete"
                        select oi.Quantity).Sum() ?? 0,
                    AvailableToPromiseMinusMinimumStock =
                        grp.Sum(g => g.AvailableToPromiseTotal) - grp.Key.MinimumStock,
                    ReorderQuantity = grp.Key.ReorderQuantity,
                    MinimumStock = grp.Key.MinimumStock,
                    QuantityOnHandMinusMinimumStock = grp.Sum(g => g.QuantityOnHandTotal) - grp.Key.MinimumStock
                };

            return query;
        }
    }
}