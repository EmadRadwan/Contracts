namespace Domain;

public class GlBudgetXref
{
    public string GlAccountId { get; set; } = null!;
    public string BudgetItemTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? AllocationPercentage { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BudgetItemType BudgetItemType { get; set; } = null!;
    public GlAccount GlAccount { get; set; } = null!;
}