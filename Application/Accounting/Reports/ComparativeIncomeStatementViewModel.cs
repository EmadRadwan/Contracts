using System.Collections.Generic;
using Application.Accounting.OrganizationGlSettings;

namespace Application.Accounting.Reports
{
    public class ComparativeIncomeStatementViewModel
    {
        // === Detailed Account-Level Balances (Comparative) ===
        public List<ComparativeAccountBalance> RevenueAccountBalances { get; set; } = new();
        public List<ComparativeAccountBalance> ContraRevenueAccountBalances { get; set; } = new();
        public List<ComparativeAccountBalance> CogsExpenseAccountBalances { get; set; } = new();
        public List<ComparativeAccountBalance> ExpenseAccountBalances { get; set; } = new();
        public List<ComparativeAccountBalance> IncomeAccountBalances { get; set; } = new();

        // === Key Summary Totals (Comparative) ===
        public decimal NetSales1 { get; set; }
        public decimal NetSales2 { get; set; }
        
        public decimal GrossMargin1 { get; set; }
        public decimal GrossMargin2 { get; set; }
        
        public decimal CogsExpenseBalanceTotal1 { get; set; }
        public decimal CogsExpenseBalanceTotal2 { get; set; }
        
        public decimal OperatingExpenses1 { get; set; }
        public decimal OperatingExpenses2 { get; set; }
        
        public decimal IncomeFromOperations1 { get; set; }
        public decimal IncomeFromOperations2 { get; set; }
        
        public decimal IncomeBalanceTotal1 { get; set; }
        public decimal IncomeBalanceTotal2 { get; set; }
        
        public decimal NetIncome1 { get; set; }
        public decimal NetIncome2 { get; set; }
    }
}
