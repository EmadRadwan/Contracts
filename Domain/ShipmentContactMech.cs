namespace Domain;

public class ShipmentContactMech
{
    public string ShipmentId { get; set; } = null!;
    public string ShipmentContactMechTypeId { get; set; } = null!;
    public string? ContactMechId { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech? ContactMech { get; set; }
    public Shipment Shipment { get; set; } = null!;
    public ShipmentContactMechType ShipmentContactMechType { get; set; } = null!;
}