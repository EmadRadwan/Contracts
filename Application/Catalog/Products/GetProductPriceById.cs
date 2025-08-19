using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Catalog.Products;

public class GetProductPriceById
{
    public class ProductPriceDto
    {
        public decimal Price { get; set; }
        public decimal QuantityIncluded { get; set; }
        public decimal PiecesIncluded { get; set; }
    }

    public class Query : IRequest<Result<ProductPriceDto>>
    {
        public string ProductId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<ProductPriceDto>>
    {
        private readonly DataContext _context;
        private readonly ILogger<Handler> _logger;

        public Handler(DataContext context, ILogger<Handler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<ProductPriceDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = (from prd in _context.Products
                    join prc in _context.ProductPrices
                        on new { prd.ProductId, ProductPriceTypeId = "DEFAULT_PRICE" } equals new
                            { prc.ProductId, prc.ProductPriceTypeId }
                    where prd.ProductId == request.ProductId
                          && !prc.ThruDate.HasValue
                    // REFACTOR: Order by FromDate descending to get the latest price, ensuring only the most recent price is selected.
                    orderby prc.FromDate descending
                    select new ProductPriceDto
                    {
                        Price = (decimal)prc.Price,
                        QuantityIncluded = prd.QuantityIncluded ?? 1m,
                        // REFACTOR: Handle null PiecesIncluded by providing a default value (e.g., 1m) to prevent InvalidOperationException when PiecesIncluded is null.
                        PiecesIncluded = prd.PiecesIncluded ?? 1m
                    })
                // REFACTOR: Use FirstOrDefaultAsync to retrieve only the latest price record, optimizing for single-row retrieval.
                .FirstOrDefaultAsync(cancellationToken);

            var result = await query;

            // REFACTOR: Added null check to handle cases where no valid price or product is found, returning a NotFound result for better error handling.
            if (result == null)
            {
                _logger.LogWarning("No valid price found for ProductId {ProductId}", request.ProductId);
                return Result<ProductPriceDto>.Failure("Product or active price not found");
            }

            return Result<ProductPriceDto>.Success(result);
        }
    }
}