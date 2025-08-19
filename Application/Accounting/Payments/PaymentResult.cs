namespace Application.Accounting.Payments;


public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public List<string> ErrorMessages { get; set; }
    public string SuccessMessage { get; set; }
}