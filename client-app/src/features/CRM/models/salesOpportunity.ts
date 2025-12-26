/**
 * Sales Opportunity (Lead/Deal) - the proper CRM Lead entity.
 *
 * KEY CONCEPT:
 * A Lead is NOT a person. A Lead is a business opportunity (potential sale).
 * People (Contacts) are linked to Leads via the contacts array.
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

    // Linked contacts
    contacts: SalesOpportunityContact[];
}

/**
 * Contact linked to a Sales Opportunity.
 * Enables many-to-many relationship between opportunities and people.
 */
export interface SalesOpportunityContact {
    partyId?: string;
    partyName?: string;
    roleTypeId?: string;  // e.g., "LEAD_CONTACT", "DECISION_MAKER", "INFLUENCER"
    roleDescription?: string;
    email?: string;
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
