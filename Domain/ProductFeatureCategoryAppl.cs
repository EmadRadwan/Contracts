namespace Domain;

public class ProductFeatureCategoryAppl
{
    public string ProductCategoryId { get; set; } = null!;
    public string ProductFeatureCategoryId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductCategory ProductCategory { get; set; } = null!;
    public ProductFeatureCategory ProductFeatureCategory { get; set; } = null!;
}