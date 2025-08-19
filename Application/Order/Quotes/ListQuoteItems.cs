using Application.Catalog.Products;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Order.Quotes;

public class ListQuoteItems
{
    public class Query : IRequest<Result<List<QuoteItemDto>>>
    {
        public string QuoteId { get; set; }
    }

    public class Handler : IRequestHandler<Query, Result<List<QuoteItemDto>>>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public Handler(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result<List<QuoteItemDto>>> Handle(Query request, CancellationToken cancellationToken)
        {
            var quoteItems = await (from itm in _context.QuoteItems
                join prd in _context.Products on itm.ProductId equals prd.ProductId
                where itm.QuoteId == request.QuoteId
                let discountAndPromoAdjustments = _context.QuoteAdjustments
                    .Where(adjustment =>
                        adjustment.QuoteId == itm.QuoteId &&
                        adjustment.QuoteItemSeqId == itm.QuoteItemSeqId &&
                        (adjustment.QuoteAdjustmentTypeId == "PROMOTION_ADJUSTMENT" ||
                         adjustment.QuoteAdjustmentTypeId == "DISCOUNT_ADJUSTMENT"))
                    .ToList() // Materialize the collection
                let totalItemDiscountAndPromotionsAdjustments =
                    discountAndPromoAdjustments.Sum(adjustment => adjustment.Amount)
                select new QuoteItemDto
                {
                    QuoteId = itm.QuoteId,
                    QuoteItemSeqId = itm.QuoteItemSeqId,
                    ProductId = itm.ProductId,
                    ProductTypeId = prd.ProductTypeId,
                    ProductName = itm.IsPromo == "Y" ? prd.ProductName + " (Promo)" : prd.ProductName,
                    IsPromo = itm.IsPromo,
                    Quantity = itm.Quantity,
                    UnitPrice = itm.QuoteUnitPrice,
                    UnitListPrice = itm.QuoteUnitListPrice,
                    IsProductDeleted = false,
                    ParentQuoteItemSeqId = itm.ParentQuoteItemSeqId,
                    TotalItemTaxAdjustments = _context.QuoteAdjustments
                        .Where(adjustment =>
                            adjustment.QuoteId == itm.QuoteId &&
                            adjustment.QuoteItemSeqId == itm.QuoteItemSeqId &&
                            (adjustment.QuoteAdjustmentTypeId == "SALES_TAX" ||
                             adjustment.QuoteAdjustmentTypeId == "VAT_TAX"))
                        .Sum(adjustment => adjustment.Amount),

                    DiscountAndPromotionAdjustments = totalItemDiscountAndPromotionsAdjustments,

                    SubTotal = itm.Quantity * itm.QuoteUnitPrice + totalItemDiscountAndPromotionsAdjustments
                }).ToListAsync();

            var result = new List<QuoteItemDto>();

            foreach (var quoteItem in quoteItems)
            {
                if (quoteItem.ParentQuoteItemSeqId != null)
                {
                    var normalItem = quoteItems.FirstOrDefault(x =>
                        x.QuoteItemSeqId == quoteItem.ParentQuoteItemSeqId);

                    if (normalItem != null)
                    {
                        var promoAdjustment = _context.QuoteAdjustments
                            .FirstOrDefault(x =>
                                x.QuoteId == quoteItem.QuoteId &&
                                x.QuoteItemSeqId == quoteItem.QuoteItemSeqId);

                        if (promoAdjustment != null) normalItem.ProductPromoId = promoAdjustment.ProductPromoId;
                    }
                }

                var quoteItemProduct = await (from prd in _context.Products
                        join prc in _context.ProductPrices on prd.ProductId equals prc.ProductId
                        join inv in _context.InventoryItems on prd.ProductId equals inv.ProductId
                        join fac in _context.Facilities on inv.FacilityId equals fac.FacilityId
                        where prd.ProductId == quoteItem.ProductId
                        select new ProductLovDto
                        {
                            ProductId = prd.ProductId,
                            ProductName = prd.ProductName,
                            FacilityName = fac.FacilityName,
                            InventoryItem = inv.InventoryItemId,
                            QuantityOnHandTotal = inv.QuantityOnHandTotal,
                            AvailableToPromiseTotal = inv.AvailableToPromiseTotal,
                            Price = prc.Price
                        }
                    ).FirstOrDefaultAsync();

                if (quoteItemProduct != null)
                {
                    quoteItem.QuoteItemProduct = quoteItemProduct;
                    result.Add(quoteItem);
                }
            }

            return Result<List<QuoteItemDto>>.Success(quoteItems);
        }
    }
}