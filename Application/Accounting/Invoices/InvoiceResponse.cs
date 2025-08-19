namespace Application.Shipments.Invoices;

public class InvoiceResponse
{
    public string InvoiceId { get; set; }
    public string InvoiceTypeId { get; set; }
    public string Message { get; set; }
    public bool Success { get; set; }
}