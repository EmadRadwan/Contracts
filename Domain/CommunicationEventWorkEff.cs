namespace Domain;

public class CommunicationEventWorkEff
{
    public string WorkEffortId { get; set; } = null!;
    public string CommunicationEventId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CommunicationEvent CommunicationEvent { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}