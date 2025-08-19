using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class AcctgTransType
{
    public AcctgTransType()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        AcctgTransTypeAttrs = new HashSet<AcctgTransTypeAttr>();
        InverseParentType = new HashSet<AcctgTransType>();
    }

    public string AcctgTransTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AcctgTransType? ParentType { get; set; }
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<AcctgTransTypeAttr> AcctgTransTypeAttrs { get; set; }
    public ICollection<AcctgTransType> InverseParentType { get; set; }
}