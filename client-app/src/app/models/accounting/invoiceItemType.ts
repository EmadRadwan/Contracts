export interface InvoiceItemType {
    invoiceItemTypeId: string;
    description: string;
    parentTypeId?: string
    hasTable: string
    defaultGlAccountId?: string
}