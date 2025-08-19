namespace Application.Order.Orders;

public class OrderItemsDto
{
    public string OrderId { get; set; }
    public string StatusDescription { get; set; }
    public string ModificationType { get; set; }
    public ICollection<OrderItemDto2> OrderItems { get; set; }
}