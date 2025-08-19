using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductFeatureApplType
{
    public ProductFeatureApplType()
    {
        InverseParentType = new HashSet<ProductFeatureApplType>();
        ProductFeatureAppls = new HashSet<ProductFeatureAppl>();
    }

    public string ProductFeatureApplTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductFeatureApplType? ParentType { get; set; }
    public ICollection<ProductFeatureApplType> InverseParentType { get; set; }
    public ICollection<ProductFeatureAppl> ProductFeatureAppls { get; set; }
}