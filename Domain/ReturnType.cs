using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ReturnType
{
    public ReturnType()
    {
        ReturnAdjustments = new HashSet<ReturnAdjustment>();
        ReturnItems = new HashSet<ReturnItem>();
    }

    public string ReturnTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public string? SequenceId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ReturnAdjustment> ReturnAdjustments { get; set; }
    public ICollection<ReturnItem> ReturnItems { get; set; }
}