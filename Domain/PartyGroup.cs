using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class PartyGroup
{
    public PartyGroup()
    {
        PartyInvitationGroupAssocs = new HashSet<PartyInvitationGroupAssoc>();
    }

    public string PartyId { get; set; } = null!;
    public string? GroupName { get; set; }
    public string? GroupNameLocal { get; set; }
    public string? OfficeSiteName { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public int? NumEmployees { get; set; }
    public string? TickerSymbol { get; set; }
    public string? Comments { get; set; }
    public string? LogoImageUrl { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public Affiliate Affiliate { get; set; } = null!;
    public ICollection<PartyInvitationGroupAssoc> PartyInvitationGroupAssocs { get; set; }
}