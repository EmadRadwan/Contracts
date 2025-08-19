namespace Domain;

public class SalesOpportunityWorkEffort
{
    public string SalesOpportunityId { get; set; } = null!;
    public string WorkEffortId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public SalesOpportunity SalesOpportunity { get; set; } = null!;
    public WorkEffort WorkEffort { get; set; } = null!;
}