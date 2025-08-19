namespace Application.Shipments.Reports;

public class TrialBalanceContext
{
    public List<string> PartyNameList { get; set; }
    public decimal PostedDebitsTotal { get; set; }
    public decimal PostedCreditsTotal { get; set; }
    public List<AccountBalance> AccountBalances { get; set; }
}