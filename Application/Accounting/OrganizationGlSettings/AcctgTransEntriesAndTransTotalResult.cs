namespace Application.Accounting.OrganizationGlSettings
{
    /// <summary>
    /// Holds the output of "GetAcctgTransEntriesAndTransTotal":
    ///   - AcctgTransAndEntries: The rows/transactions
    ///   - DebitTotal, CreditTotal
    ///   - DebitCreditDifference
    /// 
    /// Also includes properties for "rolling" or "year-to-date" values
    /// which the caller (GenerateGlAccountTrialBalance) sets each month.
    /// </summary>
    public class AcctgTransEntriesAndTransTotalResult
    {
        public List<AcctgTransAndEntryDto> AcctgTransAndEntries { get; set; }
        public decimal DebitTotal { get; set; }
        public decimal CreditTotal { get; set; }
        public decimal DebitCreditDifference { get; set; }

        // Additional fields
        public decimal? TotalOfYearToDateDebit { get; set; }
        public decimal? TotalOfYearToDateCredit { get; set; }
        public decimal? Balance { get; set; }
        public decimal? BalanceOfTheAcctgForYear { get; set; }
        // Optional: keep the Deconstruct if you wish
        public void Deconstruct(out decimal debitTotal, out decimal creditTotal)
        {
            debitTotal = this.DebitTotal;
            creditTotal = this.CreditTotal;
        }

        // constructor
        public AcctgTransEntriesAndTransTotalResult()
        {
            AcctgTransAndEntries = new List<AcctgTransAndEntryDto>();
        }
    }
}
