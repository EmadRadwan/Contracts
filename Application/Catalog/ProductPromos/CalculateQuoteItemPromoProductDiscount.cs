using Application.Catalog.Products;
using Application.Interfaces;
using Application.order.Quotes;
using Application.Order.Quotes;
using MediatR;
using Persistence;

namespace Application.Catalog.ProductPromos;

public class CalculateQuoteItemPromoProductDiscount
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

            // call CalculatePromoProductDiscount
            if (request.QuoteItemDto2.Quantity != null && request.QuoteItemDto2.UnitListPrice != null)
            {
                var promoResult = await _productService.CalculatePromoProductDiscount(
                    request.QuoteItemDto2.ProductPromoId, (int)request.QuoteItemDto2.Quantity,
                    (decimal)request.QuoteItemDto2.UnitListPrice, request.QuoteItemDto2.ProductId);

                // check if promoResult.ResultMessage is Success
                if (promoResult.ResultMessage == "Success")
                {
                    // create new QuoteItemDto2 and assign values from promoResult.QuoteItems to it
                    // and merge it with request.QuoteItemDto2
                    var newItem = new QuoteItemDto2
                    {
                        QuoteId = request.QuoteItemDto2.QuoteId,
                        ProductId = promoResult.ProductId,
                        ProductName = promoResult.ProductName + " - Promotion",
                        Quantity = promoResult.Quantity,
                        UnitListPrice = promoResult.ListPrice,
                        UnitPrice = promoResult.DefaultPrice,
                        IsPromo = "Y"
                    };

                    // invoke CalculateTax extension method on this
                    var taxAdjustments = await _quoteService.CalculateTax(newItem);
                    // add taxAdjustments to QuoteAdjustments
                    result.QuoteItemAdjustments.AddRange(taxAdjustments);


                    // create new QuoteAdjustment
                    var newQuoteAdjustment = new QuoteAdjustmentDto2
                    {
                        QuoteAdjustmentId = Guid.NewGuid().ToString(),
                        CorrespondingProductId = request.QuoteItemDto2.ProductId,
                        CorrespondingProductName = request.QuoteItemDto2.ProductName,
                        QuoteAdjustmentTypeId = "PROMOTION_ADJUSTMENT",
                        QuoteAdjustmentTypeDescription = promoResult.PromoText,
                        QuoteId = request.QuoteItemDto2.QuoteId,
                        ProductPromoId = promoResult.ProductPromoId,
                        Amount = promoResult.PromoAmount,
                        SourcePercentage = promoResult.PromoActionAmount,
                        Description = promoResult.PromoText,
                        IsManual = "N",
                        CreatedDate = stamp,
                        LastModifiedDate = stamp
                    };


                    // add new QuoteItem to QuoteItemsAndAdjustmentsDto.QuoteItems
                    result.QuoteItems.Add(newItem);
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