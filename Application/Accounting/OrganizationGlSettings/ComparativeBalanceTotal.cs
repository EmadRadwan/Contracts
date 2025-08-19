namespace Application.Accounting.OrganizationGlSettings;

/// <summary>
/// Represents a single "total" row (e.g. total assets, total liabilities)
/// combining two periods' balances.
/// </summary>
public class ComparativeBalanceTotal
{
    public string TotalName { get; set; }
    public decimal Balance1 { get; set; } // from Period1
    public decimal Balance2 { get; set; } // from Period2
}