using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ProductPromoCategory
{
    public string ProductPromoId { get; set; } = null!;
    public string ProductPromoRuleId { get; set; } = null!;
    public string ProductPromoActionSeqId { get; set; } = null!;
    public string ProductPromoCondSeqId { get; set; } = null!;
    public string ProductCategoryId { get; set; } = null!;
    public string AndGroupId { get; set; } = null!;
    public string? ProductPromoApplEnumId { get; set; }
    public string? IncludeSubCategories { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ProductCategory ProductCategory { get; set; } = null!;
    public ProductPromo ProductPromo { get; set; } = null!;
    public Enumeration? ProductPromoApplEnum { get; set; }
}