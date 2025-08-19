namespace Domain;

public class SalesOpportunityStage
{
    public SalesOpportunityStage()
    {
        SalesOpportunities = new HashSet<SalesOpportunity>();
        SalesOpportunityHistories = new HashSet<SalesOpportunityHistory>();
    }

    public string OpportunityStageId { get; set; } = null!;
    public string? Description { get; set; }
    public decimal? DefaultProbability { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<SalesOpportunity> SalesOpportunities { get; set; }
    public ICollection<SalesOpportunityHistory> SalesOpportunityHistories { get; set; }
}