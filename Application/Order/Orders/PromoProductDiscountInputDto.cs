namespace Application.Order.Orders;

public class PromoProductDiscountInputDto
{
    public string ProductPromoId { get; set; }
    public OrderItemDto2 OrderItem { get; set; }
}