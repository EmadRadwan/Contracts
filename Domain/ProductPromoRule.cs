using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductPromoRule
{
    public ProductPromoRule()
    {
        ProductPromoActions = new HashSet<ProductPromoAction>();
        ProductPromoConds = new HashSet<ProductPromoCond>();
    }

    public string ProductPromoId { get; set; } = null!;
    public string ProductPromoRuleId { get; set; } = null!;
    public string? RuleName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductPromo ProductPromo { get; set; } = null!;
    public ICollection<ProductPromoAction> ProductPromoActions { get; set; }
    public ICollection<ProductPromoCond> ProductPromoConds { get; set; }
}