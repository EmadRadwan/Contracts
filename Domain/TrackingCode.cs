namespace Domain;

public class TrackingCode
{
    public TrackingCode()
    {
        TrackingCodeOrderReturns = new HashSet<TrackingCodeOrderReturn>();
        TrackingCodeOrders = new HashSet<TrackingCodeOrder>();
        TrackingCodeVisits = new HashSet<TrackingCodeVisit>();
    }

    public string TrackingCodeId { get; set; } = null!;
    public string? TrackingCodeTypeId { get; set; }
    public string? MarketingCampaignId { get; set; }
    public string? RedirectUrl { get; set; }
    public string? OverrideLogo { get; set; }
    public string? OverrideCss { get; set; }
    public string? ProdCatalogId { get; set; }
    public string? Comments { get; set; }
    public string? Description { get; set; }
    public int? TrackableLifetime { get; set; }
    public int? BillableLifetime { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? GroupId { get; set; }
    public string? SubgroupId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public MarketingCampaign? MarketingCampaign { get; set; }
    public TrackingCodeType? TrackingCodeType { get; set; }
    public ICollection<TrackingCodeOrderReturn> TrackingCodeOrderReturns { get; set; }
    public ICollection<TrackingCodeOrder> TrackingCodeOrders { get; set; }
    public ICollection<TrackingCodeVisit> TrackingCodeVisits { get; set; }
}