namespace Domain;

public class ShipmentGatewayUsp
{
    public string ShipmentGatewayConfigId { get; set; } = null!;
    public string? ConnectUrl { get; set; }
    public string? ConnectUrlLabels { get; set; }
    public int? ConnectTimeout { get; set; }
    public string? AccessUserId { get; set; }
    public string? AccessPassword { get; set; }
    public int? MaxEstimateWeight { get; set; }
    public string? Test { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ShipmentGatewayConfig ShipmentGatewayConfig { get; set; } = null!;
}