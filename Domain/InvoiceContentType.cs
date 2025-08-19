namespace Domain;

public class InvoiceContentType
{
    public InvoiceContentType()
    {
        InverseParentType = new HashSet<InvoiceContentType>();
        InvoiceContents = new HashSet<InvoiceContent>();
    }

    public string InvoiceContentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public InvoiceContentType? ParentType { get; set; }
    public ICollection<InvoiceContentType> InverseParentType { get; set; }
    public ICollection<InvoiceContent> InvoiceContents { get; set; }
}