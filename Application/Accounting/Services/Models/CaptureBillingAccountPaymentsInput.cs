namespace Application.Accounting.Services.Models;

public class CaptureBillingAccountPaymentsInput
{
    public string InvoiceId { get; set; }
    public string BillingAccountId { get; set; }
    public decimal CaptureAmount { get; set; }
    public string OrderId { get; set; }
}