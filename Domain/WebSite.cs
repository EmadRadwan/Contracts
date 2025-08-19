namespace Domain;

public class WebSite
{
    public WebSite()
    {
        OrderHeaders = new HashSet<OrderHeader>();
        SubscriptionResources = new HashSet<SubscriptionResource>();
        WebPages = new HashSet<WebPage>();
        WebSiteContactLists = new HashSet<WebSiteContactList>();
        WebSiteContents = new HashSet<WebSiteContent>();
        WebSitePathAliases = new HashSet<WebSitePathAlias>();
        WebSiteRoles = new HashSet<WebSiteRole>();
    }

    public string WebSiteId { get; set; } = null!;
    public string? SiteName { get; set; }
    public string? HttpHost { get; set; }
    public string? HttpPort { get; set; }
    public string? HttpsHost { get; set; }
    public string? HttpsPort { get; set; }
    public string? EnableHttps { get; set; }
    public string? WebappPath { get; set; }
    public string? StandardContentPrefix { get; set; }
    public string? SecureContentPrefix { get; set; }
    public string? CookieDomain { get; set; }
    public string? VisualThemeSetId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
    public string? ProductStoreId { get; set; }
    public string? AllowProductStoreChange { get; set; }
    public string? HostedPathAlias { get; set; }
    public string? IsDefault { get; set; }
    public string? DisplayMaintenancePage { get; set; }

    public ProductStore? ProductStore { get; set; }
    public VisualThemeSet? VisualThemeSet { get; set; }
    public ICollection<OrderHeader> OrderHeaders { get; set; }
    public ICollection<SubscriptionResource> SubscriptionResources { get; set; }
    public ICollection<WebPage> WebPages { get; set; }
    public ICollection<WebSiteContactList> WebSiteContactLists { get; set; }
    public ICollection<WebSiteContent> WebSiteContents { get; set; }
    public ICollection<WebSitePathAlias> WebSitePathAliases { get; set; }
    public ICollection<WebSiteRole> WebSiteRoles { get; set; }
}