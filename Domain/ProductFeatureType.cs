using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductFeatureType
{
    public ProductFeatureType()
    {
        InverseParentType = new HashSet<ProductFeatureType>();
        ProductFeatures = new HashSet<ProductFeature>();
    }

    public string ProductFeatureTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductFeatureType? ParentType { get; set; }
    public ICollection<ProductFeatureType> InverseParentType { get; set; }
    public ICollection<ProductFeature> ProductFeatures { get; set; }
}