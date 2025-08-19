namespace Domain;

public class WorkEffortTransBox
{
    public string ProcessWorkEffortId { get; set; } = null!;
    public string ToActivityId { get; set; } = null!;
    public string TransitionId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public WorkEffort ProcessWorkEffort { get; set; } = null!;
}