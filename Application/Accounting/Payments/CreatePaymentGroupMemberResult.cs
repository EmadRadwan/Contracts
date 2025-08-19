namespace Application.Accounting.Payments;

public class CreatePaymentGroupMemberResult
{
    public string PaymentGroupId { get; set; }
    public string PaymentId { get; set; }
    public bool IsSuccess { get; set; } = false;
    public string Message { get; set; }
}
