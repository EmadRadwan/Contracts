export interface SalesCommission {
    salesCommissionId?: string;
    salesRequestId: string;
    saleTypeId: string;
    statusId?: string;
    commissionDate?: Date | string | null;

    projectId?: string;
    projectName?: string;
    apartmentName?: string;
    salePrice: number;
    collectedAmount?: number | null;

    salesRepPartyId?: string | null;
    salesRepName?: string | null;
    salesRepPercent: number;
    salesRepAmount?: number | null;
    salesRepNetAmount?: number | null;

    managerPartyId?: string | null;
    managerName?: string | null;
    managerPercent: number;
    managerAmount?: number | null;
    managerNetAmount?: number | null;

    salesRep2PartyId?: string | null;
    salesRep2Name?: string | null;
    salesRep2Percent?: number | null;
    salesRep2Amount?: number | null;
    salesRep2NetAmount?: number | null;

    manager2PartyId?: string | null;
    manager2Name?: string | null;
    manager2Percent?: number | null;
    manager2Amount?: number | null;
    manager2NetAmount?: number | null;

    externalCompanyPartyId?: string | null;
    externalCompanyName?: string | null;
    externalCompanyPercent?: number | null;
    externalCompanyGrossAmount?: number | null;
    externalCompanyNetAmount?: number | null;

    externalSalesRepPartyId?: string | null;
    externalSalesRepName?: string | null;
    externalSalesRepPercent?: number | null;
    externalSalesRepAmount?: number | null;
    externalSalesRepNetAmount?: number | null;
    hasExternalSalesRepWithholdingTaxExemption: boolean;
    externalSalesRepNationalId?: string | null;

    externalManagerPartyId?: string | null;
    externalManagerName?: string | null;
    externalManagerPercent?: number | null;
    externalManagerAmount?: number | null;
    externalManagerNetAmount?: number | null;
    hasExternalManagerWithholdingTaxExemption: boolean;
    externalManagerNationalId?: string | null;

    hasVatExemption: boolean;
    hasWithholdingTaxExemption: boolean;
    vatPercent: number;
    withholdingTaxPercent: number;

    notes?: string | null;
    createdStamp?: Date | string | null;
    lastUpdatedStamp?: Date | string | null;
}

export interface CommissionRateDefaults {
    salesRepPercent: number;
    managerPercent: number;
    externalCompanyPercent?: number | null;
    externalSalesRepPercent?: number | null;
    externalManagerPercent?: number | null;
}

export interface CommissionDefaults {
    projectId?: string;
    projectName?: string;
    apartmentName?: string;
    salePrice: number;
    collectedAmount: number;
    collectedRatio: number;
    existingCommissionId?: string | null;
    // Key = saleTypeId (COMM_SALE_DIRECT | COMM_SALE_PERSONAL | COMM_SALE_INDIRECT)
    rates: Record<string, CommissionRateDefaults>;
}

export interface ApprovedSalesRequestLovItem {
    salesRequestId: string;
    customerName?: string | null;
    apartmentName: string;
    projectName: string;
    totalPrice?: number | null;
}

export interface ApprovedSalesRequestEnvelope {
    items: ApprovedSalesRequestLovItem[];
    totalCount: number;
}
