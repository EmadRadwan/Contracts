namespace Domain;

public class ProductFeatureCategory
{
    public ProductFeatureCategory()
    {
        InverseParentCategory = new HashSet<ProductFeatureCategory>();
        ProductFeatureCategoryAppls = new HashSet<ProductFeatureCategoryAppl>();
        ProductFeatures = new HashSet<ProductFeature>();
    }

    public string ProductFeatureCategoryId { get; set; } = null!;
    public string? ParentCategoryId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductFeatureCategory? ParentCategory { get; set; }
    public ICollection<ProductFeatureCategory> InverseParentCategory { get; set; }
    public ICollection<ProductFeatureCategoryAppl> ProductFeatureCategoryAppls { get; set; }
    public ICollection<ProductFeature> ProductFeatures { get; set; }
}