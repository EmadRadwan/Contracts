namespace Domain;

public class ShipmentGatewayConfigType
{
    public ShipmentGatewayConfigType()
    {
        InverseParentType = new HashSet<ShipmentGatewayConfigType>();
        ShipmentGatewayConfigs = new HashSet<ShipmentGatewayConfig>();
    }

    public string ShipmentGatewayConfTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ShipmentGatewayConfigType? ParentType { get; set; }
    public ICollection<ShipmentGatewayConfigType> InverseParentType { get; set; }
    public ICollection<ShipmentGatewayConfig> ShipmentGatewayConfigs { get; set; }
}