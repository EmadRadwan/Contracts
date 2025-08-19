namespace Application.Order.Orders;

public class OrderPaymentInfoInput
{
    public string? OrderId { get; set; }
    public string? BillingAccountId { get; set; }
    public decimal? GrandTotal { get; set; }
    public decimal? PaymentTotal { get; set; }
    public decimal? BillingAccountAmt { get; set; }
    public List<CartPaymentInfo> PaymentInfo { get; set; }

    // You may also include any other relevant properties as needed
}
