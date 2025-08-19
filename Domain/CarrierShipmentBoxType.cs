namespace Domain;

public class CarrierShipmentBoxType
{
    public string ShipmentBoxTypeId { get; set; } = null!;
    public string PartyId { get; set; } = null!;
    public string? PackagingTypeCode { get; set; }
    public string? OversizeCode { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public Party Party { get; set; } = null!;
    public ShipmentBoxType ShipmentBoxType { get; set; } = null!;
}