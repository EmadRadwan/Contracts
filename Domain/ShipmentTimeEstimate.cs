namespace Domain;

public class ShipmentTimeEstimate
{
    public string ShipmentMethodTypeId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string RoleTypeId { get; set; } = null!;
    public string GeoIdTo { get; set; } = null!;
    public string GeoIdFrom { get; set; } = null!;
    public DateTime FromDate { get; set; }
    public DateTime? ThruDate { get; set; }
    public decimal? LeadTime { get; set; }
    public string? LeadTimeUomId { get; set; }
    public int? SequenceNumber { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public CarrierShipmentMethod CarrierShipmentMethod { get; set; } = null!;
    public Geo GeoIdFromNavigation { get; set; } = null!;
    public Geo GeoIdToNavigation { get; set; } = null!;
    public Uom? LeadTimeUom { get; set; }
}