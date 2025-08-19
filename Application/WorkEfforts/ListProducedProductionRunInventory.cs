using MediatR;
using Persistence;
using System.Threading;
using System.Threading.Tasks;
using Application.Catalog.Products.Services.Cost;
using Application.Manufacturing;
using Microsoft.EntityFrameworkCore;
using Application.Facilities.FacilityInventories;

namespace Application.WorkEfforts
{
    public class ListProducedProductionRunInventory
    {
        public class Query : IRequest<Result<List<FacilityInventoryItemDto>>>
        {
            public string? WorkEffortId { get; set; }
            public string Language { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<FacilityInventoryItemDto>>>
        {
            private readonly DataContext _context;
            private readonly IProductionRunService _productionRunService;

            public Handler(DataContext context, IProductionRunService productionRunService) {
                _context = context;
                _productionRunService = productionRunService;
            }

            public async Task<Result<List<FacilityInventoryItemDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var language = request.Language;
                var query = await (from weip in _context.WorkEffortInventoryProduceds
                join invi in _context.InventoryItems on weip.InventoryItemId equals invi.InventoryItemId
                join prty in _context.Parties on invi.PartyId equals prty.PartyId into prtyGroup
                from prty in prtyGroup.DefaultIfEmpty() // Left join to allow null PartyId
                join prd in _context.Products on invi.ProductId equals prd.ProductId
                join fac in _context.Facilities on invi.FacilityId equals fac.FacilityId
                where weip.WorkEffortId == request.WorkEffortId
                select new FacilityInventoryItemDto
                {
                    ProductId = invi.ProductId,
                    ProductName = prd.ProductName,
                    QuantityOnHandTotal = invi.QuantityOnHandTotal,
                    AvailableToPromiseTotal = invi.AvailableToPromiseTotal,
                    InventoryItemId = invi.InventoryItemId,
                    StatusId = invi.StatusId,
                    DatetimeReceived = invi.DatetimeReceived,
                    DatetimeManufactured = invi.DatetimeManufactured,
                    UnitCost = invi.UnitCost,
                    ExpireDate = invi.ExpireDate,
                    FacilityId = invi.FacilityId,
                    FacilityName = fac.FacilityName,
                    PartyId = prty != null ? prty.PartyId : null, // Handle nullable PartyId
                    PartyName = prty != null ? prty.Description : null // Handle nullable PartyName
                }).ToListAsync(cancellationToken);

                return Result<List<FacilityInventoryItemDto>>.Success(query);
            }
        }
    }
}