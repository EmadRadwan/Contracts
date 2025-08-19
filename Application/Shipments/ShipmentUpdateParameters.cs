namespace Application.Shipments;

public class ShipmentUpdateParameters
{
    public string ShipmentId { get; set; }
    public string StatusId { get; set; }
    public DateTime? EstimatedShipDate { get; set; }
    public DateTime? EstimatedArrivalDate { get; set; }
    public string OriginFacilityId { get; set; }
    public string DestinationFacilityId { get; set; }
    public string PartyIdFrom { get; set; }
    public string PartyIdTo { get; set; }
    public string PrimaryOrderId { get; set; }
    public DateTime? EventDate { get; set; }
    // Add other Shipment non-PK fields as needed
    public decimal? AdditionalShippingCharge { get; set; }
    public string AddtlShippingChargeDesc { get; set; }
    public string CurrencyUomId { get; set; }
    public string DestinationContactMechId { get; set; }
    public string DestinationTelecomNumberId { get; set; }
    public DateTime? EstimatedReadyDate { get; set; }
    public decimal? EstimatedShipCost { get; set; }
    public string HandlingInstructions { get; set; }
    public DateTime? LatestCancelDate { get; set; }
    public string OriginContactMechId { get; set; }
    public string OriginTelecomNumberId { get; set; }
    public string PicklistBinId { get; set; }
    public string PrimaryReturnId { get; set; }
    public string PrimaryShipGroupSeqId { get; set; }
}
