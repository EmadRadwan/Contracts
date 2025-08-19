namespace Application.Accounting.Payments;


public class PaymentApplicationParam
{
    public string PaymentApplicationId { get; set; } = null!;
    public string? PaymentId { get; set; }
    public string? InvoiceId { get; set; }
    public string? InvoiceItemSeqId { get; set; }
    public string? BillingAccountId { get; set; }
    public string? OverrideGlAccountId { get; set; }
    public string? ToPaymentId { get; set; }
    public string? TaxAuthGeoId { get; set; }
    public decimal? AmountApplied { get; set; }
}