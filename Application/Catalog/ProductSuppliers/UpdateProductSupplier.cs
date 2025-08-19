using Application.Core;
using Application.Interfaces;
using Application.Order.Orders;
using Domain;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductSuppliers;

public class UpdateProductSupplier
{
    // REFACTOR: Replaced SupplierProduct entity with DTO for front-end input
    // Enhances security and flexibility by decoupling the API from the database entity
    public class Command : IRequest<Result<SupplierProductDto>>
    {
        public SupplierProductUpdateDto SupplierProduct { get; set; }
    }

    

    public class Handler : IRequestHandler<Command, Result<SupplierProductDto>>
    {
        private readonly DataContext _context;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor)
        {
            _context = context;
            _userAccessor = userAccessor;
        }

        public async Task<Result<SupplierProductDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            // REFACTOR: Removed AutoMapper dependency and implemented manual mapping
            // Improves performance and provides explicit control over data mapping
            var requestDate = request.SupplierProduct.AvailableFromDate.Value.Date;
            var supplierProduct = await _context.SupplierProducts
                .Include(sp => sp.Party)
                .Include(sp => sp.CurrencyUom)
                .Include(sp => sp.QuantityUom)
                .FirstOrDefaultAsync(x => x.ProductId == request.SupplierProduct.ProductId
                                          && x.PartyId == request.SupplierProduct.PartyId
                                          && x.CurrencyUomId == request.SupplierProduct.CurrencyUomId
                                          && x.AvailableFromDate.Date == requestDate,
                    cancellationToken);

            
            if (supplierProduct == null)
                return Result<SupplierProductDto>.Failure("Product Supplier not found");
            
            // REFACTOR: Manually update entity properties from DTO
            // Ensures controlled updates and avoids overwriting unintended fields
            supplierProduct.AvailableThruDate = request.SupplierProduct.AvailableThruDate;
            supplierProduct.LastPrice = request.SupplierProduct.LastPrice;
            supplierProduct.QuantityUomId = request.SupplierProduct.QuantityUomId;
            supplierProduct.MinimumOrderQuantity = request.SupplierProduct.MinimumOrderQuantity ?? 0;
            supplierProduct.LastUpdatedStamp = DateTime.UtcNow;

            var result = await _context.SaveChangesAsync(cancellationToken) > 0;

            if (!result) return Result<SupplierProductDto>.Failure("Failed to update Product Supplier");

            // REFACTOR: Manual mapping to SupplierProductDto for response
            // Ensures consistency with front-end expectations
            var productSupplierToReturn = new SupplierProductDto
            {
                FromPartyId = new OrderPartyDto
                {
                    FromPartyId = supplierProduct.Party.PartyId,
                    FromPartyName = supplierProduct.Party.Description ?? string.Empty
                },
                CurrencyUomDescription = supplierProduct.CurrencyUom.Description,
                QuantityUomDescription = supplierProduct.QuantityUom != null
                    ? supplierProduct.QuantityUom.Description
                    : null,
                PartyName = supplierProduct.Party.Description,
                AvailableFromDate = DateTime.SpecifyKind(
                    supplierProduct.AvailableFromDate.Truncate(TimeSpan.FromSeconds(1)),
                    DateTimeKind.Utc),
                LastPrice = supplierProduct.LastPrice
            };

            return Result<SupplierProductDto>.Success(productSupplierToReturn);
        }
    }

    // Define DTOs for clarity
    public class SupplierProductUpdateDto
    {
        public string? ProductId { get; set; }
        public string? PartyId { get; set; }
        public string? CurrencyUomId { get; set; }
        public decimal? MinimumOrderQuantity { get; set; }
        public DateTime? AvailableFromDate { get; set; }
        public DateTime? AvailableThruDate { get; set; }
        public decimal? LastPrice { get; set; }
        public string? QuantityUomId { get; set; }
    }

    // Placeholder for DTO validator

}