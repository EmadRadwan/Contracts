using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class TaxAuthorityGlAccount
{
    public string TaxAuthGeoId { get; set; } = null!;
    public string TaxAuthPartyId { get; set; } = null!;
    public string OrganizationPartyId { get; set; } = null!;
    public string? GlAccountId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccount? GlAccount { get; set; }
    public Party OrganizationParty { get; set; } = null!;
    public TaxAuthority TaxAuth { get; set; } = null!;
}