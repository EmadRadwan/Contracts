namespace Application.Shipments.Reports;

public class AccountBalance : GlAccountBalanceResult
{
    public string GlAccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
}