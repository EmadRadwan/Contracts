using Application.Core;
using Application.Order.Orders;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Catalog.ProductSuppliers;

public class ListProductSuppliers
{
    public class Query : IRequest<Result<List<SupplierProductDto>>>
    {
        public string ProductId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<SupplierProductDto>>>
    {
        private readonly DataContext _context;

        public Handler(DataContext context)
        {
            _context = context;
        }

        public async Task<Result<List<SupplierProductDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Removed AutoMapper dependency and implemented manual projection
            // Improves performance by avoiding AutoMapper overhead and provides explicit control over mapping logic
            var query = _context.SupplierProducts
                // REFACTOR: Added explicit join with Parties table to ensure party data is properly retrieved
                // Improves query reliability by explicitly defining the relationship and handling nulls
                .Join(_context.Parties,
                    sp => sp.PartyId,
                    p => p.PartyId,
                    (sp, p) => new { SupplierProduct = sp, Party = p })
                .Where(x => x.SupplierProduct.ProductId == request.ProductId)
                .OrderBy(x => x.SupplierProduct.ProductId)
                .Select(x => new SupplierProductDto
                {
                    // REFACTOR: Updated mappings to use joined Party data
                    // Ensures consistent access to Party properties and aligns with explicit join
                    FromPartyId = new OrderPartyDto
                    {
                        FromPartyId = x.Party.PartyId,
                        FromPartyName = x.Party.Description ?? string.Empty
                    },
                    CurrencyUomId = x.SupplierProduct.CurrencyUomId,
                    CurrencyUomDescription = x.SupplierProduct.CurrencyUom.Description,
                    QuantityUomId = x.SupplierProduct.QuantityUomId,
                    QuantityUomDescription = x.SupplierProduct.QuantityUom != null ? x.SupplierProduct.QuantityUom.Description : null,
                    PartyName = x.Party.Description,
                    AvailableFromDate = x.SupplierProduct.AvailableFromDate,
                    AvailableThruDate = x.SupplierProduct.AvailableThruDate,
                    LastPrice = x.SupplierProduct.LastPrice,
                    MinimumOrderQuantity = x.SupplierProduct.MinimumOrderQuantity,
                })
                .AsQueryable();

            var queryString = query.ToQueryString();

            var result = await query.ToListAsync(cancellationToken);
            return Result<List<SupplierProductDto>>.Success(result);
        }
    }
}