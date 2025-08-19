using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class InventoryItemFeature
{
    public InventoryItemFeature()
    {
        // Constructor is empty as no collections are needed for this junction table
    }

    public string InventoryItemId { get; set; } = null!;

    public string ProductFeatureId { get; set; } = null!;

    public string ProductId { get; set; } = null!;

    public InventoryItem InventoryItem { get; set; } = null!;

    public ProductFeature ProductFeature { get; set; } = null!;

    public Product Product { get; set; } = null!;
}