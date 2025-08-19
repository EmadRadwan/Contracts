namespace Domain;

public class DocumentType
{
    public DocumentType()
    {
        DocumentTypeAttrs = new HashSet<DocumentTypeAttr>();
        Documents = new HashSet<Document>();
        InverseParentType = new HashSet<DocumentType>();
    }

    public string DocumentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public DocumentType? ParentType { get; set; }
    public ICollection<DocumentTypeAttr> DocumentTypeAttrs { get; set; }
    public ICollection<Document> Documents { get; set; }
    public ICollection<DocumentType> InverseParentType { get; set; }
}