import {CustomerRequestItem} from "./customerRequestItem";

export interface CustomerRequest {
    custRequestId: string;
    custRequestTypeId?: any;
    custRequestTypeDescription?: any;
    statusId?: any;
    statusDescription?: any;
    fromPartyId?: any;
    fromPartyName?: string;
    custRequestDate?: Date;
    productStoreId?: any;
    productStoreName?: any;
    salesChannelEnumId?: any;
    salesChannelEnumDescription?: any;
    currencyUomId?: any;
    currencyUomDescription?: any;
    openDateTime?: any;
    closedDateTime?: any;
    internalComment?: any;
    createdDate?: any;
    createdByUserLogin?: any;
    createdByUserLoginName?: any;
    lastModifiedByUserLogin?: any;
    lastModifiedByUserLoginName?: any;
    lastUpdatedStamp?: any;
    createdStamp?: any;
    billed?: any;
    allowSubmit?: string
    custRequestItems?: CustomerRequestItem[];
}

export interface CustomerRequestParams {
    orderBy: string;
    searchTerm?: string;
    pageNumber: number;
    pageSize: number;
}