namespace Application.Shipments;

public class UpdateShipmentResult
{
    public string ShipmentId { get; set; }
    public string ShipmentTypeId { get; set; }
    public string OldStatusId { get; set; }
    public string OldPrimaryOrderId { get; set; }
    public string OldOriginFacilityId { get; set; }
    public string OldDestinationFacilityId { get; set; }
}