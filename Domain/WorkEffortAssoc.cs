using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class WorkEffortAssoc
{
    public WorkEffortAssoc()
    {
        WorkEffortAssocAttributes = new HashSet<WorkEffortAssocAttribute>();
    }

    public string WorkEffortIdFrom { get; set; } = null!;
    public string WorkEffortIdTo { get; set; } = null!;
    public string WorkEffortAssocTypeId { get; set; } = null!;
    public int? SequenceNum { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public WorkEffortAssocType WorkEffortAssocType { get; set; } = null!;
    public WorkEffort WorkEffortIdFromNavigation { get; set; } = null!;
    public WorkEffort WorkEffortIdToNavigation { get; set; } = null!;
    public ICollection<WorkEffortAssocAttribute> WorkEffortAssocAttributes { get; set; }
}