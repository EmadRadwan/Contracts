namespace Domain;

public class BudgetType
{
    public BudgetType()
    {
        BudgetTypeAttrs = new HashSet<BudgetTypeAttr>();
        Budgets = new HashSet<Budget>();
        InverseParentType = new HashSet<BudgetType>();
    }

    public string BudgetTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BudgetType? ParentType { get; set; }
    public ICollection<BudgetTypeAttr> BudgetTypeAttrs { get; set; }
    public ICollection<Budget> Budgets { get; set; }
    public ICollection<BudgetType> InverseParentType { get; set; }
}