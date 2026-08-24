/**
 * Lead (Person) in the CRM.
 *
 * KEY CONCEPT:
 * A Lead is a PERSON (who you know).
 * Leads are linked to Sales Opportunities via SalesOpportunityRole.
 */
export interface Lead {
    partyId?: string;

    // Identity
    firstName?: string;
    middleName?: string;
    lastName?: string;
    fullName?: string;

    // Communication
    infoString?: string; //email field
    email?: string;
    phone?: string;
    mobilePhone?: string;
    mobileContactNumber?: string; //mobilePhone field

    // Address
    address1?: string;
    address2?: string;
    city?: string;
    postalCode?: string;
    geoId?: string;
    countryGeoId?: string;
    countryName?: string;

    // CRM metadata
    dataSourceId?: string;
    tags?: string[];
    marketingStatus?: string;

    // Status
    statusId?: string;
    statusDescription?: string;

    // Audit
    createdStamp?: string;
    lastContactedTime?: string;

    // Organization
    organizationPartyId?: string;
    organizationName?: string;
    leadTemperatureId: 'C' | 'F'

    // Assignment - current LEAD_OWNER relationship (undefined when unassigned)
    ownerPartyId?: string;
    ownerName?: string;
    assignedDate?: string;
}

/**
 * Current ownership of a lead.
 */
export interface LeadAssignment {
    leadPartyId: string;
    leadName?: string;
    ownerPartyId?: string;
    ownerName?: string;
    fromDate?: string;
    thruDate?: string;
    comments?: string;
}

/**
 * Payload for POST /leads/{id}/assign
 */
export interface AssignLeadRequest {
    ownerPartyId: string;
    comments?: string;
}

/**
 * One entry in a lead's ownership history.
 */
export interface LeadAssignmentHistory {
    ownerPartyId?: string;
    ownerName?: string;
    fromDate: string;
    thruDate?: string;
    comments?: string;
    /** UserLogin that performed the assignment; absent on older rows. */
    assignedByUserLogin?: string;
    isCurrent: boolean;
}

/**
 * Payload for POST /leads/bulk-assign
 */
export interface BulkAssignLeadsRequest {
    leadPartyIds: string[];
    ownerPartyId: string;
    comments?: string;
}

export interface BulkAssignError {
    leadPartyId: string;
    leadName?: string;
    reason: string;
}

export interface BulkAssignResult {
    totalReceived: number;
    successful: number;
    failed: number;
    alreadyOwned: number;
    ownerPartyId?: string;
    ownerName?: string;
    errors: BulkAssignError[];
}

/**
 * Lightweight DTO for dropdowns/pickers.
 */
export interface LeadLov {
    partyId: string;
    fullName?: string;
    email?: string;
    phone?: string;
    dataSourceId?: string;
}

/**
 * Query parameters for listing leads.
 */
export interface LeadQueryParams {
    search?: string;
    dataSourceId?: string;
    sortBy?: string;
    sortDesc?: boolean;
}
