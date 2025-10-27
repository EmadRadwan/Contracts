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
            if (string.IsNullOrEmpty(request.OrderId))
            {
                return Result<List<OrderItemDto2>>.Failure("OrderId cannot be null or empty.");
            }

            var language = request.Language ?? "en";

            var productStoreInventoryFacilityId = await _productStoreService.GetProductFacilityId();

            var orderItems = await (from itm in _context.OrderItems.AsNoTracking()
                join prd in _context.Products.AsNoTracking() on itm.ProductId equals prd.ProductId
                join uom in _context.Uoms.AsNoTracking() on itm.UomId equals uom.UomId into uomGroup
                from uom in uomGroup.DefaultIfEmpty()
                where itm.OrderId == request.OrderId
                let discountAdjustments = _context.OrderAdjustments
                    .AsNoTracking()
                    .Where(adjustment => adjustment.OrderId == itm.OrderId
                                         && adjustment.OrderItemSeqId == itm.OrderItemSeqId
                                         && adjustment.OrderAdjustmentTypeId == "DISCOUNT_ADJUSTMENT")
                    .ToList()
                let totalDiscountAdjustments = discountAdjustments.Sum(adjustment => adjustment.Amount)
                select new OrderItemDto2
                {
                    OrderId = itm.OrderId,
                    OrderItemSeqId = itm.OrderItemSeqId,
                    ProductId = itm.ProductId,
                    ProductName = prd.ProductName,
                    Quantity = itm.Quantity,
                    UnitPrice = itm.UnitPrice,
                    SubTotal = itm.Quantity * itm.UnitPrice,
                    IsProductDeleted = false,
                    FacilityId = productStoreInventoryFacilityId,
                    ValidItem = true,
                    TotalItemTaxAdjustments = _context.OrderAdjustments
                        .AsNoTracking()
                        .Where(adjustment => adjustment.OrderId == itm.OrderId
                                             && adjustment.OrderItemSeqId == itm.OrderItemSeqId
                                             && (adjustment.OrderAdjustmentTypeId == "SALES_TAX"
                                                 || adjustment.OrderAdjustmentTypeId == "VAT_TAX"))
                        .Sum(adjustment => adjustment.Amount),
                    DiscountAndPromotionAdjustments = totalDiscountAdjustments,
                    UomId = uom != null ? uom.UomId : null,
                    UomName = uom != null ? (language == "ar" ? uom.DescriptionArabic : uom.Description) : null
                }).ToListAsync(cancellationToken);

            var result = new List<OrderItemDto2>();

            foreach (var orderItem in orderItems)
            {
                var shipmentReceipts = _context.ShipmentReceipts
                    .AsNoTracking()
                    .Where(x => x.OrderId == orderItem.OrderId && x.OrderItemSeqId == orderItem.OrderItemSeqId)
                    .ToList();

                orderItem.QuantityAccepted = shipmentReceipts.Sum(x => x.QuantityAccepted) ?? 0;
                orderItem.QuantityRejected = shipmentReceipts.Sum(x => x.QuantityRejected) ?? 0;
                orderItem.IncludeThisItem = false;

                var orderItemProduct = await (from prd in _context.Products.AsNoTracking()
                    join sp in _context.SupplierProducts.AsNoTracking() on prd.ProductId equals sp.ProductId into
                        spGroup
                    from sp in spGroup.DefaultIfEmpty()
                    join uom in _context.Uoms.AsNoTracking() on prd.QuantityUomId equals uom.UomId into uomGroup
                    from uom in uomGroup.DefaultIfEmpty()
                    join iif in _context.InventoryItemFeatures.AsNoTracking() on prd.ProductId equals iif.ProductId into
                        iifGroup
                    from iif in iifGroup.DefaultIfEmpty()
                    join pf in _context.ProductFeatures.AsNoTracking()
                            .Where(pf => pf.ProductFeatureTypeId == "COLOR") on iif != null
                            ? iif.ProductFeatureId
                            : null
                        equals pf.ProductFeatureId into pfGroup
                    from pf in pfGroup.DefaultIfEmpty()
                    where prd.ProductId == orderItem.ProductId
                    select new ProductLovDto
                    {
                        ProductId = prd.ProductId,
                        ProductName = prd.ProductName,
                        ColorDescription =
                            pf != null ? (language == "ar" ? pf.DescriptionArabic : pf.Description) : null,
                        LastPrice = sp != null ? sp.LastPrice : null,
                        QuantityUom = uom != null ? uom.UomId : null,
                        UomDescription =
                            uom != null ? (language == "ar" ? uom.DescriptionArabic : uom.Description) : null
                    }).FirstOrDefaultAsync(cancellationToken);

                if (orderItemProduct != null)
                {
                    orderItem.ProductName = orderItemProduct.ProductName + " " +
                                            (orderItemProduct.ColorDescription ?? string.Empty);
                    orderItem.OrderItemProduct = orderItemProduct;
                }

                result.Add(orderItem);
            }

            return Result<List<OrderItemDto2>>.Success(result);
        }
    }
}