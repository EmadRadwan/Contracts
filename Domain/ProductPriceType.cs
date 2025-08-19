using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductPriceType
{
    public ProductPriceType()
    {
        ProductFeaturePrices = new HashSet<ProductFeaturePrice>();
        ProductPrices = new HashSet<ProductPrice>();
    }

    public string ProductPriceTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public string? DescriptionArabic { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ProductFeaturePrice> ProductFeaturePrices { get; set; }
    public ICollection<ProductPrice> ProductPrices { get; set; }
}