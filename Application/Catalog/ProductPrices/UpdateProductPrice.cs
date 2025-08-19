using Application.Interfaces;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;


namespace Application.Catalog.ProductPrices;

public class UpdateProductPrice
{
    public class Command : IRequest<Result<ProductPriceDto>>
    {
        public ProductPrice ProductPrice { get; set; }
    }

    public class Handler : IRequestHandler<Command, Result<ProductPriceDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<ProductPriceDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername(), cancellationToken);
            if (user == null)
            {
                return Result<ProductPriceDto>.Failure("User not found.");
            }

            // REFACTOR: Use PriceId for record lookup instead of composite key.
            // Simplifies query and eliminates FromDate mismatch issues (e.g., 404 errors).
            var productPrice = await _context.ProductPrices
                .FirstOrDefaultAsync(x => x.ProductPriceId == request.ProductPrice.ProductPriceId, cancellationToken);

            if (productPrice == null)
            {
                return Result<ProductPriceDto>.Failure("Product price not found.");
            }
            
            var existingPrice = await _context.ProductPrices
                .AnyAsync(x => x.ProductPriceId != request.ProductPrice.ProductPriceId // Exclude the current price
                               && x.ProductId == request.ProductPrice.ProductId
                               && x.ProductPriceTypeId == request.ProductPrice.ProductPriceTypeId
                               && x.CurrencyUomId == request.ProductPrice.CurrencyUomId
                               && x.FromDate >= request.ProductPrice.FromDate
                               && (x.ThruDate == null || x.ThruDate >= request.ProductPrice.FromDate),
                    cancellationToken);

            if (existingPrice)
            {
                return Result<ProductPriceDto>.Failure(
                    "Another price exists for this product, price type, currency, and date range.");
            }


            // REFACTOR: Update fields without modifying FromDate or checking RowVersion.
            // Preserves FromDate integrity and simplifies update logic by removing concurrency check.
            var stamp = DateTime.UtcNow;
            productPrice.Price = request.ProductPrice.Price;
            productPrice.LastUpdatedStamp = stamp;
            productPrice.ThruDate = request.ProductPrice.ThruDate;

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                return Result<ProductPriceDto>.Failure("Failed to update product price.");
            }

            // REFACTOR: Include PriceId in DTO to ensure frontend compatibility.
            // Maintains consistency with the new surrogate key approach.
            var productPriceDto = new ProductPriceDto
            {
                ProductPriceId = productPrice.ProductPriceId,
                ProductId = productPrice.ProductId,
                ProductPriceTypeId = productPrice.ProductPriceTypeId,
                CurrencyUomId = productPrice.CurrencyUomId,
                FromDate = productPrice.FromDate,
                ThruDate = productPrice.ThruDate,
                Price = productPrice.Price
            };

            return Result<ProductPriceDto>.Success(productPriceDto);
        }
    }
    
    public class ProductPriceDto
    {
        public string ProductPriceId { get; set; }
        public string ProductId { get; set; } = null!;
        public string ProductPriceTypeId { get; set; } = null!;
        public string CurrencyUomId { get; set; } = null!;
        public DateTime FromDate { get; set; }
        public DateTime? ThruDate { get; set; }
        public decimal? Price { get; set; }
    }
    
}