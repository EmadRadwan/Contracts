namespace Application.Order.Orders;

public class OrderItemsToBeTaxedDto2
{
    public List<OrderItemDto2> OrderItems { get; set; }
    public List<OrderAdjustmentDto> OrderAdjustments { get; set; }
}