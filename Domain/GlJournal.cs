using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class GlJournal
{
    public GlJournal()
    {
        AcctgTrans = new HashSet<AcctgTran>();
        PartyAcctgPreferences = new HashSet<PartyAcctgPreference>();
    }

    public string GlJournalId { get; set; } = null!;
    public string? GlJournalName { get; set; }
    public string? OrganizationPartyId { get; set; }
    public string? IsPosted { get; set; }
    public DateTime? PostedDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party? OrganizationParty { get; set; }
    public ICollection<AcctgTran> AcctgTrans { get; set; }
    public ICollection<PartyAcctgPreference> PartyAcctgPreferences { get; set; }
}