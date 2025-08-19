namespace Domain;

public class ShipmentGatewayFedex
{
    public string ShipmentGatewayConfigId { get; set; } = null!;
    public string? ConnectUrl { get; set; }
    public string? ConnectSoapUrl { get; set; }
    public int? ConnectTimeout { get; set; }
    public string? AccessAccountNbr { get; set; }
    public string? AccessMeterNumber { get; set; }
    public string? AccessUserKey { get; set; }
    public string? AccessUserPwd { get; set; }
    public string? LabelImageType { get; set; }
    public string? DefaultDropoffType { get; set; }
    public string? DefaultPackagingType { get; set; }
    public string? TemplateShipment { get; set; }
    public string? TemplateSubscription { get; set; }
    public string? RateEstimateTemplate { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ShipmentGatewayConfig ShipmentGatewayConfig { get; set; } = null!;
}