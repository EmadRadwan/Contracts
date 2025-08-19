namespace Domain;

public class Deliverable
{
    public Deliverable()
    {
        Requirements = new HashSet<Requirement>();
        WorkEffortDeliverableProds = new HashSet<WorkEffortDeliverableProd>();
    }

    public string DeliverableId { get; set; } = null!;
    public string? DeliverableTypeId { get; set; }
    public string? DeliverableName { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DeliverableType? DeliverableType { get; set; }
    public ICollection<Requirement> Requirements { get; set; }
    public ICollection<WorkEffortDeliverableProd> WorkEffortDeliverableProds { get; set; }
}