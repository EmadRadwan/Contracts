namespace Domain;

public class WorkReqFulfType
{
    public WorkReqFulfType()
    {
        WorkRequirementFulfillments = new HashSet<WorkRequirementFulfillment>();
    }

    public string WorkReqFulfTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<WorkRequirementFulfillment> WorkRequirementFulfillments { get; set; }
}