using Domain;

namespace Application.Facilities;

public class OrderHeaderInfo
{
    /// <summary>
    /// The main order header record (e.g., containing OrderId, Status, OrderDate, etc.).
    /// </summary>
    public OrderHeader OrderHeader { get; set; }

    /// <summary>
    /// The ship group within the order (e.g., shipping address, carrier, 
    /// and other group-specific data).
    /// </summary>
    public OrderItemShipGroup OrderItemShipGroup { get; set; }

    /// <summary>
    /// Detailed item information associated with this OrderHeader & ShipGroup.
    /// Each OrderItemInfo includes references to the OrderItemShipGrpInvRes entries.
    /// </summary>
    public List<OrderItemInfo> OrderItemInfoList { get; set; } = new List<OrderItemInfo>();
}
