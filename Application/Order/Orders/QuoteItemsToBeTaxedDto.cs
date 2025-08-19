using Application.Order.Quotes;

namespace Application.Order.Orders;

public class QuoteItemsToBeTaxedDto
{
    public List<QuoteItemDto2> QuoteItems { get; set; }
}