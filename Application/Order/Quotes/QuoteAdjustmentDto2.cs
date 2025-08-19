namespace Application.Order.Quotes;

public class QuoteAdjustmentDto2
{
    public string QuoteAdjustmentId { get; set; }
    public string QuoteAdjustmentTypeId { get; set; }
    public string QuoteAdjustmentTypeDescription { get; set; }
    public string QuoteId { get; set; }
    public string? ProductPromoRuleId { get; set; }
    public string? ProductPromoActionSeqId { get; set; }
    public string? QuoteItemSeqId { get; set; }
    public string? Comments { get; set; }
    public string? Description { get; set; }
    public string? ProductPromoId { get; set; }

    public DateTime? LastModifiedDate { get; set; }
    public string? IsManual { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? TaxAuthorityRateSeqId { get; set; }
    public string? TaxAuthGeoId { get; set; }
    public string? TaxAuthPartyId { get; set; }


    public decimal? Amount { get; set; }
    public string CorrespondingProductId { get; set; }
    public string? CorrespondingProductName { get; set; }
    public string? OverrideGlAccountId { get; set; }

    public decimal? SourcePercentage { get; set; }
    public bool IsAdjustmentDeleted { get; set; }
}