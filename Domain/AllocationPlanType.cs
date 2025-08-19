namespace Domain;

public class AllocationPlanType
{
    public AllocationPlanType()
    {
        AllocationPlanHeaders = new HashSet<AllocationPlanHeader>();
    }

    public string PlanTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public string? HasTable { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<AllocationPlanHeader> AllocationPlanHeaders { get; set; }
}