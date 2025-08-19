namespace Domain;

public class InvoiceItemAssoc
{
    public string InvoiceIdFrom { get; set; } = null!;
    public string InvoiceItemSeqIdFrom { get; set; } = null!;
    public string InvoiceIdTo { get; set; } = null!;
    public string InvoiceItemSeqIdTo { get; set; } = null!;
    public string InvoiceItemAssocTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? PartyIdFrom { get; set; }
    public string? PartyIdTo { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InvoiceItem InvoiceI { get; set; } = null!;
    public InvoiceItem InvoiceINavigation { get; set; } = null!;
    public InvoiceItemAssocType InvoiceItemAssocType { get; set; } = null!;
}