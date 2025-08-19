using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public class ShipmentType
{
    public ShipmentType()
    {
        InverseParentType = new HashSet<ShipmentType>();
        ShipmentTypeAttrs = new HashSet<ShipmentTypeAttr>();
        Shipments = new HashSet<Shipment>();
    }

    public string ShipmentTypeId { get; set; } = null!;
    public string? ParentTypeId { get; set; }
    public string? HasTable { get; set; }
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ShipmentType? ParentType { get; set; }
    public ICollection<ShipmentType> InverseParentType { get; set; }
    public ICollection<ShipmentTypeAttr> ShipmentTypeAttrs { get; set; }
    public ICollection<Shipment> Shipments { get; set; }
}