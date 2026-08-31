/**
 * Sales Opportunity (Lead/Deal) - the proper CRM Lead entity.
 *
 * KEY CONCEPT:
 * A Lead is a business opportunity (potential sale).
 * People (Leads) are linked to business opportunities via the leads array.
 */
export interface SalesOpportunity {
    salesOpportunityId?: string;

    // Business identity
    opportunityName?: string;
    description?: string;

    // Money
    estimatedAmount?: number;
    currencyUomId?: string;
    estimatedProbability?: number;

    // Pipeline state
    opportunityStageId?: string;
    opportunityStageName?: string;
    stageSequenceNum?: number;

    // Ownership
    ownerPartyId?: string;
    ownerName?: string;

    // Brokerage (for indirect sales)
    brokerPartyId?: string;
    brokerName?: string;

    // Lifecycle
    estimatedCloseDate?: string;
    createdStamp?: string;

    // Next action
    nextStep?: string;
    nextStepDate?: string;

    // Attribution
    dataSourceId?: string;
    marketingCampaignId?: string;

    // Type
    typeEnumId?: string;

    //Project and Product
    workEffortId?: string;
    workEffortName?: string;
    productId?: string;
    productName?: string;
    isWon: boolean
    isClosed: boolean

    // Linked leads
    leads: SalesOpportunityLead[];
}

/**
 * Sales Opportunity Action
 */

export interface SalesOpportunityAction {
        // Mandatory IDs
        salesOpportunityActionId?: string;
        salesOpportunityId?: string;

        // Main fields
        actionTypeId?: string;
        actionTypeDescription?: string;
        isAnswered?: boolean;
        actionDate?: string;
        cancelReasonId?: string;
        cancelReasonDescription?: string;
        comment?: string;

        // meeting-specific fields
        meetingTypeId?: string;
        meetingTypeDescription?: string;
        meetingLocationId?: string;
        meetingLocationDescription?: string;
        note?: string;

        // unit-related fields
        productId?: string;
        productName?: string;
        workEffortId?: string;
        workEffortName?: string;

        // audit fields
        createdStamp?: string;
    }

/**
 * Lead linked to a Sales Opportunity.
 * Enables many-to-many relationship between opportunities and people.
 */
export interface SalesOpportunityLead {
    partyId?: string;
    partyName?: string;
    roleTypeId?: string;  // defaults to "LEAD"; future: "DECISION_MAKER", "INFLUENCER"
    roleDescription?: string;
    email?: string;
    dataSourceId?: string;
    phone?: string;
}

/**
 * Pipeline Stage for board view.
 */
export interface OpportunityStage {
    opportunityStageId: string;
    description?: string;
    defaultProbability?: number;
    sequenceNum?: number;
}

export interface OpportunityAction {
    actionId: string;
    description?: string;
}

export interface OpportunityCancellationReason {
    actionId: string;
    description?: string;
}

export interface OpportunityMeetingType {
    meetingTypeId: string;
    description?: string;
}

export interface OpportunityMeetingLocation {
    meetingLocationId: string;
    description?: string;
}


/**
 * Query parameters for listing opportunities.
 */
export interface OpportunityQueryParams {
    stageId?: string;
    ownerPartyId?: string;
    closeDateFrom?: string;
    closeDateTo?: string;
    search?: string;
    sortBy?: string;
    sortDesc?: boolean;
}

/**
 * Request for updating only the stage (drag-and-drop).
 */
export interface UpdateStageRequest {
    stageId: string;
    opportunityName: string;
}

export interface SalesOpportunityHistory {
  salesOpportunityHistoryId: string;
  salesOpportunityId?: string | null;
  description?: string | null;
  nextStep?: string | null;
  estimatedAmount?: number | null;
  estimatedProbability?: number | null;
  currencyUomId?: string | null;
  estimatedCloseDate?: string | null; // DateTime -> ISO string
  opportunityStageId?: string | null;
  opportunityStageDescription?: string | null;
  changeNote?: string | null;
  modifiedByUserLogin?: string | null;
  modifiedTimestamp?: string | null; // DateTime -> ISO string
  createdStamp?: string | null;
  lastUpdatedStamp?: string | null;
  createdTxStamp?: string | null;
  lastUpdatedTxStamp?: string | null;
}
/**
 * An open opportunity a lead is already linked to. Advisory only - a lead may
 * legitimately be on several opportunities (one buyer, several units).
 */
export interface LeadOpenOpportunity {
    leadPartyId?: string;
    leadName?: string;
    salesOpportunityId?: string;
    opportunityName?: string;
    opportunityStageId?: string;
    stageDescription?: string;
    productId?: string;
    estimatedAmount?: number;
    estimatedCloseDate?: string;
}
