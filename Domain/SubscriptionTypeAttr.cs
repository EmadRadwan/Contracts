namespace Domain;

public class SubscriptionTypeAttr
{
    public string SubscriptionTypeId { get; set; } = null!;
    public string AttrName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public SubscriptionType SubscriptionType { get; set; } = null!;
}