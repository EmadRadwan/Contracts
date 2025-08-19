namespace Domain;

public class ShipmentContactMechType
{
    public ShipmentContactMechType()
    {
        ShipmentContactMeches = new HashSet<ShipmentContactMech>();
    }

    public string ShipmentContactMechTypeId { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ICollection<ShipmentContactMech> ShipmentContactMeches { get; set; }
}