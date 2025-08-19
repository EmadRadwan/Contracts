using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductPromoAction
{
    public string ProductPromoId { get; set; } = null!;
    public string ProductPromoRuleId { get; set; } = null!;
    public string ProductPromoActionSeqId { get; set; } = null!;
    public string? ProductPromoActionEnumId { get; set; }
    public string? CustomMethodId { get; set; }
    public string? OrderAdjustmentTypeId { get; set; }
    public string? ServiceName { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Amount { get; set; }
    public string? ProductId { get; set; }
    public string? PartyId { get; set; }
    public string? UseCartQuantity { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustomMethod? CustomMethod { get; set; }
    public OrderAdjustmentType? OrderAdjustmentType { get; set; }
    public ProductPromo ProductPromo { get; set; } = null!;
    public Enumeration? ProductPromoActionEnum { get; set; }
    public ProductPromoRule ProductPromoNavigation { get; set; } = null!;
}