export interface InvoiceItem {
    invoiceItemTypeDescription: string;
    invoiceItemProduct?: any
    amount: number;
    quantity: number;
    productId: any;
    overrideOrgPartyId?: any;
    productFeatureId?: any;
    taxableFlag?: any;
    taxAuthorityRateSeqId?: any;
    overrideGlAccountId?: any;
    description?: any;
    invoiceItemSeqId: string;
    uomId?: any;
    invoiceItemTypeId: string;
    productName?: string;
    inventoryItemId?: any;
    taxAuthPartyId?: any;
    parentInvoiceId?: any;
    parentInvoiceItemSeqId?: any;
    taxAuthGeoId?: any;
    invoiceId: string;
    salesOpportunityId?: any;
    isInvoiceItemDeleted?: boolean;
    price?: number
}