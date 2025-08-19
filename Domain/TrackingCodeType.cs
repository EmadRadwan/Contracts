namespace Domain;

public class TrackingCodeType
{
    public TrackingCodeType()
    {
        TrackingCodeOrderReturns = new HashSet<TrackingCodeOrderReturn>();
        TrackingCodeOrders = new HashSet<TrackingCodeOrder>();
        TrackingCodes = new HashSet<TrackingCode>();
    }

    public string TrackingCodeTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<TrackingCodeOrderReturn> TrackingCodeOrderReturns { get; set; }
    public ICollection<TrackingCodeOrder> TrackingCodeOrders { get; set; }
    public ICollection<TrackingCode> TrackingCodes { get; set; }
}