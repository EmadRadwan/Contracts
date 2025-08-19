namespace Application.Order.Orders;

public class OrderAdjustmentsDto
{
    public string OrderId { get; set; }
    public string StatusDescription { get; set; }
    public string ModificationType { get; set; }

    public ICollection<OrderAdjustmentDto2> OrderAdjustments { get; set; }
}