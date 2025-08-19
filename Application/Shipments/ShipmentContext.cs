namespace Application.Shipments;

public class ShipmentContext
{
    public string ShipmentId { get; set; }
    public string ShipmentTypeId { get; set; }
    public DateTime? EstimatedShipDate { get; set; }
    public DateTime? EstimatedArrivalDate { get; set; }
    public string OriginFacilityId { get; set; }
    public string DestinationFacilityId { get; set; }
    public string PartyIdFrom { get; set; }
    public string PartyIdTo { get; set; }
    public string HandlingInstructions { get; set; }
    
    
    public string PrimaryOrderId { get; set; }
    public string PrimaryShipGroupSeqId { get; set; }
    public string StatusId { get; set; }
    public decimal AdditionalShippingCharge { get; set; }
}