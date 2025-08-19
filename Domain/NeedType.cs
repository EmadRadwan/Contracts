namespace Domain;

public class NeedType
{
    public NeedType()
    {
        PartyNeeds = new HashSet<PartyNeed>();
        Subscriptions = new HashSet<Subscription>();
    }

    public string NeedTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<PartyNeed> PartyNeeds { get; set; }
    public ICollection<Subscription> Subscriptions { get; set; }
}