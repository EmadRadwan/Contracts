using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class StatusValidChange
{
    public StatusValidChange()
    {
        PicklistStatusHistories = new HashSet<PicklistStatusHistory>();
    }

    public string StatusId { get; set; } = null!;
    public string StatusIdTo { get; set; } = null!;
    public string? ConditionExpression { get; set; }
    public string? TransitionName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public StatusItem Status { get; set; } = null!;
    public StatusItem StatusIdToNavigation { get; set; } = null!;
    public ICollection<PicklistStatusHistory> PicklistStatusHistories { get; set; }
}