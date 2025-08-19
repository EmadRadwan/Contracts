namespace Application.Order.Orders;

public class ReceiveOfflinePaymentInput
{
    public string OrderId { get; set; }
    public string PartyId { get; set; }
    public List<PaymentDetail> PaymentDetails { get; set; }
}