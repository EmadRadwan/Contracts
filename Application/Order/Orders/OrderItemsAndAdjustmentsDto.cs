namespace Application.Order.Orders;

public class OrderItemsAndAdjustmentsDto
{
    public ICollection<OrderItemDto2> OrderItems { get; set; }
    public ICollection<OrderAdjustmentDto2> OrderAdjustments { get; set; }
}