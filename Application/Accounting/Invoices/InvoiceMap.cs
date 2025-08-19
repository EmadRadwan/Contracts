namespace Application.Shipments.Invoices;

public class InvoiceMap
{
    public string InvoiceId { get; set; }
    public string InvoiceTypeId { get; set; }
    public string CurrencyUomId { get; set; }
    public string Description { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public decimal Amount { get; set; }
    public decimal AmountApplied { get; set; }
    public decimal AmountToApply { get; set; }
}