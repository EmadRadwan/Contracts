namespace Domain;

public class SalesOpportunityCompetitor
{
    public string SalesOpportunityId { get; set; } = null!;
    public string CompetitorPartyId { get; set; } = null!;
    public string? PositionEnumId { get; set; }
    public string? Strengths { get; set; }
    public string? Weaknesses { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public SalesOpportunity SalesOpportunity { get; set; } = null!;
}