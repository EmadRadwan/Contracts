using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class WorkEffortAssocType
{
    public WorkEffortAssocType()
    {
        InverseParentType = new HashSet<WorkEffortAssocType>();
        WorkEffortAssocTypeAttrs = new HashSet<WorkEffortAssocTypeAttr>();
        WorkEffortAssocs = new HashSet<WorkEffortAssoc>();
    }

    public string WorkEffortAssocTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public WorkEffortAssocType? ParentType { get; set; }
    public ICollection<WorkEffortAssocType> InverseParentType { get; set; }
    public ICollection<WorkEffortAssocTypeAttr> WorkEffortAssocTypeAttrs { get; set; }
    public ICollection<WorkEffortAssoc> WorkEffortAssocs { get; set; }
}