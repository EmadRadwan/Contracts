namespace Application.Order.Orders;

public class CreatePaymentFromOrderResult
{
    public string PaymentId { get; set; }
    public string Message { get; set; }
    public bool IsSuccess { get; set; }
}