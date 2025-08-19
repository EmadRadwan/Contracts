namespace Domain;

public class SubscriptionResource
{
    public SubscriptionResource()
    {
        InverseParentResource = new HashSet<SubscriptionResource>();
        ProductSubscriptionResources = new HashSet<ProductSubscriptionResource>();
        Subscriptions = new HashSet<Subscription>();
    }

    public string SubscriptionResourceId { get; set; } = null!;
    public string? ParentResourceId { get; set; }
    public string? Description { get; set; }
    public string? ContentId { get; set; }
    public string? WebSiteId { get; set; }
    public string? ServiceNameOnExpiry { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content? Content { get; set; }
    public SubscriptionResource? ParentResource { get; set; }
    public WebSite? WebSite { get; set; }
    public ICollection<SubscriptionResource> InverseParentResource { get; set; }
    public ICollection<ProductSubscriptionResource> ProductSubscriptionResources { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; }
}