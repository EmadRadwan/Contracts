namespace Application.Order.Orders;

public class CreatePaymentFromPreferenceInput
{
    public string OrderPaymentPreferenceId { get; set; }
    public string PaymentFromId { get; set; }
    public string PaymentRefNum { get; set; }
    public string Comments { get; set; }
    public DateTime? EventDate { get; set; }
}
