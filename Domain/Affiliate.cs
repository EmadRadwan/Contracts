namespace Domain;

public class Affiliate
{
    public string PartyId { get; set; } = null!;
    public string? AffiliateName { get; set; }
    public string? AffiliateDescription { get; set; }
    public string? YearEstablished { get; set; }
    public string? SiteType { get; set; }
    public string? SitePageViews { get; set; }
    public string? SiteVisitors { get; set; }
    public DateTime? DateTimeCreated { get; set; }
    public DateTime? DateTimeApproved { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public PartyGroup PartyNavigation { get; set; } = null!;
}