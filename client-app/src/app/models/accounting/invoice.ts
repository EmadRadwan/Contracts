import {InvoiceItem} from "./invoiceItem";

export interface Invoice {
    invoiceId: string;
    invoiceTypeId?: string;
    invoiceTypeDescription?: string;
    partyIdFrom?: string;
    fromPartyName?: string;
    partyId?: string;
    toPartyName?: string;
    roleTypeId?: any;
    statusId?: any;
    statusDescription?: string;
    billingAccountId?: any;
    billingAccountName?: string
    contactMechId?: any;
    invoiceDate?: string;
    dueDate?: any;
    paidDate?: any;
    invoiceMessage?: any;
    referenceNumber?: any;
    description?: any;
    currencyUomId?: any;
    currencyUomName?: any;
    total?: any;
    outstandingAmount?: any;
    items?: InvoiceItem[]
    allowSubmit?: any
}

export interface InvoiceParams {
    orderBy: string;
    searchTerm?: string;
    pageNumber?: number;
    pageSize?: number;
    invoiceTypeId?: string;
}

