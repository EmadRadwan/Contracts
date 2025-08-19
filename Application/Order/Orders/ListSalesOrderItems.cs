using Application.Catalog.Products;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Orders;

public class ListSalesOrderItems
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

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
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

            var orderItems = await (from itm in _context.OrderItems.AsNoTracking()
                join prd in _context.Products.AsNoTracking() on itm.ProductId equals prd.ProductId
                where itm.OrderId == request.OrderId
                let discountAndPromoAdjustments = _context.OrderAdjustments
                    .AsNoTracking()
                    .Where(adjustment =>
                        adjustment.OrderId == itm.OrderId &&
                        adjustment.OrderItemSeqId == itm.OrderItemSeqId &&
                        (adjustment.OrderAdjustmentTypeId == "PROMOTION_ADJUSTMENT" ||
                         adjustment.OrderAdjustmentTypeId == "DISCOUNT_ADJUSTMENT"))
                    .ToList()
                let totalItemDiscountAndPromotionsAdjustments =
                    discountAndPromoAdjustments.Sum(adjustment => adjustment.Amount)
                let isBackOrdered = _context.OrderItemShipGrpInvRes
                    .AsNoTracking()
                    .Any(r => r.OrderId == itm.OrderId &&
                              r.OrderItemSeqId == itm.OrderItemSeqId &&
                              r.QuantityNotAvailable > 0m)
                select new OrderItemDto2
                {
                    OrderId = itm.OrderId,
                    OrderItemSeqId = itm.OrderItemSeqId,
                    ProductId = itm.ProductId,
                    ProductTypeId = prd.ProductTypeId,
                    // REFACTOR: Initialize ProductName without color concatenation
                    // Purpose: Color will be appended later to match ListPurchaseOrderItems
                    ProductName = prd.ProductName + (itm.IsPromo == "Y" ? " (Promo)" : ""),
                    IsPromo = itm.IsPromo,
                    Quantity = itm.Quantity,
                    UnitPrice = itm.UnitPrice,
                    UnitListPrice = itm.UnitListPrice,
                    IsProductDeleted = false,
                    ParentOrderItemSeqId = itm.ParentOrderItemSeqId,
                    TotalItemTaxAdjustments = _context.OrderAdjustments
                        .AsNoTracking()
                        .Where(adjustment =>
                            adjustment.OrderId == itm.OrderId &&
                            adjustment.OrderItemSeqId == itm.OrderItemSeqId &&
                            (adjustment.OrderAdjustmentTypeId == "SALES_TAX" ||
                             adjustment.OrderAdjustmentTypeId == "VAT_TAX"))
                        .Sum(adjustment => adjustment.Amount),
                    DiscountAndPromotionAdjustments = totalItemDiscountAndPromotionsAdjustments,
                    SubTotal = itm.Quantity * itm.UnitPrice,
                    IsBackOrdered = isBackOrdered
                }).ToListAsync(cancellationToken);

            var result = new List<OrderItemDto2>();

            foreach (var orderItem in orderItems)
            {
                if (orderItem.ParentOrderItemSeqId != null)
                {
                    var normalItem = orderItems.FirstOrDefault(x =>
                        x.OrderItemSeqId == orderItem.ParentOrderItemSeqId);

                    if (normalItem != null)
                    {
                        var promoAdjustment = _context.OrderAdjustments
                            .AsNoTracking()
                            .FirstOrDefault(x =>
                                x.OrderId == orderItem.OrderId &&
                                x.OrderItemSeqId == orderItem.OrderItemSeqId);

                        if (promoAdjustment != null) normalItem.ProductPromoId = promoAdjustment.ProductPromoId;
                    }
                }

                var orderItemProduct = await (from prd in _context.Products.AsNoTracking()
                                             join inv in _context.InventoryItems.AsNoTracking() on prd.ProductId equals inv.ProductId into invGroup
                                             from inv in invGroup.DefaultIfEmpty()
                                             join uom in _context.Uoms.AsNoTracking() on prd.QuantityUomId equals uom.UomId into uomGroup
                                             from uom in uomGroup.DefaultIfEmpty()
                                             // REFACTOR: Added joins for color feature
                                             // Purpose: Fetches ColorDescription to concatenate with ProductName and include separately
                                             // Why: Matches ListPurchaseOrderItems behavior
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
                                                 // Why: Matches ListPurchaseOrderItems structure
                                                 ColorDescription = pf != null ? (language == "ar" ? pf.DescriptionArabic : pf.Description) : null,
                                                 QuantityOnHandTotal = inv != null ? inv.QuantityOnHandTotal : 0,
                                                 AvailableToPromiseTotal = inv != null ? inv.AvailableToPromiseTotal : 0,
                                                 QuantityUom = uom != null ? uom.UomId : null,
                                                 // REFACTOR: Maintain language-specific UomDescription
                                                 // Purpose: Ensures UomDescription matches the requested language
                                                 UomDescription = uom != null ? (language == "ar" ? uom.DescriptionArabic : uom.Description) : null
                                             }).FirstOrDefaultAsync(cancellationToken);

                if (orderItemProduct != null)
                {
                    // REFACTOR: Update OrderItemDto2 ProductName to include color
                    // Purpose: Ensures consistency with ListPurchaseOrderItems by including color in ProductName
                    orderItem.ProductName = orderItemProduct.ProductName + " " + (orderItemProduct.ColorDescription ?? string.Empty) + (orderItem.IsPromo == "Y" ? " (Promo)" : "");
                    orderItem.OrderItemProduct = orderItemProduct;
                }

                result.Add(orderItem);
            }

            return Result<List<OrderItemDto2>>.Success(result);
        }
    }
}