namespace Application.Shipments.Invoices;

public class CreateInvoicesFromShipmentsResponse
{
    public List<string> InvoicesCreated { get; set; } = new List<string>();
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
}