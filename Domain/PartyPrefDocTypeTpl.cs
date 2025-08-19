namespace Domain;

public class PartyPrefDocTypeTpl
{
    public string PartyPrefDocTypeTplId { get; set; } = null!;
    public string? PartyId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? InvoiceTypeId { get; set; }
    public string? OrderTypeId { get; set; }
    public string? QuoteTypeId { get; set; }
    public string? CustomScreenId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InvoiceType? InvoiceType { get; set; }
    public OrderType? OrderType { get; set; }
    public Party? Party { get; set; }
    public PartyAcctgPreference? PartyNavigation { get; set; }
    public QuoteType? QuoteType { get; set; }
}