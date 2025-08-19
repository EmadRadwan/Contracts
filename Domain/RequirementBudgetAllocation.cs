namespace Domain;

public class RequirementBudgetAllocation
{
    public string BudgetId { get; set; } = null!;
    public string BudgetItemSeqId { get; set; } = null!;
    public string RequirementId { get; set; } = null!;
    public decimal? Amount { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BudgetItem BudgetI { get; set; } = null!;
    public Requirement Requirement { get; set; } = null!;
}