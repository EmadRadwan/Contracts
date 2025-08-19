namespace Domain;

public class BudgetRevision
{
    public BudgetRevision()
    {
        BudgetRevisionImpacts = new HashSet<BudgetRevisionImpact>();
    }

    public string BudgetId { get; set; } = null!;
    public string RevisionSeqId { get; set; } = null!;
    public DateTime? DateRevised { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Budget Budget { get; set; } = null!;
    public ICollection<BudgetRevisionImpact> BudgetRevisionImpacts { get; set; }
}