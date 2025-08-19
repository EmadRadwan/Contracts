namespace Domain;

public class SubscriptionActivity
{
    public SubscriptionActivity()
    {
        SubscriptionFulfillmentPieces = new HashSet<SubscriptionFulfillmentPiece>();
    }

    public string SubscriptionActivityId { get; set; } = null!;
    public string? Comments { get; set; }
    public DateTime? DateSent { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<SubscriptionFulfillmentPiece> SubscriptionFulfillmentPieces { get; set; }
}