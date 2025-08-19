using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class QualityStandard
{
    public string StandardId { get; set; } = null!;
    public string AttributeName { get; set; } = null!;
    public string MeasurementType { get; set; } = null!; // E.g., Numeric, Categorical, Boolean
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    // Navigation property: One QualityStandard can be linked to many ProductQualityStandards
    public ICollection<ProductQualityStandard> ProductQualityStandards { get; set; } =
        new List<ProductQualityStandard>();
}