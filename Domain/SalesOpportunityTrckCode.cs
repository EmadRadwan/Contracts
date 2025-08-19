namespace Domain;

public class SalesOpportunityTrckCode
{
    public string SalesOpportunityId { get; set; } = null!;
    public string TrackingCodeId { get; set; } = null!;
    public DateTime? ReceivedDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public SalesOpportunity SalesOpportunity { get; set; } = null!;
}