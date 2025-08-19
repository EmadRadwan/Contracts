namespace Application.Accounting.Services.Models;

public class CaptureOrderPaymentsInput
{
    // User executing the service
    // Order ID
    public string OrderId { get; set; }
    // Invoice ID
    public string InvoiceId { get; set; }
    // Amount to capture
    public decimal CaptureAmount { get; set; }
    // Optional billing account ID
    public string BillingAccountId { get; set; }
}
