using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlAccountOrganization
{
    public GlAccountOrganization()
    {
        AcctgTransEntries = new HashSet<AcctgTransEntry>();
    }

    public string GlAccountId { get; set; } = null!;
    public string OrganizationPartyId { get; set; } = null!;
    public string? RoleTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public GlAccount GlAccount { get; set; } = null!;
    public Party OrganizationParty { get; set; } = null!;
    public ICollection<AcctgTransEntry> AcctgTransEntries { get; set; }
}