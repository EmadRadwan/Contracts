namespace Application.CRM.SalesOpportunities;

/// <summary>
/// DTO representing a Sales Opportunity (Lead/Deal).
/// This is the proper CRM Lead entity - NOT a person, but a business opportunity.
/// </summary>
public class SalesOpportunityDto
{
    public string? SalesOpportunityId { get; set; }

    // Business identity
    public string? OpportunityName { get; set; }
    public string? Description { get; set; }

    // Money
    public decimal? EstimatedAmount { get; set; }
    public string? CurrencyUomId { get; set; }
    public decimal? EstimatedProbability { get; set; }

    // Pipeline state
    public string? OpportunityStageId { get; set; }
    public string? OpportunityStageName { get; set; }
    public int? StageSequenceNum { get; set; }

    // Ownership
    public string? OwnerPartyId { get; set; }
    public string? OwnerName { get; set; }

    // Brokerage (for indirect sales)
    public string? BrokerPartyId { get; set; }
    public string? BrokerName { get; set; }

    // Lifecycle
    public DateTime? EstimatedCloseDate { get; set; }
    public DateTime? CreatedStamp { get; set; }

    // Next action
    public string? NextStep { get; set; }
    public DateTime? NextStepDate { get; set; }

    // Attribution
    public string? DataSourceId { get; set; }
    public string? MarketingCampaignId { get; set; }

    // Type
    public string? TypeEnumId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? WorkEffortName { get; set; }
    public string? ProductId { get; set; }
    public string? ProductName { get; set; }
    public bool IsWon { get; set; }
    public bool IsClosed { get; set; }

    // Linked leads (for create/update)
    public List<SalesOpportunityLeadDto> Leads { get; set; } = new();
}

/// <summary>
/// DTO for leads linked to a sales opportunity.
/// Enables many-to-many relationship between opportunities and people.
/// </summary>
public class SalesOpportunityLeadDto
{
    public string? PartyId { get; set; }
    public string? PartyName { get; set; }
    public string? RoleTypeId { get; set; }  // defaults to "LEAD"; future: "DECISION_MAKER", "INFLUENCER"
    public string? RoleDescription { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? DataSourceId { get; set; }
}
