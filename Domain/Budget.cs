namespace Domain;

public class Budget
{
    public Budget()
    {
        BudgetAttributes = new HashSet<BudgetAttribute>();
        BudgetItems = new HashSet<BudgetItem>();
        BudgetReviews = new HashSet<BudgetReview>();
        BudgetRevisionImpacts = new HashSet<BudgetRevisionImpact>();
        BudgetRevisions = new HashSet<BudgetRevision>();
        BudgetRoles = new HashSet<BudgetRole>();
        BudgetScenarioApplications = new HashSet<BudgetScenarioApplication>();
        BudgetStatuses = new HashSet<BudgetStatus>();
        PaymentBudgetAllocations = new HashSet<PaymentBudgetAllocation>();
    }

    public string BudgetId { get; set; } = null!;
    public string? BudgetTypeId { get; set; }
    public string? CustomTimePeriodId { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BudgetType? BudgetType { get; set; }
    public CustomTimePeriod? CustomTimePeriod { get; set; }
    public ICollection<BudgetAttribute> BudgetAttributes { get; set; }
    public ICollection<BudgetItem> BudgetItems { get; set; }
    public ICollection<BudgetReview> BudgetReviews { get; set; }
    public ICollection<BudgetRevisionImpact> BudgetRevisionImpacts { get; set; }
    public ICollection<BudgetRevision> BudgetRevisions { get; set; }
    public ICollection<BudgetRole> BudgetRoles { get; set; }
    public ICollection<BudgetScenarioApplication> BudgetScenarioApplications { get; set; }
    public ICollection<BudgetStatus> BudgetStatuses { get; set; }
    public ICollection<PaymentBudgetAllocation> PaymentBudgetAllocations { get; set; }
}