using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class ShipmentMethodType
{
    public ShipmentMethodType()
    {
        CarrierShipmentMethods = new HashSet<CarrierShipmentMethod>();
        FacilityCarrierShipments = new HashSet<FacilityCarrierShipment>();
        OrderItemShipGroups = new HashSet<OrderItemShipGroup>();
        Picklists = new HashSet<Picklist>();
        ProductStoreShipmentMeths = new HashSet<ProductStoreShipmentMeth>();
        ProductStoreVendorShipments = new HashSet<ProductStoreVendorShipment>();
        ShipmentRouteSegments = new HashSet<ShipmentRouteSegment>();
    }

    public string ShipmentMethodTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public int? SequenceNum { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<CarrierShipmentMethod> CarrierShipmentMethods { get; set; }
    public ICollection<FacilityCarrierShipment> FacilityCarrierShipments { get; set; }
    public ICollection<OrderItemShipGroup> OrderItemShipGroups { get; set; }
    public ICollection<Picklist> Picklists { get; set; }
    public ICollection<ProductStoreShipmentMeth> ProductStoreShipmentMeths { get; set; }
    public ICollection<ProductStoreVendorShipment> ProductStoreVendorShipments { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegments { get; set; }
}