using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class PartyType
{
    public PartyType()
    {
        InverseParentType = new HashSet<PartyType>();
        Parties = new HashSet<Party>();
        PartyNeeds = new HashSet<PartyNeed>();
        PartyTypeAttrs = new HashSet<PartyTypeAttr>();
    }

    public string PartyTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyType? ParentType { get; set; }
    public ICollection<PartyType> InverseParentType { get; set; }
    public ICollection<Party> Parties { get; set; }
    public ICollection<PartyNeed> PartyNeeds { get; set; }
    public ICollection<PartyTypeAttr> PartyTypeAttrs { get; set; }
}