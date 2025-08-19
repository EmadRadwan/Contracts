namespace Application.Order.Orders;

public class ShipmentDto
{
    /// <summary>
    /// Gets or sets the Shipment ID.
    /// </summary>
    public string ShipmentId { get; set; }

    /// <summary>
    /// Gets or sets the Status ID of the shipment.
    /// </summary>
    public string StatusId { get; set; }

    /// <summary>
    /// Gets or sets the Destination Facility ID of the shipment.
    /// </summary>
    public string DestinationFacilityId { get; set; }

    // Add other relevant properties as needed
}