namespace Application.Shipments.Reports;

public class CashFlowStatementViewModel
{
    public List<TransactionTotal> OpeningCashBalanceList { get; set; }
    public decimal OpeningCashBalanceTotal { get; set; }
    public List<TransactionTotal> PeriodCashBalanceList { get; set; }
    public decimal PeriodCashBalanceTotal { get; set; }
    public List<TransactionTotal> ClosingCashBalanceList { get; set; }
    public decimal ClosingCashBalanceTotal { get; set; }
    public decimal EndingCashBalanceTotal { get; set; }
}
