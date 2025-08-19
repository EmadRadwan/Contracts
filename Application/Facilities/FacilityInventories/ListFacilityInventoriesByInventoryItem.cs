using Application.Catalog.Products;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.OData.Query;
using Persistence;
using System.Linq;

namespace Application.Facilities.FacilityInventories;

public class ListFacilityInventoriesByInventoryItem
{
    public class Query : IRequest<IQueryable<FacilityInventoryItemRecord>>
    {
        public ODataQueryOptions<FacilityInventoryItemRecord> Options { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, IQueryable<FacilityInventoryItemRecord>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IQueryable<FacilityInventoryItemRecord>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Set default language to handle null
            // Why: Prevents null reference issues and matches output language
            var language = request.Language ?? "ar";

            // REFACTOR: Use explicit IQueryable<FacilityInventoryItemRecord> for type safety
            // Why: Ensures the compiler recognizes the return type correctly
            IQueryable<FacilityInventoryItemRecord> query = from invi in _context.InventoryItems
                join prty in _context.Parties on invi.PartyId equals prty.PartyId into prtyGroup
                from prty in prtyGroup.DefaultIfEmpty()
                join prd in _context.Products on invi.ProductId equals prd.ProductId
                join fac in _context.Facilities on invi.FacilityId equals fac.FacilityId
                select new FacilityInventoryItemRecord
                {
                    ProductId = invi.ProductId,
                    ProductName = prd.ProductName,
                    // REFACTOR: Ensure ProductIdObject is instantiated
                    // Why: Fixes null ProductIdObject in output
                    ProductIdObject = new ProductLovDto
                    {
                        ProductId = prd.ProductId,
                        ProductName = prd.ProductName
                    },
                    UnitCost = invi.UnitCost,
                    QuantityOnHandTotal = invi.QuantityOnHandTotal,
                    AvailableToPromiseTotal = invi.AvailableToPromiseTotal,
                    InventoryItemId = invi.InventoryItemId,
                    StatusId = invi.StatusId,
                    DatetimeReceived = invi.DatetimeReceived,
                    ExpireDate = invi.ExpireDate,
                    FacilityId = invi.FacilityId,
                    CurrencyUomId = invi.CurrencyUomId,
                    LocationSeqId = invi.LocationSeqId,
                    FacilityName = language == "ar" ? fac.FacilityNameArabic : fac.FacilityName,
                    PartyId = prty != null ? prty.PartyId : null,
                    PartyName = prty != null ? prty.Description : null,
                    // REFACTOR: Subquery for color feature with explicit typing
                    // Why: Ensures one color feature per item, avoiding duplicates
                    ColorFeatureId = (from colorFeature in _context.InventoryItemFeatures
                                      join colorPf in _context.ProductFeatures
                                          on colorFeature.ProductFeatureId equals colorPf.ProductFeatureId
                                      where colorFeature.InventoryItemId == invi.InventoryItemId
                                          && colorPf.ProductFeatureTypeId == "COLOR"
                                      select colorFeature.ProductFeatureId).FirstOrDefault(),
                    ColorFeatureDescription = (from colorFeature in _context.InventoryItemFeatures
                                               join colorPf in _context.ProductFeatures
                                                   on colorFeature.ProductFeatureId equals colorPf.ProductFeatureId
                                               where colorFeature.InventoryItemId == invi.InventoryItemId
                                                   && colorPf.ProductFeatureTypeId == "COLOR"
                                               select language == "ar" ? colorPf.DescriptionArabic : colorPf.Description)
                                               .FirstOrDefault(),
                    // REFACTOR: Subquery for size feature with default value
                    // Why: Allows records without size features to pass OData filters
                    SizeFeatureId = (from sizeFeature in _context.InventoryItemFeatures
                                     join sizePf in _context.ProductFeatures
                                         on sizeFeature.ProductFeatureId equals sizePf.ProductFeatureId
                                     where sizeFeature.InventoryItemId == invi.InventoryItemId
                                         && sizePf.ProductFeatureTypeId == "SIZE"
                                     select sizeFeature.ProductFeatureId).FirstOrDefault() ?? "",
                    SizeFeatureDescription = (from sizeFeature in _context.InventoryItemFeatures
                                              join sizePf in _context.ProductFeatures
                                                  on sizeFeature.ProductFeatureId equals sizePf.ProductFeatureId
                                              where sizeFeature.InventoryItemId == invi.InventoryItemId
                                                  && sizePf.ProductFeatureTypeId == "SIZE"
                                              select language == "ar" ? sizePf.DescriptionArabic : sizePf.Description)
                                              .FirstOrDefault() ?? (language == "ar" ? "" : "")
                };

            // REFACTOR: Log for debugging
            // Why: Verifies if both records are in the base query before OData filtering
            var rawResults = query.ToList();
            Console.WriteLine($"Raw results count: {rawResults.Count}");
            foreach (var result in rawResults)
            {
                Console.WriteLine($"InventoryItemId: {result.InventoryItemId}, Color: {result.ColorFeatureId}, Size: {result.SizeFeatureId}");
            }

            // REFACTOR: Explicitly return IQueryable<FacilityInventoryItemRecord>
            // Why: Resolves type mismatch error
            return query;
        }
    }
}