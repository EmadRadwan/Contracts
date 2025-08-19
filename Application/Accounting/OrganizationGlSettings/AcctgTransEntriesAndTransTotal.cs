namespace Application.Accounting.OrganizationGlSettings;

/// <summary>
/// The results from getAcctgTransEntriesAndTransTotal, plus the new fields 
/// the script appends: totalOfYearToDateDebit, totalOfYearToDateCredit, 
/// balance, balanceOfTheAcctgForYear
/// </summary>
public class AcctgTransEntriesAndTransTotal
{
    // from the original service
    public List<AcctgTransAndEntryDto> AcctgTransAndEntries { get; set; }
    public decimal DebitTotal { get; set; }
    public decimal CreditTotal { get; set; }
    public decimal DebitCreditDifference { get; set; }

    // additional fields computed in the loop
    public decimal? TotalOfYearToDateDebit { get; set; }
    public decimal? TotalOfYearToDateCredit { get; set; }
    public decimal? Balance { get; set; }
    public decimal? BalanceOfTheAcctgForYear { get; set; }
}
