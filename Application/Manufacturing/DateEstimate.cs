using Domain;

namespace Application.Manufacturing;

public class DateEstimate
{
    public DateTime EstimatedShipDate { get; set; }
    public decimal RemainingQty { get; set; }
    public List<OrderItemShipGroup> Reservations { get; set; } = new List<OrderItemShipGroup>();
}