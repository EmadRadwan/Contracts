using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ReturnItemTypeMap
{
    public string ReturnItemMapKey { get; set; } = null!;
    public string ReturnHeaderTypeId { get; set; } = null!;
    public string? ReturnItemTypeId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ReturnHeaderType ReturnHeaderType { get; set; } = null!;
}