namespace Domain;

public class TrackingCodeOrderReturn
{
    public string ReturnId { get; set; } = null!;
    public string OrderId { get; set; } = null!;
    public string? OrderItemSeqId { get; set; }
    public string TrackingCodeTypeId { get; set; } = null!;
    public string? TrackingCodeId { get; set; }
    public string? IsBillable { get; set; }
    public string? SiteId { get; set; }
    public string? HasExported { get; set; }
    public DateTime? AffiliateReferredTimeStamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public OrderHeader Order { get; set; } = null!;
    public ReturnHeader Return { get; set; } = null!;
    public TrackingCode? TrackingCode { get; set; }
    public TrackingCodeType TrackingCodeType { get; set; } = null!;
}