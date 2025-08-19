namespace Domain;

public class InvoiceContent
{
    public string InvoiceId { get; set; } = null!;
    public string InvoiceContentTypeId { get; set; } = null!;
    public string ContentId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Content Content { get; set; } = null!;
    public Invoice Invoice { get; set; } = null!;
    public InvoiceContentType InvoiceContentType { get; set; } = null!;
}