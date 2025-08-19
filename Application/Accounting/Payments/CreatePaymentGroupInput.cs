namespace Application.Accounting.Payments;

public class CreatePaymentGroupInput
{
    public string PaymentGroupId { get; set; } // Required
    public string PaymentGroupTypeId { get; set; } // Required
    public string PaymentGroupName { get; set; } // Required
}
