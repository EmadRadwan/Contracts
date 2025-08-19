namespace Domain;

public class ShipmentGatewayConfig
{
    public ShipmentGatewayConfig()
    {
        ProductStoreShipmentMeths = new HashSet<ProductStoreShipmentMeth>();
    }

    public string ShipmentGatewayConfigId { get; set; } = null!;
    public string? ShipmentGatewayConfTypeId { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ShipmentGatewayConfigType? ShipmentGatewayConfType { get; set; }
    public ShipmentGatewayDhl ShipmentGatewayDhl { get; set; } = null!;
    public ShipmentGatewayFedex ShipmentGatewayFedex { get; set; } = null!;
    public ShipmentGatewayUp ShipmentGatewayUp { get; set; } = null!;
    public ShipmentGatewayUsp ShipmentGatewayUsp { get; set; } = null!;
    public ICollection<ProductStoreShipmentMeth> ProductStoreShipmentMeths { get; set; }
}