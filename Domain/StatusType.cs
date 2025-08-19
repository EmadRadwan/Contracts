using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class StatusType
{
    public StatusType()
    {
        InverseParentType = new HashSet<StatusType>();
        StatusItems = new HashSet<StatusItem>();
    }

    public string StatusTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public StatusType? ParentType { get; set; }
    public ICollection<StatusType> InverseParentType { get; set; }
    public ICollection<StatusItem> StatusItems { get; set; }
}