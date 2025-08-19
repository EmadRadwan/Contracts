namespace Application.Accounting.Payments;

public class CreatePaymentGroupMemberInput
{
    public string PaymentGroupId { get; set; } // Required
    public string PaymentId { get; set; } // Required
    public DateTime? FromDate { get; set; } // Optional
}
