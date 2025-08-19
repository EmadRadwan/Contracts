using Domain;

namespace Application.Accounting.Services;

public class PaymentStatusChangeResult
{
    public bool Success { get; set; }
    public Payment UpdatedPayment { get; set; }
    public string ErrorCode { get; set; } // REFACTOR: Added ErrorCode to categorize errors for frontend handling
    public string ErrorMessage { get; set; }
}
