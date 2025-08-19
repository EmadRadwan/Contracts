namespace Domain;

public class SalesOpportunity
{
    public SalesOpportunity()
    {
        InvoiceItems = new HashSet<InvoiceItem>();
        OrderItems = new HashSet<OrderItem>();
        SalesOpportunityCompetitors = new HashSet<SalesOpportunityCompetitor>();
        SalesOpportunityHistories = new HashSet<SalesOpportunityHistory>();
        SalesOpportunityQuotes = new HashSet<SalesOpportunityQuote>();
        SalesOpportunityRoles = new HashSet<SalesOpportunityRole>();
        SalesOpportunityTrckCodes = new HashSet<SalesOpportunityTrckCode>();
        SalesOpportunityWorkEfforts = new HashSet<SalesOpportunityWorkEffort>();
    }

    public string SalesOpportunityId { get; set; } = null!;
    public string? OpportunityName { get; set; }
    public string? Description { get; set; }
    public string? NextStep { get; set; }
    public DateTime? NextStepDate { get; set; }
    public decimal? EstimatedAmount { get; set; }
    public decimal? EstimatedProbability { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? MarketingCampaignId { get; set; }
    public string? DataSourceId { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
    public string? OpportunityStageId { get; set; }
    public string? TypeEnumId { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? CreatedByUserLoginNavigation { get; set; }
    public Uom? CurrencyUom { get; set; }
    public DataSource? DataSource { get; set; }
    public MarketingCampaign? MarketingCampaign { get; set; }
    public SalesOpportunityStage? OpportunityStage { get; set; }
    public Enumeration? TypeEnum { get; set; }
    public ICollection<InvoiceItem> InvoiceItems { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
    public ICollection<SalesOpportunityCompetitor> SalesOpportunityCompetitors { get; set; }
    public ICollection<SalesOpportunityHistory> SalesOpportunityHistories { get; set; }
    public ICollection<SalesOpportunityQuote> SalesOpportunityQuotes { get; set; }
    public ICollection<SalesOpportunityRole> SalesOpportunityRoles { get; set; }
    public ICollection<SalesOpportunityTrckCode> SalesOpportunityTrckCodes { get; set; }
    public ICollection<SalesOpportunityWorkEffort> SalesOpportunityWorkEfforts { get; set; }
}