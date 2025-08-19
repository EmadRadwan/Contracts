namespace Application.Accounting.OrganizationGlSettings;

public class ComparativeBalanceSheetResult
{
    public List<ComparativeAccountBalance> AssetAccountBalanceList { get; set; }
    public List<ComparativeAccountBalance> LiabilityAccountBalanceList { get; set; }
    public List<ComparativeAccountBalance> EquityAccountBalanceList { get; set; }
    public List<ComparativeBalanceTotal>   BalanceTotalList { get; set; }
}