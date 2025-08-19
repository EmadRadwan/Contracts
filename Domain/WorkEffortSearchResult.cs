namespace Domain;

public class WorkEffortSearchResult
{
    public WorkEffortSearchResult()
    {
        WorkEffortSearchConstraints = new HashSet<WorkEffortSearchConstraint>();
    }

    public string WorkEffortSearchResultId { get; set; } = null!;
    public string? VisitId { get; set; }
    public string? OrderByName { get; set; }
    public string? IsAscending { get; set; }
    public int? NumResults { get; set; }
    public double? SecondsTotal { get; set; }
    public DateTime? SearchDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<WorkEffortSearchConstraint> WorkEffortSearchConstraints { get; set; }
}