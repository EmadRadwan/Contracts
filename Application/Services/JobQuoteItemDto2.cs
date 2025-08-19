namespace Application.Order.Quotes;

public class JobQuoteItemDto2
{
    public string QuoteId { get; set; }
    public string QuoteItemSeqId { get; set; }
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public bool IsProductDeleted { get; set; }
    public string? ProductFeatureId { get; set; }
    public string? DeliverableTypeId { get; set; }
    public string? SkillTypeId { get; set; }
    public string? UomId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? CustRequestId { get; set; }
    public string? CustRequestItemSeqId { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? SelectedAmount { get; set; }
    public decimal? QuoteUnitPrice { get; set; }
    public DateTime? ReservStart { get; set; }
    public decimal? ReservLength { get; set; }
    public decimal? ReservPersons { get; set; }
    public string? ConfigId { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? Comments { get; set; }
    public string? IsPromo { get; set; }
    public decimal? LeadTimeDays { get; set; }
}