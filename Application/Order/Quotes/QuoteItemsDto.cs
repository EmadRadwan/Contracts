namespace Application.Order.Quotes;

public class QuoteItemsDto
{
    public string QuoteId { get; set; }
    public string StatusDescription { get; set; }
    public string ModificationType { get; set; }
    public List<QuoteItemDto2> QuoteItems { get; set; }
}