namespace Application.Order.Quotes;

public class QuoteItemsAndAdjustmentsDto
{
    public ICollection<QuoteItemDto> QuoteItems { get; set; }
    public ICollection<QuoteAdjustmentDto2> QuoteAdjustments { get; set; }
}