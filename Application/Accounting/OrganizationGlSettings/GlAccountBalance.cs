namespace Application.Accounting.OrganizationGlSettings;

/// <summary>
/// Represents a single GL Account's amounts:
///   - GlAccountId: Unique identifier of the account
///   - AccountCode, AccountName: For display/ordering
///   - D (Debits) and C (Credits) track the sums of debit/credit amounts.
///   - Balance is the final net, which can be D - C (for debit-based) or C - D (for credit-based).
/// </summary>
public class GlAccountBalance
{
    public string GlAccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public decimal D { get; set; }       // total Debits
    public decimal C { get; set; }       // total Credits
    public decimal Balance { get; set; } // final net
}