using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetProductDetailsById
{
    public class ProductDetailsDto
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string UomDescription { get; set; }
        public int AvailableToPromiseTotal { get; set; }
        public string ProductTypeId { get; set; }
        public decimal Price { get; set; }
        public decimal QuantityIncluded { get; set; }
    }

    public class Query : IRequest<Result<ProductDetailsDto>>
    {
        public string ProductId { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ProductDetailsDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<ProductDetailsDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Changed InventoryItems join to left join
            // Purpose: Allow products without inventory records to be returned
            // Why: Ensures product details are retrieved even if inventory data is missing
            var product = await (from prd in _context.Products
                    join prc in _context.ProductPrices
                        on new { prd.ProductId, ProductPriceTypeId = "DEFAULT_PRICE" }
                        equals new { prc.ProductId, prc.ProductPriceTypeId }
                    join uom in _context.Uoms
                        on prd.QuantityUomId equals uom.UomId
                    join inv in _context.InventoryItems
                        on prd.ProductId equals inv.ProductId into invGroup
                    from inv in invGroup.DefaultIfEmpty()
                    where prd.ProductId == request.ProductId
                          && !prc.ThruDate.HasValue
                    // REFACTOR: Order by FromDate descending to get the latest price
                    // Purpose: Ensure the most recent price is selected
                    // Why: Maintains consistency with previous logic
                    orderby prc.FromDate descending
                    select new ProductDetailsDto
                    {
                        ProductId = prd.ProductId,
                        ProductName = prd.ProductName,
                        UomDescription = request.Language == "ar" ? uom.DescriptionArabic :
                            request.Language == "tr" ? uom.DescriptionTurkish : uom.Description,
                        // REFACTOR: Use null-coalescing for missing inventory data
                        // Purpose: Set default values when inventory record is absent
                        // Why: Ensures valid DTO even without inventory
                        AvailableToPromiseTotal = inv != null ? (int)inv.AvailableToPromiseTotal : 0,
                        ProductTypeId = prd.ProductTypeId,
                        Price = (decimal)prc.Price,
                        QuantityIncluded = prd.QuantityIncluded ?? 1m
                    })
                // REFACTOR: Use FirstOrDefaultAsync for efficient single-row retrieval
                // Purpose: Optimize database query
                // Why: Reduces unnecessary data fetching
                .FirstOrDefaultAsync(cancellationToken);

            // REFACTOR: Updated null check message to reflect possible inventory absence
            // Purpose: Clarify failure reason for better debugging
            // Why: Improves error handling and user feedback
            if (product == null)
            {
                _logger.LogWarning("No product, active price, or UOM found for ProductId {ProductId}",
                    request.ProductId);
                return Result<ProductDetailsDto>.Failure("Product, active price, or UOM not found");
            }

            return Result<ProductDetailsDto>.Success(product);
        }
    }
}