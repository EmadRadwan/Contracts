namespace Application.Accounting.OrganizationGlSettings;

/// <summary>
/// Represents the totals for a single GL account, 
/// including the final amount plus the current fiscal period total.
/// </summary>
public class GlAccountTotal
{
    public string GlAccountId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalOfCurrentFiscalPeriod { get; set; }

    // If you want to store codes/names, you can add them:
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
}