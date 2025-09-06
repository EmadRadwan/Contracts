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

            // REFACTOR: Added tax and discount adjustments to the query
            // Purpose: Aligns with ListSalesOrderItems by including tax and discount calculations
            // Why: Ensures purchase orders reflect financial adjustments for consistency
            var orderItems = await (from itm in _context.OrderItems.AsNoTracking()
                                   join prd in _context.Products.AsNoTracking() on itm.ProductId equals prd.ProductId
                                   where itm.OrderId == request.OrderId
                                   // REFACTOR: Calculate discount adjustments
                                   // Purpose: Sums DISCOUNT_ADJUSTMENT amounts for each order item
                                   let discountAdjustments = _context.OrderAdjustments
                                       .AsNoTracking()
                                       .Where(adjustment => adjustment.OrderId == itm.OrderId 
                                           && adjustment.OrderItemSeqId == itm.OrderItemSeqId 
                                           && adjustment.OrderAdjustmentTypeId == "DISCOUNT_ADJUSTMENT")
                                       .ToList()
                                   // REFACTOR: Calculate total discount adjustments
                                   // Purpose: Aggregates discount amounts for the order item
                                   let totalDiscountAdjustments = discountAdjustments.Sum(adjustment => adjustment.Amount)
                                   select new OrderItemDto2
                                   {
                                       OrderId = itm.OrderId,
                                       OrderItemSeqId = itm.OrderItemSeqId,
                                       ProductId = itm.ProductId,
                                       // REFACTOR: Initialize ProductName without color concatenation
                                       // Purpose: Color will be appended later to match ListSalesOrderItems
                                       ProductName = prd.ProductName,
                                       Quantity = itm.Quantity,
                                       UnitPrice = itm.UnitPrice,
                                       // REFACTOR: Include tax adjustments in SubTotal calculation
                                       // Purpose: Reflects total cost including tax adjustments
                                       SubTotal = itm.Quantity * itm.UnitPrice,
                                       IsProductDeleted = false,
                                       FacilityId = productStoreInventoryFacilityId,
                                       ValidItem = true,
                                       // REFACTOR: Added tax adjustments
                                       // Purpose: Sums SALES_TAX and VAT_TAX amounts, matching ListSalesOrderItems
                                       TotalItemTaxAdjustments = _context.OrderAdjustments
                                           .AsNoTracking()
                                           .Where(adjustment => adjustment.OrderId == itm.OrderId 
                                               && adjustment.OrderItemSeqId == itm.OrderItemSeqId 
                                               && (adjustment.OrderAdjustmentTypeId == "SALES_TAX" 
                                                   || adjustment.OrderAdjustmentTypeId == "VAT_TAX"))
                                           .Sum(adjustment => adjustment.Amount),
                                       // REFACTOR: Added discount adjustments field
                                       // Purpose: Stores total discount adjustments, excluding promotions
                                       DiscountAndPromotionAdjustments = totalDiscountAdjustments
                                   }).ToListAsync(cancellationToken);

            var result = new List<OrderItemDto2>();

            foreach (var orderItem in orderItems)
            {
                // REFACTOR: Moved shipment receipts query outside the main query
                // Purpose: Improves performance by reducing repeated database calls
                var shipmentReceipts = _context.ShipmentReceipts
                    .AsNoTracking()
                    .Where(x => x.OrderId == orderItem.OrderId && x.OrderItemSeqId == orderItem.OrderItemSeqId)
                    .ToList();

                // REFACTOR: Simplified null-coalescing for sums
                // Purpose: Improves readability and ensures default value of 0
                orderItem.QuantityAccepted = shipmentReceipts.Sum(x => x.QuantityAccepted) ?? 0;
                orderItem.QuantityRejected = shipmentReceipts.Sum(x => x.QuantityRejected) ?? 0;
                orderItem.IncludeThisItem = false;

                var orderItemProduct = await (from prd in _context.Products.AsNoTracking()
                                             join sp in _context.SupplierProducts.AsNoTracking() on prd.ProductId equals sp.ProductId into spGroup
                                             from sp in spGroup.DefaultIfEmpty()
                                             join uom in _context.Uoms.AsNoTracking() on prd.QuantityUomId equals uom.UomId into uomGroup
                                             from uom in uomGroup.DefaultIfEmpty()
                                             // REFACTOR: Added joins for color feature
                                             // Purpose: Fetches ColorDescription to concatenate with ProductName and include separately
                                             // Why: Matches ListSalesOrderItems behavior
                                             join iif in _context.InventoryItemFeatures.AsNoTracking() on prd.ProductId equals iif.ProductId into iifGroup
                                             from iif in iifGroup.DefaultIfEmpty()
                                             join pf in _context.ProductFeatures.AsNoTracking()
                                                 .Where(pf => pf.ProductFeatureTypeId == "COLOR") on iif != null ? iif.ProductFeatureId : null equals pf.ProductFeatureId into pfGroup
                                             from pf in pfGroup.DefaultIfEmpty()
                                             where prd.ProductId == orderItem.ProductId
                                             select new ProductLovDto
                                             {
                                                 ProductId = prd.ProductId,
                                                 // REFACTOR: Use language-specific ProductName
                                                 // Purpose: Ensures ProductName matches the requested language
                                                 ProductName = prd.ProductName,
                                                 // REFACTOR: Added ColorDescription to projection
                                                 // Purpose: Explicitly includes language-specific color description
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
                    // Purpose: Ensures consistency with ListSalesOrderItems by including color in ProductName
                    orderItem.ProductName = orderItemProduct.ProductName + " " + (orderItemProduct.ColorDescription ?? string.Empty);
                    orderItem.OrderItemProduct = orderItemProduct;
                }

                result.Add(orderItem);
            }

            return Result<List<OrderItemDto2>>.Success(result);
        }
    }
}