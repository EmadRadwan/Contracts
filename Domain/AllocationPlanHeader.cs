namespace Domain;

public class AllocationPlanHeader
{
    public AllocationPlanHeader()
    {
        AllocationPlanItems = new HashSet<AllocationPlanItem>();
    }

    public string PlanId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string? PlanTypeId { get; set; }
    public string? PlanName { get; set; }
    public string? StatusId { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public UserLogin? LastModifiedByUserLoginNavigation { get; set; }
    public AllocationPlanType? PlanType { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<AllocationPlanItem> AllocationPlanItems { get; set; }
}