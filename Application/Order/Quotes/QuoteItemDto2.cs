namespace Application.Order.Quotes;

public class QuoteItemDto2
{
    public string QuoteId { get; set; }
    public string QuoteItemSeqId { get; set; }
    public string ProductId { get; set; }
    public string? ProductTypeId { get; set; }
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
    public decimal? UnitPrice { get; set; }
    public decimal? UnitListPrice { get; set; }


    public decimal? TotalItemAdjustments { get; set; }
    public decimal? SubTotal { get; set; }
    public DateTime? ReservStart { get; set; }
    public decimal? ReservLength { get; set; }
    public decimal? ReservPersons { get; set; }
    public string? ConfigId { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? Comments { get; set; }
    public string? IsPromo { get; set; }
    public string? ProductPromoId { get; set; }

    public decimal? LeadTimeDays { get; set; }
    public string? ParentQuoteItemSeqId { get; set; }
    public string? QuoteItemTypeId { get; set; }
    public string? StatusId { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }


    public decimal? DiscountAndPromotionAdjustments { get; set; }
}