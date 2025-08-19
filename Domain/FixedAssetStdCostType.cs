using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class FixedAssetStdCostType
{
    public FixedAssetStdCostType()
    {
        FixedAssetStdCosts = new HashSet<FixedAssetStdCost>();
        InverseParentType = new HashSet<FixedAssetStdCostType>();
    }

    public string FixedAssetStdCostTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FixedAssetStdCostType? ParentType { get; set; }
    public ICollection<FixedAssetStdCost> FixedAssetStdCosts { get; set; }
    public ICollection<FixedAssetStdCostType> InverseParentType { get; set; }
}