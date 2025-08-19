namespace Application.Shipments.OrganizationGlSettings;

public class PartyAcctgPreferenceDto
{
    public string PartyId { get; set; }
    public int? FiscalYearStartMonth { get; set; }
    public int? FiscalYearStartDay { get; set; }
    public string? TaxFormId { get; set; }
    public string? CogsMethodId { get; set; }
    public string? BaseCurrencyUomId { get; set; }
    public string? InvoiceSeqCustMethId { get; set; }
    public string? InvoiceIdPrefix { get; set; }
    public int? LastInvoiceNumber { get; set; }
    public DateTime? LastInvoiceRestartDate { get; set; }
    public string? UseInvoiceIdForReturns { get; set; }
    public string? QuoteSeqCustMethId { get; set; }
    public string? QuoteIdPrefix { get; set; }
    public int? LastQuoteNumber { get; set; }
    public string? OrderSeqCustMethId { get; set; }
    public string? OrderIdPrefix { get; set; }
    public int? LastOrderNumber { get; set; }
    public string? RefundPaymentMethodId { get; set; }
    public string? ErrorGlJournalId { get; set; }
    public string? EnableAccounting { get; set; }
    public string? InvoiceSequenceEnumId { get; set; }
    public string? OrderSequenceEnumId { get; set; }
    public string? QuoteSequenceEnumId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }
}