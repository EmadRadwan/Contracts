using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class WorkEffortType
{
    public WorkEffortType()
    {
        InverseParentType = new HashSet<WorkEffortType>();
        WorkEffortTypeAttrs = new HashSet<WorkEffortTypeAttr>();
        WorkEfforts = new HashSet<WorkEffort>();
    }

    public string WorkEffortTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public WorkEffortType? ParentType { get; set; }
    public ICollection<WorkEffortType> InverseParentType { get; set; }
    public ICollection<WorkEffortTypeAttr> WorkEffortTypeAttrs { get; set; }
    public ICollection<WorkEffort> WorkEfforts { get; set; }
}