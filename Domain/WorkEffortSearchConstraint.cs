namespace Domain;

public class WorkEffortSearchConstraint
{
    public string WorkEffortSearchResultId { get; set; } = null!;
    public string ConstraintSeqId { get; set; } = null!;
    public string? ConstraintName { get; set; }
    public string? InfoString { get; set; }
    public string? IncludeSubWorkEfforts { get; set; }
    public string? IsAnd { get; set; }
    public string? AnyPrefix { get; set; }
    public string? AnySuffix { get; set; }
    public string? RemoveStems { get; set; }
    public string? LowValue { get; set; }
    public string? HighValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public WorkEffortSearchResult WorkEffortSearchResult { get; set; } = null!;
}