namespace Application.Shipments.Reports;

public class AccountBalanceResult
{
    public List<TransactionTotal> AccountBalanceList { get; set; }
    public decimal BalanceTotal { get; set; }
}