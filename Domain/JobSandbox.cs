namespace Domain;

public class JobSandbox
{
    public JobSandbox()
    {
        ProductGroupOrders = new HashSet<ProductGroupOrder>();
    }

    public string JobId { get; set; } = null!;
    public string? JobName { get; set; }
    public DateTime? RunTime { get; set; }
    public int? Priority { get; set; }
    public string? PoolId { get; set; }
    public string? StatusId { get; set; }
    public string? ParentJobId { get; set; }
    public string? PreviousJobId { get; set; }
    public string? ServiceName { get; set; }
    public string? LoaderName { get; set; }
    public int? MaxRetry { get; set; }
    public int? CurrentRetryCount { get; set; }
    public string? AuthUserLoginId { get; set; }
    public string? RunAsUser { get; set; }
    public string? RuntimeDataId { get; set; }
    public string? RecurrenceInfoId { get; set; }
    public string? TempExprId { get; set; }
    public int? CurrentRecurrenceCount { get; set; }
    public int? MaxRecurrenceCount { get; set; }
    public string? RunByInstanceId { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? FinishDateTime { get; set; }
    public DateTime? CancelDateTime { get; set; }
    public string? JobResult { get; set; }
    public string? RecurrenceTimeZone { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? AuthUserLogin { get; set; }
    public RecurrenceInfo? RecurrenceInfo { get; set; }
    public UserLogin? RunAsUserNavigation { get; set; }
    public RuntimeDatum? RuntimeData { get; set; }
    public StatusItem? Status { get; set; }
    public TemporalExpression? TempExpr { get; set; }
    public ICollection<ProductGroupOrder> ProductGroupOrders { get; set; }
}