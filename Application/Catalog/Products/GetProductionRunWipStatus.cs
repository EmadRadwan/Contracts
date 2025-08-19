using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.Products;

public class GetProductionRunWipStatus
{
    public class Query : IRequest<Result<ProductionRunWipStatusDto>>
    {
        public string MainProductionRunId { get; set; }
        public string FinishedProductId { get; set; }
    }

    public class ProductionRunWipStatusDto
    {
        public string WorkEffortId { get; set; }
        public decimal TotalWipCapacity { get; set; } // Product.quantityIncluded
        public decimal ConsumedWip { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ProductionRunWipStatusDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<ProductionRunWipStatusDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Validate WorkEffort
            var workEffort = await _context.WorkEfforts
                .Where(we => we.WorkEffortId == request.MainProductionRunId)
                .FirstOrDefaultAsync(cancellationToken);

            if (workEffort == null)
                return Result<ProductionRunWipStatusDto>.Failure("Production run not found");

            // Get WIP Product ID and quantityIncluded
            var wipProduct = await _context.WorkEffortGoodStandards
                .Where(wgs => wgs.WorkEffortId == request.MainProductionRunId
                              && wgs.WorkEffortGoodStdTypeId == "PRUN_PROD_DELIV")
                .Join(_context.Products,
                    wgs => wgs.ProductId,
                    p => p.ProductId,
                    (wgs, p) => new { p.ProductId, p.QuantityIncluded })
                .FirstOrDefaultAsync(cancellationToken);

            if (wipProduct == null)
                return Result<ProductionRunWipStatusDto>.Failure("WIP product not found for production run");

            // Calculate ConsumedWip
            decimal consumedWip = 0;
            if (!string.IsNullOrEmpty(request.FinishedProductId))
            {
                consumedWip = (decimal)await _context.WorkEffortInventoryProduceds
                    .Where(wip => wip.WorkEffortId == request.MainProductionRunId)
                    .Join(_context.InventoryItems,
                        wip => wip.InventoryItemId,
                        ii => ii.InventoryItemId,
                        (wip, ii) => new { InventoryItem = ii })
                    .Join(_context.ProductAssocs,
                        joinResult => joinResult.InventoryItem.ProductId,
                        pa => pa.ProductId,
                        (joinResult, pa) => new { InventoryItem = joinResult.InventoryItem, ProductAssoc = pa })
                    .Where(result => result.ProductAssoc.ProductAssocTypeId == "MANUF_COMPONENT"
                                     && result.InventoryItem.ProductId == request.FinishedProductId)
                    .SumAsync(result => result.InventoryItem.QuantityOnHandTotal * result.ProductAssoc.Quantity,
                        cancellationToken);
            }

            var result = new ProductionRunWipStatusDto
            {
                WorkEffortId = request.MainProductionRunId,
                TotalWipCapacity = wipProduct.QuantityIncluded ?? 0, // From Product.quantityIncluded
                ConsumedWip = consumedWip
            };

            return Result<ProductionRunWipStatusDto>.Success(result);
        }
    }
}