using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ShipmentStatus
{
    public string StatusId { get; set; } = null!;
    public string ShipmentId { get; set; } = null!;
    public DateTime? StatusDate { get; set; }
    public string? ChangeByUserLoginId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public UserLogin? ChangeByUserLogin { get; set; }
    public Shipment Shipment { get; set; } = null!;
    public StatusItem Status { get; set; } = null!;
}