namespace Application.Accounting.Invoices;

public class InvoiceStatusDto
{
    public string InvoiceId { get; set; }
    public string StatusId { get; set; }
    public DateOnly? StatusDate { get; set; }
    public DateOnly? PaidDate { get; set; }
    public bool ActualCurrency { get; set; }
    public string? StatusDescription { get; set; }
}