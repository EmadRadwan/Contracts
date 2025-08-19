namespace Application.Accounting.Payments;

public class UpdatePaymentGroupMemberInput
{
    public string PaymentGroupId { get; set; } // Required
    public string PaymentId { get; set; } // Required
    public DateTime FromDate { get; set; } // Required
    public DateTime? ThruDate { get; set; } // Optional (used to expire)
}
