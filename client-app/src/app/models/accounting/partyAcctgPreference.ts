export interface PartyAcctgPreference {
    partyId: string;
    fiscalYearStartMonth?: number | null;
    fiscalYearStartDay?: number | null;
    taxFormId?: string | null;
    cogsMethodId?: string | null;
    baseCurrencyUomId?: string | null;
    invoiceSeqCustMethId?: string | null;
    invoiceIdPrefix?: string | null;
    lastInvoiceNumber?: number | null;
    lastInvoiceRestartDate?: Date | null;
    useInvoiceIdForReturns?: string | null;
    quoteSeqCustMethId?: string | null;
    quoteIdPrefix?: string | null;
    lastQuoteNumber?: number | null;
    orderSeqCustMethId?: string | null;
    orderIdPrefix?: string | null;
    lastOrderNumber?: number | null;
    refundPaymentMethodId?: string | null;
    errorGlJournalId?: string | null;
    enableAccounting?: string | null;
    invoiceSequenceEnumId?: string | null;
    orderSequenceEnumId?: string | null;
    quoteSequenceEnumId?: string | null;
}
