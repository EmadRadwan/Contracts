using Application.Accounting.Reports;

namespace Application.Accounting.Reports
{
    public class AccountBalanceResult
    {
        public List<TransactionTotal> AccountBalanceList { get; set; } = new();
        public decimal BalanceTotal { get; set; }
    }
}