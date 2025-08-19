using Domain;

namespace Application.Shipments;

public class QuickShipResult
{
    public List<Shipment> ShipmentShipGroupFacilityList { get; set; }
    public List<string> ShipmentIds { get; set; }
}