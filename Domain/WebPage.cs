namespace Domain;

public class WebPage
{
    public string WebPageId { get; set; } = null!;
    public string? PageName { get; set; }
    public string? WebSiteId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
    public string? ContentId { get; set; }

    public Content? Content { get; set; }
    public WebSite? WebSite { get; set; }
}