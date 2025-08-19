using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductAverageCostType
{
    public ProductAverageCostType()
    {
        InverseParentType = new HashSet<ProductAverageCostType>();
        ProductAverageCosts = new HashSet<ProductAverageCost>();
    }

    public string ProductAverageCostTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductAverageCostType? ParentType { get; set; }
    public ICollection<ProductAverageCostType> InverseParentType { get; set; }
    public ICollection<ProductAverageCost> ProductAverageCosts { get; set; }
}