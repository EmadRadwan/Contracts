namespace Application.Accounting.Services.Models;

public class InvoiceApplPaymentDto
{
    public string InvoiceId { get; set; }
    public string InvoiceTypeId { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public decimal Total { get; set; }
    public decimal AmountApplied { get; set; }
    public decimal AmountToApply { get; set; }
    public string PaymentId { get; set; }
    public DateTime? PaymentEffectiveDate { get; set; }
    public decimal PaymentAmount { get; set; }
    public string CurrencyUomId { get; set; }
}