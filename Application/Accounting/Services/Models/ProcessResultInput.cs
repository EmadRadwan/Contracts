using Domain;

namespace Application.Accounting.Services.Models;

public class ProcessResultInput
{
    public decimal CaptureAmount { get; set; }
    public string InvoiceId { get; set; }
    public bool CaptureResult { get; set; }
    public OrderPaymentPreference OrderPaymentPreference { get; set; }
    public string CaptureRefNum { get; set; }
}