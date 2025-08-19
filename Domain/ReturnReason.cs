using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ReturnReason
{
    public ReturnReason()
    {
        ReturnItems = new HashSet<ReturnItem>();
    }

    public string ReturnReasonId { get; set; } = null!;
    public string? Description { get; set; }
    public string? SequenceId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ReturnItem> ReturnItems { get; set; }
}