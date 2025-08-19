namespace Application.Accounting.Services.Models;

public class UnappliedInvoiceDto
{
    public string InvoiceId { get; set; }
    public string TypeDescription { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public decimal Amount { get; set; }
    public decimal UnappliedAmount { get; set; }
    public string CurrencyUomId { get; set; }
    public string InvoiceTypeId { get; set; }
    public string InvoiceParentTypeId { get; set; }
}