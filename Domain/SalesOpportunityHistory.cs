namespace Domain;

public class SalesOpportunityHistory
{
    public string SalesOpportunityHistoryId { get; set; } = null!;
    public string? SalesOpportunityId { get; set; }
    public string? Description { get; set; }
    public string? NextStep { get; set; }
    public decimal? EstimatedAmount { get; set; }
    public decimal? EstimatedProbability { get; set; }
    public string? CurrencyUomId { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
    public string? OpportunityStageId { get; set; }
    public string? ChangeNote { get; set; }
    public string? ModifiedByUserLogin { get; set; }
    public DateTime? ModifiedTimestamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? CurrencyUom { get; set; }
    public UserLogin? ModifiedByUserLoginNavigation { get; set; }
    public SalesOpportunityStage? OpportunityStage { get; set; }
    public SalesOpportunity? SalesOpportunity { get; set; }
}