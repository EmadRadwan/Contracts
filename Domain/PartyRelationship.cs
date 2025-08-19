using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class PartyRelationship
{
    public string PartyIdFrom { get; set; } = null!;
    public string PartyIdTo { get; set; } = null!;
    public string RoleTypeIdFrom { get; set; } = null!;
    public string RoleTypeIdTo { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public string? StatusId { get; set; }
    public string? RelationshipName { get; set; }
    public string? SecurityGroupId { get; set; }
    public string? PriorityTypeId { get; set; }
    public string? PartyRelationshipTypeId { get; set; }
    public string? PermissionsEnumId { get; set; }
    public string? PositionTitle { get; set; }
    public string? Comments { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyRelationshipType? PartyRelationshipType { get; set; }
    public PartyRole PartyRole { get; set; } = null!;
    public PartyRole PartyRoleNavigation { get; set; } = null!;
    public PriorityType? PriorityType { get; set; }
    public SecurityGroup? SecurityGroup { get; set; }
    public StatusItem? Status { get; set; }
}