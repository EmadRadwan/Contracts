import {QuoteItem} from "./quoteItem";
import {QuoteAdjustment} from "./quoteAdjustment";

export interface Quote {
    quoteId: string;
    quoteTypeId?: any;
    fromPartyId?: any;
    fromPartyName?: string;
    issueDate?: Date;
    statusId?: any;
    currencyUomId?: any;
    agreementId?: string
    productStoreId?: any;
    salesChannelEnumId?: any;
    validFromDate?: Date;
    validThruDate?: Date;
    modificationType?: string;
    grandTotal?: any;
    quoteName?: any;
    description?: any;
    lastUpdatedStamp?: any;
    createdStamp?: any;
    custRequestId?: any;
    allowSubmit?: string;
    statusDescription?: string;
    totalAdjustments?: number;
    currentMileage?: number;
    vehicleId?: any;
    chassisNumber?: string;
    customerRemarks?: any;
    internalRemarks?: any;
    quoteItems?: QuoteItem[];
    quoteAdjustments?: QuoteAdjustment[];
}

export interface QuoteParams {
    orderBy: string;
    searchTerm?: string;
    customerPhone?: string;
    customerName?: string
    pageNumber?: number;
    pageSize?: number;
    chassisNumber?: string;
    plateNumber?: string;
    ownerPartyId?: string;
}
