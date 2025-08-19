namespace Application.Shipments;

public class ShipmentParameters
{
    public string ShipmentId { get; set; }
    public string ShipmentTypeId { get; set; }
    public DateTime? EstimatedShipDate { get; set; }
    public DateTime? EstimatedArrivalDate { get; set; }
    public string OriginFacilityId { get; set; }
    public string DestinationFacilityId { get; set; }
    public string PartyIdFrom { get; set; }
    public string PartyIdTo { get; set; }
}