namespace Domain;

public class WorkRequirementFulfillment
{
    public string RequirementId { get; set; } = null!;
    public string WorkEffortId { get; set; } = null!;
    public string? WorkReqFulfTypeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Requirement Requirement { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
    public WorkReqFulfType? WorkReqFulfType { get; set; }
}