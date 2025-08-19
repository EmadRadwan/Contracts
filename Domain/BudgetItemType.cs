namespace Domain;

public class BudgetItemType
{
    public BudgetItemType()
    {
        BudgetItemTypeAttrs = new HashSet<BudgetItemTypeAttr>();
        BudgetItems = new HashSet<BudgetItem>();
        BudgetScenarioRules = new HashSet<BudgetScenarioRule>();
        GlBudgetXrefs = new HashSet<GlBudgetXref>();
        InverseParentType = new HashSet<BudgetItemType>();
    }

    public string BudgetItemTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BudgetItemType? ParentType { get; set; }
    public ICollection<BudgetItemTypeAttr> BudgetItemTypeAttrs { get; set; }
    public ICollection<BudgetItem> BudgetItems { get; set; }
    public ICollection<BudgetScenarioRule> BudgetScenarioRules { get; set; }
    public ICollection<GlBudgetXref> GlBudgetXrefs { get; set; }
    public ICollection<BudgetItemType> InverseParentType { get; set; }
}