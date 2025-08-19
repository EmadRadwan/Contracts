using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class AcctgTransEntryType
{
    public AcctgTransEntryType()
    {
        AcctgTransEntries = new HashSet<AcctgTransEntry>();
        InverseParentType = new HashSet<AcctgTransEntryType>();
    }

    public string AcctgTransEntryTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public AcctgTransEntryType? ParentType { get; set; }
    public ICollection<AcctgTransEntry> AcctgTransEntries { get; set; }
    public ICollection<AcctgTransEntryType> InverseParentType { get; set; }
}