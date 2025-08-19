using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class CostComponent
{
    public CostComponent()
    {
        CostComponentAttributes = new HashSet<CostComponentAttribute>();
    }

    public string CostComponentId { get; set; } = null!;
    public string? CostComponentTypeId { get; set; }
    public string? ProductId { get; set; }
    public string? ProductFeatureId { get; set; }
    public string? PartyId { get; set; }
    public string? GeoId { get; set; }
    public string? WorkEffortId { get; set; }
    public string? FixedAssetId { get; set; }
    public string? CostComponentCalcId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? Cost { get; set; }
    public string? CostUomId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CostComponentCalc? CostComponentCalc { get; set; }
    public CostComponentType? CostComponentType { get; set; }
    public Uom? CostUom { get; set; }
    public FixedAsset? FixedAsset { get; set; }
    public Geo? Geo { get; set; }
    public Party? Party { get; set; }
    public Product? Product { get; set; }
    public ProductFeature? ProductFeature { get; set; }
    public WorkEffort? WorkEffort { get; set; }
    public ICollection<CostComponentAttribute> CostComponentAttributes { get; set; }
}