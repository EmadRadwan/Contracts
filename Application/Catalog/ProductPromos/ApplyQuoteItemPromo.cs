using Application.Catalog.Products;
using Application.Interfaces;
using Application.order.Quotes;
using Application.Order.Quotes;
using MediatR;
using Persistence;

namespace Application.Catalog.ProductPromos;

public class ApplyQuoteItemPromo
{
    public class Command : IRequest<Result<QuoteItemPromoResultDto>>
    {
        public QuoteItemDto2 QuoteItemDto2 { get; set; }
    }


    public class Handler : IRequestHandler<Command, Result<QuoteItemPromoResultDto>>
    {
        private readonly DataContext _context;
        private readonly IProductService _productService;
        private readonly IQuoteService _quoteService;
        private readonly IUserAccessor _userAccessor;

        public Handler(DataContext context, IUserAccessor userAccessor, IQuoteService quoteService,
            IProductService productService)
        {
            _userAccessor = userAccessor;
            _context = context;
            _quoteService = quoteService;
            _productService = productService;
        }

        public async Task<Result<QuoteItemPromoResultDto>> Handle(Command request, CancellationToken cancellationToken)
        {
            var stamp = DateTime.UtcNow;


            var result = new QuoteItemPromoResultDto();

            // create switch statement for request.QuoteItemDto2.PromoActionEnumId
            // to determine which method to call

            // call CalculatePromoProductDiscount
            if (request.QuoteItemDto2.Quantity != null && request.QuoteItemDto2.UnitListPrice != null)
            {
                var promoResult = await _productService.CalculatePromoProductDiscount(
                    request.QuoteItemDto2.ProductPromoId!, (int)request.QuoteItemDto2.Quantity,
                    (decimal)request.QuoteItemDto2.UnitListPrice, request.QuoteItemDto2.ProductId!);

                // check if promoResult.ResultMessage is Success
                if (promoResult.ResultMessage == "Success")
                {
                    // create new QuoteItemDto2 and assign values from promoResult.QuoteItems to it
                    // and merge it with request.QuoteItemDto2
                    var newItem = new QuoteItemDto2
                    {
                        QuoteId = request.QuoteItemDto2.QuoteId,
                        ParentQuoteItemSeqId = request.QuoteItemDto2.QuoteItemSeqId,
                        ProductId = promoResult.ProductId,
                        ProductName = promoResult.ProductName + " - Promotion",
                        Quantity = promoResult.Quantity,
                        UnitPrice = promoResult.DefaultPrice,
                        UnitListPrice = promoResult.ListPrice,
                        DiscountAndPromotionAdjustments = promoResult.PromoAmount,
                        SubTotal = promoResult.PromoAmount,
                        QuoteItemTypeId = "PRODUCT_QUOTE_ITEM",
                        StatusId = "ITEM_CREATED",
                        IsPromo = "Y",
                        CreatedStamp = stamp,
                        LastUpdatedStamp = stamp
                    };


                    // create new QuoteAdjustment
                    var newQuoteAdjustment = new QuoteAdjustmentDto2
                    {
                        QuoteAdjustmentId = Guid.NewGuid().ToString(),
                        CorrespondingProductId = promoResult.ProductId,
                        CorrespondingProductName = promoResult.ProductName,
                        QuoteAdjustmentTypeId = "PROMOTION_ADJUSTMENT",
                        QuoteAdjustmentTypeDescription = promoResult.PromoText,
                        QuoteId = request.QuoteItemDto2.QuoteId,
                        ProductPromoId = promoResult.ProductPromoId,
                        ProductPromoRuleId = promoResult.ProductPromoRuleId,
                        ProductPromoActionSeqId = promoResult.ProductPromoActionSeqId,
                        Amount = promoResult.PromoAmount,
                        Description = promoResult.PromoText,
                        IsManual = "N",
                        CreatedDate = stamp,
                        LastModifiedDate = stamp
                    };


                    // add new QuoteItem to QuoteItemsAndAdjustmentsDto.QuoteItems
                    result.QuoteItems!.Add(newItem);
                    result.QuoteItemAdjustments.Add(newQuoteAdjustment);
                    result.ResultMessage = "Success";
                }
                else
                {
                    result.ResultMessage = promoResult.ResultMessage;
                }
            }


            return Result<QuoteItemPromoResultDto>.Success(result);
        }
    }
}