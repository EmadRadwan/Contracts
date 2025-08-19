namespace Application.Accounting.Invoices;

public class InvoiceStatusDto
{
    public string InvoiceId { get; set; }
    public string StatusId { get; set; }
    public DateTime? StatusDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public bool ActualCurrency { get; set; }
    public string? StatusDescription { get; set; }
}