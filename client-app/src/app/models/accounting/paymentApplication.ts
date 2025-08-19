export interface PaymentApplication {
    paymentApplicationId: string;
    paymentId: string;
    invoiceId?: string;
    invoiceItemSeqId?: string;
    billingAccountId?: any;
    overrideGlAccountId?: any;
    toPaymentId?: any;
    taxAuthGeoId?: any;
    amountApplied?: number;
    isPaymentApplicationDeleted: boolean
}
