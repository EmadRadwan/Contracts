namespace Application.Accounting.OrganizationGlSettings;

/// <summary>
/// Represents a single account balance row in the comparative balance sheet,
/// combining two periods' balances.
/// </summary>
public class ComparativeAccountBalance
{
    public string GlAccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public decimal Balance1 { get; set; } // from Period1
    public decimal Balance2 { get; set; } // from Period2
}