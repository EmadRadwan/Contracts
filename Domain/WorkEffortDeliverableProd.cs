namespace Domain;

public class WorkEffortDeliverableProd
{
    public string WorkEffortId { get; set; } = null!;
    public string DeliverableId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Deliverable Deliverable { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}