namespace Application.Shipments.Reports;

public class IncomeStatementViewModel
{
    public List<TransactionTotal> RevenueAccountBalances { get; set; }
    public List<TransactionTotal> ContraRevenueAccountBalances { get; set; }
    public List<TransactionTotal> ExpenseAccountBalances { get; set; }
    public List<TransactionTotal> CogsExpenseAccountBalances { get; set; }
    public List<TransactionTotal> SgaExpenseAccountBalances { get; set; }
    public List<TransactionTotal> DepreciationAccountBalances { get; set; }
    public List<TransactionTotal> IncomeAccountBalances { get; set; }
    public decimal NetSales { get; set; }
    public decimal GrossMargin { get; set; }
    public decimal IncomeFromOperations { get; set; }
    public decimal NetIncome { get; set; }
    public decimal RevenueBalanceTotal { get; set; }
    public decimal ContraRevenueBalanceTotal { get; set; }
    public decimal ExpenseBalanceTotal { get; set; }
    public decimal CogsExpenseBalanceTotal { get; set; }
    public decimal SgaExpenseBalanceTotal { get; set; }
    public decimal DepreciationBalanceTotal { get; set; }
    public decimal IncomeBalanceTotal { get; set; }
}