namespace Domain;

public class BudgetItem
{
    public BudgetItem()
    {
        BudgetItemAttributes = new HashSet<BudgetItemAttribute>();
        BudgetRevisionImpacts = new HashSet<BudgetRevisionImpact>();
        BudgetScenarioApplications = new HashSet<BudgetScenarioApplication>();
        RequirementBudgetAllocations = new HashSet<RequirementBudgetAllocation>();
    }

    public string BudgetId { get; set; } = null!;
    public string BudgetItemSeqId { get; set; } = null!;
    public string? BudgetItemTypeId { get; set; }
    public decimal? Amount { get; set; }
    public string? Purpose { get; set; }
    public string? Justification { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Budget Budget { get; set; } = null!;
    public BudgetItemType? BudgetItemType { get; set; }
    public ICollection<BudgetItemAttribute> BudgetItemAttributes { get; set; }
    public ICollection<BudgetRevisionImpact> BudgetRevisionImpacts { get; set; }
    public ICollection<BudgetScenarioApplication> BudgetScenarioApplications { get; set; }
    public ICollection<RequirementBudgetAllocation> RequirementBudgetAllocations { get; set; }
}