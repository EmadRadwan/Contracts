namespace Application.Accounting.Services.Models;

public class CapturePaymentsByInvoiceResult
{
    // Status: COMPLETE, FAILED, or ERROR
    public string Status { get; set; }
    // Optional error message
    public string ErrorMessage { get; set; }
    // Additional data (e.g., from captureOrderPayments)
    public object Data { get; set; }
}