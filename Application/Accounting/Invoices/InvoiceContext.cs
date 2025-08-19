namespace Application.Shipments.Invoices;

public class InvoiceContext
{
    public List<InvoiceMap> Invoices { get; set; }
    public List<InvoiceMap> InvoicesOtherCurrency { get; set; }

}