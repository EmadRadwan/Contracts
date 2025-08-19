namespace Application.Order.Quotes;

public class QuoteItemPromoResultDto
{
    public string ResultMessage { get; set; } = string.Empty;
    public List<QuoteItemDto2>? QuoteItems { get; set; } = new();
    public List<QuoteAdjustmentDto2> QuoteItemAdjustments { get; set; } = new();
}