namespace Domain;

public class Quote
{
    public Quote()
    {
        QuoteAdjustments = new HashSet<QuoteAdjustment>();
        QuoteAttributes = new HashSet<QuoteAttribute>();
        QuoteCoefficients = new HashSet<QuoteCoefficient>();
        QuoteItems = new HashSet<QuoteItem>();
        QuoteNotes = new HashSet<QuoteNote>();
        QuoteRoles = new HashSet<QuoteRole>();
        QuoteTerms = new HashSet<QuoteTerm>();
        QuoteWorkEfforts = new HashSet<QuoteWorkEffort>();
        SalesOpportunityQuotes = new HashSet<SalesOpportunityQuote>();
    }

    public string QuoteId { get; set; } = null!;
    public string? QuoteTypeId { get; set; }
    public string? PartyId { get; set; }

    public DateTime IssueDate { get; set; }
    public string? StatusId { get; set; }
    public string? CurrencyUomId { get; set; }
    public string? ProductStoreId { get; set; }
    public string? SalesChannelEnumId { get; set; }
    public DateTime ValidFromDate { get; set; }
    public DateTime ValidThruDate { get; set; }
    public string? QuoteName { get; set; }
    public string? Description { get; set; }
    public decimal? GrandTotal { get; set; }
    public int? CurrentMileage { get; set; }
    public string? CustomerRemarks { get; set; }
    public string? InternalRemarks { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? CurrencyUom { get; set; }
    public Party? Party { get; set; }
    public ProductStore? ProductStore { get; set; }
    public QuoteType? QuoteType { get; set; }
    public Enumeration? SalesChannelEnum { get; set; }
    public StatusItem? Status { get; set; }
    public ICollection<QuoteAdjustment> QuoteAdjustments { get; set; }
    public ICollection<QuoteAttribute> QuoteAttributes { get; set; }
    public ICollection<QuoteCoefficient> QuoteCoefficients { get; set; }
    public ICollection<QuoteItem> QuoteItems { get; set; }
    public ICollection<QuoteNote> QuoteNotes { get; set; }
    public ICollection<QuoteRole> QuoteRoles { get; set; }
    public ICollection<QuoteTerm> QuoteTerms { get; set; }
    public ICollection<QuoteWorkEffort> QuoteWorkEfforts { get; set; }
    public ICollection<SalesOpportunityQuote> SalesOpportunityQuotes { get; set; }
}