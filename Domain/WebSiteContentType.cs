namespace Domain;

public class WebSiteContentType
{
    public WebSiteContentType()
    {
        InverseParentType = new HashSet<WebSiteContentType>();
        WebSiteContents = new HashSet<WebSiteContent>();
    }

    public string WebSiteContentTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public WebSiteContentType? ParentType { get; set; }
    public ICollection<WebSiteContentType> InverseParentType { get; set; }
    public ICollection<WebSiteContent> WebSiteContents { get; set; }
}