namespace Application.Shipments.Reports
{
    /// <summary>
    /// Derived class for comparative balancing, adding a 'Balance' and 'TotalName'
    /// property so we can run the original merge code unchanged.
    /// </summary>
    public class AccountBalanceForComparative : AccountBalance
    {
        /// <summary>
        /// The numeric amount for this record (for the selected period).
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// For lines that represent "totals," we can store a label like "AccountingTotalAssets."
        /// </summary>
        public string TotalName { get; set; }
    }
}