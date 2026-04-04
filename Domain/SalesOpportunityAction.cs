using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class SalesOpportunityAction
{
    public SalesOpportunityAction()
    {
        // Default constructor for EF Core
    }

    public string SalesOpportunityActionId { get; set; } = null!;   // Surrogate PK (recommended)

    public string SalesOpportunityId { get; set; } = null!;

    // Main fields inspired by the other CRM
    public string? ActionTypeId { get; set; }           // e.g. NOT_INTERESTED, FOLLOW_UP, SITE_VISIT, PROPOSAL_SENT, CANCELLED
    public bool IsAnswered { get; set; } = false;       // The "Answer" toggle
    public DateTime? ActionDate { get; set; }           // Next Action Date
    public string? CancelReasonId { get; set; }         // When action is a cancellation
    public string? Comment { get; set; }                // Main comment (required in UI)

    // Audit fields (matching your existing style)
    public string CreatedByUserLogin { get; set; } = null!;
    public DateTime CreatedStamp { get; set; }
    public DateTime LastUpdatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }

    // Navigation Properties
    public virtual SalesOpportunity SalesOpportunity { get; set; } = null!;
    public virtual Enumeration? ActionType { get; set; }           // Recommended: use your Enumeration table
    public virtual Enumeration? CancelReason { get; set; }
    public virtual UserLogin CreatedByUserLoginNavigation { get; set; } = null!;
}