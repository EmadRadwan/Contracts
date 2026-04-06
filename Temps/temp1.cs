using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
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
        SalesOpportunityActions = new HashSet<SalesOpportunityAction>();
    }

    // Primary Key
    public string SalesOpportunityId { get; set; } = null!;

    // Core Opportunity Fields
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

    // ==================== REAL ESTATE SPECIFIC FIELDS ====================
    public string? ProductId { get; set; }        // Apartment / Unit
    public string? WorkEffortId { get; set; }     // Project / Compound / Phase

    public decimal Quantity { get; set; } = 1m;   // Usually 1, can be >1 for investors
    public decimal? UnitPrice { get; set; }       // Estimated price per unit
    public string? Notes { get; set; }            // "Client prefers 3rd floor", "Sea view", "Negotiating 5% discount"

    // Additional useful fields for property sales
    public string? PreferredFloor { get; set; }
    public string? ViewType { get; set; }         // Sea View, Golf View, City View, etc.
    public string? PaymentPlanId { get; set; }

    // Date tracking for interest
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }       // Useful for history / soft-delete of interest

    // Audit Fields
    public DateTime? CreatedStamp { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public string? CreatedByUserLogin { get; set; }
    public string? LastModifiedByUserLogin { get; set; }

    // ==================== NAVIGATION PROPERTIES ====================

    // Standard OFBiz navigations
    public virtual UserLogin? CreatedByUserLoginNavigation { get; set; }
    public virtual Uom? CurrencyUom { get; set; }
    public virtual DataSource? DataSource { get; set; }
    public virtual MarketingCampaign? MarketingCampaign { get; set; }
    public virtual SalesOpportunityStage? OpportunityStage { get; set; }
    public virtual Enumeration? TypeEnum { get; set; }

    // Real Estate navigations (one-way from Opportunity)
    public virtual Product? Product { get; set; }           // Apartment / Unit
    public virtual WorkEffort? WorkEffort { get; set; }     // Project / Compound

    // Collections (keep all existing ones)
    public virtual ICollection<InvoiceItem> InvoiceItems { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; }
    public virtual ICollection<SalesOpportunityCompetitor> SalesOpportunityCompetitors { get; set; }
    public virtual ICollection<SalesOpportunityHistory> SalesOpportunityHistories { get; set; }
    public virtual ICollection<SalesOpportunityQuote> SalesOpportunityQuotes { get; set; }
    public virtual ICollection<SalesOpportunityRole> SalesOpportunityRoles { get; set; }
    public virtual ICollection<SalesOpportunityTrckCode> SalesOpportunityTrckCodes { get; set; }
    public virtual ICollection<SalesOpportunityWorkEffort> SalesOpportunityWorkEfforts { get; set; }
    public virtual ICollection<SalesOpportunityAction> SalesOpportunityActions { get; set; }

    // Reverse navigations (you already added these)
    // public virtual ICollection<SalesOpportunity> SalesOpportunities { get; set; }  ← on Product & WorkEffort
}