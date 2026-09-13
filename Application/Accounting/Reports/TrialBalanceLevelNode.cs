namespace Application.Shipments.Reports;

// One row of the level-aware (COA hierarchy) Trial Balance: either a leaf GL account
// (IsLeaf = true, same figures as the flat Trial Balance) or a roll-up summary for a
// parent GL account in the Chart of Accounts tree (IsLeaf = false, figures are the sum
// of all descendant leaf accounts under it).
public class TrialBalanceLevelNode : GlAccountBalanceResult
{
    public string GlAccountId { get; set; }
    public string? ParentGlAccountId { get; set; }
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public int Level { get; set; }
    public bool IsLeaf { get; set; }
}
