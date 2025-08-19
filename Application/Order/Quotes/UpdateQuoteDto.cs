namespace Application.Order.Quotes;

public class UpdateQuoteDto
{
    public string QuoteId { get; set; }
    public string? StatusId { get; set; }
    public string? FacilityId { get; set; }
    public string? ProductId { get; set; }
    public string? InternalName { get; set; }
}