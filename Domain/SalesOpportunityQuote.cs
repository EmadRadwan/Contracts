namespace Domain;

public class SalesOpportunityQuote
{
    public string SalesOpportunityId { get; set; } = null!;
    public string QuoteId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Quote Quote { get; set; } = null!;
    public SalesOpportunity SalesOpportunity { get; set; } = null!;
}