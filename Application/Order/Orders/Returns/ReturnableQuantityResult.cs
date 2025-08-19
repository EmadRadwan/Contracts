namespace Application.Order.Orders.Returns;

public class ReturnableQuantityResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public decimal ReturnableQuantity { get; set; }
    public decimal ReturnablePrice { get; set; }
}