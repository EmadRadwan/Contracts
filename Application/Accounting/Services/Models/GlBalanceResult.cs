namespace Application.Accounting.Services.Models;

/// <summary>
/// Structure for returning the computed balances 
/// from computeGlAccountBalanceForTimePeriod
/// </summary>
public class GlBalanceResult
{
    public decimal OpeningBalance { get; set; }
    public decimal EndingBalance { get; set; }
    public decimal PostedDebits { get; set; }
    public decimal PostedCredits { get; set; }
}