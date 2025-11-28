using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductPrices;

public class GetLastUnitPrice
{
    public class Query : IRequest<Result<LastUnitPriceDto>>
    {
        public string ProductId { get; set; } = null!;
        public string FacilityId { get; set; } = null!;
    }

    public class Handler : IRequestHandler<Query, Result<LastUnitPriceDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<LastUnitPriceDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Priority 1 → Get latest unitCost from InventoryItem (real stock movement)
            // Why: This is the actual last purchase/receipt cost — most accurate for procurement
            var inventoryPrice = await _context.InventoryItems
                .Where(ii => ii.ProductId == request.ProductId &&
                             ii.FacilityId == request.FacilityId &&
                             ii.UnitCost != null && ii.UnitCost > 0)
                .OrderByDescending(ii => ii.LastUpdatedStamp ?? ii.CreatedStamp)
                .Select(ii => (decimal?)ii.UnitCost)
                .FirstOrDefaultAsync(cancellationToken);

            if (inventoryPrice.HasValue)
            {
                return Result<LastUnitPriceDto>.Success(new LastUnitPriceDto
                {
                    UnitPrice = inventoryPrice.Value
                });
            }
            
            return Result<LastUnitPriceDto>.Success(new LastUnitPriceDto
            {
                UnitPrice = 0
            });
            
        }
    }
    
    public class LastUnitPriceDto
    {
        public decimal? UnitPrice { get; set; }
    }
}