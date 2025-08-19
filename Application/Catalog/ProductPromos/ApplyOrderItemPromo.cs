using Application.Catalog.Products;
using Application.Interfaces;
using Application.order.Orders;
using Application.Order.Orders;
using MediatR;
using Persistence;

namespace Application.Catalog.ProductPromos;

public class ApplyOrderItemPromo
{
    public class Command : IRequest<Result<OrderItemPromoResultDto>>
    {
        public OrderItemDto2 OrderItemDto2 { get; set; }
    }


    public class Handler : IRequestHandler<Command, Result<OrderItemPromoResultDto>>
    {
        private readonly DataContext _context;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor, IOrderService orderService,
            IProductService productService)
        {
            _userAccessor = userAccessor;
            _context = context;
            _orderService = orderService;
            _productService = productService;
        }

        public async Task<Result<OrderItemPromoResultDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var stamp = DateTime.UtcNow;


            var result = new OrderItemPromoResultDto();

            // create switch statement for request.OrderItemDto2.PromoActionEnumId
            // to determine which method to call

            // call CalculatePromoProductDiscount
            if (request.OrderItemDto2.Quantity != null && request.OrderItemDto2.UnitListPrice != null)
            {
                var promoResult = await _productService.CalculatePromoProductDiscount(
                    request.OrderItemDto2.ProductPromoId!, (int)request.OrderItemDto2.Quantity,
                    (decimal)request.OrderItemDto2.UnitListPrice, request.OrderItemDto2.ProductId!);

                // check if promoResult.ResultMessage is Success
                if (promoResult.ResultMessage == "Success")
                {
                    // create new OrderItemDto2 and assign values from promoResult.OrderItems to it
                    // and merge it with request.OrderItemDto2
                    var newItem = new OrderItemDto2
                    {
                        OrderId = request.OrderItemDto2.OrderId,
                        ParentOrderItemSeqId = request.OrderItemDto2.OrderItemSeqId,
                        ProductId = promoResult.ProductId,
                        ProductName = promoResult.ProductName + " - Promotion",
                        Quantity = promoResult.Quantity,
                        UnitPrice = promoResult.DefaultPrice,
                        UnitListPrice = promoResult.ListPrice,
                        DiscountAndPromotionAdjustments = promoResult.PromoAmount,
                        SubTotal = promoResult.PromoAmount,
                        OrderItemTypeId = "PRODUCT_ORDER_ITEM",
                        StatusId = "ITEM_CREATED",
                        IsPromo = "Y",
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };


                    // create new OrderAdjustment
                    var newOrderAdjustment = new OrderAdjustmentDto2
                    {
                        OrderAdjustmentId = Guid.NewGuid().ToString(),
                        CorrespondingProductId = promoResult.ProductId,
                        CorrespondingProductName = promoResult.ProductName,
                        OrderAdjustmentTypeId = "PROMOTION_ADJUSTMENT",
                        OrderAdjustmentTypeDescription = promoResult.PromoText,
                        OrderId = request.OrderItemDto2.OrderId,
                        ProductPromoId = promoResult.ProductPromoId,
                        ProductPromoRuleId = promoResult.ProductPromoRuleId,
                        ProductPromoActionSeqId = promoResult.ProductPromoActionSeqId,
                        Amount = promoResult.PromoAmount,
                        Description = promoResult.PromoText,
                        IsManual = "N",
                        CreatedDate = stamp,
                        LastModifiedDate = stamp
                    };


                    // add new OrderItem to OrderItemsAndAdjustmentsDto.OrderItems
                    result.OrderItems!.Add(newItem);
                    result.OrderItemAdjustments.Add(newOrderAdjustment);
                    result.ResultMessage = "Success";
                }
                else
                {
                    result.ResultMessage = promoResult.ResultMessage;
                }
            }


            return Result<OrderItemPromoResultDto>.Success(result);
        }
    }
}