namespace Domain;

public class MarketingCampaign
{
    public MarketingCampaign()
    {
        ContactLists = new HashSet<ContactList>();
        InverseParentCampaign = new HashSet<MarketingCampaign>();
        MarketingCampaignNotes = new HashSet<MarketingCampaignNote>();
        MarketingCampaignPrices = new HashSet<MarketingCampaignPrice>();
        MarketingCampaignPromos = new HashSet<MarketingCampaignPromo>();
        MarketingCampaignRoles = new HashSet<MarketingCampaignRole>();
        SalesOpportunities = new HashSet<SalesOpportunity>();
        TrackingCodes = new HashSet<TrackingCode>();
    }

    public string MarketingCampaignId { get; set; } = null!;
    public string? ParentCampaignId { get; set; }
    public string? StatusId { get; set; }
    public string? CampaignName { get; set; }
    public string? CampaignSummary { get; set; }
    public decimal? BudgetedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? CurrencyUomId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? IsActive { get; set; }
    public string? ConvertedLeads { get; set; }
    public double? ExpectedResponsePercent { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public int? NumSent { get; set; }
    public DateTime? StartDate { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public string? LastModifiedByUserLogin { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? CurrencyUom { get; set; }
    public MarketingCampaign? ParentCampaign { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<ContactList> ContactLists { get; set; }
    public ICollection<MarketingCampaign> InverseParentCampaign { get; set; }
    public ICollection<MarketingCampaignNote> MarketingCampaignNotes { get; set; }
    public ICollection<MarketingCampaignPrice> MarketingCampaignPrices { get; set; }
    public ICollection<MarketingCampaignPromo> MarketingCampaignPromos { get; set; }
    public ICollection<MarketingCampaignRole> MarketingCampaignRoles { get; set; }
    public ICollection<SalesOpportunity> SalesOpportunities { get; set; }
    public ICollection<TrackingCode> TrackingCodes { get; set; }
}