namespace Application.Accounting.OrganizationGlSettings
{
    /// <summary>
    /// A container for the final "Balance Sheet" result:
    /// - Lists for each major classification (Assets, Liabilities, Equity).
    /// - Sub-classification totals (Current Assets, Long-Term, etc.).
    /// - Overall numeric totals (e.g. total assets, total liabilities/equity).
    /// </summary>
    public class BalanceSheetViewModel
    {
        // Major classifications
        public List<GlAccountBalance> AssetAccountBalanceList { get; set; }
        public List<GlAccountBalance> LiabilityAccountBalanceList { get; set; }
        public List<GlAccountBalance> EquityAccountBalanceList { get; set; }

        // Totals for these major groups
        public decimal AssetBalanceTotal { get; set; }
        public decimal LiabilityBalanceTotal { get; set; }
        public decimal EquityBalanceTotal { get; set; }
        public decimal LiabilityEquityBalanceTotal { get; set; }

        // Sub-classifications
        public decimal CurrentAssetBalanceTotal { get; set; }
        public decimal LongtermAssetBalanceTotal { get; set; }
        public decimal CurrentLiabilityBalanceTotal { get; set; }
        public decimal ContraAssetBalanceTotal { get; set; }

        // NEW: Additional "contra" sub-classifications
        public decimal AccumDepreciationBalanceTotal { get; set; }
        public decimal AccumAmortizationBalanceTotal { get; set; }

        // Additional lines you might want to display (e.g. for a summary table)
        public List<BalanceLineItem> BalanceTotalList { get; set; }
    }
}
