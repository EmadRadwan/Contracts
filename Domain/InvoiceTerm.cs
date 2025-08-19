namespace Domain;

public class InvoiceTerm
{
    public InvoiceTerm()
    {
        InvoiceTermAttributes = new HashSet<InvoiceTermAttribute>();
    }

    public string InvoiceTermId { get; set; } = null!;
    public string? TermTypeId { get; set; }
    public string? InvoiceId { get; set; }
    public string? InvoiceItemSeqId { get; set; }
    public decimal? TermValue { get; set; }
    public int? TermDays { get; set; }
    public string? TextValue { get; set; }
    public string? Description { get; set; }
    public string? UomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Invoice? Invoice { get; set; }
    public TermType? TermType { get; set; }
    public ICollection<InvoiceTermAttribute> InvoiceTermAttributes { get; set; }
}