namespace Domain;

public class DeliverableType
{
    public DeliverableType()
    {
        Deliverables = new HashSet<Deliverable>();
        QuoteItems = new HashSet<QuoteItem>();
    }

    public string DeliverableTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<Deliverable> Deliverables { get; set; }
    public ICollection<QuoteItem> QuoteItems { get; set; }
}