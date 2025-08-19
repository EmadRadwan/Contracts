namespace Domain;

public class ProductCategoryContentType
{
    public ProductCategoryContentType()
    {
        InverseParentType = new HashSet<ProductCategoryContentType>();
        ProductCategoryContents = new HashSet<ProductCategoryContent>();
    }

    public string ProdCatContentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductCategoryContentType? ParentType { get; set; }
    public ICollection<ProductCategoryContentType> InverseParentType { get; set; }
    public ICollection<ProductCategoryContent> ProductCategoryContents { get; set; }
}