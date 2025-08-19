namespace Application.Shipments.Reports;

public class GlAccountBalanceResult
{
    public decimal OpeningBalance { get; set; }
    public decimal PostedDebits { get; set; }
    public decimal PostedCredits { get; set; }
    public decimal EndingBalance { get; set; }
}