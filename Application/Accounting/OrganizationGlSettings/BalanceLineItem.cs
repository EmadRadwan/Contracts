namespace Application.Accounting.OrganizationGlSettings;

/// <summary>
/// Represents a single line item on a financial statement summary,
/// such as "AccountingTotalAssets" or "AccountingTotalLiabilitiesAndEquities."
///
/// Business Explanation:
/// If you want to provide a final line or subtotal in a Balance Sheet,
/// you can store the descriptive label in TotalName (e.g. "Total Assets")
/// and the computed Balance. This structure is then displayed in a list
/// or table to show the summary lines.
/// </summary>
public class BalanceLineItem
{
    /// <summary>
    /// The descriptive label for this line, e.g. "AccountingTotalAssets" or "CurrentLiabilities."
    /// </summary>
    public string TotalName { get; set; }

    /// <summary>
    /// The corresponding total or balance amount for this line item.
    /// </summary>
    public decimal Balance { get; set; }
}
