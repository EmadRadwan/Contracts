using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class WorkEffortGoodStandardType
{
    public WorkEffortGoodStandardType()
    {
        InverseParentType = new HashSet<WorkEffortGoodStandardType>();
        WorkEffortGoodStandards = new HashSet<WorkEffortGoodStandard>();
    }

    public string WorkEffortGoodStdTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public WorkEffortGoodStandardType? ParentType { get; set; }
    public ICollection<WorkEffortGoodStandardType> InverseParentType { get; set; }
    public ICollection<WorkEffortGoodStandard> WorkEffortGoodStandards { get; set; }
}