namespace Domain;

public class AllocationPlanItem
{
    public string PlanId { get; set; } = null!;
    public string PlanItemSeqId { get; set; } = null!;
    public string? StatusId { get; set; }
    public string? PlanMethodEnumId { get; set; }
    public string? OrderId { get; set; }
    public string? OrderItemSeqId { get; set; }
    public string ProductId { get; set; } = null!;
    public decimal? AllocatedQuantity { get; set; }
    public string? PrioritySeqId { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public UserLogin? LastModifiedByUserLoginNavigation { get; set; }
    public OrderHeader? Order { get; set; }
    public OrderItem? OrderI { get; set; }
    public AllocationPlanHeader P { get; set; } = null!;
    public Enumeration? PlanMethodEnum { get; set; }
    public StatusItem? Status { get; set; }
}