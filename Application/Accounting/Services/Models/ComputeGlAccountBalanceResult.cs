namespace Application.Accounting.Services.Models;

public class ComputeGlAccountBalanceResult
{
    public decimal OpeningBalance { get; set; }
    public decimal EndingBalance { get; set; }
    public decimal PostedDebits { get; set; }
    public decimal PostedCredits { get; set; }
}