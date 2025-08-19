namespace Domain;

public class SubscriptionType
{
    public SubscriptionType()
    {
        InverseParentType = new HashSet<SubscriptionType>();
        SubscriptionTypeAttrs = new HashSet<SubscriptionTypeAttr>();
        Subscriptions = new HashSet<Subscription>();
    }

    public string SubscriptionTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public SubscriptionType? ParentType { get; set; }
    public ICollection<SubscriptionType> InverseParentType { get; set; }
    public ICollection<SubscriptionTypeAttr> SubscriptionTypeAttrs { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; }
}