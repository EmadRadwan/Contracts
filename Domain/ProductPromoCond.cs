using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ProductPromoCond
{
    public string ProductPromoId { get; set; } = null!;
    public string ProductPromoRuleId { get; set; } = null!;
    public string ProductPromoCondSeqId { get; set; } = null!;
    public string? CustomMethodId { get; set; }
    public string? InputParamEnumId { get; set; }
    public string? OperatorEnumId { get; set; }
    public string? CondValue { get; set; }
    public string? OtherValue { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustomMethod? CustomMethod { get; set; }
    public Enumeration? InputParamEnum { get; set; }
    public Enumeration? OperatorEnum { get; set; }
    public ProductPromo ProductPromo { get; set; } = null!;
    public ProductPromoRule ProductPromoNavigation { get; set; } = null!;
}