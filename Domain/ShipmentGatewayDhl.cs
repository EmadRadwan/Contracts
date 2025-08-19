namespace Domain;

public class ShipmentGatewayDhl
{
    public string ShipmentGatewayConfigId { get; set; } = null!;
    public string? ConnectUrl { get; set; }
    public int? ConnectTimeout { get; set; }
    public string? HeadVersion { get; set; }
    public string? HeadAction { get; set; }
    public string? AccessUserId { get; set; }
    public string? AccessPassword { get; set; }
    public string? AccessAccountNbr { get; set; }
    public string? AccessShippingKey { get; set; }
    public string? LabelImageFormat { get; set; }
    public string? RateEstimateTemplate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ShipmentGatewayConfig ShipmentGatewayConfig { get; set; } = null!;
}