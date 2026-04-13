using System;

namespace Application.CRM.SalesOpportunities
{
    public class SalesOpportunityActionDto
    {
        // Mandatory IDs
        public string? SalesOpportunityActionId { get; set; }   // Generated surrogate PK
        public string SalesOpportunityId { get; set; } = null!;         // Mandatory

        // Main fields
        public string? ActionTypeId { get; set; }           // From ENUM_TYPE_ID = 'CRM_ACTION_TYPE'
        public string? ActionTypeDescription { get; set; }  
        public bool IsAnswered { get; set; } = false;
        public DateTime? ActionDate { get; set; }           // Next action / follow-up date
        public string? CancelReasonId { get; set; }         // From ENUM_TYPE_ID = 'CRM_CANCELLATION_REASON'
        public string? CancelReasonDescription { get; set; }  
        public string? Comment { get; set; }                // Main comment (required in most cases)
        public string? MeetingLocationId { get; set; }
        public string? MeetingLocationDescription {get; set;}
        public string? MeetingTypeId { get; set; }
        public string? MeetingTypeDescription {get; set;}

        public string? Note { get; set; }

        // Audit fields
        public string? CreatedByUserLogin { get; set; }
        public DateTime CreatedStamp { get; set; }
        public DateTime LastUpdatedStamp { get; set; }
        public DateTime? CreatedTxStamp { get; set; }
        public DateTime? LastUpdatedTxStamp { get; set; }
    }
}