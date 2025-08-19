using Domain;

namespace Application.Facilities;

/// <summary>
/// Encapsulates information about a single order item (or item line)
/// along with its associated inventory reservations.
/// </summary>
public class OrderItemInfo
{
    /// <summary>
    /// A collection of OISGIR (OrderItemShipGrpInvRes) records, each representing 
    /// a reservation for a portion of this order item in a particular facility or location.
    /// </summary>
    public List<OrderItemShipGrpInvRes> OrderItemShipGrpInvResList { get; set; } = new List<OrderItemShipGrpInvRes>();

    // Optionally, you can include the base OrderItem entity itself:
    // public OrderItem OrderItem { get; set; }
    
    // Add any other fields needed to manage picking logic, e.g. total quantity, etc.
}
