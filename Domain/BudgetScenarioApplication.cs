namespace Domain;

public class BudgetScenarioApplication
{
    public string BudgetScenarioApplicId { get; set; } = null!;
    public string BudgetScenarioId { get; set; } = null!;
    public string? BudgetId { get; set; }
    public string? BudgetItemSeqId { get; set; }
    public decimal? AmountChange { get; set; }
    public decimal? PercentageChange { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Budget? Budget { get; set; }
    public BudgetItem? BudgetI { get; set; }
    public BudgetScenario BudgetScenario { get; set; } = null!;
}