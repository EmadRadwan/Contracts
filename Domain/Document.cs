namespace Domain;

public class Document
{
    public Document()
    {
        DocumentAttributes = new HashSet<DocumentAttribute>();
    }

    public string DocumentId { get; set; } = null!;
    public string? DocumentTypeId { get; set; }
    public DateTime? DateCreated { get; set; }
    public string? Comments { get; set; }
    public string? DocumentLocation { get; set; }
    public string? DocumentText { get; set; }
    public byte[]? ImageData { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DocumentType? DocumentType { get; set; }
    public ShippingDocument ShippingDocument { get; set; } = null!;
    public ICollection<DocumentAttribute> DocumentAttributes { get; set; }
}