namespace Domain;

public class TelecomNumber
{
    public TelecomNumber()
    {
        OrderItemShipGroups = new HashSet<OrderItemShipGroup>();
        ShipmentDestinationTelecomNumbers = new HashSet<Shipment>();
        ShipmentOriginTelecomNumbers = new HashSet<Shipment>();
        ShipmentRouteSegmentDestTelecomNumbers = new HashSet<ShipmentRouteSegment>();
        ShipmentRouteSegmentOriginTelecomNumbers = new HashSet<ShipmentRouteSegment>();
    }

    public string ContactMechId { get; set; } = null!;
    public string? CountryCode { get; set; }
    public string? AreaCode { get; set; }
    public string? ContactNumber { get; set; }
    public string? AskForName { get; set; }
    public DateTime? LastUpdatedStamp { get; set; }
    public DateTime? LastUpdatedTxStamp { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? CreatedTxStamp { get; set; }

    public ContactMech ContactMech { get; set; } = null!;
    public ICollection<OrderItemShipGroup> OrderItemShipGroups { get; set; }
    public ICollection<Shipment> ShipmentDestinationTelecomNumbers { get; set; }
    public ICollection<Shipment> ShipmentOriginTelecomNumbers { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegmentDestTelecomNumbers { get; set; }
    public ICollection<ShipmentRouteSegment> ShipmentRouteSegmentOriginTelecomNumbers { get; set; }
}