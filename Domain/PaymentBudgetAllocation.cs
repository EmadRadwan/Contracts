namespace Domain;

public class PaymentBudgetAllocation
{
    public string BudgetId { get; set; } = null!;
    public string BudgetItemSeqId { get; set; } = null!;
    public string PaymentId { get; set; } = null!;
    public decimal? Amount { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Budget Budget { get; set; } = null!;
    public Payment Payment { get; set; } = null!;
}