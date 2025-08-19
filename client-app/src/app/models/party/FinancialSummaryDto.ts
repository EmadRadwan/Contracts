export interface FinancialSummaryDto {
    TotalSalesInvoice: number;
    TotalPurchaseInvoice: number;
    TotalPaymentsIn: number;
    TotalPaymentsOut: number;
    TotalInvoiceNotApplied: number;
    TotalPaymentNotApplied: number;
    TotalToBePaid?: number;
    TotalToBeReceived?: number;
}

export interface InvoiceApplPaymentDto {
    InvoiceId: string;
    InvoiceTypeId: string;
    InvoiceDate: string;
    Total: number;
    AmountApplied: number;
    AmountToApply: number;
    PaymentId?: string;
    PaymentEffectiveDate?: string;
    PaymentAmount: number;
    CurrencyUomId: string;
}

export interface UnappliedInvoiceDto {
    InvoiceId: string;
    TypeDescription: string;
    InvoiceDate: string;
    Amount: number;
    UnappliedAmount: number;
    CurrencyUomId: string;
    InvoiceTypeId: string;
    InvoiceParentTypeId?: string;
}
