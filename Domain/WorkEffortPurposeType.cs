using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class WorkEffortPurposeType
{
    public WorkEffortPurposeType()
    {
        InverseParentType = new HashSet<WorkEffortPurposeType>();
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string WorkEffortPurposeTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public WorkEffortPurposeType? ParentType { get; set; }
    public ICollection<WorkEffortPurposeType> InverseParentType { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}