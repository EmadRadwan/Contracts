export interface SalesRequest {
    salesRequestId: string;
    productId: string;
    apartmentPricePerM2?: number | null;
    gardenPricePerM2?: number | null;
    customerId: string;
    saleDate?: string | null;
    discount?: number | null;
    totalPrice?: number | null;
    comments?: string | null;
    statusId?: string | null;
    statusDescription?: string | null;
    advancePayment?: number | null;
    numberOfInstallments?: number | null;
    dateOfFirstInstallment?: string | null;
    durationBetweenInstallments?: number | null;
    lastUpdatedStamp?: string | null;
    createdStamp?: string | null;
    product?: { productId: string; productName: string };
    customer?: { partyId: string; fullName: string };
}