namespace Domain;

public class BudgetScenarioRule
{
    public string BudgetScenarioId { get; set; } = null!;
    public string BudgetItemTypeId { get; set; } = null!;
    public decimal? AmountChange { get; set; }
    public decimal? PercentageChange { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public BudgetItemType BudgetItemType { get; set; } = null!;
    public BudgetScenario BudgetScenario { get; set; } = null!;
}