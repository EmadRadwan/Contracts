using Application.Catalog.Products;
using Application.Catalog.ProductStores;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders;

public class ListPurchaseOrderItems
{
    public class Query : IRequest<Result<List<OrderItemDto2>>>
    {
        public string OrderId { get; set; }
        public string Language { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<OrderItemDto2>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IProductStoreService _productStoreService;

        public Handler(DataContext context, IMapper mapper, IProductStoreService productStoreService)
        {
            _context = context;
            _mapper = mapper;
            _productStoreService = productStoreService;
        }

        public async Task<Result<List<OrderItemDto2>>> Handle(Query request, CancellationToken cancellationToken)
        {
            // REFACTOR: Added null check for request.OrderId
            // Purpose: Prevents null reference exceptions
            if (string.IsNullOrEmpty(request.OrderId))
            {
                return Result<List<OrderItemDto2>>.Failure("OrderId cannot be null or empty.");
            }

            // REFACTOR: Default to English for language
            // Purpose: Ensures consistent behavior if Language is null
            var language = request.Language ?? "en";

            var productStoreInventoryFacilityId = await _productStoreService.GetProductFacilityId();
            var orderItems = (from itm in _context.OrderItems
                join prd in _context.Products on itm.ProductId equals prd.ProductId
                where itm.OrderId == request.OrderId
                select new OrderItemDto2
                {
                    OrderId = itm.OrderId,
                    OrderItemSeqId = itm.OrderItemSeqId,
                    ProductId = itm.ProductId,
                    // REFACTOR: Use language-specific ProductName
                    // Purpose: Selects ProductNameArabic for 'ar', ProductName for 'en'
                    ProductName = prd.ProductName,
                    Quantity = itm.Quantity,
                    UnitPrice = itm.UnitPrice,
                    SubTotal = itm.Quantity * itm.UnitPrice,
                    IsProductDeleted = false,
                    FacilityId = productStoreInventoryFacilityId,
                    ValidItem = true
                }).ToList();

            // Loop through orderItems to check shipment receipts
            foreach (var item in orderItems)
            {
                var shipmentReceipts = _context.ShipmentReceipts
                    .Where(x => x.OrderId == item.OrderId && x.OrderItemSeqId == x.OrderItemSeqId)
                    .ToList();
                // REFACTOR: Simplified null-coalescing for sums
                // Purpose: Improves readability and ensures default value of 0
                item.QuantityAccepted = shipmentReceipts.Sum(x => x.QuantityAccepted) ?? 0;
                item.QuantityRejected = shipmentReceipts.Sum(x => x.QuantityRejected) ?? 0;
                item.IncludeThisItem = false;
            }

            var result = new List<OrderItemDto2>();

            foreach (var orderItem in orderItems)
            {
                var orderItemProduct = await (from prd in _context.Products.AsNoTracking()
                                             join sp in _context.SupplierProducts.AsNoTracking() on prd.ProductId equals sp.ProductId into spGroup
                                             from sp in spGroup.DefaultIfEmpty()
                                             join uom in _context.Uoms.AsNoTracking() on prd.QuantityUomId equals uom.UomId into uomGroup
                                             from uom in uomGroup.DefaultIfEmpty()
                                             // REFACTOR: Added joins for color feature
                                             // Purpose: Fetches ColorDescription to concatenate with ProductName and include separately
                                             // Why: Matches GetPurchaseProductsLov behavior
                                             join iif in _context.InventoryItemFeatures.AsNoTracking() on prd.ProductId equals iif.ProductId into iifGroup
                                             from iif in iifGroup.DefaultIfEmpty()
                                             join pf in _context.ProductFeatures.AsNoTracking()
                                                 .Where(pf => pf.ProductFeatureTypeId == "COLOR") on iif != null ? iif.ProductFeatureId : null equals pf.ProductFeatureId into pfGroup
                                             from pf in pfGroup.DefaultIfEmpty()
                                             where prd.ProductId == orderItem.ProductId
                                             select new ProductLovDto
                                             {
                                                 ProductId = prd.ProductId,
                                                 // REFACTOR: Concatenate language-specific ColorDescription with ProductName
                                                 // Purpose: Includes color in ProductName, sensitive to language
                                                 // Why: Matches GetPurchaseProductsLov and older version's language handling
                                                 ProductName = prd.ProductName,
                                                 // REFACTOR: Added ColorDescription to projection
                                                 // Purpose: Explicitly includes language-specific color description
                                                 // Why: Matches GetPurchaseProductsLov structure
                                                 ColorDescription = pf != null ? (language == "ar" ? pf.DescriptionArabic : pf.Description) : null,
                                                 LastPrice = sp != null ? sp.LastPrice : null,
                                                 QuantityUom = uom != null ? uom.UomId : null,
                                                 // REFACTOR: Maintain language-specific UomDescription
                                                 // Purpose: Ensures UomDescription matches the requested language
                                                 UomDescription = uom != null ? (language == "ar" ? uom.DescriptionArabic : uom.Description) : null
                                             }).FirstOrDefaultAsync(cancellationToken);

                if (orderItemProduct != null)
                {
                    // REFACTOR: Update OrderItemDto2 ProductName to include color
                    // Purpose: Ensures consistency across the DTO
                    orderItem.ProductName = orderItemProduct.ProductName + " " + (orderItemProduct.ColorDescription ?? string.Empty);
                    orderItem.OrderItemProduct = orderItemProduct;
                }

                result.Add(orderItem);
            }

            return Result<List<OrderItemDto2>>.Success(result);
        }
    }
}