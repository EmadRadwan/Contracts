namespace Domain;

public class TemporalExpression
{
    public TemporalExpression()
    {
        JobSandboxes = new HashSet<JobSandbox>();
        TemporalExpressionAssocFromTempExprs = new HashSet<TemporalExpressionAssoc>();
        TemporalExpressionAssocToTempExprs = new HashSet<TemporalExpressionAssoc>();
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string TempExprId { get; set; } = null!;
    public string? TempExprTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? Date1 { get; set; }
    public DateTime? Date2 { get; set; }
    public int? Integer1 { get; set; }
    public int? Integer2 { get; set; }
    public string? String1 { get; set; }
    public string? String2 { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<JobSandbox> JobSandboxes { get; set; }
    public ICollection<TemporalExpressionAssoc> TemporalExpressionAssocFromTempExprs { get; set; }
    public ICollection<TemporalExpressionAssoc> TemporalExpressionAssocToTempExprs { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}