using System.Collections.Generic;

namespace Application.Accounting.Reports
{
    public class IncomeStatementViewModel
    {
        // === Detailed Account-Level Balances (for Kendo Grids) ===
        public List<TransactionTotal> RevenueAccountBalances { get; set; } = new();
        public List<TransactionTotal> ContraRevenueAccountBalances { get; set; } = new();
        public List<TransactionTotal> CogsExpenseAccountBalances { get; set; } = new();
        public List<TransactionTotal> ExpenseAccountBalances { get; set; } = new();        // SGA + Depreciation combined for the "Expenses" grid
        public List<TransactionTotal> IncomeAccountBalances { get; set; } = new();

        // === Key Summary Totals (used in frontend Totals section) ===
        public decimal RevenueBalanceTotal { get; set; }
        public decimal ContraRevenueBalanceTotal { get; set; }
        public decimal CogsExpenseBalanceTotal { get; set; }
        public decimal SgaExpenseBalanceTotal { get; set; }           // Optional: if you want to show separately
        public decimal DepreciationBalanceTotal { get; set; }         // Optional: if you want to show separately
        public decimal IncomeBalanceTotal { get; set; }

        // === Computed Income Statement Lines (Most Important) ===
        public decimal NetSales { get; set; }
        public decimal GrossMargin { get; set; }
        public decimal IncomeFromOperations { get; set; }
        public decimal NetIncome { get; set; }

        // === Optional: More Granular Breakdown (Good for future extensibility) ===
        public decimal TotalOperatingExpenses { get; set; }   // SGA + Depreciation
    }
}