namespace Application.Accounting.Payments;

public class ExpirePaymentGroupMemberInput
{
    public string PaymentGroupId { get; set; } // Required
    public string PaymentId { get; set; } // Required
    public DateTime FromDate { get; set; } // Required
}
