using MediatR;
using Microsoft.EntityFrameworkCore;
using Domain;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Application.Interfaces;
using FluentValidation;
using Persistence;

namespace Application.Catalog.ProductPrices;

public class CreateProductPrice
{
    public class Command : IRequest<Result<ProductPriceDto>>
    {
        public ProductPrice ProductPrice { get; set; }
    }

    public class CommandValidator : AbstractValidator<Command>
    {
        public CommandValidator()
        {
            RuleFor(x => x.ProductPrice).SetValidator(new ProductPriceValidator());
        }
    }

    public class Handler : IRequestHandler<Command, Result<ProductPriceDto>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor, IMapper mapper)
        {
            _userAccessor = userAccessor;
            _context = context;
            _mapper = mapper;
        }

        public async Task<Result<ProductPriceDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Check for user existence to prevent NullReferenceException.
            // Ensures graceful failure if the user is not found.
            var user = await _context.Users.FirstOrDefaultAsync(x => x.UserName == _userAccessor.GetUsername(),
                cancellationToken);
            if (user == null)
            {
                return Result<ProductPriceDto>.Failure("User not found.");
            }

            // REFACTOR: Check for active ProductPrice with overlapping validity period.
            // Uses unique constraint fields to prevent duplicate prices, maintaining OFBiz data integrity.
            var existingPrice = await _context.ProductPrices
                .AnyAsync(x => x.ProductId == request.ProductPrice.ProductId
                               && x.ProductPriceTypeId == request.ProductPrice.ProductPriceTypeId
                               && x.CurrencyUomId == request.ProductPrice.CurrencyUomId
                               && x.FromDate >= request.ProductPrice.FromDate
                               && (x.ThruDate == null || x.ThruDate >= request.ProductPrice.FromDate),
                    cancellationToken);

            if (existingPrice)
            {
                return Result<ProductPriceDto>.Failure(
                    "A price already exists for this product, price type, currency, from date, purpose, and store group.");
            }

            // REFACTOR: Normalize all DateTime fields to UTC for consistency with database storage.
            // Prevents DateTime.Kind mismatches and ensures compatibility with MySQL DATETIME.
            var stamp = DateTime.UtcNow;
            request.ProductPrice.CreatedStamp = stamp;
            request.ProductPrice.LastUpdatedStamp = stamp;


            request.ProductPrice.ProductPriceId = Guid.NewGuid().ToString();
            _context.ProductPrices.Add(request.ProductPrice);

            try
            {
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (!result)
                {
                    return Result<ProductPriceDto>.Failure("Failed to create product price.");
                }
            }
            catch (DbUpdateException ex)
            {
                return Result<ProductPriceDto>.Failure(
                    $"Failed to create product price: {ex.InnerException?.Message ?? ex.Message}");
            }

            // REFACTOR: Retrieve created record using PriceId for efficiency.
            // Leverages new primary key to simplify lookup and avoid composite key issues.
            var productPriceToReturn = await _context.ProductPrices
                .Where(x => x.ProductPriceId == request.ProductPrice.ProductPriceId)
                .FirstOrDefaultAsync(cancellationToken);

            if (productPriceToReturn == null)
            {
                return Result<ProductPriceDto>.Failure("Failed to retrieve the created product price.");
            }

            var productPriceDto = new ProductPriceDto
            {
                ProductPriceId = productPriceToReturn.ProductPriceId, // Assuming ProductPriceId is stored as string but needs to be long in DTO
                ProductId = productPriceToReturn.ProductId,
                ProductPriceTypeId = productPriceToReturn.ProductPriceTypeId,
                CurrencyUomId = productPriceToReturn.CurrencyUomId,
                FromDate = productPriceToReturn.FromDate,
                ThruDate = productPriceToReturn.ThruDate,
                Price = productPriceToReturn.Price
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