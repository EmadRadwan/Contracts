using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Domain;
[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]

public class CarrierShipmentMethod
{
    public CarrierShipmentMethod()
    {
        FacilityCarrierShipments = new HashSet<FacilityCarrierShipment>();
        OrderItemShipGroups = new HashSet<OrderItemShipGroup>();
        ShipmentCostEstimates = new HashSet<ShipmentCostEstimate>();
        ShipmentTimeEstimates = new HashSet<ShipmentTimeEstimate>();
        ShoppingLists = new HashSet<ShoppingList>();
    }

    public string ShipmentMethodTypeId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public int? SequenceNumber { get; set; }
    public string? CarrierServiceCode { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public PartyRole PartyRole { get; set; } = null!;
    public ShipmentMethodType ShipmentMethodType { get; set; } = null!;
    public ICollection<FacilityCarrierShipment> FacilityCarrierShipments { get; set; }
    public ICollection<OrderItemShipGroup> OrderItemShipGroups { get; set; }
    public ICollection<ShipmentCostEstimate> ShipmentCostEstimates { get; set; }
    public ICollection<ShipmentTimeEstimate> ShipmentTimeEstimates { get; set; }
    public ICollection<ShoppingList> ShoppingLists { get; set; }
}