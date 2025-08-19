namespace Application.Order.Orders;

public class PaymentDetail
{
    public string PaymentMethodId { get; set; } // For payment methods
    public string? PaymentMethodTypeId { get; set; } // For payment types
    public decimal Amount { get; set; }
    public string? Reference { get; set; }
}   