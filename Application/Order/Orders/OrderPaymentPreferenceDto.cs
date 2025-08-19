namespace Application.Order.Orders;

public class OrderPaymentPreferenceDto
{
    public string OrderId { get; set;}
    public string PaymentMethodTypeId { get; set;}
    public string PaymentMethodTypeDescription { get; set;}
    public string StatusId { get; set;}
    public string UomId { get; set;}
    public string UomDescription { get; set;}
    public string StatusDescription { get; set;}
    public decimal MaxAmount { get; set;}

}