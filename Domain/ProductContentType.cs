namespace Domain;

public class ProductContentType
{
    public ProductContentType()
    {
        InverseParentType = new HashSet<ProductContentType>();
        ProductContents = new HashSet<ProductContent>();
        ProductPromoContents = new HashSet<ProductPromoContent>();
    }

    public string ProductContentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductContentType? ParentType { get; set; }
    public ICollection<ProductContentType> InverseParentType { get; set; }
    public ICollection<ProductContent> ProductContents { get; set; }
    public ICollection<ProductPromoContent> ProductPromoContents { get; set; }
}