using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class PartyAcctgPreference
{
    public PartyAcctgPreference()
    {
        PartyPrefDocTypeTpls = new HashSet<PartyPrefDocTypeTpl>();
    }

    public string PartyId { get; set; } = null!;
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

    public Uom? BaseCurrencyUom { get; set; }
    public Enumeration? CogsMethod { get; set; }
    public GlJournal? ErrorGlJournal { get; set; }
    public CustomMethod? InvoiceSeqCustMeth { get; set; }
    public Enumeration? InvoiceSequenceEnum { get; set; }
    public CustomMethod? OrderSeqCustMeth { get; set; }
    public Enumeration? OrderSequenceEnum { get; set; }
    public Party Party { get; set; } = null!;
    public CustomMethod? QuoteSeqCustMeth { get; set; }
    public Enumeration? QuoteSequenceEnum { get; set; }
    public PaymentMethod? RefundPaymentMethod { get; set; }
    public Enumeration? TaxForm { get; set; }
    public ICollection<PartyPrefDocTypeTpl> PartyPrefDocTypeTpls { get; set; }
}