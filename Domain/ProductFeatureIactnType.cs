namespace Domain;

public class ProductFeatureIactnType
{
    public ProductFeatureIactnType()
    {
        InverseParentType = new HashSet<ProductFeatureIactnType>();
        ProductFeatureIactns = new HashSet<ProductFeatureIactn>();
    }

    public string ProductFeatureIactnTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductFeatureIactnType? ParentType { get; set; }
    public ICollection<ProductFeatureIactnType> InverseParentType { get; set; }
    public ICollection<ProductFeatureIactn> ProductFeatureIactns { get; set; }
}