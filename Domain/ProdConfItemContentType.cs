namespace Domain;

public class ProdConfItemContentType
{
    public ProdConfItemContentType()
    {
        InverseParentType = new HashSet<ProdConfItemContentType>();
        ProdConfItemContents = new HashSet<ProdConfItemContent>();
    }

    public string ConfItemContentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProdConfItemContentType? ParentType { get; set; }
    public ICollection<ProdConfItemContentType> InverseParentType { get; set; }
    public ICollection<ProdConfItemContent> ProdConfItemContents { get; set; }
}