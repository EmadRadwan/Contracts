namespace Domain;

public class WorkEffortStatus
{
    public string WorkEffortId { get; set; } = null!;
    public string StatusId { get; set; } = null!;
    public DateTime StatusDatetime { get; set; }
    public string? SetByUserLogin { get; set; }
    public string? Reason { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? SetByUserLoginNavigation { get; set; }
    public StatusItem Status { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}