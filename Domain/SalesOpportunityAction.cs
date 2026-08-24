using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class SalesOpportunityAction
{
    public SalesOpportunityAction() { }

    // Primary Key
    public string SalesOpportunityActionId { get; set; } = null!;

    // Foreign Key to Opportunity
    public string SalesOpportunityId { get; set; } = null!;

    // ==================== Core CRM Fields ====================

    /// <summary>
    /// Next Action (from the first dropdown) - e.g. DONE_DEAL, LOST_DEAL, FOLLOW_UP, etc.
    /// </summary>
    public string ActionTypeId { get; set; } = null!;

    /// <summary>
    /// The "Answer" toggle switch
    /// </summary>
    public bool IsAnswered { get; set; } = false;

    /// <summary>
    /// Date for this action (closing date or next action date)
    /// </summary>
    public DateTime? ActionDate { get; set; }

    /// <summary>
    /// Optional: Schedule the next follow-up action after this one
    /// Useful after "Follow up" → "Meeting", or after "Meeting" → "Following after meeting"
    /// </summary>
    public string? NextActionTypeId { get; set; }

    /// <summary>
    /// Cancel Reason - ONLY used when the action is a loss/cancellation
    /// Should be NULL for positive actions like "Done Deal"
    /// </summary>
    public string? CancelReasonId { get; set; }

    /// <summary>
    /// Main comment (required in UI)
    /// </summary>
    public string? Comment { get; set; }

    // ==================== New Fields for Meetings ====================

    /// <summary>
    /// Type of meeting (e.g. In-person, Online/Zoom, Phone Call, etc.)
    /// Only relevant when ActionType is related to a meeting
    /// </summary>
    public string? MeetingTypeId { get; set; }

    /// <summary>
    /// Location of the meeting (e.g. Office, Client Site, Coffee Shop, etc.)
    /// Only relevant for in-person meetings
    /// </summary>
    public string? MeetingLocationId { get; set; }

    /// <summary>
    /// Additional notes specific to this action/meeting
    /// </summary>
    public string? Note { get; set; }

    // ==================== Helper Properties for Business Logic ====================

    /// <summary>
    /// True for successful closure (Done Deal, Reservation, Interested, etc.)
    /// </summary>
    public bool IsWon { get; set; } = false;

    /// <summary>
    /// Returns true if this ActionType requires a Cancel Reason
    /// </summary>
    public bool RequiresCancelReason =>
        ActionTypeId is "LOST_DEAL" or "NOT_INTERESTED" or "CANCELLATION" 
        or "NO_ANSWER" or "NO_ANSWER_ALL_THE_TIME" or "UN_REACHABLE" 
        or "WRONG_NUMBER" or "WRONG_REQUEST";

    /// <summary>
    /// True if this action closes the opportunity (won or lost)
    /// </summary>
    public bool IsClosingAction =>
        ActionTypeId is "DONE_DEAL" or "LOST_DEAL" or "CANCELLATION" or "NOT_INTERESTED";

    // ==================== Audit Fields ====================

    /// <summary>
    /// AspNetUsers.Id of the creating user. Nullable - references AspNetUsers,
    /// NOT the OFBiz USER_LOGIN table, which is a partially-populated seed list.
    /// </summary>
    public string? CreatedByUserLogin { get; set; }
    public DateTime CreatedStamp { get; set; }
    public DateTime LastUpdatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }

    // ==================== Navigation Properties ====================

    public virtual SalesOpportunity SalesOpportunity { get; set; } = null!;

    public virtual Enumeration? ActionType { get; set; }
    public virtual Enumeration? NextActionType { get; set; }
    public virtual Enumeration? CancelReason { get; set; }

    // New Navigation Properties for the added fields
    public virtual Enumeration? MeetingType { get; set; }
    public virtual Enumeration? MeetingLocation { get; set; }

    public virtual AppUserLogin? CreatedByUser { get; set; }
}