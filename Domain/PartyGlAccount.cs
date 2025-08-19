using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class PartyGlAccount
{
    public string OrganizationPartyId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public string GlAccountTypeId { get; set; } = null!;
    public string? GlAccountId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccount? GlAccount { get; set; }
    public GlAccountType GlAccountType { get; set; } = null!;
    public Party OrganizationParty { get; set; } = null!;
    public Party Party { get; set; } = null!;
    public PartyRole PartyRole { get; set; } = null!;
}