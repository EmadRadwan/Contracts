namespace Domain;

public class ProductFeatureGroup
{
    public ProductFeatureGroup()
    {
        ProductFeatureCatGrpAppls = new HashSet<ProductFeatureCatGrpAppl>();
        ProductFeatureGroupAppls = new HashSet<ProductFeatureGroupAppl>();
    }

    public string ProductFeatureGroupId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ProductFeatureCatGrpAppl> ProductFeatureCatGrpAppls { get; set; }
    public ICollection<ProductFeatureGroupAppl> ProductFeatureGroupAppls { get; set; }
}