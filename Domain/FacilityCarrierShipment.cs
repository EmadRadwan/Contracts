namespace Domain;

public class FacilityCarrierShipment
{
    public string FacilityId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public string ShipmentMethodTypeId { get; set; } = null!;
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CarrierShipmentMethod CarrierShipmentMethod { get; set; } = null!;
    public Facility Facility { get; set; } = null!;
    public Party Party { get; set; } = null!;
    public ShipmentMethodType ShipmentMethodType { get; set; } = null!;
}