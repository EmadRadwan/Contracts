export interface Party {
    partyId: string;
    partyTypeId: string;
    mainRole: string;
    description: string;
    partyTypeDescription: string;
    statusDescription: string;
    preferredCurrencyUOMId: string;
    statusId: string;
    groupName: string;
    firstName: string;
    middleName: string;
    lastName: string;
    personalTitle: string;
    mobileCountryCode: string;
    mobileAreaCode: string;
    mobileContactNumber: string;
    workCountryCode: string;
    workAreaCode: string;
    workContactNumber: string;
    faxCountryCode: string;
    faxAreaCode: string;
    faxContactNumber: string;
    infoString: string;
    address1: string;
    address2: string;
    city: string;
    geoId: string;
    geoName: string;
}

export interface PartyLov {
    partyId: string;
    description: string;
}

export interface PartyParams {
    orderBy: string;
    searchTerm?: string;
    roleTypes?: string[];
    pageNumber?: number;
    pageSize?: number;
}

export interface PartyTaxStatus {
    partyId: string;
    isExempt: string;
}