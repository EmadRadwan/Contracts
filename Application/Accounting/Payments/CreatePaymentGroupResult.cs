namespace Application.Accounting.Payments;

public class CreatePaymentGroupResult
{
    public string PaymentGroupId { get; set; }
    public bool IsSuccess { get; set; } = false;
    public string Message { get; set; }
}
