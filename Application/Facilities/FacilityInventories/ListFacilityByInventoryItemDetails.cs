using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;

namespace Application.Facilities.FacilityInventories;

public class ListFacilityByInventoryItemDetails
{
    public class Query : IRequest<IQueryable<FacilityInventoryItemDetailRecord>>
    {
        public ODataQueryOptions<FacilityInventoryItemDetailRecord> Options { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<FacilityInventoryItemDetailRecord>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;


        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IQueryable<FacilityInventoryItemDetailRecord>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            const decimal Tolerance = 0.000001m;
            var language = request.Language;

            var query = from invi in _context.InventoryItems
                join invd in _context.InventoryItemDetails on invi.InventoryItemId equals invd.InventoryItemId
                join prd in _context.Products on invi.ProductId equals prd.ProductId
                join fac in _context.Facilities on invi.FacilityId equals fac.FacilityId
                where
                    (
                        Math.Abs(invd.AccountingQuantityDiff.GetValueOrDefault()) <=
                        Tolerance // AccountingQuantityDiff must be zero
                        && !(Math.Abs(invd.QuantityOnHandDiff.GetValueOrDefault()) <= Tolerance &&
                             Math.Abs(invd.AvailableToPromiseDiff.GetValueOrDefault()) <=
                             Tolerance) // Exclude if both QuantityOnHandDiff and AvailableToPromiseDiff are zero
                    )
                    || (
                        // Include "starting records" where all Diff fields are the same non-zero value
                        Math.Abs(invd.QuantityOnHandDiff.GetValueOrDefault()
                                 - invd.AvailableToPromiseDiff.GetValueOrDefault()) <= Tolerance
                        && Math.Abs(invd.QuantityOnHandDiff.GetValueOrDefault()
                                    - invd.AccountingQuantityDiff.GetValueOrDefault()) <= Tolerance
                        && Math.Abs(invd.QuantityOnHandDiff.GetValueOrDefault()) > Tolerance
                    )
                select new FacilityInventoryItemDetailRecord
                {
                    ProductId = invi.ProductId,
                    ProductName = language == "ar" ? prd.ProductNameArabic : language == "tr" ? prd.ProductNameTurkish : prd.ProductName,
                    QuantityOnHandTotal = invi.QuantityOnHandTotal,
                    AvailableToPromiseTotal = invi.AvailableToPromiseTotal,
                    InventoryItemId = invi.InventoryItemId,
                    FacilityId = invi.FacilityId,
                    FacilityName = language == "ar" ? fac.FacilityNameArabic : language == "tr" ? fac.FacilityNameTurkish : fac.FacilityName,
                    InventoryItemDetailSeqId = invd.InventoryItemDetailSeqId,
                    EffectiveDate = invd.EffectiveDate,
                    QuantityOnHandDiff = invd.QuantityOnHandDiff,
                    AvailableToPromiseDiff = invd.AvailableToPromiseDiff,
                    AccountingQuantityDiff = invd.AccountingQuantityDiff,
                    OrderId = invd.OrderId,
                    WorkEffortId = invd.WorkEffortId,
                };

            return query;
        }
    }
}