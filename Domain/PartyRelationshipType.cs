using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class PartyRelationshipType
{
    public PartyRelationshipType()
    {
        InverseParentType = new HashSet<PartyRelationshipType>();
        PartyRelationships = new HashSet<PartyRelationship>();
    }

    public string PartyRelationshipTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? PartyRelationshipName { get; set; }
    public string? Description { get; set; }
    public string? RoleTypeIdValidFrom { get; set; }
    public string? RoleTypeIdValidTo { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyRelationshipType? ParentType { get; set; }
    public RoleType? RoleTypeIdValidFromNavigation { get; set; }
    public RoleType? RoleTypeIdValidToNavigation { get; set; }
    public ICollection<PartyRelationshipType> InverseParentType { get; set; }
    public ICollection<PartyRelationship> PartyRelationships { get; set; }
}