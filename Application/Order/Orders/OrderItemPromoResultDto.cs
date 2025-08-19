namespace Application.Order.Orders;

public class OrderItemPromoResultDto
{
    public string ResultMessage { get; set; } = string.Empty;
    public List<OrderItemDto2>? OrderItems { get; set; } = new();
    public List<OrderAdjustmentDto2> OrderItemAdjustments { get; set; } = new();
}