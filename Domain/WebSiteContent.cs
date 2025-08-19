namespace Domain;

public class WebSiteContent
{
    public string WebSiteId { get; set; } = null!;
    public string ContentId { get; set; } = null!;
    public string WebSiteContentTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content Content { get; set; } = null!;
    public WebSite WebSite { get; set; } = null!;
    public WebSiteContentType WebSiteContentType { get; set; } = null!;
}