namespace Domain;

public class BudgetRevisionImpact
{
    public string BudgetId { get; set; } = null!;
    public string BudgetItemSeqId { get; set; } = null!;
    public string RevisionSeqId { get; set; } = null!;
    public decimal? RevisedAmount { get; set; }
    public string? AddDeleteFlag { get; set; }
    public string? RevisionReason { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Budget Budget { get; set; } = null!;
    public BudgetItem BudgetI { get; set; } = null!;
    public BudgetRevision BudgetRevision { get; set; } = null!;
}