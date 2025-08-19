namespace Domain;

public class WorkEffortContactMechNew
{
    public string WorkEffortId { get; set; } = null!;
    public string ContactMechId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech ContactMech { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}