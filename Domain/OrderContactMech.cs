using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class OrderContactMech
{
    public string OrderId { get; set; } = null!;
    public string ContactMechPurposeTypeId { get; set; } = null!;
    public string ContactMechId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech ContactMech { get; set; } = null!;
    public ContactMechPurposeType ContactMechPurposeType { get; set; } = null!;
    public OrderHeader Order { get; set; } = null!;
}