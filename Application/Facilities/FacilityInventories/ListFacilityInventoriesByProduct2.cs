using Domain;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Facilities.FacilityInventories;

public class ListFacilityInventoriesByProduct2
{
    public class Query : IRequest<IQueryable<FacilityInventoryRecordView>>
    {
        public ODataQueryOptions<FacilityInventoryRecordView> Options { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<FacilityInventoryRecordView>>
    {
        private readonly DataContext _context;


        public Handler(DataContext context)
        {
            _context = context;
        }
        //todo: add product UOM

        public async Task<IQueryable<FacilityInventoryRecordView>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var language = request.Language;
            var query = from fi in _context.FacilityInventoryRecordViews
                select new FacilityInventoryRecordView()
                {
                    FacilityId = fi.FacilityId,
                    FacilityName = language == "ar" ? fi.FacilityNameArabic : fi.FacilityName,
                    ProductId = fi.ProductId,
                    ProductName = fi.ProductName,
                    QuantityUomId = fi.QuantityUomId,
                    QuantityOnHandTotal = fi.QuantityOnHandTotal,
                    AvailableToPromiseTotal = fi.AvailableToPromiseTotal,
                    DefaultPrice = fi.DefaultPrice,
                    MinimumStock = fi.MinimumStock,
                    ReorderQuantity = fi.ReorderQuantity,
                    AvailableToPromiseMinusMinimumStock = fi.AvailableToPromiseMinusMinimumStock,
                    QuantityOnHandMinusMinimumStock = fi.QuantityOnHandMinusMinimumStock
                };
            return query;
        }
    }
}