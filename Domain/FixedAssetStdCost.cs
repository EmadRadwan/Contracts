using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class FixedAssetStdCost
{
    public string FixedAssetId { get; set; } = null!;
    public string FixedAssetStdCostTypeId { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? AmountUomId { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Uom? AmountUom { get; set; }
    public FixedAsset FixedAsset { get; set; } = null!;
    public FixedAssetStdCostType FixedAssetStdCostType { get; set; } = null!;
}