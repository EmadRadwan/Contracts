namespace Domain;

public class SubscriptionFulfillmentPiece
{
    public string SubscriptionActivityId { get; set; } = null!;
    public string SubscriptionId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Subscription Subscription { get; set; } = null!;
    public SubscriptionActivity SubscriptionActivity { get; set; } = null!;
}