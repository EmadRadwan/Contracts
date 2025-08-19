namespace Domain;

public class WorkEffortIcalDatum
{
    public string WorkEffortId { get; set; } = null!;
    public string? IcalData { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public WorkEffort WorkEffort { get; set; } = null!;
}