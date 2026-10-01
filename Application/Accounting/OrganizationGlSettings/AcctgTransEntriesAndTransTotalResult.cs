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

        // The month this row covers. GenerateGlAccountTrialBalance walks the fiscal year one
        // month at a time and appends a row per month, so the month used to be implicit in the
        // list index — the client could only label rows by counting from
        // CurrentTimePeriod.FromDate and re-deriving the walk. Stamping the window on the row
        // makes it self-describing, which the grid and the Excel export both need.
        public DateTime? PeriodFromDate { get; set; }
        public DateTime? PeriodThruDate { get; set; }

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
