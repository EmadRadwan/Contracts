namespace Domain;

public class WebAnalyticsConfig
{
    public string WebSiteId { get; set; } = null!;
    public string WebAnalyticsTypeId { get; set; } = null!;
    public string? WebAnalyticsCode { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}