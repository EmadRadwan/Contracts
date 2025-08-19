using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ProductQualityStandard
{
    public string ProductQualityStandardId { get; set; } = null!;
    public string ProductId { get; set; } = null!;
    public string StandardId { get; set; } = null!;
    public decimal? MinValue { get; set; } // For Numeric measurements
    public decimal? MaxValue { get; set; } // For Numeric measurements
    public string? UnitOfMeasure { get; set; } // E.g., kg, cm, ohms
    public string? CategoricalValue { get; set; } // E.g., Blue, Red|Green
    public bool? BooleanValue { get; set; } // For Boolean measurements
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;

    public QualityStandard QualityStandard { get; set; } = null!;

    // One ProductQualityStandard can be linked to many InspectionResults
    public ICollection<InspectionResult> InspectionResults { get; set; } = new List<InspectionResult>();
}