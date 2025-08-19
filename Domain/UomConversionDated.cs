using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class UomConversionDated
{
    public string UomId { get; set; } = null!;
    public string UomIdTo { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public double? ConversionFactor { get; set; }
    public string? CustomMethodId { get; set; }
    public int? DecimalScale { get; set; }
    public string? RoundingMode { get; set; }
    public string? PurposeEnumId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CustomMethod? CustomMethod { get; set; }
    public Enumeration? PurposeEnum { get; set; }
    public Uom Uom { get; set; } = null!;
    public Uom UomIdToNavigation { get; set; } = null!;
}