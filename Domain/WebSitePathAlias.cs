namespace Domain;

public class WebSitePathAlias
{
    public string WebSiteId { get; set; } = null!;
    public string PathAlias { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? AliasTo { get; set; }
    public string? ContentId { get; set; }
    public string? MapKey { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content? Content { get; set; }
    public WebSite WebSite { get; set; } = null!;
}