using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ProductPricePurpose
{
    public ProductPricePurpose()
    {
        OrderPaymentPreferences = new HashSet<OrderPaymentPreference>();
        ProductPaymentMethodTypes = new HashSet<ProductPaymentMethodType>();
        ProductPrices = new HashSet<ProductPrice>();
    }

    public string ProductPricePurposeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<OrderPaymentPreference> OrderPaymentPreferences { get; set; }
    public ICollection<ProductPaymentMethodType> ProductPaymentMethodTypes { get; set; }
    public ICollection<ProductPrice> ProductPrices { get; set; }
}