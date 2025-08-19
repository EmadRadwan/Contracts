using Domain;

namespace Application.Accounting.Services.Models;

public class RemovePaymentApplicationResult
{
    public string Message { get; set; }
    public PaymentApplication PaymentApplication { get; set; }
}