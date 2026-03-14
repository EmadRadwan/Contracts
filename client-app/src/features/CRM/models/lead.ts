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
    lastName?: string;
    personalTitle?: string;  // Mr, Mrs, Dr, etc.
    fullName?: string;

    // Communication
    email?: string;
    phone?: string;
    mobilePhone?: string;

    // Address
    address1?: string;
    address2?: string;
    city?: string;
    postalCode?: string;
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
}

/**
 * Lightweight DTO for dropdowns/pickers.
 */
export interface LeadLov {
    partyId: string;
    fullName?: string;
    email?: string;
    phone?: string;
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
