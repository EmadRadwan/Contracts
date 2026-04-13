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
    }

/**
 * Lead linked to a Sales Opportunity.
 * Enables many-to-many relationship between opportunities and people.
 */
export interface SalesOpportunityLead {
    partyId?: string;
    partyName?: string;
    roleTypeId?: string;  // e.g., "LEAD_CONTACT", "DECISION_MAKER", "INFLUENCER"
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
