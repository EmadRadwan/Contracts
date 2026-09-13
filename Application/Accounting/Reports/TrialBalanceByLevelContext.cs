namespace Application.Shipments.Reports;

// Result of the COA-level-aware Trial Balance. PostedDebitsTotal/PostedCreditsTotal
// mirror TrialBalanceContext's grand totals (sum of leaf accounts only, to avoid
// double-counting parent roll-ups). Nodes contains both the leaf accounts and their
// ancestor summary rows, ordered by AccountCode so the Chart of Accounts hierarchy
// reads top-to-bottom.
public class TrialBalanceByLevelContext
{
    public decimal PostedDebitsTotal { get; set; }
    public decimal PostedCreditsTotal { get; set; }
    public List<TrialBalanceLevelNode> Nodes { get; set; }
}
