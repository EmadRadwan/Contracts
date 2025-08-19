namespace Domain;

public class WorkEffortContentType
{
    public WorkEffortContentType()
    {
        InverseParentType = new HashSet<WorkEffortContentType>();
        WorkEffortContents = new HashSet<WorkEffortContent>();
    }

    public string WorkEffortContentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public WorkEffortContentType? ParentType { get; set; }
    public ICollection<WorkEffortContentType> InverseParentType { get; set; }
    public ICollection<WorkEffortContent> WorkEffortContents { get; set; }
}