namespace API.Controllers.Accounting;

public class InvoiceTotalsDto
{
    public string InvoiceId { get; set; }
    public decimal Total { get; set; }
    public decimal OutstandingAmount { get; set; }
}