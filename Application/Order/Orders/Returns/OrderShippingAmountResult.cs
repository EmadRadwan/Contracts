namespace Application.Order.Orders.Returns;

public class OrderShippingAmountResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public decimal ShippingAmount { get; set; }
}