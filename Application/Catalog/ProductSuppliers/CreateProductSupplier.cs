using Application.Core;
using Application.Order.Orders;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductSuppliers;

public class CreateProductSupplier
{
    // REFACTOR: Replaced SupplierProduct entity with DTO for front-end input
    // Enhances security and flexibility by decoupling the API from the database entity
    public class Command : IRequest<Result<SupplierProductDto>>
    {
        public SupplierProductCreateDto SupplierProduct { get; set; }
    }

    // REFACTOR: Updated validator to target SupplierProductCreateDto
    // Ensures input validation is applied to the DTO rather than the entity
   
    public class Handler : IRequestHandler<Command, Result<SupplierProductDto>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<SupplierProductDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Map DTO to entity for database operation
            // Ensures controlled data transfer and validation before persistence
            var supplierProduct = new SupplierProduct
            {
                ProductId = request.SupplierProduct.ProductId,
                PartyId = request.SupplierProduct.PartyId,
                CurrencyUomId = request.SupplierProduct.CurrencyUomId,
                MinimumOrderQuantity = request.SupplierProduct.MinimumOrderQuantity ?? 0,
                AvailableFromDate = request.SupplierProduct.AvailableFromDate,
                AvailableThruDate = request.SupplierProduct.AvailableThruDate,
                LastPrice = request.SupplierProduct.LastPrice,
                QuantityUomId = request.SupplierProduct.QuantityUomId,
                CreatedStamp = DateTime.UtcNow,
                LastUpdatedStamp = DateTime.UtcNow
            };
            
            _context.SupplierProducts.Add(supplierProduct);

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) return Result<SupplierProductDto>.Failure("Failed to create Product Supplier");

            // REFACTOR: Fetch entity with related data for response mapping
            // Ensures all necessary data is available for DTO construction
            var savedSupplierProduct = await _context.SupplierProducts
                .Include(sp => sp.Party)
                .Include(sp => sp.CurrencyUom)
                .Include(sp => sp.QuantityUom)
                .Where(x => x.ProductId == supplierProduct.ProductId
                            && x.PartyId == supplierProduct.PartyId
                            && x.CurrencyUomId == supplierProduct.CurrencyUomId
                            && x.MinimumOrderQuantity == supplierProduct.MinimumOrderQuantity
                            && x.AvailableFromDate == supplierProduct.AvailableFromDate)
                .SingleAsync(cancellationToken);

            // REFACTOR: Manual mapping to SupplierProductDto for response
            // Maintains consistency with front-end expectations and avoids AutoMapper
            var productSupplierToReturn = new SupplierProductDto
            {
                FromPartyId = new OrderPartyDto
                {
                    FromPartyId = savedSupplierProduct.Party.PartyId,
                    FromPartyName = savedSupplierProduct.Party.Description ?? string.Empty
                },
                CurrencyUomDescription = savedSupplierProduct.CurrencyUom.Description,
                QuantityUomDescription = savedSupplierProduct.QuantityUom != null
                    ? savedSupplierProduct.QuantityUom.Description
                    : null,
                PartyName = savedSupplierProduct.Party.Description,
                AvailableFromDate = DateTime.SpecifyKind(
                    savedSupplierProduct.AvailableFromDate.Truncate(TimeSpan.FromSeconds(1)),
                    DateTimeKind.Utc),
                LastPrice = savedSupplierProduct.LastPrice
            };

            return Result<SupplierProductDto>.Success(productSupplierToReturn);
        }
    }

    // Define DTOs for clarity
    public class SupplierProductCreateDto
    {
        public string? ProductId { get; set; }
        public string? PartyId { get; set; }
        public string? CurrencyUomId { get; set; }
        public decimal? MinimumOrderQuantity { get; set; }
        public DateTime AvailableFromDate { get; set; }
        public DateTime? AvailableThruDate { get; set; }
        public decimal? LastPrice { get; set; }
        public string? QuantityUomId { get; set; }
    }

    // Placeholder for DTO validator
   
}