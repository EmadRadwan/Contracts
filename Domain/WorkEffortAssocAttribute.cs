namespace Domain;

public class WorkEffortAssocAttribute
{
    public string WorkEffortIdFrom { get; set; } = null!;
    public string WorkEffortIdTo { get; set; } = null!;
    public string WorkEffortAssocTypeId { get; set; } = null!;
    public DateTime? FromDate { get; set; }
    public string AttrName { get; set; } = null!;
    public string? AttrValue { get; set; }
    public string? AttrDescription { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public WorkEffortAssoc? WorkEffortAssoc { get; set; }
}