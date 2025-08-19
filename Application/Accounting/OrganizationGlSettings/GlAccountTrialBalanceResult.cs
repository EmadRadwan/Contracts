using Domain;

namespace Application.Accounting.OrganizationGlSettings;

/// <summary>
/// Final "TrialBalanceResult" equivalent to your "context" in the script.
/// Holds major outputs: 
///  - the chosen time period ID
///  - isDebitAccount
///  - glAccount details
///  - openingBalance
///  - the final monthly breakdown (glAcctgTrialBalanceList)
/// </summary>
public class GlAccountTrialBalanceResult
{
    public string TimePeriodId { get; set; }
    public bool? IsDebitAccount { get; set; }
    public GlAccount GlAccount { get; set; }
    public decimal OpeningBalance { get; set; }
    public CustomTimePeriod CurrentTimePeriod { get; set; }

    public List<AcctgTransEntriesAndTransTotalResult> GlAcctgTrialBalanceList { get; set; } = new();
}