using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class FixedAssetType
{
    public FixedAssetType()
    {
        FixedAssetTypeAttrs = new HashSet<FixedAssetTypeAttr>();
        FixedAssets = new HashSet<FixedAsset>();
        InverseParentType = new HashSet<FixedAssetType>();
        WorkEffortFixedAssetStds = new HashSet<WorkEffortFixedAssetStd>();
    }

    public string FixedAssetTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAssetType? ParentType { get; set; }
    public ICollection<FixedAssetTypeAttr> FixedAssetTypeAttrs { get; set; }
    public ICollection<FixedAsset> FixedAssets { get; set; }
    public ICollection<FixedAssetType> InverseParentType { get; set; }
    public ICollection<WorkEffortFixedAssetStd> WorkEffortFixedAssetStds { get; set; }
}