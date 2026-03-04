export interface SalesRequest {
    salesRequestId: string;
    productId?: string | null;           // kept for backward compat, but apartmentId is primary
    apartmentId?: string | null;
    apartmentName?: string | null;
    projectName?: string | null;
    floorNumber?: string | null;
    isChequesDelivered?: string | null;
    apartmentSpaceM2?: number | null;
    gardenSpaceM2?: number | null;
    apartmentPricePerM2?: number | null;
    gardenPricePerM2?: number | null;
    apartmentType?: string | null;
    apartmentStatusId?: string | null;
    apartmentStatusDescription?: string | null;
    apartmentReservedBySalesRequestId?: string | null;

    fromPartyId?: string | null;         // customer
    fromPartyName?: string | null;
    fromPartyPhone?: string | null;

    employeePartyId?: string | null;
    employeeName?: string | null;
    employeePhone?: string | null;

    customerId: string;                  // kept, but fromPartyId is used more
    saleDate?: string | null;
    discount?: number | null;
    totalPrice?: number | null;
    advancePayment?: number | null;
    maintenanceDeposit?: number | null;
    numberOfInstallments?: number | null;
    monthsBetweenInstallments?: number | null;  // rename from durationBetweenInstallments
    dateOfFirstInstallment?: string | null;
    comments?: string | null;
    statusId?: string | null;
    statusDescription?: string | null;
    advancePercent?: number | null;
    maintenancePercent?: number | null;

    lastUpdatedStamp?: string | null;
    createdStamp?: string | null;

    // Optional nested (if used elsewhere)
    product?: { productId: string; productName: string };
    customer?: { partyId: string; fullName: string };
}