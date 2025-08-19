namespace Domain;

public class BudgetScenario
{
    public BudgetScenario()
    {
        BudgetScenarioApplications = new HashSet<BudgetScenarioApplication>();
        BudgetScenarioRules = new HashSet<BudgetScenarioRule>();
    }

    public string BudgetScenarioId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<BudgetScenarioApplication> BudgetScenarioApplications { get; set; }
    public ICollection<BudgetScenarioRule> BudgetScenarioRules { get; set; }
}