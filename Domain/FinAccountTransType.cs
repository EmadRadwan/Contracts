using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class FinAccountTransType
{
    public FinAccountTransType()
    {
        FinAccountTrans = new HashSet<FinAccountTran>();
        FinAccountTransTypeAttrs = new HashSet<FinAccountTransTypeAttr>();
        InverseParentType = new HashSet<FinAccountTransType>();
    }

    public string FinAccountTransTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public FinAccountTransType? ParentType { get; set; }
    public ICollection<FinAccountTran> FinAccountTrans { get; set; }
    public ICollection<FinAccountTransTypeAttr> FinAccountTransTypeAttrs { get; set; }
    public ICollection<FinAccountTransType> InverseParentType { get; set; }
}