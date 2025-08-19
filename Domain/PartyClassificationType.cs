using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class PartyClassificationType
{
    public PartyClassificationType()
    {
        InverseParentType = new HashSet<PartyClassificationType>();
        PartyClassificationGroups = new HashSet<PartyClassificationGroup>();
    }

    public string PartyClassificationTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public PartyClassificationType? ParentType { get; set; }
    public ICollection<PartyClassificationType> InverseParentType { get; set; }
    public ICollection<PartyClassificationGroup> PartyClassificationGroups { get; set; }
}