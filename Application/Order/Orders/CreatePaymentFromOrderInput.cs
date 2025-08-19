namespace Application.Order;

public class CreatePaymentFromOrderInput
{
    public string OrderId { get; set; }
    public string PaymentMethodId { get; set; }
    public string PaymentMethodTypeId { get; set; }
    public string PaymentRefNum { get; set; }
    public string Comments { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow; // Default value set here
}
