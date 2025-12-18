export interface ReserveRequest {
    // -----------------------------------------------------------------
    // Primary key
    // -----------------------------------------------------------------
    reserveRequestId: string;

    // -----------------------------------------------------------------
    // Foreign keys
    // -----------------------------------------------------------------
    productId: string;
    fromPartyId: string;        // Customer
    employeePartyId?: string | null;

    // -----------------------------------------------------------------
    // Business fields
    // -----------------------------------------------------------------
    reserveDate?: Date | string | null;     // API may return string; handleDatesArray will convert
    reserveAmount?: number | null;
    comments?: string | null;
    payMethod?: string | null;
    statusId?: string | null;

    // -----------------------------------------------------------------
    // Audit stamps
    // -----------------------------------------------------------------
    lastUpdatedStamp?: Date | string | null;
    createdStamp?: Date | string | null;

    // -----------------------------------------------------------------
    // Navigation properties (populated by API if included)
    // -----------------------------------------------------------------
    status?: StatusItem | null;
    product?: Product | null;
    customer?: Party | null;                // FromParty (customer)
    employee?: Party | null;                // Employee who handled the request

    // -----------------------------------------------------------------
    // Optional flattened fields (commonly added by API for grid display)
    // -----------------------------------------------------------------
    // REFACTOR: Added commonly used denormalized fields for easier grid binding
    // Purpose: Avoid deep navigation (e.g., product?.apartmentName) in Kendo Grid if API flattens them
    // Context: Adjust or remove based on what your backend actually returns
    apartmentName?: string | null;
    projectName?: string | null;
    fromPartyName?: string | null;          // Customer name
    employeeName?: string | null;
    statusDescription?: string | null;
}
