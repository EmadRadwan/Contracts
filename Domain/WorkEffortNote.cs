namespace Domain;

public class WorkEffortNote
{
    public string WorkEffortId { get; set; } = null!;
    public string NoteId { get; set; } = null!;
    public string? InternalNote { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public NoteDatum Note { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}